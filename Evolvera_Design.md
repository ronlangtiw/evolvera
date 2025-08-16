# Evolvera Simulation Design Document

## Overview
Evolvera is an emergent civilization simulation system where tribes and civilizations evolve organically based on player choices, random events, and knowledge domain progression. Unlike fixed technology trees, growth emerges from domain values and actions.

---

## Core Knowledge Domains
Each civilization has six tracked domains:

- **Survival (S)**: Food, shelter, resilience.
- **Production (P)**: Crafting, farming, infrastructure, economy.
- **Warfare (W)**: Raiding, defense, strategy.
- **Exploration (E)**: Discovery, trade routes, expansion.
- **Social (SO)**: Governance, diplomacy, cohesion.
- **Expression (X)**: Art, ritual, ideology, culture.

### Domain Rules
- Start value: 5.0
- Growth: Earned through XP from actions, adjusted by diminishing returns:
  ```
  delta = xp / (2.0 * (1.0 + current * 0.05))
  ```
- Domains interact indirectly (trade boosts Social and Production, raiding hurts Social but boosts Warfare).

---

## Actions
Actions each turn generate XP in one or more domains:

- `hunt`: +1 Survival, +0.5 Warfare  
- `farm`: +2 Survival, +1 Production  
- `raid`: +2 Warfare, +1 Production, –0.5 Social  
- `trade`: +1 Exploration, +1 Social, +0.5 Production  
- `ritual`: +1.5 Expression, +0.5 Social  
- `explore`: +1.5 Exploration, +0.5 Survival  
- `infrastructure`: +1.5 Production, +0.5 Social  

Two actions are chosen per turn (currently random).

---

## Events
Events occur randomly (~30% chance per turn). They modify domain growth:

- **Drought**: –0.45 Survival, –0.15 Social; 0.9× Survival XP
- **Flood**: +0.3 Survival, –0.15 Production; 0.93× Production XP
- **Plague**: –0.45 Social, –0.15 Expression; 0.9× Exploration XP

---

## Milestones & Ages
Progression is milestone-based:

- **Minor milestone**: Every +5 in a domain  
  → Unlocks a small perk (+0.1 bonus to the domain).  
- **Major milestone**: Every +15 in a domain  
  → Unlocks a breakthrough (+0.5 bonus, narrative “Age” flavor).

### Age Flavors
Not hardcoded sequentially — instead dynamically generated based on milestone context. Examples:

- Age of Spears (Warfare)  
- Age of Faith (Expression + Social)  
- Age of Irrigation (Survival + Production)  
- Age of Exchange (Trade/Exploration focus)

---

## National Identity (NI)
Represents cohesion and cultural identity (0–10). Formula:

```
NI = Clamp(0, 10,
    5.0
    + 0.25 * (Social – 10)
    + 0.20 * (Expression – 10)
    + 0.10 * (Survival – 10)
    – 0.07 * max(0, Warfare – Social)
)
```

- High NI = stability, unity, resilience  
- Low NI = fragmentation, risk of splinter groups

---

## Example Output (20 turns)
```
== Evolvera Sim (turns=20, seed=12345) ==
T01 [hunt, hunt]: S=5.8 P=5.0 W=5.4 E=5.0 SO=5.0 X=5.0 | NI=2.3
T05 [hunt, explore]: S=8.0 P=6.2 W=6.4 E=7.1 SO=4.6 X=4.9 | NI=2.3 [Event: Plague]
T10 [farm, trade]: S=9.9 P=8.6 W=8.6 E=8.0 SO=5.1 X=6.1 | NI=2.7
T20 [hunt, infrastructure]: S=13.3 P=12.5 W=11.9 E=10.8 SO=6.3 X=6.6 | NI=3.3
```

---

## Current Implementation (C# .NET 8)

### Program.cs (main sim loop)
```csharp
// full Program.cs code included here (see chat logs)
```

---

## Next Steps
- Add **AgeNamer.cs** to generate dynamic age names based on domain progress.  
- Add **AgeEffects.cs** for flavor boosts/penalties.  
- Expand **action list** to include diplomacy, ideology, technology, monuments.  
- Add **CSV tables** for content generation (names, events, traits).  
- Build UI/visualization (Unity prototype).

