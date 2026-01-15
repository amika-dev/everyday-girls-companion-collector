# UI Polish Guide — Everyday Girls: Companion Collector

**Status:** AUTHORITATIVE  
**Scope:** Visual hierarchy, spacing, tone, and presentation  
**Non-goals:** New features, layout redesigns, system changes

This document defines the intended visual tone and polish for the application.

It was derived from a before/after analysis of a polished section and should be treated
as the source of truth for UI decisions.

If a UI change conflicts with this document, the document takes precedence.

---

## Overall Design Shift Summary

The transformation moves from a utilitarian status widget to a relationship-focused profile panel. The "before" version treats the partner as data to display; the "after" version treats the partner as a person to cherish. The visual language shifts from functional reporting to emotional storytelling.

The core philosophy: the character is the hero, not the system.

---

## Concrete Design Rules

### Visual Hierarchy

Primary names should feel like headlines, not labels. The partner's name is the largest, most prominent text element — styled emotionally (accent color, generous size), not as a form field value.

Section titles should be soft context, not bold headers. Use small, uppercase, muted text with letter-spacing (e.g., "TOGETHER WITH") to set context without competing with the hero content.

Labels and values should be visually distinct. Labels are muted/secondary; values are right-aligned and styled as the "answer" the user cares about.

---

### Spacing and Density

Generous breathing room around portraits and names. The portrait should have clear separation from text; the name should not feel cramped.

Stat rows should have consistent vertical rhythm. Each row is its own unit with padding, not a packed list.

Whitespace is a design element. The panel should feel spacious, not efficient.

---

### Typography Tone

Context labels: Small, uppercase, muted gray, letter-spaced. These whisper, not shout.

Hero text (names): Large, accent-colored, emotionally weighted. This is the thing you're meant to feel.

Stat labels: Normal case, muted, paired with emoji for warmth.

Stat values: Right-aligned or clearly separated, normal weight, readable but not emphatic.

---

### Card and Container Treatment

Backgrounds should have subtle warmth. Use a gentle gradient (white → warm cream or soft peach) rather than flat white.

Borders and shadows should be soft. Avoid hard edges; prefer subtle box-shadow and generous border-radius.

Portraits should have presence. Use a colored ring (accent pink) with a soft outer glow or shadow to lift them off the background.

---

### Emotional Tone

Language should be relational, not possessive. Prefer "Together With" over "Your Partner." The framing should feel like companionship, not ownership.

Include relationship timeline details. "First Met" and "Days Together" emphasize history and emotional investment, not just current status.

Use emoji sparingly as warmth markers. Small icons (📅, 🌱, ☁️, ♥) before stat labels add approachability without clutter.

---

## Guidance for Application to Other Screens

When polishing other sections, apply these principles:


| Element              | Before (avoid)                         | After (prefer)                                              |
|----------------------|----------------------------------------|-------------------------------------------------------------|
| Section titles       | Bold, functional, direct               | Small, muted, contextual subtitle                           |
| Character/girl names | Inline with other data                 | Hero-sized, accent-colored, breathing room                  |
| Stat presentation    | Label: Value inline                    | Separated rows, label left (muted), value right             |
| Card backgrounds     | Flat white                             | Soft gradient toward warm tones                             |
| Portraits/images     | Small, minimal border                  | Larger, accent-colored ring, subtle glow                    |
| Tone of language     | System-focused ("Your X")              | Relationship-focused ("Together With", "Days Together")     |
| Density              | Compact, efficient                     | Spacious, rhythmic, restful                                 |


---

## Summary Principles (for mechanical application)

Names are heroes. Make them big, colored, and central.

Context whispers. Small uppercase muted subtitles, not loud headers.

Stats breathe. Vertical rhythm, separated rows, emoji accents.

Warmth through color. Soft gradients, accent rings, glows.

Relational language. The app is about companionship, not inventory.

Spaciousness over efficiency. This is a cozy app, not a dashboard.
