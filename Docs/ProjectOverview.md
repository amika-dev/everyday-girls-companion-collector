# Everyday Girls Companion Collector — Project Overview

## 1. What the app is

Everyday Girls: Companion Collector is a small ASP.NET Core MVC web app where each user builds a personal “collection” of girls from a global pool, and then performs a small set of once-per-day actions on a fixed daily reset schedule.

The core “comfy daily routine” loop is:

1. **Daily Roll**: once per day, roll to generate today’s 5 adoption candidates (that you do not already own).
2. **Daily Adopt**: once per day, pick **one** of those rolled candidates to adopt (subject to the collection cap).
3. **Daily Interaction**: once per day, interact with your current “partner” to gain **+1 bond** and see a random line of dialogue (based on a personality tag you assign).

The app treats “today” using a server-defined date concept (`ServerDate`) tied to a **daily reset at 18:00 UTC**.

---

## 2. Key user flows

Navigation is defined in `Views/Shared/_Layout.cshtml` and shows:
- Main Menu (`/Home/Index`)
- Daily Adopt (`/DailyAdopt/Index`)
- Interact (`/Interaction/Index`)
- Collection (`/Collection/Index`)

All gameplay features require authentication (`[Authorize]` on controllers).

### Main Menu

**Entry point:** `HomeController.Index()` (`Controllers/HomeController.cs`)

Flow:
1. Ensures the user is authenticated; otherwise redirects to `/Account/Login`.
2. Ensures there is a `UserDailyState` row for the user (creates one if missing).
3. Loads the user (`AspNetUsers`) including the partner (`Include(u => u.Partner)`).
4. If a partner exists, loads the corresponding `UserGirl` row to show:
   - bond (`UserGirl.Bond`)
   - date met (`UserGirl.DateMetUtc`)
   - personality tag (`UserGirl.PersonalityTag`)
5. Builds `MainMenuViewModel` with:
   - daily availability flags (Roll/Adopt/Interaction) via `IDailyStateService`
   - `TimeUntilReset` via `IDailyStateService.GetTimeUntilReset()`
   - partner details (if present)
   - computed “Days Together” via `MainMenuViewModel.PartnerDaysSinceAdoption`

UI details (`Views/Home/Index.cshtml`):
- Shows countdown (“Next Reset In”) using `<strong data-countdown-seconds="...">`
- Shows three status cards: Daily Roll / Daily Adopt / Daily Interaction (Available/Used)
- Shows a partner panel if partner exists; otherwise a “No partner yet” message
- “Interact” button is disabled if no partner

Constraints surfaced here:
- Daily reset time is explicitly displayed as **18:00 UTC**.
- “Days Together” is computed using `DateTime.UtcNow - PartnerDateMet` (see `MainMenuViewModel`).

---

### Daily Adopt (roll + adopt rules)

**Controller:** `DailyAdoptController` (`Controllers/DailyAdoptController.cs`)

#### A) View current daily roll/adopt state (`GET /DailyAdopt/Index`)
Flow:
1. Ensures `UserDailyState` exists (creates if missing).
2. Computes `serverDate` via `IDailyStateService.GetCurrentServerDate()`.
3. Loads candidates only when:
   - the daily roll is **not available** (meaning it was used already), AND
   - `dailyState.CandidateDate == serverDate`
4. Candidate IDs are stored in `UserDailyState` as `Candidate1GirlId`..`Candidate5GirlId`.
5. Fetches `ownedCount` from `UserGirls`.
6. Returns `DailyAdoptViewModel`:
   - `IsDailyRollAvailable`, `IsDailyAdoptAvailable`
   - `Candidates` (0–5 `Girl` rows)
   - `TimeUntilReset`
   - `OwnedGirlsCount`
   - `IsCollectionFull` is computed in the view model from `GameConstants.MaxCollectionSize`

UI (`Views/DailyAdopt/Index.cshtml`):
- If roll is available: shows “Use Daily Roll”.
- If roll was already used: shows today’s candidate cards (if available).
- Adopt buttons:
- disabled if Daily Adopt already used
- disabled if collection is full
- otherwise posts to `POST /DailyAdopt/Adopt` with a confirm dialog (via `confirmAdopt()` in `site.js`)

