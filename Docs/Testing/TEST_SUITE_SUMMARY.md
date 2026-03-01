# Test Suite Implementation Summary

## Overview
Comprehensive test suite for Everyday Girls: Companion Collector, covering both unit-level and integration-level testing:

### Unit Test Coverage
- Server-day calculation (18:00 UTC reset)
- Daily roll system logic (candidate generation)
- Daily adoption system rules (max 30, first adoption sets partner)
- Daily interaction system (bond increase: 90% +1, 10% +2)
- Profile system (display name validation, once-per-reset rule, collection totals)
- Friends system (add/remove friends, friend list, user search, friend profile view, friend collection view)
- Leaderboard system (dense ranking, tie-break ordering, page boundary consistency, pagination)

### Integration Test Coverage
- Full gameplay flows through HTTP → controllers → services → database
- **All integration tests make real HTTP requests** to controller endpoints and verify both HTTP responses and database state changes
- **Fresh DbContext scopes for assertions**: Integration tests use new scoped DbContext instances with AsNoTracking() for all database assertions after HTTP requests, avoiding EF tracking artifacts and ensuring test reliability
- **Strengthened redirect assertions**: All tests verify redirect status codes (302/303), check that redirects are not to login pages, and assert expected target paths where applicable
- Daily roll availability and reset boundaries (via `/DailyAdopt/UseRoll`)
- Adoption flow with collection limits and partner setting (via `/DailyAdopt/Adopt`)
- Interaction with partner and bond accumulation (via `/Interaction/Do`)
- Partner management (switching, abandonment validation) (via `/Collection/SetPartner`, `/Collection/Abandon`)
- Cross-controller partner management flows (DailyAdopt → Collection → Interaction)
- Leaderboard rendering and dense-rank correctness (via `GET /Leaderboards`)
- Access control (users can only modify their own data via authenticated HTTP requests)
- Test infrastructure verification (InfrastructureTests)

## Abstractions Introduced

To make the code testable, we introduced two key abstractions:

### IClock / SystemClock
- **Purpose**: Abstracts `DateTime.UtcNow` to allow time-based testing
- **Location**: `Abstractions/IClock.cs`, `Abstractions/SystemClock.cs`
- **Consumers**: `DailyStateService`, `DailyAdoptController`

### IRandom / SystemRandom
- **Purpose**: Abstracts `Random.Shared` to allow deterministic testing
- **Location**: `Abstractions/IRandom.cs`, `Abstractions/SystemRandom.cs`
- **Consumers**: `DailyRollService`, `InteractionController`

## Services Created

### DailyRollService
- **Purpose**: Encapsulates candidate generation logic
- **Location**: `Services/IDailyRollService.cs`, `Services/DailyRollService.cs`
- **Responsibility**: Shuffles available girls and selects N candidates
- **Registered**: Scoped in `Program.cs`

### AdoptionService
- **Purpose**: Encapsulates adoption rules and validation
- **Location**: `Services/IAdoptionService.cs`, `Services/AdoptionService.cs`
- **Responsibility**: 
  - Validates collection size limits (max 30)
  - Determines if first adoption should set partner
- **Registered**: Scoped in `Program.cs`

### ProfileService
- **Purpose**: Reads profile summaries and enforces display name change rules
- **Location**: `Services/IProfileService.cs`, `Services/ProfileService.cs`, `Services/DisplayNameChangeResult.cs`
- **Responsibility**:
  - Queries username, partner details, total bond, and total companion count in two DB calls
  - Validates display name format (4–16 alphanumeric, mirrors DB CHECK constraint)
  - Enforces case-insensitive uniqueness via `DisplayNameNormalized`
  - Enforces once-per-reset change limit via `DailyCadence.GetServerDateFromUtc` and `LastDisplayNameChangeUtc`
- **Registered**: Scoped in `Program.cs`

## Refactored Code

### DailyStateService
- **Change**: Injected `IClock` instead of using `DateTime.UtcNow`
- **Impact**: Now fully testable with mocked time

### DailyAdoptController
- **Changes**:
  - Injected `IDailyRollService`, `IClock` (removed direct `IRandom`)
  - Uses `_rollService.GenerateCandidates()` instead of inline shuffling
  - Uses `_clock.UtcNow` instead of `DateTime.UtcNow`
- **Impact**: Roll logic now testable, adoption timestamp controllable

