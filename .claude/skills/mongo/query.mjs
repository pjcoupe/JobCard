#!/usr/bin/env node
/**
 * Read-only MongoDB query helper for the jobcard databases.
 *
 * Reuses the driver already installed in webappNode/node_modules and the
 * connection details in webappNode/.env, so there is nothing extra to install.
 *
 * Long strings (base64Image on jobPictures, for one) are truncated to keep a
 * result readable; pass --maxString to change that.
 *
 * Usage:
 *   node query.mjs <db> <collection> [filterJson] [options]
 *   node query.mjs --eval "<expression>"
 *
 * Options:
 *   --limit N          default 5
 *   --projection JSON
 *   --sort JSON
 *   --count            return a count instead of documents
 *   --distinct FIELD
 *   --maxString N      truncate strings longer than this, default 200 (0 = off)
 *   --dbs              list databases and their collections
 *
 * --eval runs an async expression with `client` and `db(name)` in scope, e.g.
 *   node query.mjs --eval "db('wheel').collection('jobPictures').countDocuments({})"
 */
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const webappNode = resolve(here, '../../../webappNode');

const { MongoClient, ObjectId } = await import(
  pathToFileURL(resolve(webappNode, 'node_modules/mongodb/lib/index.js')).href
);

/** Let filters use extended JSON for the types plain JSON cannot carry. */
function reviver(_key, value) {
  if (value && typeof value === 'object') {
    if (typeof value.$oid === 'string') return new ObjectId(value.$oid);
    if (typeof value.$date === 'string') return new Date(value.$date);
  }
  return value;
}
const parse = (text) => JSON.parse(text, reviver);

/** Same precedence the app uses: MONGO_IP/MONGO_PORT, else MONGO_URL, else localhost. */
function mongoUrl() {
  const env = {};
  try {
    for (const line of readFileSync(resolve(webappNode, '.env'), 'utf8').split(/\r?\n/)) {
      const t = line.trim();
      if (!t || t.startsWith('#')) continue;
      const eq = t.indexOf('=');
      if (eq > 0) env[t.slice(0, eq).trim()] = t.slice(eq + 1).trim().replace(/^["']|["']$/g, '');
    }
  } catch {
    // no .env — fall through to defaults
  }
  const host = process.env.MONGO_IP ?? env.MONGO_IP;
  const port = process.env.MONGO_PORT ?? env.MONGO_PORT ?? '27017';
  if (host) return `mongodb://${host}:${port}`;
  return process.env.MONGO_URL ?? env.MONGO_URL ?? 'mongodb://localhost:27017';
}

const argv = process.argv.slice(2);
function flag(name) {
  const i = argv.indexOf(`--${name}`);
  if (i < 0) return undefined;
  return argv[i + 1];
}
const has = (name) => argv.includes(`--${name}`);
const positional = [];
for (let i = 0; i < argv.length; i++) {
  if (argv[i].startsWith('--')) {
    if (!['count', 'dbs'].includes(argv[i].slice(2))) i++;
    continue;
  }
  positional.push(argv[i]);
}

const maxString = Number(flag('maxString') ?? 200);

/** Keep output readable: shorten long strings and big binaries. */
function trim(value, depth = 0) {
  if (value === null || value === undefined) return value;
  if (typeof value === 'string') {
    if (maxString > 0 && value.length > maxString) {
      return `${value.slice(0, maxString)}…[${value.length} chars total]`;
    }
    return value;
  }
  if (Buffer.isBuffer(value)) return `<Buffer ${value.length} bytes>`;
  if (value?._bsontype === 'Binary') return `<BSON Binary ${value.length?.() ?? '?'} bytes>`;
  if (value instanceof Date) return value.toISOString();
  if (Array.isArray(value)) return value.map((v) => trim(v, depth + 1));
  if (typeof value === 'object') {
    if (value?._bsontype === 'ObjectId') return `ObjectId(${value.toHexString()})`;
    const out = {};
    for (const [k, v] of Object.entries(value)) out[k] = trim(v, depth + 1);
    return out;
  }
  return value;
}

const client = new MongoClient(mongoUrl(), { serverSelectionTimeoutMS: 10000 });
await client.connect();
const db = (name) => client.db(name);

try {
  if (has('dbs')) {
    const listed = await client.db().admin().listDatabases({ nameOnly: true });
    const out = {};
    for (const d of listed.databases) {
      if (['admin', 'config', 'local'].includes(d.name)) continue;
      out[d.name] = (await client.db(d.name).listCollections().toArray()).map((c) => c.name);
    }
    console.log(JSON.stringify(out, null, 2));
  } else if (has('eval')) {
    const expression = flag('eval');
    const run = new Function('client', 'db', 'ObjectId', `return (async () => (${expression}))()`);
    console.log(JSON.stringify(trim(await run(client, db, ObjectId)), null, 2));
  } else {
    const [dbName, collectionName, filterJson] = positional;
    if (!dbName || !collectionName) {
      console.error('Usage: node query.mjs <db> <collection> [filterJson] [options]');
      process.exit(2);
    }
    const collection = client.db(dbName).collection(collectionName);
    const filter = filterJson ? parse(filterJson) : {};

    if (has('count')) {
      console.log(JSON.stringify({ count: await collection.countDocuments(filter) }));
    } else if (flag('distinct')) {
      console.log(JSON.stringify(trim(await collection.distinct(flag('distinct'), filter)), null, 2));
    } else {
      let cursor = collection.find(filter).limit(Number(flag('limit') ?? 5));
      if (flag('projection')) cursor = cursor.project(parse(flag('projection')));
      if (flag('sort')) cursor = cursor.sort(parse(flag('sort')));
      console.log(JSON.stringify(trim(await cursor.toArray()), null, 2));
    }
  }
} finally {
  await client.close();
}
