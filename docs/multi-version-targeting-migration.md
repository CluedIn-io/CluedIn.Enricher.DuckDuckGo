# Migrating a Connector/Enricher to Multi-Version Targeting

This document tracks the migration of `CluedIn.Enricher.DuckDuckGo` from a single-version build to
the multi-version targeting pattern. Part of a larger effort migrating the CluedIn.Provider/
Connector/Enricher repos; prior art consulted: `CluedIn.Connector.Dataverse.V2`,
`CluedIn.Enricher.Gleif`, `CluedIn.Enricher.OpenCorporates`, `CluedIn.Enricher.Brreg`,
`CluedIn.Enricher.Permid`, `CluedIn.Enricher.GoogleMaps`, `CluedIn.Crawling.MasterDataServices`,
`CluedIn.Connector.AzureEventHubs` — all of which have their own
`docs/multi-version-targeting-migration.md`.

Branch: `feature/multi-version-targeting` (off `origin/develop`).

---

## Overview

Target matrix: `4.7.0`, `4.8.0`, `5.0.0-beta.*` — verified independently against this repo's own
`NuGet.config` feeds via a throwaway restore (`5.0.0-*` resolves to `5.0.0-beta.576`; `4.7.0` and
`4.8.0` also restore cleanly, no extra feed needed beyond the existing `nuget.org`/`develop`/
`release`/`AzurePipelines` sources). 4.6.0 excluded — no evidence this enricher's small
`ExternalSearchProvider` API surface needs it, matching the enricher precedent.

Note: this repo's default/checkout branch is `master`, not `develop`, but `origin/develop` exists
and is what the migration branch was created from (and what the PR targets), matching every other
repo in this effort.

---

## Step 1 — Pipeline template (`azure-pipelines.yml`)

Status: **Done**

Replaced `crawler.build.yml` (steps-template, `pool: windows-latest`, old `ref: refs/heads/refactor`)
with `crawler.build.jobs.yml` (jobs-template, `pool: ubuntu-22.04` per job, `multiVersionCluedInTargets`
for 4.7.0/4.8.0/5.0.0-beta.*). Kept the `variables: - group: nuget` the original file had (unrelated
to this migration, left as-is).

---

## Step 2 — `Directory.Build.props`

Status: **Done**

Honours `CluedInMultiVersionTargetFramework` (net10.0 local fallback); derives `CLUEDIN_V47`/
`V48`/`V50` `DefineConstants`; pins `LangVersion` to 13.0 (preemptively, based on the `CS8936`
net6.0 trap prior repos in this effort hit — not confirmed to affect this repo specifically, but
cheap to pin).

---

## Step 3 — `Packages.props`

Status: **Done**

`_CluedIn` guarded. Split test-package versions (`Microsoft.NET.Test.Sdk`, `AutoFixture.Xunit2`/
`Xunit3`, `xunit`/`xunit.v3`, `xunit.runner.visualstudio`) on `CLUEDIN_V50`. `CluedIn.Testing.Base`
switched to the version-suffixed package ID (`CluedIn.Testing.Base.$(_CluedInPackageSuffix)` →
`.470`/`.480`/`.500`), confirmed to exist on the feed for all three legs by a clean restore/build,
not assumed.

---

## Step 4 — Test projects

Status: **Done**

- `test/Directory.Build.props` — stripped to `IsTestProject` + `coverlet.msbuild`/
  `Microsoft.NET.Test.Sdk`/`Moq` only; removed the unconditional `AutoFixture.Xunit3`/`xunit.v3`/
  `CluedIn.Testing.Base` refs (would clash with the pre-5.0 legs' xunit v2 refs — same CS0433 risk
  the MasterDataServices doc describes).
