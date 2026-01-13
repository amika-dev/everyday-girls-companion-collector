# Everyday Girls: Companion Collector  
## Game Design Document (GDD)

**Genre:** Single-player web-based collection & light interaction  
**Scope:** Small, cozy project (weeks, not months)  
**Monetization:** None  
**Audience:** Friends / niche enjoyment  

---

## Core Pillars

- Meaningful daily choice  
- Attachment over optimization  
- Extremely low mechanical depth  
- Clear stopping points (1–3 minutes/day)

---

## High-Level Concept

The player logs in daily to view a **Daily Adopt** offering of five randomly selected girls. They may adopt at most one per day, or choose to adopt none.

Adopted girls are added to the player’s collection, where one girl may be designated as the player’s **partner**. The partner is the only girl the player may interact with that day, granting **+1 Bond** through a single daily interaction.

There is no combat, no economy, and no progression beyond light attachment and collection curation.

---

## Core Systems Overview

### Daily Systems (All Independent)

- **Daily Roll** – Reveals five candidates once per day; adoption optional  
- **Daily Adoption** – Selects one of the five candidates and adds them to the collection  
- **Daily Interaction** – One interaction per day with current partner  
- **Daily Reset** – Server-defined boundary resets availability  

Each daily system has its own **Available / Used** state.

---

## User & Authentication

Users must create an account:

- Email + password  
- Standard login / logout  
- All progression is account-bound  
- No anonymous play  

---

## Main Menu (Hub Screen)

### Displays

- Daily Adopt: Available / Used  
- Daily Interaction: Available / Used  

### Partner Panel (if partner exists)

- Partner name  
- Partner image  

### Navigation

- Daily Adopt  
- Interact (disabled if no partner)  
- Collection  

If the user has never adopted a girl:

- Partner panel is empty  
- Interaction screen is inaccessible  

---

## Girls & Collection Rules

### Global Rules

- Maximum owned girls per user: **100**  
- Global girl pool is always larger than 100  
- Users can never own all girls  

### Girl Representation

Each girl consists of:

- Unique ID  
- Name  
- Single image  
- Bond value  
- Date met  
- “Partner” indicator (if applicable)  
- Personality tag dropdown (5 options)

---

## Daily Adopt Screen

### Entry Behavior

When navigating to the Daily Adopt screen:

#### If Daily Pull = Available

- The five candidates are **not visible**
- Screen shows:
  - Countdown to next daily reset
  - Button: **“Use Daily Pull”**

#### If Daily Pull = Used

- Show the persisted five candidates for the day
- Show countdown to reset

---

### Using Daily Pull

When the user clicks **“Use Daily Pull”**:

- Mark Daily Pull as **Used**
- Generate five random girls:
  - Must not already be owned by the user
  - Must respect the global pool
- Persist this list for the current day
- Display them immediately

The list **cannot be rerolled** that day.

---

### Adopting a Girl

From the displayed five:

- User may choose **one** girl to adopt
- Adoption is optional — the user may leave without adopting

On **Adopt** click:

- Show confirmation:
  > “Adopt [Name]? You can only adopt one girl today.”

On confirm:

- Add girl to user collection
- Store:
  - Date met = today
  - Bond = 0
  - Personality tag = default (first enum value)
- If this is the first adoption ever:
  - Automatically set as partner
- Mark Daily Adopt = Used
- Further adoption disabled until reset

---

### Ownership Cap

If user already owns 100 girls:

- Adopt buttons are disabled
- Message shown:
  > “Collection limit reached (100). Abandon someone (not your partner) to adopt new girls.”

---

## Partner System

- A user may have only **one** partner  
- First adopted girl automatically becomes partner  
- User may change partner at any time from Collection  
- Partner cannot be abandoned  
- No “unset partner” option exists  

If a user has never adopted:

- Partner does not exist  
- Interaction screen is inaccessible  

---

## Bond System

- Each girl has a **Bond value** (integer)
- Bond starts at **0**
- Bond increases only through Daily Interaction
- No levels, no caps, no bonuses  

Bond is:

- Displayed numerically  
- Used only for sorting and personal attachment  

---

## Interaction Screen

### Access Rules

- Accessible only if user has a partner

### Displays

- Partner name  
- Partner image  
- Interact button  
- Daily Interaction: Available / Used  

### Interaction Rules

- User may interact **once per day**
- Interaction:
  - Grants +1 Bond to partner
  - Displays one random dialogue line
  - Selected from the partner’s personality tag pool
  - Repeats allowed

After use, interaction becomes **Used** until reset.

---

## Personality Tags

- Exactly **5** personality tags  
- Implemented as an enum  
- Default tag is the first enum value  
- User can change a girl’s personality tag any time  

Personality tags:

- Only affect dialogue pool  
- Have no mechanical impact  

---

## Collection Screen

### Grid View

- Displays owned girls in a **2 × 5 grid** (10 per page)
- Pagination via left/right arrows

Each tile shows:

- Name  
- Image  

### Sorting (One Active at a Time)

Buttons at top:

- Bond (Descending) — default  
- Oldest  
- Newest  

#### Sorting Rules

**Bond**
- Primary: bond DESC
- Tie-breaker: date met ASC

**Oldest**
- date met ASC

**Newest**
- date met DESC

---

## Girl Modal

Opened by clicking a grid tile.

### Displays

- Name  
- Image  
- Bond value  
- Date met  
- Partner indicator (if applicable)

### Actions

- Set as Partner  
- Change Personality Tag  
- Abandon (disabled if she is partner)

### Abandon Confirmation

> “Abandon [Name]? This cannot be undone.”

Confirm / Cancel

Abandoning removes the girl permanently.

---

## Daily Reset

- Server-defined daily reset time

Resets:

- Daily Adopt → Available  
- Daily Interaction → Available  
- Daily Adopt candidates are cleared at reset  

Countdown timers are calculated from server time.

---

## Design Intent Summary

This game is intentionally:

- Shallow  
- Cozy  
- Choice-driven  
- Resistant to optimization  
- Impossible to “finish”