namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using MongoDB.Bson;

    public class XeroContactMatch
    {
        public string ContactID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
    }

    public class XeroTenant
    {
        public string tenantId { get; set; }
        public string tenantName { get; set; }
    }

    public class XeroInvoiceResult
    {
        public bool Success { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string RawResponse { get; set; }
    }

    public sealed class XeroOAuthCallbackResult
    {
        private XeroOAuthCallbackResult(bool success, string code, string errorMessage)
        {
            this.Success = success;
            this.Code = code;
            this.ErrorMessage = errorMessage;
        }

        public bool Success { get; private set; }
        public string Code { get; private set; }
        public string ErrorMessage { get; private set; }

        public static XeroOAuthCallbackResult Succeeded(string code)
        {
            return new XeroOAuthCallbackResult(true, code, null);
        }

        public static XeroOAuthCallbackResult Failed(string errorMessage)
        {
            return new XeroOAuthCallbackResult(false, null, errorMessage);
        }
    }

    public static class XeroService
    {
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

        public static string GetDefaultMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode) || mode == "Draft")
            {
                return "Draft";
            }
            return "AuthoriseAndEmail";
        }

        public static string BuildAuthorizeUrl(SettingsSettingsDoc settings, string state)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.xeroClientId) || string.IsNullOrWhiteSpace(settings.xeroRedirectUri))
            {
                throw new InvalidOperationException("Xero client settings are incomplete.");
            }
            string scope = Uri.EscapeDataString("openid profile accounting.invoices accounting.payments accounting.contacts");// email accounting.transactions accounting.contacts offline_access");
            return "https://login.xero.com/identity/connect/authorize" +
                   "?response_type=code" +
                   "&client_id=" + Uri.EscapeDataString(settings.xeroClientId) +
                   "&redirect_uri=" + Uri.EscapeDataString(settings.xeroRedirectUri) +
                   "&scope=" + scope +
                   "&state=" + Uri.EscapeDataString(state);
        }

        public static void OpenAuthInBrowser(string url)
        {
            Process.Start(url);
        }

        public static async Task<bool> ExchangeCodeAsync(SettingsSettingsDoc settings, string authCode)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://identity.xero.com/connect/token");
                string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.xeroClientId + ":" + settings.xeroClientSecret));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type","authorization_code"},
                    {"code", authCode},
                    {"redirect_uri", settings.xeroRedirectUri}
                });
                var response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                var map = Json.Deserialize<Dictionary<string, object>>(content);
                object accessToken = map.ContainsKey("access_token") ? map["access_token"] : null;
                var updates = new List<KeyValuePair<string, dynamic>>
                {
                    // activeXeroToken is the field shared with the web app; xeroAccessToken
                    // is written alongside it for older builds. See ActiveToken.
                    new KeyValuePair<string, dynamic>("activeXeroToken", accessToken),
                    new KeyValuePair<string, dynamic>("xeroAccessToken", accessToken),
                    new KeyValuePair<string, dynamic>("xeroRefreshToken", map.ContainsKey("refresh_token") ? map["refresh_token"] : null)
                };
                if (map.ContainsKey("expires_in"))
                {
                    int seconds = Convert.ToInt32(map["expires_in"]);
                    updates.Add(new KeyValuePair<string, dynamic>("xeroTokenExpiresAtUtc", DateTime.UtcNow.AddSeconds(seconds - 60)));
                }
                await DataAccess.UpdateSettingsFieldsAsync(updates);
                return true;
            }
        }

        // How long the refresh lease is held, and how long a caller that lost the lease
        // waits for the winner before giving up and trying itself.
        private const int TokenLeaseSeconds = 30;
        private const int TokenLeaseWaitMs = 500;
        private const int TokenLeaseWaitAttempts = 20;

        /// <summary>
        /// The Xero access token currently in use. activeXeroToken is the shared field that
        /// both this app and the web app read and write, so the two run off one Xero
        /// connection. xeroAccessToken is still written alongside it and is used as a
        /// fallback here, so a partially upgraded fleet keeps working.
        /// </summary>
        public static string ActiveToken(SettingsSettingsDoc settings)
        {
            if (settings == null)
            {
                return "";
            }
            if (!string.IsNullOrWhiteSpace(settings.activeXeroToken))
            {
                return settings.activeXeroToken;
            }
            return settings.xeroAccessToken == null ? "" : settings.xeroAccessToken;
        }

        /// <summary>True when the stored token is present and not about to expire.</summary>
        private static bool TokenIsFresh(SettingsSettingsDoc settings)
        {
            return settings != null
                && !string.IsNullOrWhiteSpace(ActiveToken(settings))
                && settings.xeroTokenExpiresAtUtc.HasValue
                && settings.xeroTokenExpiresAtUtc.Value > DateTime.UtcNow.AddMinutes(1);
        }

        /// <summary>
        /// Copy a freshly stored token set onto the caller's in-memory settings object, so a
        /// caller that does not re-read settings still uses the new token rather than the
        /// stale one it loaded.
        /// </summary>
        private static void CopyTokenInto(SettingsSettingsDoc target, SettingsSettingsDoc source)
        {
            if (target == null || source == null)
            {
                return;
            }
            target.activeXeroToken = source.activeXeroToken;
            target.xeroAccessToken = source.xeroAccessToken;
            target.xeroRefreshToken = source.xeroRefreshToken;
            target.xeroTokenExpiresAtUtc = source.xeroTokenExpiresAtUtc;
        }

        public static async Task<bool> EnsureValidTokenAsync(SettingsSettingsDoc settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.xeroRefreshToken))
            {
                return false;
            }
            if (TokenIsFresh(settings))
            {
                return true;
            }

            // The web app or another window may have refreshed since our caller loaded
            // settings, in which case there is nothing to do but pick up the new token.
            SettingsSettingsDoc latest = await DataAccess.findSettings();
            if (TokenIsFresh(latest))
            {
                CopyTokenInto(settings, latest);
                return true;
            }

            // Only one app may call Xero's token endpoint at a time: a refresh rotates the
            // refresh token and retires the old one, so a simultaneous second call is
            // rejected and that app has to reconnect.
            bool holdsLease = await DataAccess.TryAcquireXeroTokenLockAsync(TokenLeaseSeconds);
            if (!holdsLease)
            {
                for (int attempt = 0; attempt < TokenLeaseWaitAttempts; attempt++)
                {
                    await Task.Delay(TokenLeaseWaitMs);
                    latest = await DataAccess.findSettings();
                    if (TokenIsFresh(latest))
                    {
                        CopyTokenInto(settings, latest);
                        return true;
                    }
                }
                // Whoever held the lease never finished. Rather than leave the user stuck,
                // try ourselves — by now the lease has expired so this should succeed.
                holdsLease = await DataAccess.TryAcquireXeroTokenLockAsync(TokenLeaseSeconds);
                if (!holdsLease)
                {
                    return false;
                }
            }

            try
            {
                // Always refresh with the newest refresh token on record, not the one our
                // caller happened to load.
                string refreshToken = (latest != null && !string.IsNullOrWhiteSpace(latest.xeroRefreshToken))
                    ? latest.xeroRefreshToken
                    : settings.xeroRefreshToken;

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://identity.xero.com/connect/token");
                    string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.xeroClientId + ":" + settings.xeroClientSecret));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"grant_type","refresh_token"},
                        {"refresh_token", refreshToken}
                    });
                    var response = await client.SendAsync(request);
                    string content = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                            "XERO TOKEN REFRESH FAILED: HTTP {0} {1}. Body: {2}",
                            (int)response.StatusCode, response.ReasonPhrase,
                            string.IsNullOrEmpty(content) ? "(empty)" : content));
                        return false;
                    }
                    var map = Json.Deserialize<Dictionary<string, object>>(content);
                    string newAccessToken = map.ContainsKey("access_token") ? Convert.ToString(map["access_token"]) : ActiveToken(settings);
                    string newRefreshToken = map.ContainsKey("refresh_token") ? Convert.ToString(map["refresh_token"]) : refreshToken;
                    var updates = new List<KeyValuePair<string, dynamic>>
                    {
                        new KeyValuePair<string, dynamic>("activeXeroToken", newAccessToken),
                        new KeyValuePair<string, dynamic>("xeroAccessToken", newAccessToken),
                        new KeyValuePair<string, dynamic>("xeroRefreshToken", newRefreshToken)
                    };
                    DateTime? expiresAt = null;
                    if (map.ContainsKey("expires_in"))
                    {
                        int seconds = Convert.ToInt32(map["expires_in"]);
                        expiresAt = DateTime.UtcNow.AddSeconds(seconds - 60);
                        updates.Add(new KeyValuePair<string, dynamic>("xeroTokenExpiresAtUtc", expiresAt));
                    }
                    await DataAccess.UpdateSettingsFieldsAsync(updates);

                    settings.activeXeroToken = newAccessToken;
                    settings.xeroAccessToken = newAccessToken;
                    settings.xeroRefreshToken = newRefreshToken;
                    if (expiresAt.HasValue)
                    {
                        settings.xeroTokenExpiresAtUtc = expiresAt;
                    }
                    return true;
                }
            }
            finally
            {
                await DataAccess.ReleaseXeroTokenLockAsync();
            }
        }

        public static async Task<List<XeroTenant>> GetTenantsAsync(SettingsSettingsDoc settings)
        {
            var list = new List<XeroTenant>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ActiveToken(settings));
                var response = await client.GetAsync("https://api.xero.com/connections");
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return list;
                }
                var rows = Json.Deserialize<List<Dictionary<string, object>>>(content);
                foreach (var row in rows)
                {
                    list.Add(new XeroTenant
                    {
                        tenantId = row.ContainsKey("tenantId") ? Convert.ToString(row["tenantId"]) : "",
                        tenantName = row.ContainsKey("tenantName") ? Convert.ToString(row["tenantName"]) : ""
                    });
                }
            }
            return list;
        }

        public static async Task<List<XeroContactMatch>> FindContactsAsync(SettingsSettingsDoc settings, string tenantId, string businessName)
        {
            var matches = new List<XeroContactMatch>();
            string term = businessName != null ? businessName.Trim() : "";
            if (string.IsNullOrEmpty(term))
            {
                return matches;
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ActiveToken(settings));
                client.DefaultRequestHeaders.Add("xero-tenant-id", tenantId);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string url = "https://api.xero.com/api.xro/2.0/Contacts?searchTerm=" + Uri.EscapeDataString(term);
                var response = await client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return matches;
                }
                var root = Json.Deserialize<Dictionary<string, object>>(content);
                if (!root.ContainsKey("Contacts"))
                {
                    return matches;
                }
                var contacts = root["Contacts"] as System.Collections.ArrayList;
                if (contacts == null)
                {
                    return matches;
                }
                foreach (Dictionary<string, object> c in contacts)
                {
                    string name = c.ContainsKey("Name") ? Convert.ToString(c["Name"]) : "";
                    if (string.IsNullOrEmpty(name) || name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                    matches.Add(new XeroContactMatch
                    {
                        ContactID = c.ContainsKey("ContactID") ? Convert.ToString(c["ContactID"]) : "",
                        Name = name,
                        EmailAddress = c.ContainsKey("EmailAddress") ? Convert.ToString(c["EmailAddress"]) : ""
                    });
                }
            }
            return matches;
        }

        public static async Task<XeroInvoiceResult> CreateInvoiceAsync(SettingsSettingsDoc settings, string tenantId, string contactId, string mode, string reference, List<Dictionary<string, object>> lineItems, DateTime dueDate)
        {
            var result = new XeroInvoiceResult();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ActiveToken(settings));
                client.DefaultRequestHeaders.Add("xero-tenant-id", tenantId);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var invoice = new Dictionary<string, object>();
                invoice["Type"] = "ACCREC";
                invoice["Contact"] = new Dictionary<string, object> { { "ContactID", contactId } };
                invoice["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                invoice["DueDate"] = dueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                invoice["Reference"] = reference;
                invoice["LineItems"] = lineItems;
                invoice["LineAmountTypes"] = "Exclusive";
                invoice["CurrencyCode"] = "NZD";
                invoice["Status"] = mode == "AuthoriseAndEmail" ? "AUTHORISED" : "DRAFT";
                var body = new Dictionary<string, object> { { "Invoices", new List<Dictionary<string, object>> { invoice } } };
                string payload = Json.Serialize(body);
                var response = await client.PostAsync("https://api.xero.com/api.xro/2.0/Invoices", new StringContent(payload, Encoding.UTF8, "application/json"));
                string content = await response.Content.ReadAsStringAsync();
                result.RawResponse = content;
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                    result.ErrorMessage = content;
                    return result;
                }
                var root = Json.Deserialize<Dictionary<string, object>>(content);
                var invoices = root["Invoices"] as System.Collections.ArrayList;
                if (invoices != null && invoices.Count > 0)
                {
                    var first = invoices[0] as Dictionary<string, object>;
                    result.Success = true;
                    result.InvoiceId = first.ContainsKey("InvoiceID") ? Convert.ToString(first["InvoiceID"]) : "";
                    result.InvoiceNumber = first.ContainsKey("InvoiceNumber") ? Convert.ToString(first["InvoiceNumber"]) : "";
                    result.Status = first.ContainsKey("Status") ? Convert.ToString(first["Status"]) : "";
                }
                if (result.Success && mode == "AuthoriseAndEmail" && !string.IsNullOrWhiteSpace(result.InvoiceId))
                {
                    await client.PostAsync("https://api.xero.com/api.xro/2.0/Invoices/" + result.InvoiceId + "/Email", new StringContent("", Encoding.UTF8, "application/json"));
                }
            }
            return result;
        }

        public static async Task<Dictionary<string, object>> GetInvoiceAsync(SettingsSettingsDoc settings, string tenantId, string invoiceId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ActiveToken(settings));
                client.DefaultRequestHeaders.Add("xero-tenant-id", tenantId);
                var response = await client.GetAsync("https://api.xero.com/api.xro/2.0/Invoices/" + invoiceId);
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var root = Json.Deserialize<Dictionary<string, object>>(content);
                return root;
            }
        }

        public static async Task<XeroInvoiceResult> UpdateInvoiceStatusAsync(SettingsSettingsDoc settings, string tenantId, string invoiceId, string status)
        {
            var result = new XeroInvoiceResult();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ActiveToken(settings));
                client.DefaultRequestHeaders.Add("xero-tenant-id", tenantId);
                var body = new Dictionary<string, object>
                {
                    {
                        "Invoices", new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object>
                            {
                                {"InvoiceID", invoiceId},
                                {"Status", status}
                            }
                        }
                    }
                };
                string payload = Json.Serialize(body);
                var response = await client.PostAsync("https://api.xero.com/api.xro/2.0/Invoices", new StringContent(payload, Encoding.UTF8, "application/json"));
                string content = await response.Content.ReadAsStringAsync();
                result.RawResponse = content;
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                    result.ErrorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "HTTP {0} {1}. Body: {2}",
                        (int)response.StatusCode,
                        response.ReasonPhrase,
                        string.IsNullOrWhiteSpace(content) ? "(empty)" : content);
                    return result;
                }
                result.Success = true;
                return result;
            }
        }

        public static BsonArray BuildLineItemsSnapshot(List<Dictionary<string, object>> lineItems)
        {
            var array = new BsonArray();
            foreach (var line in lineItems)
            {
                array.Add(BsonDocument.Parse(Json.Serialize(line)));
            }
            return array;
        }

        public static bool IsLocalHttpRedirectUri(string redirectUri)
        {
            Uri u;
            if (string.IsNullOrWhiteSpace(redirectUri) || !Uri.TryCreate(redirectUri.Trim(), UriKind.Absolute, out u))
            {
                return false;
            }
            if (!string.Equals(u.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return string.Equals(u.Host, "localhost", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<XeroOAuthCallbackResult> CaptureAuthorizationCodeFromLocalRedirectAsync(
            string redirectUri,
            string expectedState,
            Action openAuthorizeUrl,
            TimeSpan waitTimeout,
            CancellationToken cancellationToken)
        {
            Uri redirect;
            if (string.IsNullOrWhiteSpace(redirectUri) || !Uri.TryCreate(redirectUri.Trim(), UriKind.Absolute, out redirect))
            {
                return XeroOAuthCallbackResult.Failed("Redirect URI is not a valid absolute URL.");
            }
            if (!XeroService.IsLocalHttpRedirectUri(redirectUri))
            {
                return XeroOAuthCallbackResult.Failed("Redirect URI must be http://localhost... for automatic login capture.");
            }
            string expectedPath = XeroService.NormalizeCallbackPath(redirect.AbsolutePath);
            string prefix = XeroService.BuildListenerPrefix(redirect);
            if (string.IsNullOrEmpty(prefix))
            {
                return XeroOAuthCallbackResult.Failed("Could not build a listener prefix for the redirect URI.");
            }
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            try
            {
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                return XeroOAuthCallbackResult.Failed(
                    "Could not listen on the redirect URL (port in use or permission denied). " +
                    "If this is permission-related, an administrator may need to run: netsh http add urlacl url=" + prefix + " user=DOMAIN\\user " +
                    Environment.NewLine + "Details: " + ex.Message);
            }
            catch (Exception ex)
            {
                return XeroOAuthCallbackResult.Failed("Could not start local redirect listener: " + ex.Message);
            }
            IDisposable stopRegistration = null;
            try
            {
                stopRegistration = cancellationToken.Register(
                    () =>
                    {
                        try
                        {
                            listener.Stop();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        catch (HttpListenerException)
                        {
                        }
                    });
                openAuthorizeUrl();
                Task<HttpListenerContext> contextTask = listener.GetContextAsync();
                Task delayTask = Task.Delay(waitTimeout, cancellationToken);
                Task completed = await Task.WhenAny(contextTask, delayTask).ConfigureAwait(false);
                if (completed != contextTask)
                {
                    try
                    {
                        listener.Stop();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (HttpListenerException)
                    {
                    }
                    try
                    {
                        await contextTask.ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (HttpListenerException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return XeroOAuthCallbackResult.Failed("Authorization cancelled.");
                    }
                    return XeroOAuthCallbackResult.Failed("Timed out waiting for Xero redirect.");
                }
                HttpListenerContext context;
                try
                {
                    context = await contextTask.ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return XeroOAuthCallbackResult.Failed("Authorization cancelled.");
                    }
                    return XeroOAuthCallbackResult.Failed("Local redirect listener stopped before a response was received.");
                }
                catch (HttpListenerException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return XeroOAuthCallbackResult.Failed("Authorization cancelled.");
                    }
                    return XeroOAuthCallbackResult.Failed("Local redirect listener closed before a response was received.");
                }
                return XeroService.ProcessOAuthRedirectRequest(context, expectedPath, expectedState);
            }
            finally
            {
                if (stopRegistration != null)
                {
                    stopRegistration.Dispose();
                }
                try
                {
                    listener.Stop();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (HttpListenerException)
                {
                }
                try
                {
                    listener.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private static XeroOAuthCallbackResult ProcessOAuthRedirectRequest(
            HttpListenerContext context,
            string expectedPath,
            string expectedState)
        {
            try
            {
                Uri requestUrl = context.Request.Url;
                string actualPath = XeroService.NormalizeCallbackPath(requestUrl.AbsolutePath);
                if (!string.Equals(actualPath, expectedPath, StringComparison.Ordinal))
                {
                    XeroService.WriteHtmlResponse(context.Response, 404, "<p>Not found.</p>");
                    return XeroOAuthCallbackResult.Failed("Unexpected redirect path.");
                }
                string query = requestUrl.Query;
                if (string.IsNullOrEmpty(query))
                {
                    XeroService.WriteHtmlResponse(context.Response, 200, "<p>Missing authorization response.</p>");
                    return XeroOAuthCallbackResult.Failed("Redirect did not include a query string.");
                }
                string error = XeroService.GetQueryValue(query, "error");
                if (!string.IsNullOrEmpty(error))
                {
                    string desc = XeroService.GetQueryValue(query, "error_description");
                    string msg = string.IsNullOrWhiteSpace(desc) ? error : error + ": " + desc;
                    XeroService.WriteHtmlResponse(context.Response, 200, "<p>Authorization was not completed. You can close this tab.</p>");
                    return XeroOAuthCallbackResult.Failed(msg);
                }
                string state = XeroService.GetQueryValue(query, "state");
                if (!string.Equals(state, expectedState, StringComparison.Ordinal))
                {
                    XeroService.WriteHtmlResponse(context.Response, 200, "<p>Authorization state did not match. Close this tab and try Connect again.</p>");
                    return XeroOAuthCallbackResult.Failed("Authorization state did not match.");
                }
                string code = XeroService.GetQueryValue(query, "code");
                if (string.IsNullOrWhiteSpace(code))
                {
                    XeroService.WriteHtmlResponse(context.Response, 200, "<p>No authorization code was returned. You can close this tab.</p>");
                    return XeroOAuthCallbackResult.Failed("Redirect did not include an authorization code.");
                }
                XeroService.WriteHtmlResponse(
                    context.Response,
                    200,
                    "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Xero</title></head><body><p>You can close this tab and return to JobCard.</p></body></html>");
                return XeroOAuthCallbackResult.Succeeded(code);
            }
            catch (Exception ex)
            {
                return XeroOAuthCallbackResult.Failed("Failed while reading the redirect: " + ex.Message);
            }
        }

        private static void WriteHtmlResponse(HttpListenerResponse response, int statusCode, string htmlBody)
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/html; charset=utf-8";
            byte[] body = Encoding.UTF8.GetBytes(htmlBody);
            response.ContentLength64 = body.Length;
            using (Stream output = response.OutputStream)
            {
                output.Write(body, 0, body.Length);
            }
        }

        private static string GetQueryValue(string query, string name)
        {
            if (string.IsNullOrEmpty(query) || query[0] != '?')
            {
                return null;
            }
            string[] parts = query.Substring(1).Split('&');
            foreach (string part in parts)
            {
                int eq = part.IndexOf('=');
                string key;
                string value;
                if (eq < 0)
                {
                    key = Uri.UnescapeDataString(part);
                    value = string.Empty;
                }
                else
                {
                    key = Uri.UnescapeDataString(part.Substring(0, eq));
                    value = Uri.UnescapeDataString(part.Substring(eq + 1));
                }
                if (string.Equals(key, name, StringComparison.Ordinal))
                {
                    return value;
                }
            }
            return null;
        }

        private static string NormalizeCallbackPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return "/";
            }
            string trimmed = absolutePath.TrimEnd('/');
            if (trimmed.Length == 0)
            {
                return "/";
            }
            return trimmed;
        }

        private static string BuildListenerPrefix(Uri redirect)
        {
            string path = redirect.AbsolutePath;
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path + "/";
            }
            int port = redirect.Port;
            if (port <= 0)
            {
                port = 80;
            }
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}://{1}:{2}{3}",
                Uri.UriSchemeHttp,
                redirect.Host,
                port,
                path);
        }
    }
}