- **Deleted `test/unit/Directory.Build.props`** — dead scaffold pinning ancient `Moq 4.5.30`/
  `Should 1.1.20` with no `test/unit` csproj to consume it (same pattern GoogleMaps' repo had).
- Added conditional `ItemGroup`s (xunit v3+AutoFixture.Xunit3 vs xunit v2+AutoFixture.Xunit2, plus
  the suffixed `CluedIn.Testing.Base`) directly to
  `test/integration/ExternalSearch.DuckDuckGo.Integration.Tests.csproj`, and a `GlobalUsings.cs`
  for the `AutoFixture` namespace split (no `Xunit.Abstractions` needed — no `ITestOutputHelper`
  usage in this repo's tests).
- Both integration tests are pre-existing `[Theory(Skip = ...)]` (GitHub issue 829, "TODO currently
  failing") — unrelated to this migration, confirmed via `dotnet test` showing "No test is
  available" (expected, given both are skip-marked) rather than a real discovery failure.

---

## Step 5 — `NuGet.config` casing

Status: **Done**

`git mv`'d `Nuget.config` → `NuGet.config` (two-step rename). No extra feed needed — confirmed
`CluedIn.Core` restores cleanly at `4.7.0`/`4.8.0`/`5.0.0-*` against the existing feeds.

---

## Step 6 — API compatibility audit across 4.7.0 / 4.8.0 / 5.0.0-beta.*

Status: **Done** — both src projects and the integration test project verified locally (real
`dotnet build`, not reasoned about) against all three legs, 0 errors.

### Finding 1: RestSharp major-version break (same family GoogleMaps/Gleif/OpenCorporates/Permid hit)

`DuckDuckGoExternalSearchProvider.cs` was written against the newer RestSharp API (`Method.Get`,
`RestClientOptions` ctor, `RestResponse` parameter type) — CluedIn 4.7/4.8 (net6.0) resolve
RestSharp 106.x, which has none of those. Fixed with `#if CLUEDIN_V50` guards: a `HttpGetMethod`
const (`Method.Get`/`Method.GET`), the `RestClient` construction (`RestClientOptions` ctor is
107+-only — old branch uses the plain `RestClient(string)` ctor + object-initializer `UserAgent`,
which exists on both API generations), and the `ConstructVerifyConnectionResponse` parameter type
(`RestResponse` vs `IRestResponse` — `Execute()`'s return type differs by generation, and C# won't
implicitly convert an interface-typed local to a concrete-typed parameter).

### Finding 2: `Nager.PublicSuffix` major-version break — same library, same file, as `CluedIn.Enricher.Brreg`

`Net/DomainName.cs` used `SimpleHttpRuleProvider` and the `Nager.PublicSuffix.RuleProviders`/
`Exceptions` sub-namespaces — these only exist in `Nager.PublicSuffix` 3.8.0 (resolved for CluedIn
5.0+/net10.0). CluedIn 4.7/4.8 (net6.0) resolve `Nager.PublicSuffix` 2.4.0, a flat namespace with no
`SimpleHttpRuleProvider` (confirmed via `Reflection.Assembly.LoadFrom` + `GetTypes()` against both
DLLs in the local NuGet cache, not guessed). Fixed identically to Brreg's repo (same package,
literally the same source file across both repos): `#if CLUEDIN_V50` swap between
`SimpleHttpRuleProvider` and `WebTldRuleProvider`, and the `using` statements for the version-only
sub-namespaces. This repo doesn't consume `DomainInfo.TLD`/`TopLevelDomain` anywhere, so unlike
Brreg's fix, no `GetTopLevelDomain` helper was needed.

---

## Step 7 — Reset the semantic version (`GitVersion.yml`)

Status: **Done**

```yaml
next-version: 1.0
ignore:
  sha: []
  commits-before: 2026-06-20T00:00:00
```

Highest pre-existing tag **by commit date** (not semver value) is `4.7.1` at
`2026-06-17T17:20:16+10:00` — note this repo also has an oddity: a `5.0.0` tag exists but at an
*earlier* commit date (`2026-05-19T12:25:33+10:00`) than `4.7.1`, so excluding everything before
`4.7.1`'s date also excludes `5.0.0`; no separate handling needed.

**Padded to 2 full days, not 1**, per `CluedIn.Enricher.Gleif`'s finding earlier in this same
migration effort: `GitVersion.Tool 5.9.0` appears to parse `commits-before` using local machine
time, not UTC, and a tight (same-day or ~9-hour) margin was confirmed on two other repos in this
effort to silently fail — `MajorMinorPatch` stayed at the old `4.x` value with no error. Verified
directly with the pinned tool (not the globally-installed one, which fails outright on this repo's
`pull-request: tag: pr` config with a schema mismatch):

```
MajorMinorPatch: "1.0.0"
SemVer: "1.0.0-multi-version-targeting.93"
BranchName: "feature/multi-version-targeting"
```

---

## Checklist

- [x] `azure-pipelines.yml` — switched to `crawler.build.jobs.yml` with `multiVersionCluedInTargets` (4.7.0, 4.8.0, 5.0.0-beta.*); pool switched `windows-latest` → `ubuntu-22.04`; `pipelineTemplateRef` fixed
- [x] `Directory.Build.props` — honours `CluedInMultiVersionTargetFramework` with net10.0 local fallback; `DefineConstants` derived; `LangVersion` pinned to 13.0
- [x] `Packages.props` — `_CluedIn` guarded; test packages + `CluedIn.Testing.Base` split by `CLUEDIN_V50`
- [x] `test/Directory.Build.props` — stripped to properties + common packages only; `test/unit/Directory.Build.props` deleted (dead scaffold)
- [x] Integration test csproj — conditional xunit v2/v3 + AutoFixture + `CluedIn.Testing.Base` selection; `GlobalUsings.cs` added
- [x] `NuGet.config` — renamed from `Nuget.config`; feeds confirmed sufficient for all three legs
- [x] Source — `#if CLUEDIN_V50` guards for the RestSharp 106↔114 break (3 call sites) and the `Nager.PublicSuffix` 2.4.0↔3.8.0 break (1 call site), both verified against restored DLLs, not guessed
- [x] `GitVersion.yml` — `next-version: 1.0`; `ignore.commits-before: 2026-06-20T00:00:00` (2-day padding); verified `MajorMinorPatch: "1.0.0"` with the pinned GitVersion.Tool 5.9.0
- [x] `src/` builds clean (0 errors) for all three legs on both projects, verified locally via real `dotnet build`
- [x] Integration test project builds clean for all three legs; both tests are pre-existing skips, unrelated to this migration
- [ ] Push branch and confirm the actual Azure DevOps pipeline run is green end-to-end (all three legs + `Multi-version: publish`)
