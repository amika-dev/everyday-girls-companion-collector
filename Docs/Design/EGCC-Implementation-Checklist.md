# Everyday Girls: Companion Collector  
## Implementation Checklist

---

## A. CONTROLLERS, ROUTES, AND ACTIONS (ASP.NET MVC)

---

### 1) Authentication

**Controller:** AccountController (or Identity scaffolding)

**Routes:**

- GET /account/register  
- POST /account/register  
- GET /account/login  
- POST /account/login  
- POST /account/logout  

---

### 2) Main Menu (Hub)

**Controller:** HomeController  
**Route:** GET /

**Hub must display these daily status indicators (all are “Available / Used”):**

- Daily Roll: Available / Used  
- Daily Adopt: Available / Used  
- Daily Interaction: Available / Used  

**Hub must also display partner panel (if partner exists):**

- Partner name  
- Partner picture  

**Navigation:**

- Daily Roll / Adopt screen  
- Interact (disabled if no partner exists)  
- Collection  

---

### 3) Daily Roll / Daily Adopt Screen

**Controller:** DailyAdoptController (name can be DailyController if you prefer)

This single page handles:

- Rolling the five options (Daily Roll)  
- Viewing the persisted five options  
- Adopting one (Daily Adopt)  

---

#### 3.1 View Daily Screen (Roll + Adopt UI)

**Route:** GET /daily

**UI always shows:**

- Countdown to next reset  

**If Daily Roll = Available:**

- Show button: “Use Daily Roll”  
- Do NOT show the five options yet  

**If Daily Roll = Used:**

- Show today’s persisted five options immediately (name and image)  
- Adopt buttons are shown but gated by Daily Adopt availability and cap rules  

**If Daily Roll = Used AND Daily Adopt = Used:**

- Still show today’s five options  
- Adopt buttons disabled (or show “Daily Adopt Used”)  

---

#### 3.2 Use Daily Roll (Generate Today’s Five)

**Route:** POST /daily/use-roll

**Logic:**

- Confirm Daily Roll is Available  
- Mark Daily Roll as Used for today  
- Generate five random girls:
  - Must not already be owned by the user  
- Persist the five candidate IDs for today  
- Redirect back to GET /daily (now shows the five)  

**Important rule:**

- Once Daily Roll is used for the day, the five options must be fixed for that day  
- Returning to the screen must show the same five options until reset  
- Users cannot reroll the same day just by leaving or not adopting  

---

#### 3.3 Adopt a Girl (Choose One of the Five)

**Route:** POST /daily/adopt

**Input:**

- GirlId  

**Preconditions:**

- Daily Roll must be Used today (the five exist)  
- GirlId must be one of the five persisted candidates for today  
- Daily Adopt must be Available today  
- User must own fewer than 100 girls  

**UI requirement:**

Confirmation dialog:

> “Adopt [Name]? You can only adopt one girl today.”

**On confirm:**

- Add girl to user collection:
  - Date Met = today  
  - Bond = 0  
  - Personality Tag = default (first enum value)  
- If this is the user’s first-ever adopted girl:
  - Set as partner automatically  
- Mark Daily Adopt as Used for today  
- Redirect back to GET /daily  

**If user owns 100 girls:**

- Disable adopt action  
- Show message:

> “Collection limit reached (100). Abandon someone to adopt new girls. Your current partner cannot be abandoned.”

---

### 4) Interaction Screen

**Controller:** InteractionController  

---

#### 4.1 View Interaction Screen

**Route:** GET /interact

**Precondition:**

- User must have a partner  
- If no partner exists, redirect to Main Menu  

**UI:**

- Partner name + picture  
- Interact button  
- Daily Interaction status (Available / Used)  

---

#### 4.2 Perform Interaction

**Route:** POST /interact/do

**Preconditions:**

- Partner exists  
- Daily Interaction is Available  

**On interaction:**

- Partner bond += 1  
- Mark Daily Interaction as Used today  
- Choose one random dialogue line from the partner’s personality tag pool  
- Repeats allowed  

---

### 5) Collection Screen

**Controller:** CollectionController  

---

#### 5.1 View Collection Grid

**Route:** GET /collection

**Query parameters:**

- sort = bond | oldest | newest  
- page = integer  

**Display:**

- 10 girls per page (2 rows × 5)  
- Pagination arrows  

**Sorting:**

- Bond (default): bond descending, tie → oldest first (date met ascending)  
- Oldest: date met ascending  
- Newest: date met descending  

---

#### 5.2 Girl Modal

**Displays:**

- Name  
- Image  
- Bond value  
- Date met  
- Personality tag  
- Partner indicator  

**Actions:**

- Set as Partner  
- Change personality tag  
- Abandon (disabled if partner)  

**Abandon confirmation:**

> “Abandon [Name]? This cannot be undone.”

---

#### 5.3 Set Partner

**Route:** POST /collection/set-partner

**Rules:**

- Girl must be owned  
- Partner switches immediately  
- No “unset partner”  

---

#### 5.4 Set Personality Tag

**Route:** POST /collection/set-tag

**Input:**

- GirlId  
- PersonalityTag enum value  

---

