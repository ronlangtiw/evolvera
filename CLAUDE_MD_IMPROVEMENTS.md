# Recommended CLAUDE.md Improvements

## 1. Add Design Document Hierarchy Section

**Why**: Future developers need to know which document is authoritative.

```markdown
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
```

## 2. Add Missing Implementation Verification Steps

**Why**: Developers need to verify assumptions before making changes.

```markdown
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
```

## 3. Add Testing & Validation Section

**Why**: Emergent systems need validation procedures.

```markdown
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
```

## 4. Clarify Design Decision Process

**Why**: Prevent unauthorized filling of open questions.

```markdown
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
```

## 5. Add Common Development Scenarios

**Why**: Provide patterns for typical tasks.

```markdown
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
```

## 6. Add Critical File Cross-Reference

**Why**: Help developers find where concepts are actually implemented.

```markdown
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
```

## 7. Add Version & Phase Tracking

**Why**: Code references "phase3_demo.json" but phases aren't documented.

```markdown
## Development Phases

The codebase evolved through informal phases:

- **Phase 1**: Single civilization, basic domains, simple economy
- **Phase 2**: Settlement expansion, carrying capacity, legitimacy
- **Phase 3**: Units, resources, map generation, territorial influence
- **Phase 4+**: Unimplemented approved features (see Design Status Matrix)

Current save file: `saves/phase3_demo.json`
Current version marker: `SaveGame.cs:15` = "0.1-phase1" (outdated?)

**Action**: Synchronize version markers with actual phase
```
