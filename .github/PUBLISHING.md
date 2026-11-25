# Publishing Sisk packages to NuGet.org

The packages in this repository are published to NuGet.org through **Trusted
Publishing** (OIDC). No long-lived API key is stored in the repository: each
workflow run exchanges a GitHub OIDC token for a short-lived NuGet API key
that expires in about one hour.

Every workflow is triggered manually from the **Actions** tab.

## Packages

| Package | Workflow file | Project | Symbols |
| --- | --- | --- | --- |
| `Sisk.HttpServer` | `publish-sisk-httpserver.yml` | `src/Sisk.Core.csproj` | no |
| `Sisk.Cadente` | `publish-sisk-cadente.yml` | `cadente/Sisk.Cadente/Sisk.Cadente.csproj` | yes |
| `Sisk.Cadente.CoreEngine` | `publish-sisk-cadente-coreengine.yml` | `cadente/Sisk.Cadente.CoreEngine/Sisk.Cadente.CoreEngine.csproj` | yes |
| `Sisk.IniConfiguration.Core` | `publish-sisk-iniconfiguration-core.yml` | `extensions/Sisk.IniConfiguration.Core/Sisk.IniConfiguration.Core.csproj` | yes |
| `Sisk.IniConfiguration` | `publish-sisk-iniconfiguration.yml` | `extensions/Sisk.IniConfiguration/Sisk.IniConfiguration.csproj` | no |
| `Sisk.Documenting` | `publish-sisk-documenting.yml` | `extensions/Sisk.Documenting/Sisk.Documenting.csproj` | no |
| `Sisk.Documenting.Html` | `publish-sisk-documenting-html.yml` | `extensions/Sisk.Documenting.Html/Sisk.Documenting.Html.csproj` | no |
| `Sisk.ModelContextProtocol` | `publish-sisk-modelcontextprotocol.yml` | `extensions/Sisk.ModelContextProtocol/Sisk.ModelContextProtocol.csproj` | no |
| `Sisk.Monitoring` | `publish-sisk-monitoring.yml` | `extensions/Sisk.Monitoring/Sisk.Monitoring.csproj` | no |
| `Sisk.JsonRpc` | `publish-sisk-jsonrpc.yml` | `extensions/Sisk.JsonRPC/Sisk.JsonRPC.csproj` | yes |
| `Sisk.Helpers.mitmproxy` | `publish-sisk-mitmproxy.yml` | `extensions/Sisk.Helpers.mitmproxy/Sisk.Helpers.mitmproxy.csproj` | no |

The discontinued packages (`Sisk.BasicAuth`, `Sisk.SslProxy`, `Sisk.ServiceProvider`)
have no publishing workflow.

## Registering the policies on nuget.org

Sign in to [nuget.org](https://www.nuget.org/account/trustedpublishing) and create
one trusted publishing policy per workflow file.

NuGet matches the workflow file name **exactly** (case-insensitive), so a
wildcard such as `publish-*.yml` does not work. Each policy uses:

- **Repository Owner:** `sisk-http`
- **Repository:** `core`
- **Workflow File:** the file name from the table above, without the
  `.github/workflows/` path
- **Environment:** leave empty, because these workflows do not declare one
- **Scopes:** `Push new packages and package versions`
- **Glob Pattern:** the package id, for example `Sisk.HttpServer`

The **Glob Pattern** controls which packages the policy may publish. Set it to
the exact package id so that a workflow cannot publish a package it does not
own.

The first use of a policy locks it to the repository and owner identifiers.
Until then the policy stays temporarily active for seven days.

## Publishing order

Some packages depend on others through project references, and the dependency
version written into the `.nuspec` comes from the referenced project. Publish
the dependencies first:

1. `Sisk.HttpServer`
2. `Sisk.Cadente`
3. `Sisk.Cadente.CoreEngine`
4. `Sisk.IniConfiguration.Core`
5. `Sisk.IniConfiguration`
6. `Sisk.Documenting`
7. `Sisk.Documenting.Html`
8. `Sisk.ModelContextProtocol`
9. `Sisk.Monitoring`
10. `Sisk.JsonRpc`
11. `Sisk.Helpers.mitmproxy`

Publishing an extension before its dependency produces a package whose
dependency range points to a version that does not exist on NuGet.org yet.

## Workflow behaviour

Each workflow delegates to the shared composite action at
[`.github/actions/publish-nuget`](./actions/publish-nuget/action.yml), which:

1. packs the project in `Release`;
2. verifies that exactly one `.nupkg` was produced and that it belongs to the
   expected package id;
3. requests the short-lived API key through `NuGet/login@v1`;
4. pushes the package to `https://api.nuget.org/v3/index.json`.

For the packages that ship symbols, the action also pushes the `.snupkg` on its
own. The package push already uploads it, but it skips the symbol package when
the `.nupkg` was a duplicate, so the extra push lets a re-run repair a run that
failed after the package upload.

Use the `dry-run` input to pack and verify without publishing.

### Symbols

NuGet.org's symbol server only accepts the `.snupkg` format. The packages that
ship a portable PDB therefore publish a symbol package; the remaining ones
embed their PDBs in the assembly, so their symbol package would be empty and
is disabled.

### Versioning

The published version always comes from the `<Version>` property in the
project file. Passing `-p:Version` to `dotnet pack` also rewrites the
dependency ranges of the referenced projects, so the workflows do not override
the version. Bump the project file before running the workflow.

`--skip-duplicate` is set, so re-running a workflow that already published the
same version reports the conflict and continues instead of failing.