#### 5.5 Abandon Girl

**Route:** POST /collection/abandon

**Rules:**

- Girl must be owned  
- Girl must NOT be the partner  
- Confirmation required  
- On confirm, delete the UserGirls row  

---

## B. DATABASE SCHEMA (MINIMAL)

---

### 1) Girls

- GirlId (PK)  
- Name  
- ImageUrl  

---

### 2) Users (Identity)

**Additional field:**

- PartnerGirlId (nullable)  

---

### 3) UserGirls

- UserId (PK part)  
- GirlId (PK part)  
- DateMetUtc  
- Bond (int, default 0)  
- PersonalityTag (enum int/tinyint, default 0)  

**Indexes:**

- UserId + Bond DESC + DateMetUtc ASC  
- UserId + DateMetUtc ASC/DESC  

---

### 4) UserDailyState (Roll / Adopt / Interaction separated)

- UserId (PK)  

**Daily cooldown markers:**

- LastDailyRollDate (date, nullable)  
- LastDailyAdoptDate (date, nullable)  
- LastDailyInteractionDate (date, nullable)  

**Persisted candidates:**

- CandidateDate (date, nullable)  
- Candidate1GirlId … Candidate5GirlId (int)  

---

## C. DAILY STATE MACHINE (PLAIN ENGLISH PSEUDOCODE)

---

### 1) Determine the current ServerDate and the next reset time

- Use a fixed reset time (example: 04:00 server timezone)  
- If now is after today’s reset time:
  - ServerDate = today  
- Else:
  - ServerDate = yesterday  

Countdown is time until the next reset.

---

### 2) Determine availability (separate for Roll, Adopt, Interaction)

**Daily Roll is Available if:**

- LastDailyRollDate != ServerDate  

**Daily Adopt is Available if:**

- LastDailyAdoptDate != ServerDate  

**Daily Interaction is Available if:**

- LastDailyInteractionDate != ServerDate  

---

### 3) Daily screen behavior (GET /daily)

- If Daily Roll is Available:
  - Show “Use Daily Roll” and countdown  
  - Do not show candidates  

- If Daily Roll is Used:
  - Show candidates persisted for CandidateDate = ServerDate  
  - Show countdown  
  - Adopt buttons enabled only if Daily Adopt is Available and user is under cap  

---

### 4) Using Daily Roll (POST /daily/use-roll)

- If Daily Roll already used today:
  - Redirect back (no changes)  
- Else:
  - Set LastDailyRollDate = ServerDate  
  - Generate 5 random girls not owned by user  
  - Save CandidateDate = ServerDate and candidate IDs  
  - Redirect back to daily page  

---

### 5) Adopting (POST /daily/adopt)

**Validate:**

- Daily Roll was used today  
- CandidateDate == ServerDate  
- GirlId is one of the 5 candidates  
- Daily Adopt is available today  
- Owned count < 100  

**If valid:**

- Insert into UserGirls (Bond = 0, default tag, DateMet = today)  
- If PartnerGirlId is null, set PartnerGirlId to GirlId  
- Set LastDailyAdoptDate = ServerDate  
- Redirect back  

---

### 6) Interacting (POST /interact/do)

**Validate:**

- Partner exists  
- Daily Interaction available today  

**If valid:**

- Partner bond += 1  
- Set LastDailyInteractionDate = ServerDate  
- Choose random dialog line from tag pool  
- Display result  

---

## D. SUGGESTED BUILD ORDER

1. Identity login/register  
2. Seed Girls table  
3. UserGirls + PartnerGirlId logic  
4. UserDailyState + ServerDate helper + countdown  
5. Daily Roll: Use button + candidate persistence  
6. Daily Adopt: confirmation + insert + cap checks  
7. Collection grid + sorting + pagination  
8. Modal: set partner, set tag, abandon confirmation  
9. Interaction: bond +1 + random dialog pool  

---

## C. Additional Specifications

- This is going to be hosted on Microsoft Azure with PaaS (Azure App Service + managed database)  
- Target framework version = .NET 10  
- Auth Choice = ASP.NET Core Identity  
- Frontend Style = Razor Views + Controllers  
- Managed database on Azure: Azure SQL Database with EF Core + ASP.NET Identity  

---

## D. UI Design Intent

- The UI should be clean, minimal, and readable.  
- The tone should be lighthearted and cozy, not flashy or busy.  
- Prioritize clarity and usability over visual polish.  
- Use simple, structured layouts (cards, grids, panels).  
- Avoid heavy animations, complex JavaScript, or SPA-style behavior.  
- Pages should be server-rendered using Razor Views.  
- Confirmation dialogs should be simple and unobtrusive.  
- The UI should feel intentionally small and focused, not feature-dense.  

---

## E. Scope Control / Non-Goals

- Do not add features that are not explicitly described in the GDD or Implementation Checklist.  
- Do not add monetization, microtransactions, ads, or premium currencies.  
- Do not add chat, messaging, leaderboards, or competitive systems.  
- Do not refactor the project into a SPA framework or client-heavy architecture.  
- Do not introduce background workers, scheduled jobs, or real-time systems.  
- Stop implementation once the MVP described in the documents is complete.