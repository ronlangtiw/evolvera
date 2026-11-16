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

---

# Player Direction, Civilization Autonomy, and Core Gameplay

> **Design consolidation:** November 16, 2026. This section captures the dedicated Player Direction and Civilization Autonomy discussion. It distinguishes adopted design decisions from proposed implementation mechanisms. Earlier sections document the prototype and may describe superseded mechanics.

## A. Vision and immutable role selection

Evolvera is a world simulation composed of multiple autonomous civilizations. The player is **not** the civilization itself; a player occupies a role within one civilization and, through successive historical representatives, participates in that civilization's continuing history. The player selects one of the following roles at game creation; the role **does not change during the playthrough**:

| Mode | Player's permanent role | Primary expression of intent |
| --- | --- | --- |
| Direct | Government leader | Commands: allocate, move, construct, harvest, negotiate, etc. |
| Guided | Counselor or strategist advising leadership | Advice, recommendations, objectives, priorities |
| Emergent | Influencer of public opinion (an individual or group) | Ideas, aspirations, social and cultural influence |

All three functions exist in **every** civilization whether controlled by people or by AI. The two functions not occupied by the human player are simulated. The roles interact and may cooperate, compete, or resist one another. Government leaders do not have unlimited power: valid directives may still be refused, obstructed, or provoke dissent or rebellion based on legitimacy, cultural values, institutional backing, circumstances, and capacity.

**Role versus representative:** The role is permanent, but the individual(s) representing the player change across years/generations. Succession is contextual: the simulation establishes eligibility and the outcome, influenced by relationships, preparation, institutions, and past decisions. The player always retains a role and a perspective within the civilization, even after a major setback; effective authority or reach may fall to almost nothing. The civilization's knowledge, history, and consequences persist. Personal reputation/standing need not transfer wholesale. A Direct player might become the leader of a replacement government following a coup; the new leader's inherited position and constraints follow the historical simulation.

## B. Minimal background simulation for actors and institutions

Historical actors and institutions are **produced by the civilization and are not directly authored or controlled by the player**. A popular player representative may contribute to the emergence of like-minded actors, but disagreement persists because of different material interests, exposure, local circumstances, experiences, and individual variation. Capable leaders may help develop capable successors through experience and institutions but cannot guarantee them.

Scope constraint: this is a **small supporting system**, not a detailed population/life simulator. Maintain aggregate populations and communities plus a bounded pool of consequential actors and summarized institutions. Use a few behavioral tendencies, beliefs, support relationships, and event-driven updates; retain only consequential historical memory. No exhaustive family trees, individual daily routines, or lifelong NPC simulation are required for first implementation. It should make leadership transitions, social influence, and resistance interesting without becoming the game's main workload.

## C. World generation, allocation, and opponent AI

Every game begins with **multiple civilizations**, whether single-player or multiplayer. The generated world's size influences the number of starting civilization allocations; geography and viable starting regions should also affect the final distribution. Each human player is assigned exactly **one distinct civilization**. No two human players occupy the same civilization. Every allocation without a human player is controlled by AI throughout the game. More civilizations can later emerge, split, merge, or disappear through simulation.

Players compete through their civilizations but may cooperate via diplomacy, trade, knowledge sharing, and mutual assistance. All civilizations share one world clock and the same basic physical/economic simulation. Multiplayer planning and outcomes should be resolved by one authoritative world state.

**AI skill** is configured during game creation using a **global default with optional variation** among AI-controlled civilizations. Skill is separate from civilization personality, knowledge, starting circumstances, and history. Civilizations of identical skill must not all pursue the same objectives or act alike. Behavioral diversity must emerge from conditions, culture, leaders, history and preferences; its detailed mechanics are **deferred**. Suggestions such as skill tiers, weighted variation, individual overrides, or no artificial resource bonuses are **proposals**, not additional confirmed settings.

## D. The map, territory, units, and objects

