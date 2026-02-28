# PROJECT OVERVIEW

**Status:** AUTHORITATIVE  
**Scope:** Core gameplay intent, progression philosophy, and feature boundaries  
**Non-goals:** Technical implementation details, UI design rules, or visual presentation

This document defines the intended design goals and conceptual scope of the game.

It serves as the primary reference for what the experience should feel like, how progression is meant to function, and what types of features belong within the project.

If a proposed feature or change conflicts with the principles described in this document, this document takes precedence.

**Everyday Girls: Companion Collector**

**Status:** Under active development  
**Last Updated:** February 2026
**Target Framework:** .NET 10
**Brief Description:** Cozy, menu-driven web game for collecting and bonding with companions through daily routines.
**Project Goal:** This project is intended as a single-player, personal progression experience with no multiplayer or monetization features.

---

**This document should be updated whenever significant features, architecture changes, or workflows are modified. This should be updated to REFLECT those changes, not explain what the changes were.**

---

## Project Summary

**Everyday Girls: Companion Collector** is a lighthearted, menu-driven web game where players collect companions through a small daily routine, build bonds over time, and manage a growing personal roster at their own pace.

The game is designed around **intentional, low-pressure progression**. Each day offers limited actions, encouraging relaxed play over time rather than long sessions, optimization, or competitive elements.

**Core Gameplay Loop:**
- Discover and adopt new companions through a daily roll system (5 random candidates per day)
- Assign a single active partner and interact once per day to build bonds
- Manage and sort a collection of companions (maximum 30)
- Progress slowly and casually without time pressure or competitive elements

The focus of the experience is **simplicity, routine, and gradual growth** rather than depth or complexity.

**Target Audience:** Players who enjoy cozy, relationship-focused games with minimal time commitment.

---

## Technology Stack

### Backend
- **Framework:** ASP.NET Core MVC (.NET 10)
- **Language:** C# (with nullable reference types enabled, implicit usings enabled)
- **Database:** SQL Server with Entity Framework Core 10.0.3
  - **Azure SQL resilience:** Transient fault retry policy (10 retries, 10 second max delay)
  - **Startup migrations:** Automatic migrations on app start with non-fatal error handling
  - **Database seeding:** Optional seeding controlled by `Seeding:Enable` configuration
  - **Integration test database:** SQLite in-memory mode via `Testing:UseSqlite` configuration flag
- **Authentication:** ASP.NET Core Identity
  - **Password confirmation:** Registration requires password confirmation (Compare validation)
- **Architecture Pattern:** Classic MVC with service layer
- **Reverse Proxy Support:** Forwarded headers configured for Azure App Service deployment
  - Handles `X-Forwarded-For` and `X-Forwarded-Proto` headers
  - Ensures correct HTTPS redirection and client IP detection behind reverse proxy

### Frontend
- **View Engine:** Razor Pages/Views
- **CSS Framework:** Bootstrap 5
- **JavaScript:** Vanilla JavaScript (ES6+)

### Development Tools
- **IDE:** Visual Studio 2022 or later (recommended for .NET 10)
- **Database Migrations:** Entity Framework Core CLI tools
- **Version Control:** Git

### Testing Tools
- **Framework:** xUnit v3 (unit and integration tests)
- **Assertions:** FluentAssertions
- **Mocking:** Moq (unit tests only)
- **Unit test DB:** `Microsoft.EntityFrameworkCore.InMemory` (service-level tests without HTTP)
- **Integration test DB:** SQLite via `Microsoft.EntityFrameworkCore.Sqlite` (full HTTP pipeline tests)
- **Integration host:** `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`)

### Key NuGet Packages

**Main project:**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.3)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.3)
- `Microsoft.EntityFrameworkCore.Sqlite` (10.0.3) — used for integration test SQLite path
- `Microsoft.EntityFrameworkCore.Tools` (10.0.3)

**Test projects (`EverydayGirls.Tests.Unit`, `EverydayGirls.Tests.Integration`):**
- `xunit.v3` (3.2.2)
- `xunit.runner.visualstudio` (3.1.5)
- `Microsoft.NET.Test.Sdk` (18.3.0)
- `FluentAssertions` (8.8.0)
- `Moq` (4.20.72) — unit tests only
- `Microsoft.EntityFrameworkCore.InMemory` (10.0.3) — unit tests only
- `Microsoft.AspNetCore.Mvc.Testing` (10.0.3) — integration tests only
- `coverlet.collector` (8.0.0)

---

## Application Architecture

### MVC Pattern Overview

This application follows the **classic ASP.NET Core MVC pattern**:

1. **Models** define the data structure and business entities
2. **Views** (Razor templates) render the HTML presentation layer
3. **Controllers** handle HTTP requests, coordinate business logic, and return views
4. **Services** provide reusable business logic (dependency injection pattern)

### Architecture Layers

```
┌─────────────────────────────────────────────┐
│           Presentation Layer                │
│  (Razor Views + JavaScript + Bootstrap)     │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│          Controller Layer                   │
│  (Request handling, ViewModels, routing)    │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│           Service Layer                     │
│  (Business logic, daily state management)   │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│           Data Access Layer                 │
│  (Entity Framework Core + DbContext)        │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│           Database (SQL Server)             │
└─────────────────────────────────────────────┘
```

### Key Design Patterns

- **Service Pattern:** Business logic abstracted into services (`IDailyStateService`, `IDialogueService`)
- **Repository Pattern:** Implicit through Entity Framework Core `DbContext`
- **Dependency Injection:** Constructor injection for all services and DbContext
- **ViewModel Pattern:** Separate view models for presentation logic (`MainMenuViewModel`, `CollectionViewModel`, etc.)
- **Identity Framework:** Authentication and user management through ASP.NET Core Identity
- **Claims Factory:** `ApplicationUserClaimsFactory` stamps `DisplayName` into the auth cookie at sign-in, making it available via `User.FindFirst("DisplayName")` without a per-request database query

