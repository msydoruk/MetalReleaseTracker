# Migrate Settings from appsettings.json to Database

Migrate the specified configuration section from `IOptions<T>` (bound to appsettings.json) to runtime DB-backed settings via `ISettingsService`.

## Input

$ARGUMENTS — the setting class name and/or appsettings section to migrate (e.g. "SmtpSettings", "NotificationConfig").

## Steps

1. **Find all usages** of `IOptions<T>` for the target settings class across the entire solution:
   - Service classes, job classes, background services
   - Tests (unit, integration, smoke, benchmarks)
   - DI registration (`Configure<T>` calls)
   - Docker compose environment variable overrides

2. **Add DB-reading method** to `ISettingsService` / `SettingsService`:
   - Query the `Settings` table by category
   - Parse values into the typed settings class
   - Provide sensible defaults for each property

3. **Replace `IOptions<T>`** with `ISettingsService` in all consumers:
   - For scoped services: load settings at method start
   - For singleton/long-lived services: use lazy caching (`_cached ??= await ...`)
   - For `BackgroundService`: create scope via `IServiceScopeFactory`
   - Pass settings as parameters to private methods (don't store stale field)

4. **Update `SettingsSeedService`**: embed default values as constants, remove the `IOptions<T>` injection

5. **Clean up registration**:
   - Remove `Configure<T>` from `AppSettingsRegistrationExtension`
   - Remove the section from `appsettings.json`
   - Remove env var overrides from `docker-compose.yml`

6. **Update tests and benchmarks**:
   - Replace `Options.Create(new T { ... })` with mock/stub `ISettingsService`
   - Check `Tests/` folder AND `MetalReleaseTracker.Benchmarks/` project

7. **Build**: `dotnet build src/MetalReleaseTracker.sln` — must be 0 errors

## Checklist before done

- [ ] No remaining `IOptions<T>` references for the migrated type
- [ ] No remaining appsettings section
- [ ] No remaining docker-compose env vars for the section
- [ ] Seed service embeds defaults (no IOptions dependency)
- [ ] All tests/benchmarks updated
- [ ] Build passes with 0 errors
