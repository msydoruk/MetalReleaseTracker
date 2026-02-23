# Fix AI Verification Prompt

Investigate and fix incorrect AI verification results for a specific band/album.

## Input

$ARGUMENTS — the band name and/or album title that was incorrectly verified (e.g. "SARCASM Stellar Stream Obscured").

## Steps

1. **Find the data** in the database:
   ```sql
   -- Search CatalogueIndex for the entry
   SELECT ci."Id", ci."BandName", ci."AlbumTitle", ci."DistributorCode", ci."Status", ci."BandReferenceId"
   FROM "CatalogueIndex" ci
   WHERE ci."BandName" ILIKE '%<band>%' OR ci."AlbumTitle" ILIKE '%<album>%';
   ```

2. **Check BandReference** — is the matched band actually the correct one?
   ```sql
   -- Look up the band reference
   SELECT br."BandName", br."MetalArchivesId", br."Genre"
   FROM "BandReferences" br WHERE br."Id" = '<BandReferenceId>';

   -- Check discography
   SELECT bd."AlbumTitle", bd."AlbumType", bd."Year"
   FROM "BandDiscography" bd WHERE bd."BandReferenceId" = '<BandReferenceId>';
   ```

3. **Check for name collisions** — are there multiple bands with the same name?
   ```sql
   SELECT br."BandName", br."MetalArchivesId", br."Genre"
   FROM "BandReferences" br WHERE br."BandName" ILIKE '<band>';
   ```

4. **Check AI verification result** if it exists:
   ```sql
   SELECT av."IsUkrainian", av."ConfidenceScore", av."AiAnalysis", av."AdminDecision"
   FROM "AiVerifications" av WHERE av."CatalogueIndexId" = '<entry_id>';
   ```

5. **Read the current prompt**:
   ```sql
   SELECT "SystemPrompt" FROM "AiAgents" WHERE "IsActive" = true;
   ```

6. **Diagnose the root cause** — common issues:
   - **Name collision**: Band name matches a Ukrainian band, but the album belongs to a different band with the same name. The prompt should instruct AI to check if the album appears in the discography.
   - **Missing discography check**: Prompt says "strong evidence" without checking album match.
   - **Ambiguous instructions**: Prompt doesn't handle edge cases (no discography provided, partial matches, etc.)

7. **Fix the prompt** — update both:
   - `SettingsSeedService.cs` `DefaultSystemPrompt` constant (for new deployments)
   - Database via SQL UPDATE (for running instance):
     ```sql
     UPDATE "AiAgents" SET "SystemPrompt" = '...', "UpdatedAt" = NOW() WHERE "IsActive" = true;
     ```

8. **Verify** — rebuild container and re-run verification for the affected entries to confirm the fix.

## Key Principle

The AI prompt must handle **name collisions** explicitly: if a discography is provided but the album is NOT in it, the AI should conclude it's a different band with the same name and return `isUkrainian: false`.