### Database Resilience

The application implements resilient database connectivity suitable for Azure SQL:

- **Transient fault retry policy:** SQL Server connections automatically retry on transient failures (network issues, throttling, etc.) with exponential backoff (up to 10 retries, 10 second max delay)
- **Graceful startup:** Database migrations run automatically on startup but do not crash the app if the database is temporarily unavailable. Errors are logged and the web app remains available.
- **Seeding:** Database seeding (controlled by `Seeding:Enable` configuration) runs after successful migrations and logs completion status.

This design ensures the application can:
- Handle temporary Azure SQL throttling or network issues
- Start successfully even when the database is warming up or experiencing brief outages
- Automatically apply schema changes on deployment without manual intervention

### Data Flow Example

1. User navigates to `/Home/Index`
2. `HomeController.Index()` method executes (authorized users only)
3. Controller retrieves user data from `ApplicationDbContext`
4. Controller calls `IDailyStateService` to check daily action availability
5. Controller builds `MainMenuViewModel` with necessary data
6. View renders using the ViewModel
7. JavaScript (`countdown.js`) handles client-side timer updates

---

## Project Structure

### Root-Level Organization

```
EverydayGirlsCompanionCollector/
├── Controllers/          # MVC controllers (request handlers)
├── Views/               # Razor view templates (.cshtml)
├── Models/              # Data models and ViewModels
│   ├── Entities/        # Database entities (Girl, UserGirl, etc.)
│   ├── Enums/           # Enumerations (PersonalityTag)
│   └── ViewModels/      # View-specific data transfer objects
├── Services/            # Business logic services
├── Abstractions/        # Testability abstractions (IClock, IRandom)
├── Data/                # Database context and initialization
├── Migrations/          # EF Core database migrations
├── Constants/           # Application constants (GameConstants)
├── Utilities/           # Helper classes (DailyCadence)
├── wwwroot/             # Static files (CSS, JS, images)
│   ├── css/             # Custom stylesheets
│   ├── js/              # JavaScript files
│   ├── images/          # Static images (girl portraits)
│   └── lib/             # Frontend libraries (Bootstrap, jQuery)
├── Properties/          # Launch settings
├── Docs/                # Documentation
│   ├── Design/          # Design documents (UI_DESIGN_CONTRACT.md)
│   └── Testing/         # Test suite documentation
├── EverydayGirls.Tests.Unit/       # xUnit v3 unit tests
├── EverydayGirls.Tests.Integration/ # xUnit v3 integration tests (HTTP + SQLite)
├── Program.cs           # Application entry point and configuration
├── appsettings.json     # Configuration settings
└── *.csproj             # Project file
```

### Key Folders and Responsibilities

#### `/Controllers`
Contains all MVC controllers that handle HTTP requests:
- `HomeController.cs` - Main menu/hub page
- `DailyAdoptController.cs` - Daily roll and adoption system
- `InteractionController.cs` - Partner interaction and dialogue
- `CollectionController.cs` - Collection viewing, sorting, partner selection
- `AccountController.cs` - User registration, login, logout
- `GuideController.cs` - Gameplay tips and hints display
- `ProfileController.cs` - Profile summary and display name change
- `FriendsController.cs` - Friends list, user search, add-friend, remove-friend, friend profile, and friend collection routes

#### `/Views`
Razor templates organized by controller:
- `/Views/Shared/` - Layout, partial views, and reusable components
  - `_Layout.cshtml` - Master layout template (navigation, footer)
  - `_PartnerPanel.cshtml` - Reusable partner display component
  - `_DailyStatusIndicators.cshtml` - Daily action status display
- `/Views/Home/` - Main menu/hub views
- `/Views/DailyAdopt/` - Roll and adoption views
- `/Views/Interaction/` - Partner interaction views
- `/Views/Collection/` - Collection grid and management views
- `/Views/Account/` - Authentication views (login, register)
- `/Views/Guide/` - Gameplay tips and hints views
- `/Views/Profile/` - Profile summary with display name modal
- `/Views/Friends/` - Friends list, add-friends search, friend profile (read-only), friend collection (read-only, paged) views, and `_FriendGirlModal` partial for immutable companion detail modal