#### B) Use Daily Roll (`POST /DailyAdopt/UseRoll`)
Rules (enforced in `UseRoll()`):
1. Requires an existing `UserDailyState` (otherwise redirects back).
2. Roll can only be used if `IDailyStateService.IsDailyRollAvailable(dailyState)` is true.
3. Builds the candidate pool as: all `Girls` not already owned by the user.
4. Randomizes with `Random.Shared.Shuffle(allCandidates)` then takes the first `GameConstants.DailyCandidateCount`.
5. Persists:
   - `dailyState.LastDailyRollDate = serverDate`
   - `dailyState.CandidateDate = serverDate`
   - `Candidate1GirlId..Candidate5GirlId` set from the selected candidates
6. Redirects back to `Index`.

Important constraints:
- Candidates are “unique in the roll” because they are taken from a shuffled array.
- There is no explicit guard in code if the global pool has fewer than 5 unowned girls; the code uses `ElementAtOrDefault` when persisting. (Behavior: some candidate slots may become null. The UI loads only IDs that have values.)

#### C) Adopt a candidate (`POST /DailyAdopt/Adopt?girlId=...`)
Rules (enforced in `Adopt(int girlId)`):
1. Requires `UserDailyState` to exist.
2. Roll must have been used already:
   - If roll is still available, adoption is blocked with: “Daily Roll must be used before adopting.”
3. Candidates must be for today:
   - Requires `dailyState.CandidateDate == serverDate`
4. The chosen `girlId` must be one of the persisted candidate IDs.
5. Daily Adopt can only be used if `IDailyStateService.IsDailyAdoptAvailable(dailyState)` is true.
6. Collection cap enforced:
   - if owned count `>= GameConstants.MaxCollectionSize`, adopt is blocked.
7. On success:
   - creates `UserGirl` row with:
     - `DateMetUtc = DateTime.UtcNow`
     - `Bond = 0`
     - `PersonalityTag = PersonalityTag.Cheerful` (default enum value)
   - if the user has no partner yet (`PartnerGirlId == null`), sets partner to this `girlId`
   - sets `dailyState.LastDailyAdoptDate = serverDate`
   - saves changes

Constraints summary:
- **Daily roll: 1/day**
- **Daily adopt: 1/day**
- Must roll before adopting
- Must adopt from the rolled list for that day
- **Collection cap:** `GameConstants.MaxCollectionSize` (currently `30`)
- First adopted girl becomes partner automatically

---

### Interaction (partner interaction rules)

**Controller:** `InteractionController` (`Controllers/InteractionController.cs`)

#### A) View partner interaction screen (`GET /Interaction/Index`)
Flow:
1. Loads `ApplicationUser` with partner navigation (`Include(u => u.Partner)`).
2. If no partner exists:
   - sets TempData error “You don't have a partner yet. Adopt a girl first!”
   - redirects to `Home/Index`
3. Loads partner ownership row from `UserGirls` for bond + personality tag.
4. Ensures `UserDailyState` exists (creates if missing).
5. Builds `InteractionViewModel`:
   - partner (`Girl`)
   - `PartnerBond`, `PartnerTag`
   - `IsDailyInteractionAvailable` (via `IDailyStateService`)
   - `TimeUntilReset`
   - optional `Dialogue` pulled from `TempData["Dialogue"]`

UI (`Views/Interaction/Index.cshtml`):
- Shows countdown
- Shows partner portrait, bond, personality
- Shows a dialogue box if present
- Interact button posts to `POST /Interaction/Do`, disabled if already used today

#### B) Do interaction (`POST /Interaction/Do`)
Rules:
1. Requires a partner (`PartnerGirlId != null`).
2. Requires `UserDailyState` exists (otherwise redirects back).
3. Daily Interaction can only be performed if `IDailyStateService.IsDailyInteractionAvailable(dailyState)` is true.
4. Loads partner `UserGirl` record and increments `Bond += 1`.
5. Marks interaction used for the day:
   - `dailyState.LastDailyInteractionDate = serverDate`
