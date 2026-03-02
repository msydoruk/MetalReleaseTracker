# Database Schema Reference

## ParserServiceDb (PostgreSQL, port 5434)

Container: `metalrelease_postgres_parser`, credentials in `.env`

### Tables & Columns

**BandReferences** — Ukrainian bands from Metal Archives
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| BandName | varchar(500) | | |
| MetalArchivesId | bigint | | Unique |
| Genre | varchar(500) | | |
| LastSyncedAt | timestamptz | | |

**BandDiscography** — Album entries per BandReference
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| BandReferenceId | uuid | | FK → BandReferences |
| AlbumTitle | varchar(500) | | |
| NormalizedAlbumTitle | varchar(500) | | Unique with BandReferenceId |
| AlbumType | varchar(100) | | Full-length, EP, Demo, etc. |
| Year | int | YES | |

**CatalogueIndex** — Full distributor catalogue with status tracking
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| DistributorCode | int | | Enum: 1-7 |
| BandName | varchar(500) | | As listed by distributor |
| AlbumTitle | varchar(500) | | |
| RawTitle | varchar(1000) | | Original unparsed title |
| DetailUrl | varchar(2000) | | Unique with DistributorCode |
| Status | int | | 0=New, 1=Relevant, 2=NotRelevant, 3=AiVerified, 4=Deleted |
| MediaType | int | YES | 0=CD, 1=LP, 2=Tape |
| BandReferenceId | uuid | YES | FK → BandReferences |
| BandDiscographyId | uuid | YES | FK → BandDiscography (set on AI confirm) |
| CreatedAt | timestamptz | | |
| UpdatedAt | timestamptz | | |

**CatalogueIndexDetails** — Parsed album details (1:1 with CatalogueIndex)
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| CatalogueIndexId | uuid | | FK → CatalogueIndex (unique) |
| DistributorCode | int | | |
| BandName | varchar(500) | | |
| SKU | varchar(200) | YES | |
| Name | varchar(500) | YES | |
| Genre | varchar(500) | YES | |
| Price | real | | |
| PurchaseUrl | varchar(2000) | YES | |
| PhotoUrl | varchar(2000) | YES | |
| Media | int | YES | 0=CD, 1=LP, 2=Tape |
| Label | varchar(500) | YES | |
| Press | varchar(500) | YES | |
| Description | text | YES | |
| Status | int | YES | 0=New, 1=Restock, 2=PreOrder |
| CanonicalTitle | varchar(500) | YES | From BandDiscography match |
| OriginalYear | int | YES | From BandDiscography match |
| ChangeType | int | | 0=New, 1=Updated, 2=Deleted, 3=Active |
| PublicationStatus | int | | 0=Unpublished, 1=Published |
| LastPublishedAt | timestamptz | YES | |
| CreatedAt | timestamptz | | |
| UpdatedAt | timestamptz | | |

**AiVerifications** — AI verification results per CatalogueIndex entry
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| CatalogueIndexId | uuid | | FK → CatalogueIndex |
| BandName | varchar(500) | | Snapshot of band name |
| AlbumTitle | varchar(500) | | Snapshot of album title |
| IsUkrainian | boolean | | AI result |
| ConfidenceScore | double | | 0.0-1.0 |
| AiAnalysis | varchar(4000) | | AI reasoning text |
| MatchedBandDiscographyId | uuid | YES | FK → BandDiscography |
| AdminDecision | int | YES | 0=Confirmed, 1=Rejected, null=pending |
| AdminDecisionDate | timestamptz | YES | |
| CreatedAt | timestamptz | | |

**AiAgents** — AI agent configuration (Claude API)
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| Name | varchar(200) | | |
| Description | varchar(1000) | YES | |
| SystemPrompt | text | | Template with {{bandName}}, {{albumTitle}}, {{discography}} |
| Model | varchar(100) | | e.g. claude-sonnet-4-20250514 |
| MaxTokens | int | | |
| MaxConcurrentRequests | int | | |
| DelayBetweenBatchesMs | int | | |
| ApiKey | text | | |
| IsActive | boolean | | Only one active at a time |
| CreatedAt | timestamptz | | |
| UpdatedAt | timestamptz | | |

**ParsingSources** — Distributor URL configuration
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| DistributorCode | int | | |
| Name | varchar(200) | | |
| ParsingUrl | varchar(2000) | | |
| IsEnabled | boolean | | |
| CreatedAt | timestamptz | | |
| UpdatedAt | timestamptz | | |

**Settings** — Runtime configuration (key-value)
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Key | varchar(200) | PK | |
| Value | text | | JSON value |
| Category | varchar(100) | | GeneralParser, BandReference, FlareSolverr, ParsingSources, ClaudeApi |
| UpdatedAt | timestamptz | | |

### FK Relationships
```
AiVerifications.CatalogueIndexId → CatalogueIndex.Id
AiVerifications.MatchedBandDiscographyId → BandDiscography.Id
BandDiscography.BandReferenceId → BandReferences.Id (CASCADE)
CatalogueIndex.BandDiscographyId → BandDiscography.Id (SET NULL)
CatalogueIndex.BandReferenceId → BandReferences.Id (SET NULL)
CatalogueIndexDetails.CatalogueIndexId → CatalogueIndex.Id (CASCADE, unique)
```

---

## CoreDataServiceDb (PostgreSQL, port 5436)

Container: `metalrelease_postgres_coredata`, credentials in `.env`

### Tables & Columns

**Albums** — Final album records served to frontend
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| DistributorId | uuid | | FK → Distributors |
| BandId | uuid | | FK → Bands |
| SKU | text | | |
| Name | text | | |
| Genre | text | YES | |
| Price | real | | |
| PurchaseUrl | text | | |
| PhotoUrl | text | | |
| Media | varchar(50) | | CD, LP, Tape |
| Label | text | | |
| Press | text | | |
| Description | text | YES | |
| CreatedDate | timestamptz | | |
| LastUpdateDate | timestamptz | YES | |
| Status | varchar(50) | YES | New, Restock, PreOrder |

**Bands** — Band records
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| Name | text | | |
| Description | text | YES | |
| Genre | text | YES | |
| PhotoUrl | text | YES | |

**Distributors** — Distributor records
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| Name | text | | |
| Code | int | | DistributorCode enum |

**Feedbacks** — User feedback messages
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| Message | text | | |
| Email | text | YES | |
| CreatedDate | timestamptz | | |

**RefreshTokens** — JWT refresh tokens
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int | PK | Auto-increment |
| UserId | text | | FK → AspNetUsers |
| Token | text | | |
| ExpiryDate | timestamptz | | |
| IsUsed | boolean | | |
| IsRevoked | boolean | | |
| Created | timestamptz | | |

**UserFavorites** — User's favorite albums
| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uuid | PK | |
| UserId | text | | |
| AlbumId | uuid | | FK → Albums |
| CreatedDate | timestamptz | | |

**ASP.NET Identity tables**: AspNetUsers, AspNetRoles, AspNetRoleClaims, AspNetUserClaims, AspNetUserLogins, AspNetUserRoles, AspNetUserTokens — standard Identity schema for Google OAuth + JWT auth.

### FK Relationships
```
Albums.BandId → Bands.Id
Albums.DistributorId → Distributors.Id
UserFavorites.AlbumId → Albums.Id
RefreshTokens.UserId → AspNetUsers.Id
+ standard ASP.NET Identity FKs
```