### InteractionController
- **Change**: Injected `IRandom` instead of using `Random.Shared`
- **Impact**: Bond calculation now testable with deterministic values

### Program.cs
- **Additions**:
  - `builder.Services.AddSingleton<IClock, SystemClock>()`
  - `builder.Services.AddSingleton<IRandom, SystemRandom>()`
  - `builder.Services.AddScoped<IDailyRollService, DailyRollService>()`
  - `builder.Services.AddScoped<IAdoptionService, AdoptionService>()`
  - `builder.Services.AddScoped<IProfileService, ProfileService>()`

## Unit Tests Created

### DailyCadenceTests (11 tests)
**Location**: `EverydayGirls.Tests.Unit/Utilities/DailyCadenceTests.cs`

Tests:
- Server date calculation before/at/after 18:00 UTC
- Days since adoption counting
- Edge cases (midnight, noon, month boundaries)

### DailyStateServiceTests (22 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/DailyStateServiceTests.cs`

Tests:
- `GetCurrentServerDate()` with various times
- `GetTimeUntilReset()` calculation accuracy
- Daily action availability checks (Roll, Adopt, Interaction)
- Null safety validation
- Reset boundary behavior

### DailyRollServiceTests (7 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/DailyRollServiceTests.cs`

**Testing Approach**: Uses mocked IRandom.Shuffle with callback to apply deterministic transformations (e.g., reversing array) and verify service uses the shuffled result.

Tests:
- Candidate generation with sufficient/insufficient girls
- Empty array handling
- Zero/negative count handling
- Null validation
- Shuffle is called exactly once before selection
- Deterministic shuffle verification (reverses array to prove selection uses post-shuffle order)

**Key Insight**: Instead of asserting specific shuffled positions (which would be non-deterministic), tests use callback mocking to apply known transformations and verify the service correctly selects from the transformed array.

### AdoptionServiceTests (18 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/AdoptionServiceTests.cs`

Tests:
- Collection size limits (0-30 boundary)
- Max collection size enforcement
- First adoption sets partner rule
- Subsequent adoptions don't override partner
- Input validation (negative sizes, etc.)
- Integration scenarios (first/second/full collection)

### InteractionBondTests (9 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/InteractionBondTests.cs`

**Testing Approach**: Uses mocked IRandom with deterministic values to verify bond calculation formula.

Tests:
- +2 bond when random value < 10 (special moment threshold)
- +1 bond when random value >= 10 (normal threshold)
- Edge cases at boundaries (0, 9, 10, 99)
- Deterministic sequence verification (cycles through 0-99 to prove 10%/90% threshold split)
- Independent calculations across consecutive interactions

**Key Insight**: Rather than relying on probabilistic outcomes, tests feed known sequences to IRandom and verify exact expected results. This eliminates flakiness and proves the formula logic is correct.

### ProfileServiceTests (29 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/ProfileServiceTests.cs`

**Testing Approach**: Uses the EF Core InMemory provider (one fresh database per test class instance) for lightweight, no-disk data access, plus `Mock<IClock>` for deterministic ServerDate control. Users, girls, and UserGirl records are seeded directly via DbContext helpers.

**Infrastructure addition**: `Microsoft.EntityFrameworkCore.InMemory` added to the unit test project to support service-level EF Core testing without HTTP overhead.

Tests:
- Profile totals return zero when the player has no companions
- Total bond and companion count are correctly summed across multiple companions
- Partner fields (name, image URL, bond) are null when no partner is assigned
- Partner details are populated from the correct UserGirl record
- Partner bond is not double-counted in the total bond
- `CanChangeDisplayName` is true when the display name has never been changed
- `CanChangeDisplayName` is false when changed on the current ServerDate
- `CanChangeDisplayName` is true when changed on a previous ServerDate
- Format validation rejects names that are too short, too long, or contain non-alphanumeric characters (including whitespace and symbols)
- Valid-format names pass the format check without a format error
- Same name (exact case and different case) is rejected before DB queries are made
- Second change in the same ServerDate is rejected by the once-per-reset rule
- A change on a previous ServerDate is permitted
- Name already taken by another user (exact and different case) is rejected
- A successful change persists all three fields: `DisplayName`, `DisplayNameNormalized`, `LastDisplayNameChangeUtc`
- A first-ever change (null `LastDisplayNameChangeUtc`) succeeds