6. Saves.
7. Generates a random dialogue line by tag:
   - `_dialogueService.GetRandomDialogue(partnerData.PersonalityTag)`
   - stored in `TempData["Dialogue"]` so it appears on next GET.

Constraints summary:
- Interaction is **only** with the current partner.
- **Daily interaction: 1/day**.
- Interaction effect is **+1 bond** each time.

---

### Collection (sorting/pagination, partner setting, tagging, abandoning, etc.)

**Controller:** `CollectionController` (`Controllers/CollectionController.cs`)

#### A) Browse collection (`GET /Collection/Index?sort=bond&page=1`)
Rules/behavior:
- Pagination:
  - `PageSize` is a private constant: `10` (comment: “10 per page (2x5 grid)”)
- Sort modes (`sort` query param):
  - `"bond"` (default): order by bond descending, then date met ascending
  - `"oldest"`: date met ascending
  - `"newest"`: date met descending
- Returns `CollectionViewModel` containing:
  - list of `CollectionGirlViewModel` for the current page
  - sort mode, current page, total pages, and current partner girl ID
- Each `CollectionGirlViewModel` includes `DaysSinceAdoption` computed via `DailyCadence.GetDaysSinceAdoption()`

UI (`Views/Collection/Index.cshtml`):
- Sort buttons for Bond / Oldest / Newest
- Grid of cards; each card has a “View Details” button that opens a Bootstrap modal
- Pagination controls (“Previous” / “Next”)
- Modal includes:
- Set as Partner
- Change Personality Tag (dropdown of 5 values)
- Abandon (disabled in modal UI when the girl is partner; uses `confirmAbandon()` in `site.js`)

#### B) Set partner (`POST /Collection/SetPartner`)
Rules:
1. Verifies the user owns the selected girl (`UserGirls.Any()`).
2. Sets `ApplicationUser.PartnerGirlId = girlId`.
3. Saves and redirects back to the same sort/page (passed via `returnSort` / `returnPage` hidden fields).

#### C) Set personality tag (`POST /Collection/SetTag`)
Rules:
1. Loads the user’s `UserGirl` row for the selected girl.
2. Updates `UserGirl.PersonalityTag = tag`.
3. Saves and redirects back to the same sort/page.

Note: Personality tag affects dialogue only (per enum docs in `Models/Enums/PersonalityTag.cs`).

#### D) Abandon girl (`POST /Collection/Abandon`)
Rules:
1. Ensures the girl is owned by the user.
2. Prevents abandoning the current partner:
   - if `user.PartnerGirlId == girlId` => error “You cannot abandon your partner.”
3. Otherwise deletes `UserGirl` row and saves.

Constraints summary:
- Partner can be changed anytime.
- Partner cannot be abandoned.
- Abandon permanently deletes the ownership row (no archival).

---

## 3. Daily reset rules

### What determines “today” (ServerDate)

The app does **not** use local time zones. It uses a server-calculated “day” called `ServerDate`:

- Reset time: **18:00 UTC**
- Logic is implemented in `DailyStateService.GetCurrentServerDate()` (`Services/DailyStateService.cs`):
  - If current UTC time is **at or after 18:00**, `ServerDate = today` (UTC calendar date).
  - If current UTC time is **before 18:00**, `ServerDate = yesterday` (UTC calendar date minus 1).

This is used to determine if daily actions were already used “today”.

### When reset happens

There is **no scheduled job** or background task.
“Reset” is effectively computed on-demand:
- Each availability check compares stored dates to `ServerDate`:
  - `LastDailyRollDate != ServerDate`
  - `LastDailyAdoptDate != ServerDate`
  - `LastDailyInteractionDate != ServerDate`

Because of this design, daily actions “reset” automatically as soon as `ServerDate` changes at 18:00 UTC.

### Countdown timers: computation and display

Server-side calculation:
- `DailyStateService.GetTimeUntilReset()` computes a `TimeSpan` until the next 18:00 UTC boundary:
  - if now >= 18:00 UTC => next reset is tomorrow at 18:00 UTC
  - else => next reset is today at 18:00 UTC

