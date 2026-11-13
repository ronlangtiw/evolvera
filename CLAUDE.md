# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Evolvera** is an emergent civilization simulation where tribes and civilizations evolve organically based on player choices, random events, and knowledge domain progression. The core principle (EVO-DESIGN-001) is that **complexity emerges from interactions between simple systems**, not from continuously adding variables.

The simulation is built in C# .NET 8 as a console-based prototype with plans for future UI/visualization (Unity).

## Build & Run

```powershell
# Build the project
dotnet build src/SimCore/Evolvera.SimCore.sln

# Run the simulation (20 turns by default)
dotnet run --project src/SimCore/Evolvera.SimCore.csproj

# The simulation is interactive - use commands during each turn:
#   end - finish current turn
#   units - list all units with positions and movement points
#   mv <id> <dir> - move unit (dirs: N,S,E,W,NE,NW,SE,SW)
#   info <x> <y> - inspect tile details
#   harvest - worker harvests resources at current location
#   found <name> - worker founds new settlement (must be 4+ tiles from existing)
#   map - redisplay map
#   help - show available commands
```

The simulation saves to `saves/phase3_demo.json` after completion.

## Quick Reference: Implementation Status

### ✅ Fully Implemented (Prototype)
- Six knowledge domains with diminishing returns growth (`Domains.cs`, `Actions.cs`)
- Settlement system with population, food economy, legitimacy (`Settlement.cs`, `Population.cs`)
- 7 action types generating multi-domain XP (hunt, farm, raid, trade, ritual, explore, infrastructure)
- Environmental events: drought, flood, plague (`EnvEvents.cs` - 22% chance/turn)
- AI-driven settlement expansion based on prosperity/crowding (`Expansion.cs`)
- Procedural map generation with terrain, rivers, resources (`MapGen.cs`)
- Unit system: Scout, Worker, Militia, Caravan with terrain-based movement (`Units.cs`, `Movement.cs`)
- National Identity calculation with hotspot penalty (`Civilization.cs`)
- Save/load with JSON serialization (`SaveGame.cs`)

### ⚠️ Partially Implemented
- **Milestones & Ages**: Designed (+5/+15 thresholds) but no handler found
- **Event XP multipliers**: Design describes 0.9×/0.93× multipliers, code applies direct XP changes
- **NI loyalty blend**: Proposed 40/60 formula is **unapproved**, do NOT implement

### ❌ Not Implemented (Approved Design)
- **Player Control Modes** (Direct/Guided/Emergent) - Section 2, approved Nov 11
- **Capability vs Willingness** separation - Section 2.1, approved distinction
- **Local Culture & Loyalty** - Section 3, accepted direction
- **Emergent Knowledge System** (EKS-001: Possibility/Discovery/Application) - Section 4, approved constraint
- **Research Steering** (EKS-002) - Section 5, accepted direction
- **Scientific Plausibility** framework (SPE-001) - Section 6, accepted boundaries
- **Planetary Generation** (PGF-001) - Section 7, accepted requirements

See `Evolvera_Design_Updated_2026-11-13.md` for full status of each system.

## Core Architecture

### Domain-Driven Growth System
The simulation tracks six knowledge domains (S, P, W, E, SO, X) that represent civilization capabilities:
- **Survival (S)**: Food, shelter, resilience
- **Production (P)**: Crafting, farming, infrastructure, economy
- **Warfare (W)**: Raiding, defense, strategy
- **Exploration (E)**: Discovery, trade routes, expansion
- **Social (SO)**: Governance, diplomacy, cohesion
- **Expression (X)**: Art, ritual, ideology, culture

**Key implementation detail**: Domain growth uses diminishing returns: `delta = xp / (2.0 * (1.0 + current * 0.05))` (see `DomainMath.Delta` in `Sim/Models/Domains.cs`).

### Settlement-Centric Simulation
- Civilizations are composed of settlements (camps, hamlets, towns, cities)
- Each settlement has local domain values, population, food stores, and legitimacy
- Civilization-level domains are **population-weighted aggregates** of settlement values (see `Civilization.RecalculateRollup`)
- Settlements can autonomously expand when conditions are met (population > 100, exploration > 8)