**Key Insight**: Using the EF Core InMemory provider avoids fragile `DbSet` mocking while keeping tests fast. The InMemory provider does not enforce the SQL Server–specific CHECK constraint, so service-level format validation is what the tests exercise — matching real production behavior.

### FriendsQueryTests (23 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/FriendsQueryTests.cs`

**Testing Approach**: Uses EF Core InMemory provider with directly seeded `FriendRelationship`, `ApplicationUser`, `Girl`, and `UserGirl` records. Tests both `GetFriendsAsync` and `SearchUsersByDisplayNameAsync`.

Tests — `SearchUsersByDisplayNameAsync`:
- Case-insensitive starts-with matching
- Whitespace trimming from search input
- Empty/whitespace input returns empty result
- Self is excluded from results
- `IsAlreadyFriend` flag is marked correctly for existing friends
- Total count and page slice are correct
- Page < 1 clamps to page 1
- Page size of 0 uses the default
- Partner image is included when partner exists; null when no partner
- Companion count and total bond are included in search results; zero when no companions

Tests — `GetFriendsAsync`:
- Returns empty list when user has no friends
- Ordered by `DisplayName` ascending
- Partner details populated when friend has a partner; null fields when no partner
- Correct total count and page slice across multiple friends
- Page clamping and default page-size behaviour
- Companion count and total bond summarised per friend

### FriendProfileQueryTests (6 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/FriendProfileQueryTests.cs`

**Testing Approach**: Uses EF Core InMemory provider with `Mock<IClock>` for deterministic `DaysTogether` calculation.

Tests:
- Returns `null` when the target user does not exist
- Populates `DisplayName` correctly
- Populates all partner panel fields (name, image URL, bond, days together) when partner is set
- Returns null partner fields when user has no partner
- Populates account summary (total companions, total bond)
- Returns zero stats when user has no companions

### FriendsServiceTests (12 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/FriendsServiceTests.cs`

**Testing Approach**: Uses EF Core InMemory provider with `ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))` because `FriendsService` uses a transaction for the bidirectional insert.

Tests — `TryAddFriendAsync`:
- Creates two bidirectional `FriendRelationship` rows on success
- Returns `CannotAddSelf` when requester and target are the same user
- Returns `UserNotFound` when the target user does not exist
- Returns `AlreadyFriends` when a forward relationship row already exists
- Returns `AlreadyFriends` when one direction was pre-inserted
- Returns `AlreadyFriends` when both directions were pre-inserted
- Returns null error fields (success state) on a valid add

Tests — `TryRemoveFriendAsync`:
- Removes both directional rows when friendship exists
- Returns `NotFriends` when no relationship exists
- Returns `CannotRemoveSelf` for self-removal
- Succeeds and removes the single row when only one direction exists (data-integrity repair)
- Returns null error fields (success state) on valid removal

### FriendCollectionQueryTests (12 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/FriendCollectionQueryTests.cs`

**Testing Approach**: Uses EF Core InMemory provider with `Mock<IClock>` for deterministic `DaysTogether` computation. Tests both `GetFriendCollectionAsync` and `GetFriendGirlDetailsAsync`.

Tests — `GetFriendCollectionAsync`:
- Returns empty result when target user has no companions
- Returns correctly paged results with total count
- Default sort is bond DESC, then `DateMet` ASC (oldest first among ties)
- `IsPartner` flag is correctly set for the user's current partner
- Page < 1 clamps to page 1; page size of 0 uses the default
- `SortOldest` orders by `DateMet` ASC
- `SortNewest` orders by `DateMet` DESC

Tests — `GetFriendGirlDetailsAsync`:
- Returns `null` when the girl is not in the user's collection
- Returns all expected DTO fields for a valid owned girl
- `IsPartner` is false when the user's partner is a different girl
- `DateMet` and `DaysTogether` are correctly computed

### LeaderboardQueryTests (14 tests)
**Location**: `EverydayGirls.Tests.Unit/Services/LeaderboardQueryTests.cs`

**Testing Approach**: Uses EF Core InMemory provider — no HTTP, no mocks. Tests call the public `GetTotalBondLeaderboardAsync` and `GetCompanionLeaderboardAsync` methods directly, exercising the private `AssignDenseRanks` helper indirectly.