Client-side display:
- Pages render `<strong data-countdown-seconds="...">` where the value is `(int)TimeUntilReset.TotalSeconds`.
- `wwwroot/js/countdown.js` reads that value, computes a target timestamp, and updates the UI once per second in `H:MM:SS` format.
- When it reaches zero, the script calls `location.reload()` to refresh the page state.

Services/methods responsible:
- `IDailyStateService.GetCurrentServerDate()`
- `IDailyStateService.GetTimeUntilReset()`
- `DailyStateService` implements both.

### Days since adoption calculation

"Days Together" or "Days Since Adoption" represents the **number of ServerDate transitions** that have occurred since adoption:
- Calculated via `DailyCadence.GetDaysSinceAdoption()` in `Utilities/DailyCadence.cs`
- Logic:
  1. Convert the adoption UTC timestamp to its ServerDate equivalent using the 18:00 UTC rule
  2. Compute: `currentServerDate - adoptionServerDate` (as `DateOnly.DayNumber` difference)
- **Important:** If a girl is adopted and the next reset has not yet occurred, the value is **0**.

---

## 4. Data model & database schema

Entity configuration lives in `Data/ApplicationDbContext.cs`, which extends `IdentityDbContext<ApplicationUser>`.

### Tables / entities

#### `AspNetUsers` (Identity) + `ApplicationUser`
File: `Models/Entities/ApplicationUser.cs`
- Primary key: `Id` (string, Identity)
- Added field: `PartnerGirlId` (nullable int)
- Navigation: `Partner` (`Girl?`)
Relationship configuration:
- In `ApplicationDbContext.OnModelCreating()`:
  - `ApplicationUser.PartnerGirlId` FK -> `Girls.GirlId`
  - `OnDelete(DeleteBehavior.Restrict)` (so deleting a `Girl` referenced as partner is restricted)

#### `Girls` (`Girl`)
File: `Models/Entities/Girl.cs`
Represents the global pool of adoptable girls.
- Primary key: `GirlId` (int identity)
- `Name` (required, max length 100)
- `ImageUrl` (required, max length 500)

Seed behavior:
- `Data/DbInitializer.cs` seeds the `Girls` table if it is empty.
- Currently seeds **58** `Girl` rows with image paths `/images/girls/001.jpg` ... `/images/girls/058.jpg`.
- The seeder calls `context.Database.Migrate()` first.

#### `UserGirls` (`UserGirl`)
File: `Models/Entities/UserGirl.cs`
Represents the ownership + relationship state per user per girl.
- Composite primary key: (`UserId`, `GirlId`)
- Fields:
  - `DateMetUtc` (DateTime): adoption time stored in UTC
  - `Bond` (int)
  - `PersonalityTag` (enum stored as int)
- Relationships:
  - FK `UserId` -> `AspNetUsers.Id` with cascade delete
  - FK `GirlId` -> `Girls.GirlId` with restrict delete

Indexes (configured in `ApplicationDbContext`):
- `IX_UserGirls_UserId_Bond_DateMet` on `(UserId, Bond, DateMetUtc)` with descending on Bond as configured via `.IsDescending(false, true, false)`
- `IX_UserGirls_UserId_DateMet_Asc` on `(UserId, DateMetUtc)`
- `IX_UserGirls_UserId_DateMet_Desc` on `(UserId, DateMetUtc)` with DateMet descending `.IsDescending(false, true)`

These support the three sorting modes used in `CollectionController`.

#### `UserDailyStates` (`UserDailyState`)
File: `Models/Entities/UserDailyState.cs`
One row per user for daily action tracking and persisted daily roll candidates.
- Primary key: `UserId` (string)
- Fields:
  - `LastDailyRollDate` (`DateOnly?`)
  - `LastDailyAdoptDate` (`DateOnly?`)
  - `LastDailyInteractionDate` (`DateOnly?`)
  - `CandidateDate` (`DateOnly?`)
  - `Candidate1GirlId`..`Candidate5GirlId` (`int?`)