### Actions & Events System
- **Actions**: Each turn, settlements perform two random actions (hunt, farm, raid, trade, ritual, explore, infrastructure) that generate domain XP
- **Environmental Events**: ~22% chance per turn per settlement (drought, flood, plague) that modify domains and apply temporary XP multipliers
- Actions and events are defined in `Sim/Systems/Actions.cs` and `Sim/Systems/EnvEvents.cs`

### Map & World Generation
- Procedurally generated hex-like grid maps with terrain types (Plains, Forest, Mountain, Desert, Coast)
- Rivers flow from high to low elevation
- Resources (Food, Minerals, Strategic) distributed with varying richness
- Map generation in `Sim/World/MapGen.cs` uses cellular automata and flood-fill algorithms

### Unit & Movement System
- Units (Scout, Worker, Warrior, Settler) have movement points and type-specific capabilities
- Movement costs vary by terrain (Mountains cost 2, others cost 1)
- Workers can harvest resources and found settlements
- Implementation in `Sim/Units/Units.cs` and `Sim/Systems/Movement.cs`

## Key Design Decisions & Status

### Implemented (Prototype)
- Six knowledge domains with diminishing returns growth
- Settlement expansion, population, and local economies
- Terrain generation with rivers and resources
- Unit movement, harvesting, and settlement founding
- Environmental events affecting domain growth
- National Identity calculation based on domain balance

### Design Direction (Not Yet Implemented)
The design documents describe several future systems that are **approved but not coded**:

1. **Player Control Modes** (Direct / Guided / Emergent): Different levels of civilization autonomy and player influence
2. **Resistance & Willingness**: Separating "can the civilization do this?" from "will they comply?"
3. **Local Culture & Loyalty**: Settlement-specific cultural identity and loyalty to central authority
4. **Emergent Knowledge System** (EKS-001): Possibility/Discovery/Application engines replacing fixed tech trees
5. **Planetary Generation** (PGF-001): Causally coherent world history from planetary age, geology, extinctions
6. **Scientific Plausibility** (SPE-001): Plausible, internally consistent science without fantasy elements

**IMPORTANT**: Do not assume these systems exist in the code. The design documents (`Evolvera_Design_Updated_2026-11-13.md`) clearly mark what is "Approved principle" vs "Proposed mechanism" vs "Prototype only".

## National Identity (NI) Formula

Current implementation in `Civilization.cs`:
```csharp
NI = 5.0 
    + 0.25 * (Social - 10)
    + 0.20 * (Expression - 10)
    + 0.10 * (Survival - 10)
    - 0.07 * max(0, Warfare - Social)
    - (population-weighted local hotspot penalty)
```

A loyalty-blended formula was proposed but **not approved or implemented**.

## Code Organization

```
src/SimCore/
├── Program.cs              # Main simulation loop and CLI
├── Sim/
│   ├── Models/            # Core data structures
│   │   ├── Civilization.cs   # Civ state and NI calculation
│   │   ├── Settlement.cs     # Settlement state and kinds
│   │   ├── Domains.cs        # Domain struct and growth math
│   │   └── Geometry.cs       # Vec2 for positions
│   ├── Systems/           # Game logic systems
│   │   ├── Actions.cs        # Action XP definitions
│   │   ├── EnvEvents.cs      # Environmental events
│   │   ├── Expansion.cs      # Settlement founding logic
│   │   ├── Population.cs     # Food/population economy
│   │   ├── Movement.cs       # Unit movement
│   │   ├── ResourceUse.cs    # Resource harvesting
│   │   └── InfluenceMap.cs   # Territorial control
│   ├── World/             # Map and terrain
│   │   ├── MapGen.cs         # Procedural map generation
│   │   ├── Map.cs            # Map data structure
│   │   ├── Terrain.cs        # Terrain types
│   │   └── Resources.cs      # Resource types
│   ├── Units/             # Unit types and state
│   │   └── Units.cs
│   └── Save/              # Persistence
│       └── SaveGame.cs
```

## Development Guidelines

### Fundamental Design Constraint
**EVO-DESIGN-001**: Complexity should emerge from interactions between simple systems, not from continuously adding variables.

Before adding new state variables, ask:
1. What new behavior does this enable?
2. Can existing state and interactions already express this behavior?
3. Can a developer/player trace *why* this outcome occurred?

