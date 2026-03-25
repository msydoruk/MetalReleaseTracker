# Notification System Architecture

## Overview

The notification system tracks album changes (price drops, restocks) from distributor websites and delivers real-time alerts to users via Telegram bot and in-app notifications.

## System Components

```
ParserService ──► Kafka ──► CoreDataService ──► Telegram Bot
 (scraping)     (events)    (notifications)     (delivery)
```

### Services

| Component | Role |
|-----------|------|
| **ParserService** | Scrapes distributors, detects album changes, publishes Kafka events |
| **CoreDataService** | Consumes events, generates notifications, sends via Telegram |
| **Telegram Bot** | Delivers messages, handles account linking |

### Database Tables (CoreDataServiceDb)

| Table | Purpose |
|-------|---------|
| `UserNotifications` | Stores all notification records per user |
| `TelegramLinks` | Maps UserId ↔ Telegram ChatId |
| `TelegramLinkTokens` | Temporary 8-char tokens for account linking (10-min TTL) |
| `UserAlbumWatches` | Tracks which albums a user is watching (by BandId + CanonicalTitle) |
| `UserFavorites` | User collection (Favorite / Want / Owned) |

See [database-schema.md](database-schema.md) for full column definitions.

---

## Notification Types

| Type | Trigger | Emoji |
|------|---------|-------|
| `PriceDrop` | Album price decreased | `🔻` |
| `BackInStock` | Changed from OutOfStock → InStock | `✅` |
| `Restock` | Changed from OutOfStock → any other status | `🔄` |
| `NewVariant` | New album pressing detected / admin broadcast | `🆕` |

---

## Flow 1: User Registration → Telegram Linking

### Step 1: User registers on the website

```
User                        CoreDataService              PostgreSQL
  │                              │                          │
  │── POST /api/auth/register ──►│                          │
  │   { email, password }        │── INSERT ───────────────►│
  │                              │                   AspNetUsers
  │◄── { token, user } ─────────│                          │
```

### Step 2: User generates a link token

```
User (/profile)             CoreDataService              PostgreSQL
  │                              │                          │
  │── POST /api/telegram/       ─►│                          │
  │   link-token (JWT auth)      │                          │
  │                              │ Generate "XK4MN7PQ"      │
  │                              │── INSERT ────────────────►│
  │                              │              TelegramLinkTokens
  │                              │              (expires: now + 10min)
  │◄── { token, botUsername } ───│                          │
```

### Step 3: User sends token to bot in Telegram

```
User (Telegram)           Telegram Servers            CoreDataService
  │                            │                           │
  │── /start XK4MN7PQ ───────►│                           │
  │                            │── POST /api/telegram/ ───►│
  │                            │   webhook                 │
  │                            │   { message: {            │
  │                            │     text: "/start XK..",  │
  │                            │     chat: { id: 77777 }   │
  │                            │   }}                       │
  │                            │                           │
  │                            │          HandleStartCommandAsync():
  │                            │          1. Find token in DB
  │                            │          2. Validate not expired
  │                            │          3. Create TelegramLink:
  │                            │             UserId ↔ ChatId 77777
  │                            │          4. Delete used token
  │                            │                           │
  │                            │◄── sendMessage ───────────│
  │  "Account linked!" ◄───────│                           │
```

### Step 4: Verify link status

```
GET /api/telegram/status → { isLinked: true }
DELETE /api/telegram/unlink → removes TelegramLink
```

---

## Flow 2: Album Watching

Users subscribe to album changes by "watching" an album. Watches are stored by band + canonical title (not album ID) so notifications fire for any variant of that album across distributors.

```
User (/albums/drudkh)       CoreDataService              PostgreSQL
  │                              │                          │
  │── POST /api/watches/        ─►│                          │
  │   {albumId}                  │── INSERT ────────────────►│
  │                              │              UserAlbumWatches
  │◄── 200 OK ──────────────────│   { UserId, BandId,      │
  │                              │     CanonicalTitle:       │
  │                              │     "Estrangement" }      │
```