- Relationship:
  - One-to-one with `ApplicationUser` via `UserId` FK, cascade delete

Candidate behavior:
- Candidates are only considered valid if `CandidateDate` matches `ServerDate` (enforced by controller logic).

### EF Core migrations

Initial migration: `Migrations/20260113012916_InitialCreate.cs`
- Confirms:
  - `Girls` table has `GirlId`, `Name (nvarchar(100))`, `ImageUrl (nvarchar(500))`
  - `UserGirls` has composite PK and FK constraints described above
  - `UserDailyStates` is keyed by `UserId` and stores `DateOnly` values via SQL type `date`

---

## 5. Code architecture & folder layout

This is a conventional ASP.NET Core MVC layout.

### `Controllers/`

- `AccountController`
  - Authentication (Register/Login/Logout)
  - On successful registration, creates a `UserDailyState` row and then auto-signs-in.

- `HomeController`
- Main Menu / hub screen
- Shows daily availability, reset countdown, partner summary
- Uses `DailyCadence` to compute partner days together

- `DailyAdoptController`
  - Daily Roll (`UseRoll`) and Daily Adopt (`Adopt`)
  - Reads/writes `UserDailyState` candidates and usage dates
  - Enforces collection cap

- `InteractionController`
  - Displays partner interaction screen and executes daily interaction (+1 bond)
  - Uses `IDialogueService` to generate dialogue

- `CollectionController`
- Collection browsing with sorting + pagination
- Partner selection, personality tagging, abandoning
- Uses `DailyCadence` to compute days since adoption for each girl

### `Services/`

- `IDailyStateService` / `DailyStateService`
  - Defines and implements:
    - `GetCurrentServerDate()`
    - `GetTimeUntilReset()`
    - availability checks for Roll/Adopt/Interaction

- `IDialogueService` / `DialogueService`
  - Provides random lines of dialogue keyed by `PersonalityTag`
  - Uses an in-memory dictionary and `Random.Shared.Next(...)`
  - Returns `"..."` if the tag is not found in the dictionary

### `Utilities/`

- `DailyCadence`
  - Static helper for computing values based on the app's daily reset cadence
  - Methods:
    - `GetServerDateFromUtc(DateTime)`: converts UTC timestamp to ServerDate
    - `GetDaysSinceAdoption(DateOnly currentServerDate, DateTime dateMetUtc)`: computes number of ServerDate transitions since adoption

### `Models/`
- `Models/Entities/`
  - EF Core entities: `Girl`, `UserGirl`, `UserDailyState`, `ApplicationUser`

- `Models/Enums/`
  - `PersonalityTag` enum (Cheerful, Shy, Energetic, Calm, Playful)

- `Models/ViewModels/`
- View models used by MVC views:
  - `MainMenuViewModel`
  - `DailyAdoptViewModel`
  - `InteractionViewModel`
  - `CollectionViewModel` + `CollectionGirlViewModel`
    - (Also present but not shown above: `InteractionViewModel` exists and is used by `InteractionController`.)

### `Views/`
Key views:
- `Views/Home/Index.cshtml` (Main Menu)
- `Views/DailyAdopt/Index.cshtml`
- `Views/Interaction/Index.cshtml`
- `Views/Collection/Index.cshtml`
- `Views/Shared/_Layout.cshtml` for navigation + scripts

### `wwwroot/`
- `wwwroot/css/site.css`
  - Custom styling on top of Bootstrap for a card-based “cozy” look
  - Includes image sizing rules and shared “girl card” utilities

- `wwwroot/js/site.js`
- Minimal JS: confirm dialogs for adopt (`confirmAdopt`) and abandon (`confirmAbandon`)

- `wwwroot/js/countdown.js`
  - Countdown timer behavior and auto-reload when it hits 0

No other JS framework/pattern is used; interaction is simple server-rendered pages + small in-page scripts.

---

## 6. Configuration & environments

### `appsettings.json` vs `appsettings.Development.json`

- `appsettings.json`
  - Has placeholder connection string: `"DefaultConnection": "[connection string needed]"`
  - Seeding defaults to disabled: `"Seeding": { "Enable": false }`

