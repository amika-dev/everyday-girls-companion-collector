# PROJECT OVERVIEW

**Status:** AUTHORITATIVE  
**Scope:** Core gameplay intent, progression philosophy, and feature boundaries  
**Non-goals:** Technical implementation details, UI design rules, or visual presentation

This document defines the intended design goals and conceptual scope of the game.

It serves as the primary reference for what the experience should feel like, how progression is meant to function, and what types of features belong within the project.

If a proposed feature or change conflicts with the principles described in this document, this document takes precedence.

**Everyday Girls: Companion Collector**

**Status:** Under active development  
**Last Updated:** January 2026  
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
- **Database:** SQL Server with Entity Framework Core 9.0.0
- **Authentication:** ASP.NET Core Identity
- **Architecture Pattern:** Classic MVC with service layer

### Frontend
- **View Engine:** Razor Pages/Views
- **CSS Framework:** Bootstrap 5
- **JavaScript:** Vanilla JavaScript (ES6+)

### Development Tools
- **IDE:** Visual Studio 2022 or later (recommended for .NET 10)
- **Database Migrations:** Entity Framework Core CLI tools
- **Version Control:** Git

### Key NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (9.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (9.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (9.0.0)

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
│   └── Design/          # Design documents (UI_DESIGN_CONTRACT.md)   
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

#### `/Models`
All data models and ViewModels:
- **Entities/** - Database-mapped classes:
  - `Girl.cs` - Global pool of adoptable companions
  - `UserGirl.cs` - User-owned companion with bond/personality data
  - `UserDailyState.cs` - Tracks daily action availability per user
  - `ApplicationUser.cs` - Extended Identity user with partner tracking
- **Enums/** - `PersonalityTag.cs` (Cheerful, Shy, Energetic, Calm, Playful, Tsundere, Cool, Doting, Yandere)
- **ViewModels/** - View-specific DTOs:
  - `MainMenuViewModel.cs` - Hub screen data
  - `DailyAdoptViewModel.cs` - Roll/adopt screen data
  - `InteractionViewModel.cs` - Interaction screen data
  - `CollectionViewModel.cs` - Collection grid data
- **Other Models:**
  - `GameplayTip.cs` - Gameplay tip/hint record

#### `/Services`
Business logic services (all registered via dependency injection):
- `DailyStateService.cs` - Manages daily reset logic (18:00 UTC), action availability
- `DialogueService.cs` - Provides random personality-based dialogue lines
- `DailyRollService.cs` - Encapsulates candidate generation (shuffling and selection)
- `AdoptionService.cs` - Validates adoption rules (max collection size, first-adopt-sets-partner)
- `GameplayTipService.cs` - Provides gameplay tips and hints

#### `/Abstractions`
Testability abstractions for external dependencies:
- `IClock.cs` / `SystemClock.cs` - Abstracts `DateTime.UtcNow` for time-dependent logic
- `IRandom.cs` / `SystemRandom.cs` - Abstracts `Random.Shared` for randomness

These abstractions enable deterministic unit testing by allowing tests to inject mocked implementations with controlled behavior.

#### `/Data`
Database access layer:
- `ApplicationDbContext.cs` - EF Core database context (extends IdentityDbContext)
- `DbInitializer.cs` - Seeds initial girl data into global pool

#### `/Migrations`
Entity Framework Core migration files (auto-generated, do not modify manually)

#### `/Constants`
Application-wide constants:
- `GameConstants.cs` - Max collection size (30), daily candidate count (5), reset hour (18 UTC)

#### `/Utilities`
Helper classes:
- `DailyCadence.cs` - Computes server dates and days-since-adoption based on 18:00 UTC reset

#### `/wwwroot`
Static web assets:
- **css/** - `site.css` (custom styles following UI_DESIGN_CONTRACT.md)
- **js/** - `site.js` (confirm dialogs), `countdown.js` (daily reset timer)
- **images/girls/** - Character portrait images (001.jpg, 002.jpg. etc.)
- **lib/** - Third-party libraries (Bootstrap, jQuery)

---

## Core Features

### 1. User Authentication
- Email-based registration and login (ASP.NET Core Identity)
- Password requirements: minimum 6 characters
- Automatic daily state initialization on registration
- Persistent login with "Remember Me" option

### 2. Main Menu (Hub)
- Displays countdown to next daily reset (18:00 UTC)
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

**AspNetUsers** (Identity + Partner Tracking)
- Standard ASP.NET Core Identity fields
- `PartnerGirlId` (int, nullable) - Custom field
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
  }
}
```

**Configuration Responsibilities:**
- Database connection string (required)
- Seeding toggle (run `DbInitializer` on startup)
- Logging levels

#### `appsettings.Development.json` (not tracked in Git)
- Override connection strings for local development
- Enable detailed logging

### Data Seeding

**DbInitializer.cs:**
- Seeds all available girls into `Girls` table on first run
- Only runs if `Seeding:Enable` is `true` in appsettings
- Triggered from `Program.cs` during application startup
- Safe to run multiple times (checks if data exists)

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

4. **Apply Migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run Application:**
   ```bash
   dotnet run
   ```
   Or press F5 in Visual Studio.

6. **Access Application:**
   - Navigate to the URL shown in the console (typically https://localhost:xxxx)
   - Register a new account
   - Seeding will auto-populate girls on first run

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

2. **Apply migrations:** If you modified entities
   ```bash
   dotnet ef database update
   ```

3. **Run locally:** Test the feature end-to-end
   ```bash
   dotnet run
   ```

4. **Verify UI:** Check responsive design (mobile + desktop)
   - Does it match the warm, cozy design system?
   - Are character names prominent?
   - Is spacing generous?
   - Are error messages conversational?

5. **Test authentication flow:** Ensure authorized pages redirect properly

6. **Test daily reset logic:** Verify actions respect reset timing

---

## Related Documentation

- **UI/UX Design Rules:** `Docs/Design/UI_DESIGN_CONTRACT.md` (AUTHORITATIVE for all visual decisions)
- **Implementation Summary:** `Docs/IMPLEMENTATION_SUMMARY.md` (Current feature status)

---

## License and Usage

This repository is made public for viewing purposes.

**No license is granted** to use, copy, modify, or distribute the code.  
All rights reserved.

---

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| Jan 2026 | 1.0 | Initial PROJECT_OVERVIEW.md created based on actual codebase |

---

**For questions or clarifications, refer to the authoritative design documents in `/Docs/Design/` or examine the actual implementation in the codebase.**