Tests — TotalBond dense ranking:
- Distinct scores receive sequential dense ranks (1, 2, 3 — no gaps)
- Two users with identical score share the same rank; next distinct score receives the next dense rank
- Tie that straddles a page boundary: both users receive the same rank value across pages
- After a page-boundary tie, the next distinct lower score receives the correct next dense rank (no ordinal gap)

Tests — CompanionBond dense ranking:
- Distinct bonds → sequential dense ranks
- Two users with identical companion bond share the same rank; next distinct bond receives the next rank
- Page-boundary tie: last entry on page 1 and first entry on page 2 share the same rank

Tests — Stable tie-break ordering:
- TotalBond ties are broken by `DisplayNameNormalized` ascending (all tied users share rank 1)
- CompanionBond ties are broken by `DisplayNameNormalized` ascending

Tests — Pagination:
- TotalBond: page size is respected (result count capped at `LeaderboardPageSize`)
- TotalBond: `TotalCount` includes all users regardless of page
- TotalBond: page 2 returns the correct slice and correct starting rank
- CompanionBond: page size is respected
- CompanionBond: `TotalCount` only includes users who own the queried companion

### Combined Total
**200 tests** covering unit logic and end-to-end HTTP integration flows:
- **163 unit tests**: Fast, isolated tests for business logic, date calculations, and service behaviour
- **37 integration tests**: Full HTTP request/response cycle tests verifying controllers, services, and database together

| Class | Runtime tests | Project |
|---|---|---|
| `DailyCadenceTests` | 11 | Unit |
| `DailyStateServiceTests` | 22 | Unit |
| `DailyRollServiceTests` | 7 | Unit |
| `AdoptionServiceTests` | 18 | Unit |
| `InteractionBondTests` | 9 | Unit |
| `ProfileServiceTests` | 29 | Unit |
| `FriendsQueryTests` | 23 | Unit |
| `FriendProfileQueryTests` | 6 | Unit |
| `FriendsServiceTests` | 12 | Unit |
| `FriendCollectionQueryTests` | 12 | Unit |
| `LeaderboardQueryTests` | 14 | Unit |
| `DailyRollIntegrationTests` | 4 | Integration |
| `DailyAdoptIntegrationTests` | 5 | Integration |
| `InteractionIntegrationTests` | 5 | Integration |
| `PartnerManagementIntegrationTests` | 8 | Integration |
| `InfrastructureTests` | 5 | Integration |
| `LeaderboardsIntegrationTests` | 10 | Integration |
| **Total** | **200** | |

All tests pass consistently with deterministic behaviour.

## Testing Philosophy

### Polish and Reliability Improvements

The integration test suite has been polished to maximize reliability and clarity:

- **Fresh DbContext Scopes for Assertions**: All integration tests now use fresh scoped `DbContext` instances with `AsNoTracking()` for database assertions after HTTP requests. This pattern avoids EF tracking artifacts, ensures queries reflect actual persisted state, and eliminates false positives from stale cached entities.

- **Strengthened Redirect Assertions**: All HTTP response validations now verify:
  1. Status code is a redirect (302/303)
  2. Redirect is not to login page (confirms authentication succeeded)
  3. Redirect target path matches expected controller (e.g., `/DailyAdopt`, `/Interaction`, `/Collection`)

- **Improved Failure Messages**: Removed debug blocks and added descriptive assertion messages to make test failures immediately actionable.

- **Encoding Cleanup**: Fixed encoding artifacts in XML documentation comments and flow diagrams (replaced `?` with `→` in flow descriptions).

These improvements ensure tests are robust, maintainable, and provide clear diagnostic information when failures occur.

### Deterministic Over Probabilistic

All tests in this suite follow a strict deterministic approach:

- **No Real Randomness**: Tests never rely on actual random number generation
- **Controlled Inputs**: IRandom is mocked to return known, predictable sequences
- **Exact Expectations**: Assertions verify precise outcomes, not tolerance ranges or distributions
- **Zero Flakiness**: Tests produce identical results on every run

### How We Test Randomness

When testing code that uses randomness (bond calculation, candidate shuffling):

1. **Mock IRandom**: Use Moq to control what "random" values are returned
2. **Feed Known Sequences**: Provide deterministic input sequences (e.g., 0-99 repeating)
3. **Assert Exact Results**: Verify the formula/logic produces expected outputs for those inputs
4. **Prove Formula Correctness**: Tests verify the _logic_ is correct, not that randomness is truly random