#### `/Models`
All data models and ViewModels:
- **Entities/** - Database-mapped classes:
  - `Girl.cs` - Global pool of adoptable companions
  - `UserGirl.cs` - User-owned companion with bond, personality, and skill data
  - `UserDailyState.cs` - Tracks daily action availability per user
  - `ApplicationUser.cs` - Extended Identity user with profile, currency, and partner tracking
  - `TownLocation.cs` - Configuration data for town locations where companions can be assigned
  - `FriendRelationship.cs` - Tracks friend relationships between users
  - `CompanionAssignment.cs` - Tracks companion assignments to town locations
  - `UserTownLocationUnlock.cs` - Tracks which locked locations a user has unlocked
- **Enums/**
  - `PersonalityTag.cs` (Cheerful, Shy, Energetic, Calm, Playful, Tsundere, Cool, Doting, Yandere)
  - `SkillType.cs` (Charm, Focus, Vitality)
- **ViewModels/** - View-specific DTOs:
  - `MainMenuViewModel.cs` - Hub screen data
  - `DailyAdoptViewModel.cs` - Roll/adopt screen data
  - `InteractionViewModel.cs` - Interaction screen data
  - `CollectionViewModel.cs` - Collection grid data
  - `RegisterViewModel.cs` - Registration form with password confirmation
  - `ProfileViewModel.cs` - Profile page summary data (display name, partner details, collection totals)
  - `FriendProfileViewModel.cs` - Read-only friend profile data (no edit flags)
  - `FriendListItemDto.cs` - Friend list item with partner avatar, companions count, and total bond
  - `UserSearchResultDto.cs` - Add-friends search result with friendship status and Add eligibility flag
  - `FriendGirlListItemDto.cs` - Read-only companion list item for friend collection
  - `FriendGirlDetailsDto.cs` - Read-only companion detail for friend "More About Her" modal
- **Other Models:**
  - `GameplayTip.cs` - Gameplay tip/hint record
  - `PagedResult.cs` - Generic paged result type with items, total count, and page metadata

#### `/Services`
Business logic services (all registered via dependency injection):
- `ApplicationUserClaimsFactory.cs` - Stamps `DisplayName` into the auth cookie at sign-in via `IUserClaimsPrincipalFactory`
- `DailyStateService.cs` - Manages daily reset logic (18:00 UTC), action availability
- `DialogueService.cs` - Provides random personality-based dialogue lines
- `DailyRollService.cs` - Encapsulates candidate generation (shuffling and selection)
- `AdoptionService.cs` - Validates adoption rules (max collection size, first-adopt-sets-partner)
- `GameplayTipService.cs` - Provides gameplay tips and hints
- `ProfileService.cs` - Reads profile summaries and enforces display name change rules
- `DisplayNameChangeResult.cs` - Result record returned from display name change attempts
- `FriendsQuery.cs` - Read-only paginated queries for friend list and user search by display name
- `FriendsService.cs` - Write service for bidirectional friend creation and removal with transaction safety
- `AddFriendResult.cs` - Result record returned from add-friend attempts
- `RemoveFriendResult.cs` - Result record returned from remove-friend attempts
- `FriendProfileQuery.cs` - Read-only query for viewing a friend's profile (partner panel, account summary)
- `FriendCollectionQuery.cs` - Read-only paginated query for viewing a friend's companion collection and details

#### `/Abstractions`
Testability abstractions for external dependencies:
- `IClock.cs` / `SystemClock.cs` - Abstracts `DateTime.UtcNow` for time-dependent logic
- `IRandom.cs` / `SystemRandom.cs` - Abstracts `Random.Shared` for randomness

These abstractions enable deterministic unit testing by allowing tests to inject mocked implementations with controlled behavior.

#### `/Data`
Database access layer:
- `ApplicationDbContext.cs` - EF Core database context (extends IdentityDbContext)
- `DbInitializer.cs` - Seeds initial girl data and town locations into database

#### `/Migrations`
Entity Framework Core migration files (auto-generated, do not modify manually)

#### `/Constants`
Application-wide constants:
- `GameConstants.cs` - Max collection size (30), daily candidate count (5), reset hour (18 UTC), display name length limits (4–16), friends page size (5)
- `DatabaseConstraints.cs` - SQL CHECK constraint definitions used in migrations and DbContext configuration

#### `/Utilities`
Helper classes:
- `DailyCadence.cs` - Computes server dates and days-since-adoption based on 18:00 UTC reset

#### `/wwwroot`
Static web assets:
- **css/** - `site.css` (custom styles following UI_DESIGN_CONTRACT.md)
- **js/** - `site.js` (confirm dialogs), `countdown.js` (daily reset timer), `timezone-display.js` (local time conversion)
- **images/girls/** - Character portrait images (001.jpg, 002.jpg. etc.)
- **lib/** - Third-party libraries (Bootstrap, jQuery)

---

## Core Features

### 1. User Authentication
- Email-based registration and login (ASP.NET Core Identity)
- Password requirements: minimum 6 characters
- Registration requires password confirmation (users must type password twice)
- Automatic daily state initialization on registration
- Persistent login with "Remember Me" option

### 2. Main Menu (Hub)
- Displays countdown to next daily reset (18:00 UTC)
- Reset time displayed in user's local timezone (client-side conversion)
- Shows daily action status indicators (Roll, Adopt, Interaction)
- Partner panel showing current partner's details:
  - Name, image, personality tag
  - Bond level (increases via daily interactions)
  - Days together (since adoption)
  - First met date
- Navigation to all major features

### 3. Daily Roll System
- **Daily Roll:** Generate 5 random companion candidates once per day
- Candidates persist until next reset (stored in `UserDailyState`)
- Candidates are drawn from global pool (`Girls` table)
- Each candidate shows name and portrait

### 4. Daily Adoption System
- **Daily Adopt:** Choose one candidate from the daily roll to adopt
- Maximum collection size: 30 companions
- First adoption automatically sets that companion as partner
- Adopted companions are added to user's collection with:
  - Bond starting at 0
  - Default personality tag (Cheerful)
  - Date met timestamp

### 5. Partner System
- Users designate one owned companion as their active "partner"
- Partner appears on main menu and interaction screen
- Can change partner from collection screen at any time

### 6. Daily Interaction System
- **Daily Interaction:** Spend time with partner once per day
- Bond increase mechanics:
  - 90% chance: Grants +1 bond to partner
  - 10% chance: Grants +2 bond to partner (special moment)
  - When +2 bond occurs, a success banner is displayed: "Something about today felt a little special ✨ Bond +2!"
  - No indication is shown on normal +1 bond days
- Displays random dialogue based on partner's personality tag
- 9 personality types with unique dialogue pools:
  - **Cheerful** - Happy, optimistic
  - **Shy** - Quiet, reserved
  - **Energetic** - Lively, enthusiastic
  - **Calm** - Peaceful, relaxed
  - **Playful** - Fun, teasing
  - **Tsundere** - Prickly but caring
  - **Cool** - Aloof, composed
  - **Doting** - Affectionate, gently protective
  - **Yandere** - Intensely affectionate, slightly obsessive

### 7. Collection Management
- View all owned companions in a paginated grid (10 per page, 2x5 layout)
- Sort options:
  - **By Bond** (default) - Highest bond first, then by date met
  - **Oldest First** - Earliest adoption date
  - **Newest First** - Latest adoption date
- Each companion card shows:
  - Name, portrait, bond level
  - Personality tag, days together
  - Partner indicator (if currently selected)
- **Set Partner** action - Designate any owned companion as partner
- **Abandon** action - Remove companion from collection (with confirmation)
- **Change Tag** action - Reassign personality tag (affects dialogue only)

### 8. Daily Reset System
- Reset time: **18:00 UTC daily**
- "Server Date" logic: Before 18:00 = previous day, after 18:00 = current day
- Resets availability for:
  - Daily Roll
  - Daily Adopt
  - Daily Interaction
- Countdown timer on all screens shows time until next reset
- Reset time displayed in user's local timezone (e.g., "Daily reset at 1:00 PM")
  - Automatically converts from 18:00 UTC to local time
  - Handles DST correctly
  - Falls back to UTC display if browser doesn't support Intl API
- Page auto-refreshes when countdown reaches zero

### 9. Guide System
- **Guide Page** - Displays all gameplay tips and hints in a calm, organized format
- **Login Screen Tips** - Shows one random gameplay tip on the login page (refreshed on each page load)
- Tips cover:
  - Bond building mechanics
  - Collection size limits
  - Daily reset timing
  - Roll persistence
  - Partner management
  - First adoption behavior
  - Personality tag effects
- Accessible to both authenticated and non-authenticated users
- Available via navigation link in both logged-in and logged-out states

### 10. Profile System
- **Profile Page** (`/Profile`) - Personal summary of the player's identity and companion collection
- Horizontal profile card layout displaying:
  - Partner portrait as the large avatar (falls back to letter circle if no partner)
  - Player's chosen display name with inline edit button
  - Partner info section (matching the Home page partner panel):
    - Partner name, First Met date, Days Together, Personality tag, Bond level
    - Single portrait only (used as avatar); no duplicate portrait in the partner block
  - Total bond across all companions in the collection
  - Total number of companions collected
- **Display name customization** via Bootstrap modal:
  - Display names must be 4–16 alphanumeric characters (no spaces or special characters)
  - Case-insensitive uniqueness enforced across all players
  - Can be changed once per daily reset cycle; edit button is disabled until next reset
  - Modal shows friendly validation errors returned by the backend service
  - Uses PRG pattern: success redirects with a brief confirmation message
- Navigation links to Friends and Add Friends pages
- Accessible from the main navigation bar

### 11. Friends System
- **Friends Page** (`/Friends`) - Displays the user's friend list with partner avatars and display names
  - Empty state with a warm message and link to Add Friends
  - Friend cards show display name, partner avatar (or letter fallback), partner subtitle, companions count (🌸), and total bond (✨)
  - Expandable action row per card (⋯ button) with Profile, Collection, and Remove Friend actions
  - Remove Friend uses confirmation modal with gentle messaging and POST form (anti-forgery)
  - Paginated: 5 friends per page; styled Prev/Next controls with page indicator
- **Add Friends Page** (`/Friends/Add`) - Search users by display name and add them as friends
  - Starts-with search on display name (case-insensitive via normalized column)
  - Search results show display name, avatar, Add button (disabled if already friends), companions count (🌸), and total bond (✨)
  - Paginated: 5 results per page; styled Prev/Next controls preserving `q`; `q` and `page` preserved across POST redirect
  - Success and error feedback via TempData inline messages
- **Add Friend action** (POST `/Friends/Add`) - Calls bidirectional add-friend service and redirects via PRG
- **Remove Friend action** (POST `/Friends/{friendUserId}/Remove`) - Calls bidirectional remove-friend service; friendship-gated (404 if not friends); redirects via PRG to friends list
- **Friend Profile Page** (`/Friends/{friendUserId}/Profile`) - Read-only profile view for a friend
  - Friendship-gated: returns 404 if viewer is not friends with the target user (does not leak user existence)
  - Mirrors the user's own Profile page layout: horizontal card with partner avatar, hero name, partner panel (First Met, Days Together, Personality, Bond), and Account Summary
  - Fully read-only: no edit pencil, no display name change, no mutation controls
  - Link to friend's collection page
- **Friend Collection Page** (`/Friends/{friendUserId}/Collection`) - Read-only paged companion collection for a friend
  - Friendship-gated: returns 404 if viewer is not friends with the target user
  - Matches user's own Collection page card layout with portrait, name, partner badge
  - Paginated: 5 companions per page; styled gallery navigation (Earlier/More Companions)
  - "More About Her" button on each card opens an immutable modal via AJAX-loaded partial view
  - Immutable modal shows companion stats (Bond, First Met, Days Together, Personality) with no action buttons
  - **Girl details endpoint** (GET `/Friends/{friendUserId}/GirlDetails/{girlId}`) returns `_FriendGirlModal` partial; friendship-gated
- **Backend:**
  - **Friends list query** - Retrieves a paginated list of friends with partner details, companions count, total bond, ordered by display name
  - **User search** - Paginated starts-with search on display name, excludes self, marks friendship status, includes companions count and total bond
  - **Add friend service** - Creates bidirectional friend relationships in a single transaction with duplicate/race-condition safety
  - **Remove friend service** - Deletes bidirectional friend relationships in a single transaction with race-condition safety
  - **Friend profile query** - Read-only query returning a friend's profile data (display name, partner panel fields, companions count, total bond). No edit flags.
  - **Friend collection query** - Read-only paginated query returning a friend's companion collection (name, image, bond, personality, days together, partner indicator). Includes immutable detail DTO for "More About Her" modal (skills, date met, etc.). No action flags.
  - **Friendship gate** - Server-side authorization helper in `FriendsController` that checks `FriendRelationships` via `DbContext.AnyAsync`; returns 404 on failure without leaking user existence
  - **Paging primitive** - Generic `PagedResult<T>` record with Items, TotalCount, Page, PageSize, computed TotalPages/HasPrevious/HasNext, and input clamping

---

## Frontend Organization

### View Architecture

All views inherit from `/Views/Shared/_Layout.cshtml`, which provides:
- Navigation bar (context-aware: logged in vs. logged out)
- Footer
- Bootstrap and custom CSS/JS includes
- Consistent page structure

### CSS Organization (`wwwroot/css/site.css`)

The stylesheet follows a **cozy, warm design system** as defined in `UI_DESIGN_CONTRACT.md`:

**CSS Variable System:**
```css
/* Core colors */
--eg-accent: Soft rose/pink (#d88ca6)
--eg-gradient-warm: Cream-to-peach gradient
--eg-text-primary: Dark warm gray
--eg-text-muted: Mid-tone gray
--eg-shadow-base: Soft shadow for depth
```

**Component Classes:**
- `.eg-card` - Primary content container with warm gradient
- `.eg-hero-name` - Large, accent-colored character names
- `.eg-subtitle` - Small, uppercase, letter-spaced section labels
- `.eg-stat-row` / `.eg-stat-label` / `.eg-stat-value` - Key-value pair displays
- `.eg-badge` - Status indicators (success/neutral)
- `.countdown-panel` - Daily reset timer container
- `.eg-portrait-ring` - Character portrait with accent border

**Design Philosophy (from UI_DESIGN_CONTRACT.md):**
- Characters are heroes, not data records
- Warm, cozy tone (not corporate/admin)
- Generous spacing, soft colors
- Emoji as visual punctuation (sparingly)
- Conversational, gentle language
- Emotional hierarchy: character names > stats > labels

### JavaScript Organization

#### `site.js`
- Minimal utility functions
- Confirmation dialogs with conversational tone:
  - `confirmAbandon(girlName)` - Gentle "part ways" confirmation
  - `confirmAdopt(girlName)` - Warm "welcome home" confirmation

#### `countdown.js`
- Self-contained countdown timer module (IIFE pattern)
- Targets elements with `[data-countdown-seconds]` attribute
- Updates every second
- Auto-refreshes page when countdown reaches zero
- Calculates from target timestamp to avoid client-side clock drift
- Cleanup on page unload

### Bootstrap Integration

- Bootstrap 5 provides base grid, utilities, and components
- Custom CSS overrides Bootstrap defaults for warmth and personality
- Responsive design: mobile-first, gracefully scales to desktop
- Focus states customized to match accent color

---

## Data and Configuration

### Database Schema

#### Tables (Core Entities)

**Girls** (Global Pool)
- `GirlId` (PK, int, identity)
- `Name` (string, max 100 chars)
- `ImageUrl` (string, max 500 chars)

**UserGirls** (User Ownership + Bond Data)
- `UserId` (PK, string - composite key part 1)
- `GirlId` (PK, int - composite key part 2)
- `DateMetUtc` (DateTime)
- `Bond` (int)
- `PersonalityTag` (enum/int)
- Foreign Keys: `UserId` → `AspNetUsers.Id`, `GirlId` → `Girls.GirlId`
- Indexes for sorting: by Bond DESC, by DateMet ASC/DESC

**UserDailyStates** (Daily Action Tracking)
- `UserId` (PK, string)
- `LastDailyRollDate` (DateOnly, nullable)
- `LastDailyAdoptDate` (DateOnly, nullable)
- `LastDailyInteractionDate` (DateOnly, nullable)
- `CandidateDate` (DateOnly, nullable)
- `Candidate1GirlId` through `Candidate5GirlId` (int, nullable)
- `TodayAdoptedGirlId` (int, nullable)
- Foreign Key: `UserId` → `AspNetUsers.Id`

**AspNetUsers** (Identity + Profile + Partner Tracking)
- Standard ASP.NET Core Identity fields
- `DisplayName` (string, 4–16 alphanumeric chars) - Player's chosen display name
- `DisplayNameNormalized` (string) - Uppercase version for case-insensitive lookups
- `LastDisplayNameChangeUtc` (DateTime, nullable) - Enforces once-per-reset change limit
- `CurrencyBalance` (int) - Player's currency balance
- `PartnerGirlId` (int, nullable) - Foreign key to current partner companion
- Foreign Key: `PartnerGirlId` → `Girls.GirlId`

### Configuration Files

#### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "[SQL Server connection string]"
  },
  "Seeding": {
    "Enable": false  // Set to true to seed girls on startup
  },
  "Testing": {
    "UseSqlite": false  // Set to true by integration tests to swap to SQLite
  }
}
```

**Configuration Responsibilities:**
- Database connection string (required)
- Seeding toggle (run `DbInitializer` on startup)
- `Testing:UseSqlite` — when `true`, the app registers SQLite instead of SQL Server (used automatically by the integration test `WebApplicationFactory`; never set manually in production)
- Logging levels

#### `appsettings.Development.json` (not tracked in Git)
- Override connection strings for local development
- Enable detailed logging

### Data Seeding

**DbInitializer.cs:**
- Seeds all available girls into `Girls` table on first run
- Only runs if `Seeding:Enable` is `true` in appsettings
- Triggered from `Program.cs` during application startup after migrations are applied
- Safe to run multiple times (checks if data exists)

**Database Migrations:**
- Migrations are applied automatically on every application startup via `context.Database.Migrate()`
- No need to manually run `dotnet ef database update` in production
- Ensures database schema is always up-to-date with the application code

**Manual Seeding:**
1. Set `"Seeding": { "Enable": true }` in `appsettings.json`
2. Run application
3. Set back to `false` to prevent re-seeding

---

## Development Workflow

### Initial Setup

1. **Prerequisites:**
   - Visual Studio 2022 (17.12+) or Visual Studio Code with C# extension
   - .NET 10 SDK installed
   - SQL Server (LocalDB, Express, or full edition)

2. **Clone Repository:**
   ```bash
   git clone https://github.com/amika-dev/everyday-girls-companion-collector
   cd everyday-girls-companion-collector
   ```

3. **Configure Database:**
   - Create or update `appsettings.Development.json` (not tracked in Git):
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EverydayGirls;Trusted_Connection=True;MultipleActiveResultSets=true"
       },
       "Seeding": {
         "Enable": true
       }
     }
     ```

4. **Run Application:**
   ```bash
   dotnet run
   ```
   Or press F5 in Visual Studio.
   - Migrations will be applied automatically on startup
   - Seeding will run automatically if enabled in configuration

5. **Access Application:**
   - Navigate to the URL shown in the console (typically https://localhost:xxxx)
   - Register a new account
   - Seeding will auto-populate girls on first run (if enabled)

### Common Development Tasks

#### Add a New Database Migration
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

#### Rollback a Migration
```bash
dotnet ef database update PreviousMigrationName
dotnet ef migrations remove
```

#### Add New Girl to Global Pool
1. Place image in `/wwwroot/images/girls/` (e.g., `031.jpg`)
2. Add entry to `DbInitializer.cs`:
   ```csharp
   new Girl { Name = "NewName", ImageUrl = "/images/girls/031.jpg" }
   ```
3. Enable seeding or manually insert via SQL

#### Add New Personality Tag
1. Update `PersonalityTag` enum in `/Models/Enums/PersonalityTag.cs`
2. Add dialogue pool to `DialogueService.cs`
3. Generate and apply EF migration if needed

#### Modify Daily Reset Time
1. Update `GameConstants.DailyResetHourUtc` in `/Constants/GameConstants.cs`
2. Update footer text in `_Layout.cshtml`
3. No database migration needed (calculated in real-time)

### Debugging and Testing

#### Run with Hot Reload
```bash
dotnet watch run
```
Changes to code auto-restart the application.

#### View Database Contents
- Use SQL Server Management Studio (SSMS)
- Use Visual Studio's SQL Server Object Explorer
- Use Azure Data Studio

#### Check Migration Status
```bash
dotnet ef migrations list
```

#### Clear All Data (Development Only)
```bash
dotnet ef database drop
dotnet ef database update
```

### Build and Publish

#### Debug Build
```bash
dotnet build
```

#### Release Build
```bash
dotnet build -c Release
```

#### Publish for Deployment
```bash
dotnet publish -c Release -o ./publish
```

### Azure App Service Deployment

When deploying to Azure App Service (or any reverse proxy environment):

1. **Forwarded Headers Configuration:**
   - The application is configured to handle `X-Forwarded-For` and `X-Forwarded-Proto` headers
   - This ensures correct HTTPS redirection and client IP detection behind Azure's reverse proxy
   - No additional Azure configuration needed - the app automatically trusts forwarded headers

2. **Recommended Azure Settings:**
   - **HTTPS Only:** Enable "HTTPS Only" in Azure App Service settings
   - **Connection String:** Set `DefaultConnection` in Configuration > Connection strings
   - **Seeding:** Set `Seeding__Enable` to `true` in Configuration > Application settings for initial deployment only

3. **Environment Variables:**
   - Azure App Service automatically sets `ASPNETCORE_ENVIRONMENT` to `Production`
   - HTTPS redirection will work correctly with forwarded headers
   - HSTS is enabled automatically in non-development environments

4. **Database:**
   - Migrations run automatically on app startup
   - Ensure Azure SQL firewall allows connections from your App Service
   - Transient fault retry policy handles temporary Azure SQL throttling

---

## Coding Conventions and Standards

### General Principles

1. **Follow existing patterns** - Match the style already present in the codebase
2. **Minimal abstractions** - Don't add interfaces unless needed for external dependencies or testing
3. **Least exposure** - Default to `private`, only expose what's necessary
4. **Explicit null handling** - Nullable reference types enabled; use `ArgumentNullException.ThrowIfNull()`
5. **Comments explain why, not what** - Public APIs should have XML doc comments
6. **Async all the way** - All I/O operations use async/await with `CancellationToken` where appropriate

### Naming Conventions

- **Controllers:** `NameController.cs` (e.g., `HomeController`)
- **Services:** `IServiceName` interface + `ServiceName` implementation
- **ViewModels:** `FeatureNameViewModel.cs` (e.g., `MainMenuViewModel`)
- **Entities:** Singular nouns (e.g., `Girl`, `UserGirl`)
- **DbSets:** Plural nouns (e.g., `Girls`, `UserGirls`)
- **Async methods:** Must end with `Async` suffix
- **Private fields:** Prefix with underscore (e.g., `_context`, `_userManager`)

### File Organization

- **One class per file** (except nested types)
- **File name matches class name** (e.g., `Girl.cs` contains `Girl` class)
- **Namespace matches folder structure** (e.g., `Models.Entities.Girl`)

### Controller Patterns

**Standard Controller Structure:**
```csharp
[Authorize] // If authentication required
public class FeatureController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IService _service;

    public FeatureController(ApplicationDbContext context, IService service)
    {
        _context = context;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 1. Get user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // 2. Fetch data
        var data = await _context.Entity.Where(e => e.UserId == userId).ToListAsync();
        
        // 3. Build ViewModel
        var viewModel = new FeatureViewModel { Data = data };
        
        // 4. Return view
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Action(int id)
    {
        // 1. Get user ID
        // 2. Validate ownership/state
        // 3. Perform action
        // 4. Save changes
        // 5. Set TempData for feedback
        // 6. Redirect
        return RedirectToAction(nameof(Index));
    }
}
```

### Service Patterns

**Interface-based services:**
```csharp
// Service interface in /Services/IFeatureService.cs
public interface IFeatureService
{
    bool CheckCondition(UserDailyState state);
}