**Key design**: Watches use `BandId + CanonicalTitle` not `AlbumId`. This means watching any variant of "Drudkh - Estrangement" (CD, LP, Tape from different distributors) triggers notifications for all of them.

---

## Flow 3: Automatic Notifications (Album Changes)

This is the main notification pipeline triggered by the parser detecting changes.

### Step 1: Parser detects a change

```
Distributor Website        ParserService                   Kafka
  │                            │                             │
  │  Drudkh - Estrangement     │                             │
  │  Price: €12 (was €15)      │                             │
  │                            │                             │
  │◄── HTTP scrape ────────────│                             │
  │                            │                             │
  │                            │ Compares with previous:     │
  │                            │ €15 → €12 = price change!   │
  │                            │                             │
  │                            │── Publish ─────────────────►│
  │                            │   AlbumProcessedEvent       │
  │                            │           albums-processed-topic
```

### Step 2: Consumer processes the event

```
Kafka              AlbumProcessedEventConsumer         PostgreSQL
  │                         │                              │
  │── Consume ─────────────►│                              │
  │                         │                              │
  │                         │── SELECT existing album ────►│
  │                         │◄── { price: 15.00 } ────────│
  │                         │                              │
  │                         │ Compare:                     │
  │                         │ old=€15, new=€12             │
  │                         │ €12 < €15 → PriceDrop        │
  │                         │                              │
  │                         │── UPDATE album price ───────►│
```

### Step 3: Generate notifications for watchers

```
AlbumProcessedEventConsumer          NotificationService
  │                                        │
  │── GenerateNotificationsAsync() ───────►│
  │   (existingAlbum, newEvent, bandId)    │
  │                                        │
  │                          1. Check NotificationsEnabled setting
  │                          2. Determine change type (PriceDrop)
  │                          3. Query UserAlbumWatches:
  │                             WHERE BandId = X
  │                             AND CanonicalTitle = "Estrangement"
  │                          4. Create UserNotification per watcher
  │                          5. Save to DB
  │                          6. Send via TelegramBotService
```

### Step 4: Deliver to Telegram

```
NotificationService      TelegramBotService          Telegram API
  │                            │                          │
  │── SendNotifications ──────►│                          │
  │   [notifications]          │                          │
  │                            │ For each notification:   │
  │                            │ 1. Find TelegramLink     │
  │                            │    by UserId              │
  │                            │ 2. Get ChatId             │
  │                            │                          │
  │                            │── POST sendMessage ─────►│
  │                            │   chatId: 77777           │
  │                            │   text: "🔻 *Drudkh -    │
  │                            │   Estrangement*           │
  │                            │   Price dropped from      │
  │                            │   €15.00 to €12.00"       │
  │                            │                          │
  │                            │◄── 200 OK ──────────────│
```

### Step 5: User receives notification

The user sees the message in Telegram and can also view it in the website's notification bell (header icon with unread count badge).

---

## Flow 4: Admin Broadcast

Admins can send notifications to all users or a subset from the admin panel.

```
Admin Panel                  CoreDataService             PostgreSQL
  │                               │                          │
  │── POST /api/admin/           ─►│                          │
  │   notifications/broadcast     │                          │
  │   { content, type }           │                          │
  │                               │── SELECT all users ─────►│
  │                               │◄── [User A, User B] ────│
  │                               │                          │
  │                               │ For each user:           │
  │                               │── INSERT notification ──►│
  │                               │                          │
  │                               │── Send via Telegram      │
  │                               │   (only linked users)    │
  │                               │                          │
  │◄── { createdCount: 2,        │                          │
  │      sentCount: 1 }           │  (1 linked, 1 not)      │
```

---

## Webhook Setup

The Telegram webhook is registered automatically on CoreDataService startup.

```
CoreDataService startup
  │
  │── TelegramExtensions.RegisterTelegramWebhook()
  │
  │── POST https://api.telegram.org/bot<TOKEN>/setWebhook
  │   { url: "https://metal-release.com/api/telegram/webhook" }
  │
  │── Telegram stores this URL
  │── All future messages to the bot are forwarded to this URL
```

**Why webhook, not polling?**