**Example**: The bond calculation test feeds a sequence of 0-99 repeated 10 times (1000 values total). With this known input, exactly 100 values are < 10, and 900 values are >= 10. This proves the formula `random < 10 ? 2 : 1` correctly implements the 10%/90% threshold split—without relying on probability.

### Benefits of This Approach

- **Reliability**: Tests never fail randomly
- **Speed**: No need to run thousands of iterations to achieve statistical confidence
- **Clarity**: Test failures clearly indicate logic errors, not statistical noise
- **Reproducibility**: Failures can be debugged with exact input sequences

## Benefits

1. **Testability**: Core logic is now isolated from external dependencies (time, randomness, antiforgery validation)
2. **Maintainability**: Business rules are documented through tests
3. **Confidence**: Changes to game mechanics can be verified automatically
4. **Documentation**: Tests serve as executable specifications of game rules
5. **Integration Coverage**: End-to-end flows verify controllers, services, and database persistence work together correctly
6. **Regression Protection**: 200 deterministic tests catch breaking changes immediately

## Integration Test Infrastructure

### Test Doubles Created

#### TestClock
- **Purpose**: Provides controllable time for deterministic server date calculations
- **Location**: `EverydayGirls.Tests.Integration/TestDoubles/TestClock.cs`
- **Features**:
  - Set fixed UTC time via `SetUtcNow(DateTime)`
  - Advance time by duration via `Advance(TimeSpan)`
  - Thread-safe for concurrent test execution
- **Usage**: Injected into WebApplicationFactory to replace production `IClock`

#### TestRandom
- **Purpose**: Provides scriptable randomness for deterministic bond calculations and shuffling
- **Location**: `EverydayGirls.Tests.Integration/TestDoubles/TestRandom.cs`
- **Features**:
  - Sequential mode: Returns values from predefined sequence (cycles when exhausted)
  - Fixed mode: Returns single value repeatedly via `SetFixedValue(int)`
  - Implements Fisher-Yates shuffle with controlled randomness
- **Usage**: Injected into WebApplicationFactory to replace production `IRandom`

#### TestAntiforgery
- **Purpose**: Disables antiforgery token validation for POST requests in tests
- **Location**: `EverydayGirls.Tests.Integration/TestDoubles/TestAntiforgery.cs`
- **Features**:
  - Implements `IAntiforgery` interface
  - Always returns `true` for `IsRequestValidAsync()`
  - `ValidateRequestAsync()` is a no-op (never throws)
  - Allows POST requests without actual antiforgery tokens
- **Usage**: Injected into WebApplicationFactory to replace production `IAntiforgery` service
- **Why Needed**: Controller actions have `[ValidateAntiForgeryToken]` attributes; replacing the service ensures validation always passes in tests

#### TestAuthHandler
- **Purpose**: Provides test authentication without requiring actual login flow
- **Location**: `EverydayGirls.Tests.Integration/Infrastructure/TestAuthHandler.cs`
- **Features**:
  - Custom authentication scheme for tests
  - Claims are set via helper methods in `IntegrationTestHelpers`
  - Bypasses Identity password validation
- **Usage**: Registered in TestWebApplicationFactory as the default authentication scheme

### TestWebApplicationFactory
- **Location**: `EverydayGirls.Tests.Integration/Infrastructure/TestWebApplicationFactory.cs`
- **Purpose**: Custom `WebApplicationFactory<Program>` for HTTP-level integration testing
- **Configuration**:
  - Uses configuration flags (`Testing:UseSqlite=true` and `ConnectionStrings:DefaultConnection`) to influence Program.cs provider selection
  - Also performs explicit DI service removal and re-registration to guarantee SQLite in-memory database is used with shared connection
  - Applies schema via `EnsureCreated()` (fast, no migrations needed)
  - Injects `TestClock` and `TestRandom` for deterministic behavior
  - Replaces `IAntiforgery` with `TestAntiforgery` to disable token validation (service-level bypass)
  - Replaces authentication with `TestAuthHandler` for test user simulation
  - Connection remains open for factory lifetime (required for SQLite in-memory)
- **Usage**: Create in test constructor, access `TestClock` and `TestRandom` properties, dispose after tests
- **Why Both Config Flags AND Service Swaps**: Program.cs conditionally registers DbContext early (before ConfigureWebHost runs). Config flags ensure the correct provider is chosen initially, and explicit service swaps guarantee the test's open SqliteConnection is used.