- `appsettings.Development.json`
  - Uses LocalDB:
    - `Server=(localdb)\mssqllocaldb;Database=EverydayGirlsCC;Trusted_Connection=True;MultipleActiveResultSets=true`
  - Seeding enabled: `"Seeding": { "Enable": true }`

### Local DB setup (SQL Server / LocalDB)

`Program.cs` registers EF Core SQL Server provider:
- `builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(...))`

By default in development, the app expects **SQL Server LocalDB**.

Production assumption:
- **Unknown / not found in code as a specific provider assumption.**
  - There is no Azure-specific configuration code in `Program.cs`.
  - Archived docs mention Azure deployment ideas, but they are explicitly “NOT authoritative” (`Docs/Archived/MVP/IMPLEMENTATION_SUMMARY.md`).

### Identity/auth configuration

`Program.cs` configures Identity with simplified MVP password rules:
- digits: not required
- lowercase: not required
- uppercase: not required
- non-alphanumeric: not required
- required length: `6`

Cookie paths (`Program.cs`):
- login: `/Account/Login`
- access denied: `/Account/Login`
- logout: `/Account/Logout` (note: actual logout action exists as `POST Account/Logout`)

---

## 7. UI conventions & styling rules

### Styling approach

- Uses Bootstrap (`~/lib/bootstrap/...`) plus `wwwroot/css/site.css`.
- Layout uses card panels and responsive CSS grids.

Key layout patterns in `site.css`:
- `.main-menu-container`, `.daily-adopt-container`, `.interaction-container`, `.collection-container` define max widths and centering.
- Grids:
  - `.daily-status-grid`: responsive auto-fit grid
  - `.candidates-grid`: responsive card grid
  - `.collection-grid`: fixed 5 columns desktop, 3 columns on medium, 2 columns on small

### Image rules

`site.css` defines a shared utility:
- `.girl-image-square { aspect-ratio: 1 / 1; object-fit: cover; }`

And forces key image classes to use 1:1 aspect ratio:
- `.partner-image`, `.partner-image-large`, `.candidate-image`, `.collection-image`, `.modal-girl-image { aspect-ratio: 1 / 1; }`

Sizing strategy:
- Candidate and collection images use `width: 100%` with `max-width` and `height: auto`, relying on `aspect-ratio` to keep them square.

### Shared partials/components

- Bootstrap modal is implemented inline in `Views/Collection/Index.cshtml` (not a partial).
- Countdown is a shared pattern (same attribute-based markup) and is implemented once in `wwwroot/js/countdown.js`.

Unknown / not found:
- No Razor partials for partner panel or status indicators exist in the currently-read views; everything is inline for these pages.
  - (Archived docs reference partial views, but they are not authoritative.)

---

## 8. Important constants and limits

### Central constants: `Constants/GameConstants.cs`
- `GameConstants.MaxCollectionSize = 30`
  - Enforced in `DailyAdoptController.Adopt()`
  - Used by `DailyAdoptViewModel.IsCollectionFull` and displayed on the Daily Adopt view

- `GameConstants.DailyCandidateCount = 5`
  - Used in `DailyAdoptController.UseRoll()` (`Take(...)`)

- `GameConstants.DailyResetHourUtc = 18`
  - Used in `DailyStateService` to build `ResetTime`

### Other hard-coded limits / values
- Collection page size:
  - `CollectionController` has `private const int PageSize = 10`

- Dialogue lines:
  - `DialogueService` has 5 tags with 10 lines each (in-memory list literals)

- Personality tag values:
  - `PersonalityTag` enum is 0..4
  - Collection modal dropdown uses hard-coded `<option value="0">Cheerful</option>` ... `<option value="4">Playful</option>`

- Daily candidate storage is fixed at 5 columns:
  - `UserDailyState.Candidate1GirlId`..`Candidate5GirlId`
  - This implicitly ties the DB schema to `GameConstants.DailyCandidateCount == 5`. (No dynamic schema behavior exists.)

---

## 9. Known issues / TODOs (as observed in repo)

Only items clearly supported by code currently read:

- **Countdown timer only targets the first element** with `[data-countdown-seconds]`:
  - `countdown.js` uses `document.querySelector(...)` (singular), not `querySelectorAll`.
  - This is fine today because each page renders one countdown, but it would not handle multiple timers on the same page.

- **Archived documentation mismatch** (non-code issue, but present in repo):
  - `Docs/Archived/MVP/IMPLEMENTATION_SUMMARY.md` states collection cap 100 and other details that do not match current `GameConstants.MaxCollectionSize = 30`.
  - This file is explicitly labeled “ARCHIVED DOCUMENT” and “NOT authoritative”, but it may confuse reviewers.

Unknown / not found:
- No explicit `TODO:`/`FIXME:` comments were identified in the searched codepaths (search results were dominated by archived docs and third-party license files).

---

## 10. How to run locally (step-by-step)

### Prerequisites
- .NET SDK compatible with the project (workspace notes indicate targeting **.NET 10**).
- SQL Server LocalDB (for the default development connection string in `appsettings.Development.json`).
- EF Core tooling (optional but recommended):
  - `dotnet-ef` global tool, or use Visual Studio’s Package Manager Console.

### Steps

1. **Clone and open the solution**
   - Open the repository in Visual Studio.

2. **Restore packages**
   - CLI:
     ```bash
     dotnet restore
     ```
   - Or build the solution in Visual Studio.

3. **Confirm connection string**
   - `appsettings.Development.json` uses:
     - `Server=(localdb)\mssqllocaldb;Database=EverydayGirlsCC;Trusted_Connection=True;MultipleActiveResultSets=true`

4. **Apply database migrations**
   - Using `dotnet-ef`:
     ```bash
     dotnet ef database update
     ```
   - Using Package Manager Console:
     ```powershell
     Update-Database
     ```

   Notes:
   - `DbInitializer.Initialize(...)` calls `context.Database.Migrate()` as well, but migrations should still be applied explicitly during setup to ensure schema is created.

5. **Run the site**
   - CLI:
     ```bash
     dotnet run
     ```
   - Or use Visual Studio Run/Debug.

6. **Seeding behavior**
   - In development, `appsettings.Development.json` sets `"Seeding:Enable": true`.
   - On startup, `Program.cs` checks `Seeding:Enable` and runs `DbInitializer.Initialize(context)`, which seeds `Girls` if empty.

7. **Default URLs/ports**
   - **Unknown / not found in repo files read.**
   - (Ports are typically defined in `Properties/launchSettings.json`, but that file was not read in this task.)

---

## 11. Glossary (optional but helpful)

- **ServerDate**: The app’s concept of “today” for daily actions. Implemented in `DailyStateService.GetCurrentServerDate()` using an 18:00 UTC boundary.

- **ResetTime**: The time-of-day used for the daily boundary. In code, it is a `TimeOnly` built from `GameConstants.DailyResetHourUtc` (18:00).

- **DailyState / `UserDailyState`**: A per-user record storing:
  - when daily actions were last used (as `DateOnly`)
  - today’s persisted roll candidates (as 5 nullable `GirlId` columns)
  - the `CandidateDate` indicating which ServerDate those candidates belong to

- **Daily Roll**: The once-per-ServerDate action that picks 5 random unowned girls and persists them to `UserDailyState`.

- **Daily Adopt**: The once-per-ServerDate action that lets the user adopt exactly one of the rolled candidates, creating a `UserGirl` row.

- **Partner**: The currently selected girl for interactions, stored as `ApplicationUser.PartnerGirlId` (FK to `Girls`). The first adoption auto-assigns a partner.

- **Bond**: An integer stored on `UserGirl` that increases by 1 per daily interaction.

- **PersonalityTag**: A user-assigned enum stored on `UserGirl`. It only affects which dialogue pool `DialogueService` uses.

- **Days Since Adoption / Days Together**: The number of ServerDate transitions (not elapsed time) that have occurred since a girl was adopted. Computed via `DailyCadence.GetDaysSinceAdoption()`. If adoption occurred and no reset has happened yet, the value is 0.