The world is generated as a map at game start and contains terrain, water, vegetation, wildlife, resource deposits, units, territory/influence, settlements and structures. The player's civilization has units that interact with the world; a 'unit' can represent an individual or group of people (e.g., hunter-gatherers, workers, soldiers). It is not necessary to model every citizen individually. Territory denotes a civilization's sphere of presence or control, which need not imply perfectly enforceable authority.

Players may establish or relocate temporary camps, settlements, towns, cities and capitals according to actual circumstances and capabilities. Structures may exist on the world map or inside settlements (e.g., housing, storage, industry, monuments, defensive works). The player may direct or influence the creation of a new facility, improvement of an existing facility/process, or repurposing of an object. Mobile populations and relocatable camps remain viable; permanent infrastructure cannot simply be moved without cost.

## E. One common map-centered interface

**One interface for all modes; different available options.** The player can either (1) select a unit, then an action and a target or (2) select a map object/target, then an action and appropriate available unit(s). Equivalent inputs should resolve to equivalent underlying requests.

The context menu shows actions that the civilization can feasibly attempt with its current capabilities and what that player role can express. Example expressions:

- **Direct:** Select workers and a resource deposit -> command harvesting/mining with people, tools and available capacity.
- **Guided:** Select resource area -> advise, 'We need more [resource] from this area'; AI government chooses whether/how to pursue it.
- **Emergent:** Select region or settlement -> promote the aspiration, 'Let's build a walled city'; the idea may spread and potentially affect future government projects, without directly ordering one.

The common action description is **Who** (people/unit), **What** (activity or goal), **Where** (map target), **How** (known process), **With what** (tools/materials), and optionally **Why** (priority/intent). The player should not be forced to fill in every field; sensible defaults use existing practices. The simulation checks population, process, tools, resources, environment, knowledge, logistics, and authority. The player must be able to express aims beyond established capabilities, where appropriate, through investigation/experimentation instead of a false immediate action. The six domains represent accumulated knowledge/capability, not hardcoded ideologies. Cultural preferences and historical experiences independently influence what people want or accept.

Initial activity vocabulary proposed for a prototype: move/relocate, explore, gather/harvest, hunt/fish, build, experiment, organize, and interact. Civilizational study can investigate an unknown subject, improve an existing practice, or seek a new solution. No predetermined technology tree is required. Discovery combines directed investigations with incidental/autonomous learning as people work. Finding a resource does **not** imply knowing how to exploit it.

## F. Starting situation and early economy

**Mobile beginning:** The starting community is mobile and chooses when and where to camp or establish a settlement. A small population with modest supplies must contend with food, water, shelter, wildlife, tools, and transport. The opening is survival-focused but remains open to mobility and alternate paths; permanent settlement is not mandatory. Examples discussed used 36 total people, 24 available worker-equivalents, 12 food units and no permanent settlements; these are **illustrative, not fixed starting values**.

**Capacity rather than action points:** People (who), processes (how), and tools (with what) form the central action logic. More people and better tools generally improve productivity, subject to terrain, constraints and diminishing returns. Multiple activities may run concurrently if capacity is available. The illustrative labor allocation was 12 worker-equivalents gathering food, 6 building shelter/acquiring materials, and 6 exploring, with none left unassigned. Workers assigned to standing operations need not be reassigned every turn. The game's workforce is available directed labor after normal household and non-directed activities, not a count of all persons.

## G. Time, duration estimates, and turn boundaries

**Confirmed turn model (supersedes fixed-length or player-selected interval proposals):**

1. The **base world-time unit is one day**. The world clock is shared by all civilizations.
2. Each finite player action has a duration expressed as an **estimate** and may last multiple days. Actions run concurrently where capacity permits.
3. A turn advances until the **earliest actual completion of an outstanding player action**, or an earlier **qualifying interruption event**. Estimates give a provisional expected endpoint; the actual event schedule governs time. At the next boundary, determine the next minimum from **remaining** durations.
4. AI-controlled activities progress and finish in the background, including all non-player civilizations and unoccupied roles. **Their routine completions do not end the human turn**; an AI-caused critical event can interrupt it.
5. Multiplayer follows the same rule using the earliest player-action completion **across all human players**. Every civilization advances through the same elapsed days.
6. At a shared multiplayer boundary, players whose activities remain incomplete are given the opportunity to **interrupt or continue their own activities**. Unfinished work and consumed resources are not reset. Routine standing work need not manufacture a daily turn endpoint.
7. Actions are constrained by labor, tools, materials, knowledge, logistics and authority. The same workers/resources cannot be committed twice. Pauses, cancellations and reallocations preserve completed work and impose actual losses/costs where applicable.
8. An interrupt stops advancement at the relevant decision point. The player responds within the authority and information available to their fixed role. Prior world time and outcomes are not retroactively undone.