// Implementation in /Services/FeatureService.cs
public class FeatureService : IFeatureService
{
    public bool CheckCondition(UserDailyState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        // Implementation
    }
}

// Registration in Program.cs
builder.Services.AddScoped<IFeatureService, FeatureService>();
```

### View Patterns

**Standard Razor View Structure:**
```razor
@model FeatureViewModel
@{
    ViewData["Title"] = "Page Title";
}

<div class="container">
    <!-- Content -->
</div>

@section Scripts {
    <script src="~/js/feature.js"></script>
}
```

**Use HTML helpers and tag helpers:**
- `asp-controller`, `asp-action` for links
- `@Html.AntiForgeryToken()` for forms
- `asp-validation-for` for model validation

### Database Patterns

**Always use async EF Core methods:**
```csharp
// Good
var users = await _context.Users.ToListAsync();
var user = await _context.Users.FindAsync(id);
await _context.SaveChangesAsync();

// Avoid
var users = _context.Users.ToList();
```

**Use `Include()` for navigation properties:**
```csharp
var user = await _context.Users
    .Include(u => u.Partner)
    .FirstOrDefaultAsync(u => u.Id == userId);
```

### Error Handling

**Use TempData for user-facing messages:**
```csharp
TempData["Error"] = "Something went wrong.";
TempData["Success"] = "Action completed successfully!";
return RedirectToAction("Index");
```

**Validate user ownership:**
```csharp
var ownsGirl = await _context.UserGirls
    .AnyAsync(ug => ug.UserId == userId && ug.GirlId == girlId);