### Implementation Priorities
1. **Separation of concerns**: Keep simulation engine, presentation, and persistence separate
2. **Reproducibility**: Save generation seed, configuration, and version with world data
3. **Traceability**: Log causal decisions and explanations for emergence
4. **Test via simulation**: Validate mechanics through seeded runs, not just compilation

### Open Design Questions
Many systems have approved design direction but unresolved implementation details:
- Autonomy modes: rules for disobedience and partial compliance
- Culture & loyalty: minimum representation without violating EVO-DESIGN-001
- Knowledge discovery: how to avoid building a disguised tech tree
- Planetary generation: coherent physics without excessive simulation complexity
- Milestone/Age recognition: meaningful eras from patterns vs predetermined sequences

**Do not fill in these open questions without explicit user approval.** The design documents (`Evolvera_Design_Updated_2026-11-13.md` Section 11) list all open decisions.

## Making Design Decisions

### ✅ You MAY Decide
- Code organization and refactoring (within established architecture)
- Performance optimizations that don't change behavior
- Bug fixes that restore intended design behavior
- Adding diagnostic logging and explanatory output

### ⛔ You MUST NOT Decide (Ask First)
- Resolving any "Open Design Decision" from Updated Design Section 11
- Changing domain growth formulas or NI calculation
- Implementing unapproved proposals (40/60 NI blend, research budgets)
- Adding new state variables without EVO-DESIGN-001 justification
- Changing event probabilities, action XP values, or milestone thresholds

### ❓ Gray Areas (Propose, Then Ask)
- New action types (must fit existing domain model)
- Additional resource types (must have emergent interaction)
- Settlement kinds beyond current 6 types
- New unit types with unique movement/capabilities

## Design Document Authority

When conflicts arise, this is the resolution order:

1. **Evolvera_Design_Updated_2026-11-13.md** - PRIMARY AUTHORITY
   - Sections 1-12 contain all approved principles and open questions
   - Status markers: "Approved principle", "Accepted direction", "Proposed mechanism"
   
2. **Actual C# Implementation** - CURRENT REALITY
   - May differ from design due to prototype evolution
   - See "Historical Discrepancies" in updated design Section 10.1
   
3. **Evolvera_Design.md** - HISTORICAL BASELINE
   - Original design, preserved for reference
   - Superseded by updated design where conflicts exist

4. **specs/algorithm.md** - Currently minimal, future detailed mechanics
5. **docs/vision.md** - High-level vision only

### Design Status Conventions

The master design document clearly distinguishes:
- **Approved principle**: Explicitly accepted direction
- **Accepted design direction**: User-specified requirement, implementation open
- **Proposed mechanism**: Suggested implementation, not ratified
- **Prototype only**: Present in code, not definitive design

### Historical Discrepancies
The design doc notes several inconsistencies between original design and current code:
- Event probability: Design says ~30%, code uses 22%
- Event XP multipliers: Described in design, not clearly implemented in code
- Milestone rewards: Designed at +5/+15 intervals, handler not found in Program.cs
- Save file location: Changed from `saves/demo.json` to `saves/phase3_demo.json`

**These discrepancies are intentional documentation**, not errors to silently fix.

## Before Modifying Core Systems

**ALWAYS verify current state before changing:**

1. **Check if milestone system exists**: `grep -r "milestone" src/SimCore/`
2. **Verify event probability**: Check `EnvEvents.cs:14` for current value
3. **Find NI formula**: Check `Civilization.cs` NiFrom method
4. **Locate action XP values**: Check `Actions.cs` static properties

**Known unimplemented approved features** (do not assume they exist):
- Player control modes (Direct/Guided/Emergent)
- Capability vs willingness evaluation
- Local culture diversity beyond inheritance
- Knowledge discovery engines (Possibility/Discovery/Application)
- Planetary history generation

## Testing Emergent Behavior

### Seeded Reproducibility Tests
```powershell
# Run with known seed to verify deterministic behavior
dotnet run --project src/SimCore/Evolvera.SimCore.csproj

# Seed is hardcoded in Program.cs:17 (rng seed 12345, map seed 1337)
# To test different scenarios, modify these values
```

