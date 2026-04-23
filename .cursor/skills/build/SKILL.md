---
name: build
description: Build this project with Visual Studio MSBuild and report errors with proposed fixes.
disable-model-invocation: true
---

# Build

Use this skill when the user asks to build the `JobCard` project from Cursor.

## Goal

Run a full solution build with the .NET Framework MSBuild toolchain, verify whether it succeeded, and clearly report errors if present.

## Steps

1. Ensure working directory is the repository root.
2. Locate Visual Studio using `vswhere.exe`:
   - `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`
3. Build the solution with MSBuild (not `dotnet build`):
   - `& "<VS_PATH>\MSBuild\Current\Bin\MSBuild.exe" "Anita Job Card.sln" /t:Restore,Build /p:Configuration=Release`
4. Parse output:
   - If build succeeds, report success and any warnings.
   - If build fails, list the actionable error(s), root cause, and at least 2 possible fixes.
5. Do not implement fixes unless the user explicitly approves.

## Notes

- Always use PowerShell call operator `&` before quoted executable paths.
- Do not use `dotnet build` for this solution when COM reference resolution is required.