if (!ownsGirl)
{
    TempData["Error"] = "You don't own that companion.";
    return RedirectToAction("Index", "Home");
}
```

### CSS Conventions

**Follow BEM-like naming (from UI_DESIGN_CONTRACT.md):**
- Block: `.eg-card`
- Element: `.eg-card__header`
- Modifier: `.eg-card--warm`

**Use CSS variables for theming:**
```css
/* Define in :root */
--eg-accent: #d88ca6;

/* Use in components */
.eg-button {
    background-color: var(--eg-accent);
}
```

### JavaScript Conventions

**Use strict mode and IIFE for modules:**
```javascript
(function() {
    'use strict';
    // Module code
})();
```

**Target elements with data attributes:**
```html
<div data-countdown-seconds="3600"></div>
```
```javascript
var element = document.querySelector('[data-countdown-seconds]');
```

---

## Guidance for AI Assistants

### Understanding This Project

When working with this codebase, AI assistants should:

1. **Recognize the project type:** ASP.NET Core MVC web application with Razor views (not Blazor, not Web API)
2. **Understand the design philosophy:** This is a cozy, character-focused game, not a CRUD app or admin panel
3. **Respect existing patterns:** Match naming, structure, and coding style already present
4. **Defer to UI_DESIGN_CONTRACT.md:** All UI/UX decisions must align with the design contract in `/Docs/Design/`

### Approaching Modifications

**Before making changes:**

1. **Read related code first:**
   - Check controller actions to understand request flow
   - Review existing ViewModels before creating new ones
   - Examine similar features for pattern consistency

2. **Identify the layer:**
   - **UI change?** → Modify Views and/or CSS
   - **Business logic?** → Update Controllers or Services
   - **Data structure?** → Modify Entities and create migration
   - **Configuration?** → Update appsettings or GameConstants

3. **Consider dependencies:**
   - Will this affect authentication? (Check `[Authorize]` attributes)
   - Does it need a new service? (Register in `Program.cs`)
   - Does it change the database? (Generate migration)
   - Does it affect daily reset logic? (Check `DailyStateService`)

**Where to place new code:**

| Type of Code | Location | Example |
|--------------|----------|---------|
| New entity | `/Models/Entities/` | `NewEntity.cs` |
| New enum | `/Models/Enums/` | `NewEnum.cs` |
| New ViewModel | `/Models/ViewModels/` | `NewFeatureViewModel.cs` |
| New controller | `/Controllers/` | `NewFeatureController.cs` |
| New service interface | `/Services/` | `INewService.cs` |
| New service implementation | `/Services/` | `NewService.cs` |
| New view | `/Views/ControllerName/` | `ActionName.cshtml` |
| New partial view | `/Views/Shared/` | `_ComponentName.cshtml` |
| New JavaScript | `/wwwroot/js/` | `feature.js` |
| New CSS rules | `/wwwroot/css/site.css` | Append to existing file |
| New constant | `/Constants/GameConstants.cs` | Add to existing class |
| New utility | `/Utilities/` | `HelperName.cs` |

### Maintaining Consistency

**When adding a new feature:**

1. **Follow the existing flow:**
   - Controller → Service (if needed) → DbContext → ViewModel → View
   - Use dependency injection for services
   - Always use async/await for database operations

2. **Match existing UI patterns:**
   - Use `.eg-card` for main content containers
   - Use `.eg-hero-name` for character names
   - Use `.eg-badge` for status indicators
   - Follow spacing and color system from `site.css`

3. **Maintain authentication flow:**
   - Add `[Authorize]` to controllers that require login
   - Retrieve user ID: `User.FindFirstValue(ClaimTypes.NameIdentifier)!`
   - Validate user ownership before modifying data

4. **Handle errors gracefully:**
   - Use conversational error messages (match existing tone)
   - Set `TempData["Error"]` or `TempData["Success"]`
   - Redirect to safe fallback pages (usually "Index", "Home")

5. **Update all affected layers:**
   - If adding a property to an entity → Create migration
   - If changing a controller action → Update corresponding view
   - If adding a service → Register in `Program.cs`
   - If modifying database → Update `DbInitializer` if needed

### Common Tasks Reference

**Add a new daily action:**
1. Add `LastNewActionDate` to `UserDailyState` entity
2. Create migration: `dotnet ef migrations add AddNewAction`
3. Add method to `IDailyStateService` and implement in `DailyStateService`
4. Add property to relevant ViewModel (e.g., `MainMenuViewModel`)
5. Update controller to check availability
6. Add UI indicator in view

**Add a new page:**
1. Create controller in `/Controllers/` with `[Authorize]` if needed
2. Create ViewModel in `/Models/ViewModels/`
3. Create view in `/Views/ControllerName/`
4. Add navigation link in `_Layout.cshtml`
5. Follow existing UI patterns from `site.css`

**Modify the daily reset time:**
1. Update `GameConstants.DailyResetHourUtc`
2. Update any user-facing text mentioning "18:00 UTC"
3. No database migration needed (time is calculated dynamically)

### Anti-Patterns to Avoid

❌ **Don't:**
- Add interfaces for every class (only for services with business logic)
- Create unnecessary abstraction layers
- Expose internal implementation details as `public`
- Use sync-over-async (blocking on async code)
- Ignore the UI_DESIGN_CONTRACT.md design rules
- Create separate CSS files per page (use the unified `site.css`)
- Use JavaScript frameworks (stick to vanilla JS)

✅ **Do:**
- Reuse existing services and utilities
- Follow the established MVC pattern
- Use Entity Framework Core properly (async, Include, eager loading)
- Match the cozy, warm UI tone
- Keep the scope minimal and focused
- Add XML doc comments to public APIs
- Test database changes with migrations

### Testing Changes

After making modifications:

1. **Build the project:** Ensure no compilation errors
   ```bash
   dotnet build
   ```

2. **Run the test suite:** Verify no regressions
   ```bash
   dotnet test
   ```
   Or run with coverage:
   ```bash
   dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
   ```

3. **Apply migrations:** If you modified entities
   ```bash
   dotnet ef database update
   ```

4. **Run locally:** Test the feature end-to-end
   ```bash
   dotnet run
   ```

5. **Verify UI:** Check responsive design (mobile + desktop)
   - Does it match the warm, cozy design system?
   - Are character names prominent?
   - Is spacing generous?
   - Are error messages conversational?

6. **Test authentication flow:** Ensure authorized pages redirect properly

7. **Test daily reset logic:** Verify actions respect reset timing

---

## Test Suite

The project has comprehensive automated test coverage across two test projects.

### Unit Tests (`EverydayGirls.Tests.Unit`)

Fast, isolated tests for business logic using xUnit v3, Moq, and FluentAssertions. The EF Core InMemory provider is used for service-level tests that need a database without HTTP overhead.

| Test Class | Tests | What It Covers |
|---|---|---|
| `DailyCadenceTests` | 8 | Server-day calculation, days-since-adoption edge cases |
| `DailyStateServiceTests` | 22 | Daily action availability, reset boundaries, null safety |
| `DailyRollServiceTests` | 7 | Candidate generation, shuffle determinism, edge counts |
| `AdoptionServiceTests` | 16 | Collection size limits, first-adopt-sets-partner rule |
| `InteractionBondTests` | 5 | Bond formula: +1/+2 thresholds, deterministic sequences |
| `ProfileServiceTests` | 20 | Display name validation, once-per-reset rule, totals |
| `FriendsQueryTests` | — | Paginated friend list and user search queries |
| `FriendsServiceTests` | — | Bidirectional add/remove with transaction safety |
| `FriendProfileQueryTests` | — | Read-only friend profile data |
| `FriendCollectionQueryTests` | — | Read-only paginated friend collection and detail modal |

**Total unit tests: 95+**

### Integration Tests (`EverydayGirls.Tests.Integration`)

Full end-to-end tests using `WebApplicationFactory` and SQLite. All tests make real HTTP requests and verify both HTTP responses and persisted database state.

| Test Class | What It Covers |
|---|---|
| `DailyRollIntegrationTests` | Roll availability, persistence, and reset boundary |
| `DailyAdoptIntegrationTests` | Adoption flow, collection limits, partner auto-set |
| `InteractionIntegrationTests` | Partner interaction, bond accumulation |
| `PartnerManagementIntegrationTests` | Set partner, abandon, cross-controller flows |
| `InfrastructureTests` | Test infrastructure self-verification |

**Total integration tests: 27**

### Test Abstractions

Two abstractions were introduced to make time- and randomness-dependent code testable:

- **`IClock` / `SystemClock`** — Abstracts `DateTime.UtcNow`. Consumed by `DailyStateService` and `DailyAdoptController`.
- **`IRandom` / `SystemRandom`** — Abstracts `Random.Shared`. Consumed by `DailyRollService` and `InteractionController`.

Both are registered as singletons in `Program.cs` and replaced with `TestClock` / `TestRandom` in integration tests via `WebApplicationFactory` service overrides.

### Running Tests

```bash
# All tests
dotnet test

# With coverage (requires dotnet-coverage tool)
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

Detailed test philosophy, infrastructure design, and per-class test descriptions are in `Docs/Testing/TEST_SUITE_SUMMARY.md` and `Docs/Testing/TESTING_GUIDE.md`.

---

## Related Documentation

- **UI/UX Design Rules:** `Docs/Design/UI_DESIGN_CONTRACT.md` (AUTHORITATIVE for all visual decisions)
- **Test Suite Summary:** `Docs/Testing/TEST_SUITE_SUMMARY.md` (per-class test descriptions, philosophy, infrastructure)
- **Testing Guide:** `Docs/Testing/TESTING_GUIDE.md` (how to run and extend tests)
- **Implementation Summary:** `Docs/IMPLEMENTATION_SUMMARY.md` (Current feature status)

---

## License and Usage

This repository is made public for viewing purposes.

**No license is granted** to use, copy, modify, or distribute the code.  
All rights reserved.

---

**For questions or clarifications, refer to the authoritative design documents in `/Docs/Design/` or examine the actual implementation in the codebase.**