**Example:** In single-player, concurrent actions estimated at 7, 30 and 45 days lead to an **expected** next review near Day 7, barring an interruption. If the first action actually completes at Day 9, the boundary may instead be Day 9 (or earlier if another action completes sooner). In multiplayer, the earliest actual player-action completion among all players governs the shared boundary.

**Duration estimation:** Familiarity/experience improves precision. A first major construction project may have a broad error range; after many comparable constructions across cities and generations, estimates should become accurate. Implement this **simply** via a small accumulated familiarity measure per broad activity type, combined with a base duration and bounded estimate variation. Estimate error must not itself change actual work duration. Experience persists across representative transitions. The specific percentages and formula proposed in discussion are illustrative, not finalized.

**Unresolved operational edge case:** If all humans have only continuous operations or no finite actions, the system needs a non-disruptive review/advance mechanism; an optional fallback interval was suggested but not formally adopted. Also to be tuned: event severity, simultaneous events, uncertainty and re-estimation, turn pacing and anti-stalling in multiplayer.

## H. Intent and autonomy rules

- **Direct:** Players issue concrete commands to units and government resources. Mechanically possible commands are normally followed, but established values, leadership legitimacy, institutions and factions can resist or rebel. Direct is not a shortcut around other simulation systems.
- **Guided:** Players advise leadership about priorities, objectives, locations and strategies; the government independently considers and may accept, reinterpret, modify, or decline the recommendation.
- **Emergent:** Players stimulate public aspirations and beliefs via the influential person/group of their era. Social reach and public responses determine whether these ideas influence autonomous conduct, institutions or the government. Players cannot simply assign labor through an influencer interface.

All modes need understandable explanations: why an order was resisted, why advice failed, or why an idea spread or faded. Authority, credibility, popularity, cultural alignment and practical feasibility matter but should be initially modeled compactly, not through a comprehensive political-life simulation.

## I. Implementation guidance and known prototype gaps

The uploaded .NET 8 SimCore prototype already represents a map, terrain, resources, units, settlements, domains, events, primitive expansion and a CLI for movement/harvesting/founding. Its current two random settlement actions per turn, fixed 20-turn demonstration loop, basic worker model and simple resource thresholds are **prototype scaffolding**, not settled mechanics for the new action-based turn design. No new rules in this section are claimed to have been implemented in source code.

Recommended modular progression: (1) shared activity/request structures and map selection, (2) capacity and finite-duration scheduler, (3) event interrupt/resume and saves, (4) context-specific role translation and autonomy responses, (5) single-player with multiple AI civilizations, (6) synchronized one-player-per-civilization multiplayer, (7) AI behavior diversity and deeper content. Preserve determinism and a server-authoritative pathway without building multiplayer networking first.

## J. Explicitly deferred design topics

- Detailed AI play-style variation among opponents of equal skill.
- Precise acceptance/resistance, legitimacy, advisory persuasion and cultural influence algorithms.
- Successor and institution background implementation beyond the lightweight agreed scope.
- Final world size-to-civilization count calibration and AI tier weights.
- Specific initial action durations, output rates, wildlife/resource sets, exploration risk and research discovery probabilities.
- Event interruption thresholds and a fallback time-advance rule without finite player actions.
- Multiplayer disconnections, planning deadlines, stalling prevention, victory conditions, and diplomacy details.

**Decision register:** See `Evolvera_Decision_Log.md` for adopted, proposed, deferred and superseded items.