### IntegrationTestHelpers
- **Location**: `EverydayGirls.Tests.Integration/Infrastructure/IntegrationTestHelpers.cs`
- **Purpose**: Utility methods for common test setup tasks
- **Methods**:
  - `SeedGirlsAsync(context, count)` - Populate girl pool with test data
  - `CreateTestUserAsync(serviceProvider, email, password)` - Create authenticated user with Identity
  - `AdoptGirlAsync(context, userId, girlId, ...)` - Add UserGirl record
  - `SetPartnerAsync(context, userId, girlId)` - Set user's partner
  - `UpdateDailyStateAsync(context, userId, ...)` - Configure daily action availability
  - `GetDbContext(serviceProvider)` - Access scoped DbContext for assertions

## Integration Tests Created

### DailyRollIntegrationTests (4 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/DailyRollIntegrationTests.cs`

**Scenarios Tested**:
- Roll generates 5 candidates and persists state via HTTP POST to `/DailyAdopt/UseRoll`
- Roll is blocked when already used today (HTTP redirect, state unchanged)
- Roll becomes available after 18:00 UTC reset (HTTP succeeds after reset)
- Roll excludes owned girls from candidates (HTTP POST generates valid candidates)

**Key Behaviors Verified**:
- HTTP request → `/DailyAdopt/UseRoll` endpoint → Controller → Service → Database → HTTP response
- Database state changes (LastDailyRollDate, CandidateXGirlId fields) verified via fresh DbContext scope
- Service integration (`IDailyRollService`, `IDailyStateService`)
- Server date boundary logic (18:00 UTC)
- Redirect target path verification (redirects to `/DailyAdopt` on success)

### DailyAdoptIntegrationTests (5 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/DailyAdoptIntegrationTests.cs`

**Scenarios Tested**:
- Adoption adds UserGirl record and marks adopt as used via HTTP POST to `/DailyAdopt/Adopt`
- Adoption is blocked when already adopted today (HTTP redirect, no girl added)
- Adoption is blocked when collection is full (30 girls) (HTTP redirect, collection limit enforced)
- First adoption sets partner automatically (HTTP POST verifies partner auto-set in DB)
- Second adoption does NOT change partner (HTTP POST verifies partner unchanged in DB)

**Key Behaviors Verified**:
- HTTP request → `/DailyAdopt/Adopt` endpoint → Controller → Service → Database → HTTP response
- UserGirl creation with correct timestamps (via TestClock), verified via fresh DbContext scope
- Collection size enforcement (`AdoptionService`)
- Partner setting logic (first adoption rule)
- Daily state persistence (LastDailyAdoptDate, TodayAdoptedGirlId)
- Redirect target path verification (redirects to `/DailyAdopt` on success)

### InteractionIntegrationTests (5 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/InteractionIntegrationTests.cs`

**Scenarios Tested**:
- Interaction increases bond by +1 (90% case, random >= 10) via HTTP POST to `/Interaction/Do`
- Interaction increases bond by +2 (10% case, random < 10) via HTTP POST to `/Interaction/Do`
- Interaction is blocked when already interacted today (HTTP redirect, DB unchanged)
- Interaction is blocked without partner (HTTP redirect, partner validation)
- Interaction becomes available after reset (HTTP succeeds after 18:00 UTC)

**Key Behaviors Verified**:
- HTTP request → `/Interaction/Do` endpoint → Controller → Service → Database → HTTP response
- Bond calculation formula (controlled via TestRandom)
- Partner requirement validation (redirects if no partner)
- Daily state persistence (LastDailyInteractionDate)
- HTTP status code verification (redirect but not to login)
- Database state changes (bond increases by 1 or 2, daily state marked as used) verified via fresh DbContext scope
- Redirect target path verification (redirects to `/Interaction` on success)

### PartnerManagementIntegrationTests (8 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/PartnerManagementIntegrationTests.cs`