| | Polling | Webhook |
|---|---------|---------|
| Initiator | Bot asks Telegram | Telegram pushes to bot |
| Latency | Depends on interval | Instant |
| Requires public URL | No | Yes (HTTPS) |
| Best for | Local dev | Production |

The server is accessible at `https://metal-release.com` → webhook is the correct choice.

---

## Bot Commands

| Command | Description |
|---------|-------------|
| `/start {token}` | Link Telegram account using a token from the website |
| `/start` (no token) | Show welcome message with instructions |
| `/my` | List currently watched albums |
| `/help` | Show available commands |

---

## Feature Toggles

Stored in the `Settings` table (category: `FeatureToggles`):

| Key | Effect |
|-----|--------|
| `TelegramBotEnabled` | Master switch for all Telegram functionality |
| `NotificationsEnabled` | Master switch for notification generation |

When disabled, the respective functionality is silently skipped (no errors).

---

## API Endpoints

### User Endpoints (JWT required)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/notifications` | Paginated notification list |
| GET | `/api/notifications/unread-count` | Unread notification count |
| PUT | `/api/notifications/{id}/read` | Mark single notification as read |
| PUT | `/api/notifications/read-all` | Mark all notifications as read |
| POST | `/api/telegram/link-token` | Generate Telegram link token |
| GET | `/api/telegram/status` | Check if Telegram is linked |
| DELETE | `/api/telegram/unlink` | Unlink Telegram account |
| POST | `/api/watches/{albumId}` | Watch an album |
| DELETE | `/api/watches/{albumId}` | Unwatch an album |
| GET | `/api/watches/{albumId}/check` | Check if watching |
| GET | `/api/watches/keys` | Get all watched keys |

### Admin Endpoints (Admin role required)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/admin/notifications/stats` | Notification statistics |
| POST | `/api/admin/notifications/broadcast` | Send broadcast to users |
| GET | `/api/admin/telegram/stats` | Telegram bot statistics |
| GET | `/api/admin/telegram/linked-users` | List linked users |

### Webhook Endpoint (Telegram → Server)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/telegram/webhook` | Receives updates from Telegram |

---

## Frontend Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `NotificationBell` | Header | Shows unread count, last 10 notifications, mark-as-read |
| `WatchButton` | Album pages | Toggle watch/unwatch for an album |
| `NotificationsPage` | Admin panel | Stats dashboard + broadcast form |
| `TelegramPage` | Admin panel | Linked users list + bot status |

### Notification Bell Behavior

- Polls unread count every 60 seconds
- Popover shows last 10 notifications
- Click on notification → mark as read + navigate to album
- "Mark All as Read" button in popover

---

## Entity Relationships

```
AspNetUsers ─────┬──── TelegramLinks (1:1)
                 │         UserId ↔ ChatId
                 │
                 ├──── UserAlbumWatches (1:many)
                 │         UserId + BandId + CanonicalTitle
                 │
                 ├──── UserFavorites (1:many)
                 │         UserId + AlbumId + Status
                 │
                 └──── UserNotifications (1:many)
                           UserId + AlbumId? + Type + Message

Bands ───────────┬──── UserAlbumWatches (via BandId)
                 │
Albums ──────────┴──── UserNotifications (via AlbumId, nullable)
                 │
                 └──── UserFavorites (via AlbumId)
```

---

## Key Design Decisions

1. **Watch by CanonicalTitle, not AlbumId** — A single album (e.g., "Estrangement") may exist as CD, LP, and Tape across multiple distributors. Watching by `BandId + CanonicalTitle` ensures the user gets notified for any variant.

2. **Webhook over polling** — The server has a public HTTPS URL, making webhooks the optimal choice for instant delivery without polling overhead.

3. **Token-based linking** — Users generate a short-lived token on the website and send it to the bot. This avoids exposing user credentials to Telegram and ensures the linking is user-initiated.

4. **Non-blocking Telegram delivery** — If Telegram sending fails (user blocked bot, network issue), the notification is still saved in the database. Users can see it in-app even if Telegram delivery failed.

5. **Feature toggles** — Both notifications and Telegram bot can be disabled independently via admin settings without code changes or redeployment.