### Validation Checklist for New Features
- [ ] Can you explain an emergent outcome using existing systems?
- [ ] Does the feature add state variables? Justify per EVO-DESIGN-001
- [ ] Is behavior traceable (why did this happen)?
- [ ] Does it work in all three player control modes (when implemented)?
- [ ] Test with multiple world seeds for robustness

### Known Good Baseline
Run the unmodified simulation and compare:
- Turn 1: Capital should have S≈5.8-6.2 after 2 random actions
- Turn 20: Civilization should have 2-4 settlements if expansion occurred
- NI should remain between 2.0-4.0 for balanced domain growth

## Common Development Scenarios

### Scenario: Add a New Action Type
1. Add static property to `Actions.cs` with domain XP profile
2. Add to `ActionBag` array
3. Verify it doesn't break domain balance with seeded test
4. Document why this action is necessary (what behavior gap does it fill?)

### Scenario: Modify Domain Growth Formula
⛔ **STOP** - This requires approval per EVO-DESIGN-001
- Domain growth is a foundational mechanic
- Changes affect all progression and balance
- Propose change with justification, test results, and alternatives

### Scenario: Implement Milestones
1. Check `Evolvera_Design.md` Section "Milestones & Ages" for original spec
2. Check Updated Design Section 11.10 - milestone implementation is an **open question**
3. Do NOT implement predetermined age sequences (violates emergent principle)
4. Propose mechanism for dynamic age recognition from actual patterns

### Scenario: Add Diagnostic Logging
✅ Encouraged - helps with emergence traceability
```csharp
// Good: Explain why expansion occurred
Console.WriteLine($"Expansion triggered: surplus={s.SurplusRatio():F2}, crowding={s.Population/s.CarryCapacity():F2}");

// Good: Track domain changes
Console.WriteLine($"Action {actionName} → S+{xp.S:F2} P+{xp.P:F2} [from {before.S:F1} to {after.S:F1}]");
```

### Scenario: Player Wants to Change Event Probability
1. Note this is a documented discrepancy (22% vs 30%)
2. Changing requires choosing authoritative value
3. Ask: "Should we standardize on 22%, 30%, or make it configurable?"
4. Update both code AND design docs after decision

## Where to Find Key Concepts

| Concept | Primary File | Related Files |
|---------|--------------|---------------|
| Domain growth formula | `Domains.cs:33` DomainMath.Delta | `Actions.cs`, `EnvEvents.cs` |
| Action XP definitions | `Actions.cs:12-18` | `Program.cs:72-77` (application) |
| National Identity formula | `Civilization.cs:35` NiFrom | `Settlement.cs:27` (Legitimacy) |
| Event probability | `EnvEvents.cs:14` | Design says 30% |
| Settlement expansion logic | `Expansion.cs:55` ShouldExpand | `Program.cs:146-156` (AI loop) |
| Resource prerequisites | `Resources.cs:33` MeetsPrereq | Domain thresholds |
| Carrying capacity | `Settlement.cs:49` CarryCapacity | Affects expansion |
| Population dynamics | `Population.cs:12` StepEconomy | Food production/consumption |
| Map generation | `MapGen.cs:29` Generate | Noise-based elevation/moisture |
| Unit movement costs | `Movement.cs:20` TryMove | `Terrain.cs:19` MoveCost table |

## Development Phases

The codebase evolved through informal phases:

- **Phase 1**: Single civilization, basic domains, simple economy
- **Phase 2**: Settlement expansion, carrying capacity, legitimacy
- **Phase 3**: Units, resources, map generation, territorial influence
- **Phase 4+**: Unimplemented approved features (see Implementation Status below)

Current save file: `saves/phase3_demo.json`
Current version marker: `SaveGame.cs:15` = "0.1-phase1" (outdated?)

**Action**: Synchronize version markers with actual phase

## Future Architecture
Suggested structure (not yet implemented):
- `docs/MASTER_DESIGN.md` - Consolidated design
- `docs/DECISIONS.md` - Design decision records
- `docs/MECHANICS.md` - Detailed mechanics specifications
- `docs/ARCHITECTURE.md` - Technical architecture
- `docs/ROADMAP.md` - Implementation roadmap