**Scenarios Tested**:
- First adoption sets partner automatically (HTTP POST to `/DailyAdopt/Adopt` verifies partner auto-set)
- Subsequent adoptions do NOT change partner (HTTP POST to `/DailyAdopt/Adopt` verifies partner unchanged)
- Partner switching updates and persists (HTTP POST to `/Collection/SetPartner`)
- Abandoning partner is blocked (HTTP POST to `/Collection/Abandon` redirects, DB unchanged)
- Abandoning non-partner succeeds (HTTP POST to `/Collection/Abandon` removes girl, partner unchanged)
- Interaction without partner is blocked (HTTP POST to `/Interaction/Do` redirects, no partner)
- Interaction with partner succeeds (HTTP POST to `/Interaction/Do` increases bond)
- Users cannot modify other users' partners (HTTP POST to `/Collection/SetPartner` with wrong user, ownership validation)

**Key Behaviors Verified**:
- HTTP request → Multiple controller endpoints (`/DailyAdopt/Adopt`, `/Collection/SetPartner`, `/Collection/Abandon`, `/Interaction/Do`)
- Cross-controller partner rules (DailyAdopt sets partner → Collection manages partner → Interaction requires partner)
- Partner persistence across operations (via real database), verified via fresh DbContext scope
- Access control (user isolation via HTTP authentication headers)
- HTTP status code verification (redirects but not to login)
- Database state changes (partner set/changed, girls removed, bond increased) verified via fresh DbContext scope
- Redirect target path verification (redirects to appropriate controller paths)

### InfrastructureTests (5 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/InfrastructureTests.cs`

**Scenarios Tested**:
- Factory can create and access database (smoke test for test harness)
- TestClock is injected and controllable (verify time abstraction works)
- TestRandom is injected and deterministic (verify randomness abstraction works)
- Database can seed and query girls (verify data seeding helpers work)
- Database can create users (verify Identity integration works)

**Key Behaviors Verified**:
- Test infrastructure setup and configuration
- Dependency injection of test doubles
- SQLite in-memory database connectivity
- Identity framework integration in test environment

### LeaderboardsIntegrationTests (10 tests)
**Location**: `EverydayGirls.Tests.Integration/Controllers/LeaderboardsIntegrationTests.cs`

**Scenarios Tested — TotalBond (`GET /Leaderboards?type=0`)**:
- Returns HTTP 200
- Renders user display names in descending bond order in the HTML
- Renders correct dense-rank badges when the top two users are tied (`#1` appears twice, `#2` once, `#3` absent)
- Shows pagination controls when results exceed `LeaderboardPageSize`

**Scenarios Tested — CompanionBond (`GET /Leaderboards?type=1&girlId=N`)**:
- Returns HTTP 200 with correct ordering and rank badges (distinct bonds → `#1`, `#2`, `#3`)
- Hero title contains `"Bond with {CompanionName}"` (produced by the controller)
- Portrait `<img>` is rendered when the companion's `ImageUrl` is non-empty
- Letter-circle fallback is rendered when `ImageUrl` is empty (view uses `IsNullOrEmpty`)
- Tied users share the same `#N` rank badge; next distinct bond receives the next badge

**Scenarios Tested — POST rejection**:
- `POST /Leaderboards` returns `405 Method Not Allowed` (action is `[HttpGet]`-only)

**Key Behaviors Verified**:
- Full HTTP → `LeaderboardsController` → `LeaderboardQuery` → SQLite → Razor view rendering path
- Dense ranking rendered in HTML via `_LeaderboardRow` partial (`#N` badge strings)
- `IsNullOrEmpty` branching in the hero portrait partial for portrait vs letter-circle
- `"Bond with X"` title string produced by the controller when `selectedCompanion` is resolved
- Pagination container and Next button appear only when `TotalPages > 1`
- No mutations accepted (POST rejected at framework level)

## What Integration Tests Cover End-to-End

### Complete Gameplay Flows Verified

1. **New Player Onboarding**:
   - User registration → Daily state initialization → First roll → First adoption → Partner auto-set → Interaction available

2. **Daily Routine (Happy Path)**:
   - Roll for 5 candidates → Adopt one → Interact with partner → Bond increases → Reset at 18:00 UTC → Repeat

3. **Collection Management**:
   - Adopt multiple girls over time → Switch partner → Change personalities → Abandon unwanted girls (not partner) → Sort by various criteria

4. **Edge Cases and Restrictions**:
   - Collection full (30 girls) → Adoption blocked
   - Already rolled/adopted/interacted today → Actions blocked
   - No partner → Interaction blocked
   - Try to adopt non-candidate → Rejected
   - Try to abandon partner → Rejected
   - Try to modify another user's data → Rejected (ownership validation)

### Technical Integration Points Tested

- **HTTP → Controllers**: Full request/response cycle with HttpClient against real controller endpoints:
  - `/DailyAdopt/UseRoll` (POST) - Daily roll generation
  - `/DailyAdopt/Adopt` (POST) - Adopt candidate from daily roll
  - `/Interaction/Do` (POST) - Daily partner interaction
  - `/Collection/SetPartner` (POST) - Change active partner
  - `/Collection/Abandon` (POST) - Remove girl from collection
  - `/Leaderboards` (GET) - TotalBond and CompanionBond leaderboard rendering
- **Controllers → Services**: Daily state checks, adoption rules, bond calculations, leaderboard queries
- **Services → Database**: Entity CRUD operations, query correctness, dense-rank computation
- **TestClock → Time-dependent Logic**: Server date calculation, reset boundaries, "days together" computation
- **TestRandom → Randomness-dependent Logic**: Bond calculation (+1 vs +2), candidate shuffling
- **Authentication**: TestAuthHandler processes header-based identity claims for test users
- **Antiforgery**: Service-level bypass via TestAntiforgery (IAntiforgery replacement)
- **Identity Integration**: User creation, authentication flow setup
- **Entity Framework Core**: SQLite in-memory provider, schema creation, migrations compatibility

## Remaining Test Coverage Gaps

### Currently Not Covered (Future Work)

1. **Friends System — Integration Tests**:
   - The Friends system is fully unit-tested (`FriendsQueryTests`, `FriendProfileQueryTests`, `FriendsServiceTests`, `FriendCollectionQueryTests`) but has no dedicated HTTP integration tests
   - End-to-end flows through `FriendsController` (add friend, remove friend, view friend profile, browse friend collection) are not tested at the HTTP layer
   - No assertions against rendered HTML for the friends index, add, or profile pages

2. **DialogueService Content Tests**:
   - Personality-based dialogue pools not yet unit-tested
   - Integration tests verify dialogue is returned, but not content quality/variety
   - Would benefit from tests verifying each personality tag has adequate dialogue coverage

3. **View Rendering Tests**:
   - Razor view compilation and rendering not tested beyond HTML content assertions in leaderboard integration tests
   - ViewModels are tested indirectly through integration tests
   - Full HTML structure and client-side interactions not validated

4. **JavaScript Tests**:
   - `countdown.js` and `site.js` not covered
   - Would require browser automation (e.g., Playwright) or JavaScript test framework (e.g., Jest)
   - Client-side timer behavior and UI interactions not verified

5. **Migration Tests**:
   - Database migration correctness not explicitly tested
   - Schema compatibility between SQL Server (prod) and SQLite (test) assumed
   - Migrations are run manually and verified in development, but not automated

6. **Edge Case Scenarios**:
   - Concurrent user actions (race conditions) not explicitly tested
   - Network failure/timeout handling not covered
   - Extremely large collections (approaching limits) not stress-tested

### Why These Gaps Exist

- **Focus on Core Logic**: Prioritized game mechanics and controller/service integration over presentation layer
- **MVP Scope**: Essential gameplay flows and business rules are fully covered; UI polish and advanced scenarios deferred
- **Tooling Constraints**: JavaScript and view rendering tests would require additional test infrastructure

## How to Run Tests

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test .\EverydayGirls.Tests.Unit\EverydayGirls.Tests.Unit.csproj
```

### Run Integration Tests Only
```bash
dotnet test .\EverydayGirls.Tests.Integration\EverydayGirls.Tests.Integration.csproj
```

### Run with Coverage (dotnet-coverage)
```bash
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

### Build Verification
```bash
dotnet build
```

All tests should pass with zero failures on every run (deterministic design).

## Adherence to Conventions

- All tests follow `Method_Condition_ExpectedResult` naming pattern
- Using xUnit + Moq (as specified in TESTING_GUIDE.md)
- Tests are deterministic (no actual time/randomness dependencies)
- Unit tests are fast (no database, no HTTP)
- Tests cover edge cases and boundaries
- Proper use of Theory/InlineData for parameterized tests
- **Documentation**: Each test file includes clear header comments explaining:
  - What dependencies are mocked and why
  - What scenarios the tests cover
  - What behaviors the tests prove
- **Inline Documentation**: Individual tests use Arrange/Act/Assert sections with explanatory comments
