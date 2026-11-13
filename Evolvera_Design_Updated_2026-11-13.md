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

# Design Update: November 11–13, 2026

> **Document status:** Consolidation of recent Evolvera conversations into the existing design. This is a **design specification and decision record**, not a claim that these features are implemented. The earlier sections above have been preserved as historical baseline; inconsistencies with the currently supplied C# snapshot are documented below.
>
> **Evidence/status conventions:** **Approved principle** = explicitly accepted direction; **Accepted design direction** = user-specified requirement, with implementation/algorithms still open; **Proposed mechanism** = a suggested way to implement the direction, not yet ratified; **Prototype only** = demonstrated or present in source, not the definitive design.

## 1. Foundational Design Rule — EVO-DESIGN-001

**Status: Approved principle (November 11, 2026).**

**Complexity should emerge from interactions between simple systems, not from continuously adding more variables.**

This is a system-wide constraint, not just a preference for streamlined source code:

- Favor small, comprehensible, reusable rules that interact to produce consequences and novel outcomes.
- Justify every proposed state variable: what new behavior does it make possible, and can existing state and interactions already express that behavior?
- Keep behavior traceable: developers and players should be able to understand *why* a civilization acted, advanced, resisted, fractured, or suffered a consequence.
- Evaluate new mechanics by simulation and observable cause-and-effect, not merely by the number of features or possible combinations.
- Avoid attaching arbitrary bonuses or scripted progression to labels if the effects can emerge from actual capabilities and circumstances.

**Design test:** Can an observer explain a surprising emergent result using a small number of environmental, social, knowledge, and decision interactions? If not, simplify or improve diagnostics before adding complexity.

## 2. Player Direction and Civilization Autonomy

**Status: Direct / Guided / Emergent modes approved (November 11, 2026); detailed AI and interface algorithms not yet finalized.**

Evolvera supports three ways for the player to relate to their civilization. These are *control policies over a common simulation*, not different physics or progression systems.

| Mode | Player's role | Civilization's role | Key rule |
|---|---|---|---|
| **Direct** | Issues specific directives | Attempts to carry out directives under real constraints | An order does not create missing resources, expertise, logistics, or physical possibility |
| **Guided** | Sets priorities, strategies, or development emphasis | Selects approaches and timing, adapts to circumstances, and may delay or resist | Steering is meaningful but does not guarantee obedience or outcomes |
| **Emergent** | Communicates broad aims or desired outcomes | Determines its own actions in response to values, needs, pressures, and opportunities | The society is an actor, not merely a puppet of the player's commands |

### 2.1 Resistance is not inability

**Status: Approved distinction.** Always analyze separately:

1. **Capability / feasibility:** Can the proposed action be accomplished given actual knowledge, materials, labor, geography, institutions, and constraints?
2. **Willingness / acceptance:** Do the relevant population, institutions, settlements, or leaders intend to comply, support, adapt, obstruct, or refuse?

A society may *want* something it cannot yet do, or *be able* to do something it opposes. These lead to different responses: discovery/investment/alternatives versus persuasion, institutional change, compromise, conflict, or defection. A Direct-mode command can compel an attempt or prioritize effort but must not bypass feasibility.

### 2.2 Proposed shared directive pipeline

**Status: Proposed mechanism, not an approved formula.**

`Player intent → evaluate capabilities → evaluate willingness/legitimacy → choose action or response → apply material and social consequences → record explanation`

Possible outcomes include execution, partial execution, delay, substitution, resistance, and failure. Proposed diagnostics should distinguish **why** an action was blocked from **who** opposed it. The weighting of these outcomes and any numeric resistance model remain open.

## 3. Settlement Culture, Loyalty, and National Identity

**Status: Accepted direction toward local culture and loyalty; composite NI weighting is unfinalized.**

National Identity (NI) must become more than a global number derived solely from aggregate domain scores. Societies may develop locally distinct outlooks and loyalties under unequal conditions, geography, experience, leadership, and player policy. Settlements should be able to share a civilization while disagreeing about its direction or their relationship to central authority.

### 3.1 Distinguish related concepts

- **Local cultural character:** Historically formed norms, priorities, practices, beliefs, and adaptations. Specific representation remains to be designed.
- **Loyalty:** A settlement's willingness to remain aligned with a polity or authority, separate from capacity to obey a particular instruction.
- **Legitimacy:** Perceived rightfulness or acceptability of leadership and institutions; an initial per-settlement `Legitimacy` value already exists in the prototype.
- **National Identity / cohesion:** Aggregate strength of shared belonging and integration, with risks of fracture where local divergence and loyalty conflict.

These should not automatically be collapsed into a single mechanic, but also should not multiply into independent variables without evidence that the distinction changes behavior (EVO-DESIGN-001).

### 3.2 National Identity calculation — historical and candidate

The **existing prototype formula** remains in the original NI section above. The supplied `Civilization.cs` additionally subtracts a population-weighted local warfare-versus-social hotspot penalty, and the civilization's domain values are population-weighted across settlements.

A **recent candidate** was discussed as a blend of legacy domain-based cohesion and settlement loyalty:

`NI_candidate = 0.40 × legacy_cohesion + 0.60 × aggregated_loyalty`

**Important:** The 40/60 weighting was *proposed but not finalized*. Do not replace the current implemented NI formula on this basis. Scale normalization, loyalty aggregation, changing affiliation, and treatment of cultural divergence remain unresolved.

### 3.3 Intended emergent outcomes

A civilization could have high production or military capacity and still experience political fragmentation; a low-capability settlement may remain strongly loyal; shared institutions may compete with local identity; orders may face uneven compliance across settlements. These are desired consequences to model, **not assertions that the existing source already simulates them**.

## 4. Emergent Knowledge and Discovery — EKS-001

**Status: No universal technology tree is an approved constraint. The engine decomposition below is a proposed design (November 13, 2026), not implemented.**

Civilizations must not reproduce an inevitable Earth-history sequence of technologies or innovations. What can be discovered depends on the world's actual properties, available materials and phenomena, population and institutions, prior knowledge, culture, opportunities, experimentation, need, and chance.

The same world can support materially different trajectories; different worlds may not offer the same resources or paths at all. No civilization should receive an invention simply because a predetermined era threshold or a domain score was reached.

### 4.1 Proposed engines

- **Possibility engine:** Determines which phenomena, principles, materials, and physical/biological opportunities actually exist within the generated world and its scientific rules. Not every possible invention is available in every history.
- **Discovery engine:** Determines which possibilities a society encounters, investigates, infers, or learns from another society; includes uncertainty and serendipity.
- **Application engine:** Converts knowledge into practicable techniques, institutions, products, methods, or deployment, subject to costs, skills, materials, organization, and risk.

These are **conceptual responsibilities**, not necessarily three separate code classes or services.

### 4.2 Knowledge states and capability

**Status: Proposed representation.** Rather than a binary unlocked/locked technology flag, distinguish awareness/evidence, partial understanding, tested understanding, practical use, and mature or widespread application where those distinctions affect outcomes. The conversation also considered breadth of knowledge, depth of understanding, and practical capability as separate dimensions; whether all need stored numeric fields remains undecided.

### 4.3 World-specific science profiles

**Status: Proposed approach with approved internal-consistency constraint.** The generator can establish a coherent profile of actual world conditions, constraints, and discoverable phenomena. This supports divergent histories without arbitrary magic or a hidden conventional technology tree. Scientific variation must honor the constraints in Section 6.

## 5. Guided Knowledge Development — EKS-002

**Status: Accepted player-steering direction; mechanics proposed (November 13, 2026).**

Research, experimentation, knowledge exchange, and technological application must remain emergent **even when a player prioritizes them**.

- The player can emphasize goals, allocate attention or resources, support relevant institutions, and encourage investigation.
- Society can organize effort differently depending on its autonomy policy, institutions, culture, and constraints.
- Priorities alter incentives and opportunities; **they do not guarantee a named discovery** or bypass the world's available scientific possibilities.
- Unexpected discoveries, negative results, cross-domain spillovers, external knowledge, and unintended consequences remain possible.
- A civilization can discover knowledge without immediately being able—or willing—to apply it.

**Open:** Specific budgets, knowledge-diffusion algorithms, institutional models, and how centralized/guided/independent research autonomy maps onto the broader Direct/Guided/Emergent control modes. The labels describe closely related but not yet formally unified policies.

## 6. Scientific Plausibility and Progression — SPE-001

**Status: Scientific boundaries and tradeoff requirement accepted (November 13, 2026); detailed scoring/modeling unresolved.**

Evolvera should present **plausible, non-fantasy, non-sci-fi civilization development** on a world compatible with human life. It may include speculative but **internally consistent** science and plausible alternatives to Earth's specific history. Known physical constraints and conservation of energy/matter still apply. Do not solve obstacles by unexplained violations of the world's rules.

### 6.1 Human-compatible world envelope

The simulated environment must support the game's human starting premise. World variation should lead to coherent alternative conditions, not unconstrained exotic planets. The setting is an alternate Earth-like world with established physical rules, not a license for supernatural resources or arbitrary technologies.

### 6.2 Plausibility layers (working framing)

The discussion framed opportunities in layers such as **plausible**, **extrapolative**, and **frontier/speculative**, with requirements for internally consistent mechanisms and supporting evidence. **No thresholds or exhaustive definitions were finalized.** These labels should not be treated as a hardcoded technology progression sequence.

### 6.3 Separate finding, using, and deploying

A discovery does not imply immediate usable engineering; a working technique does not imply sustainable, safe, or broadly deployed practice. Applications can fail for reasons of resources, energy, environmental damage, institutions, cost, operational skill, or social refusal.

### 6.4 Achievement, risk, and restraint

Progression should be rewarding and create a sense of accomplishment, while preserving meaningful risk and agency. **“Just because you can, doesn't mean you should.”** Novel capabilities may involve sacrifice, unintended harm, or durable changes to social and ecological conditions. The design should not imply every newly available application is optimal or desirable.

## 7. Procedural Planetary Foundation — PGF-001

**Status: Player configuration requirements accepted (November 13, 2026); generation algorithms not finalized or implemented.**

World generation should create a **causally coherent planetary history** that yields the current geology, landscape, resources, biosphere, opportunities, and hazards. It should not be a collection of independent random terrain features.

### 7.1 Player-facing configuration

The initial world-setup design exposes configurable parameters for:

- **Planetary/world age**
- **Landmass / continental configuration**
- **Geology / geological character**
- **Extinction history**, potentially **multiple earlier extinction events**
- **Plausibility selection**, within the agreed scientifically coherent, human-compatible envelope
- **Randomize** individual or all exposed parameters

The exact UI choices, numeric ranges, geological presets, and meaning of the plausibility setting have not been finalized.

### 7.2 Fixed constraints

- The world is the **third planet** in its system.
- It has **one moon**.
- The starting situation must be **human-compatible**.
- The generator operates under established physical constraints and the accepted speculative-but-consistent science policy.

### 7.3 Derived rather than freely selected

Gravity and atmospheric pressure are **not currently intended as ordinary exposed setup sliders**. They should emerge coherently from planetary age and generative history/conditions. This is a design requirement about presentation and causal generation, **not** a finalized physical formula saying age alone determines gravity or pressure.

### 7.4 Causal generation sequence — conceptual proposal

`Player configuration + world seed → coherent planetary/geologic history → environment and physical state → ecology and surviving biosphere → resource/opportunity/risk landscape → civilization starting conditions`

The actual dependency graph and simulation fidelity are open. Extinctions should affect which organisms survive and how environments change; multiple events may leave legacies that change later possibilities. They **do not guarantee the appearance of specific inventions or cultures**.

### 7.5 Reproducibility

**Proposed technical requirement:** Save the generation seed, user-selected configuration, and generator version alongside world data so the generated world can be reproduced and migrations/debugging can be reasoned about.

## 8. How the New Systems Fit Together

**Status: Architectural synthesis, not an adopted implementation contract.**

The new decisions are intended to interact through a small set of explainable causal relationships:

1. **Planetary history** determines available terrain, ecological pressures, materials, hazards, and viable opportunities.
2. **Civilization and settlement conditions** determine needs, capability, practices, local culture, legitimacy, and loyalty.
3. **Player direction and civilization autonomy** influence what is attempted, prioritized, rejected, or delayed.
4. **Discovery** depends on actual possibility, experience, institutional effort, and chance; **application** depends on implementation capacity and consequences.
5. **Actions and events** change resources, population, domain progress, social relationships, and the next decision context.
6. **Cultural divergence and loyalty** affect the strength of shared identity, compliance, stability, and possible fragmentation.
7. **Milestones and age descriptions** reflect achieved patterns; they must not substitute for the underlying cause-and-effect process.

**Illustrative example (not a scripted feature):** A mineral-rich region favors experimentation with new materials. A locally distinctive settlement develops practical expertise but resists a central directive to redirect its production. Its refusal must be evaluated separately from its technical capability. Continued local advantage and different priorities may change its relationship to central authority, while environmental depletion and safety costs alter whether new applications remain attractive.

## 9. Architecture and Development Workflow

**Status: Recommended architectural direction, not proof of current repository layout.**

- Separate the **simulation engine**, **presentation/input**, and **persistence/diagnostics** so emergent rules can be tested without depending on a graphical interface.
- Keep generated world state, player-selected setup, random seed, and simulation version inspectable.
- Support explanation logs for causal decisions, refusals, discovery, adverse effects, and divergence.
- Keep **approved design**, **proposed mechanics**, and **implemented code** visibly distinct in documentation.
- Record changes to mechanics in version-controlled Markdown documents; use `CLAUDE.md` as an index and working instructions, **not** the sole store of design knowledge.
- Favor regression scenarios and seeded runs that compare different control modes, world histories, and civilization responses.

Suggested future reference documents: `docs/MASTER_DESIGN.md`, `docs/DECISIONS.md`, `docs/MECHANICS.md`, `docs/ARCHITECTURE.md`, and `docs/ROADMAP.md`. Creating those files is an organizational suggestion; this update makes no claim that they already exist.

## 10. Snapshot of Implemented vs Proposed Features

Based on the **uploaded C# snapshot**, not a live build or GitHub inspection:

| Area | Evidence in supplied code | Recent design status |
|---|---|---|
| Six knowledge domains and diminishing returns | `Domains.cs`, `Actions.cs` | Implemented prototype |
| Settlement population, food, legitimacy | `Settlement.cs`, `Population.cs` | Implemented simplified model |
| Single civilization, settlement expansion | `Program.cs`, `Expansion.cs` | Implemented simplified model |
| National Identity | `Civilization.cs` | Existing formula; loyalty-composite candidate unapproved |
| Terrain, rivers and sparse resources | `MapGen.cs`, `Map.cs`, `Resources.cs` | Prototype map generation; PGF-001 planetary history unimplemented |
| Environmental events | `EnvEvents.cs` | Implemented limited event table |
| Unit movement and harvesting | `Movement.cs`, `ResourceUse.cs`, `Units.cs` | Implemented basic actions |
| Direct / Guided / Emergent modes | No mechanism identified in uploaded files | Approved design, not implemented |
| Local culture and loyalty model | No mechanism identified in uploaded files | Design direction, incomplete |
| Possibility / Discovery / Application engines | No mechanism identified in uploaded files | EKS-001 proposal, not implemented |
| Research priorities / autonomous institutions | No mechanism identified in uploaded files | EKS-002 design direction, not implemented |
| SPE scientific classification and tradeoffs | No comprehensive mechanism identified | Design constraints, not implemented |
| Planet age, landmass, extinction-history setup | No mechanism identified in uploaded files | PGF-001 design direction, not implemented |
| Fully dynamic Ages and multi-civilization interactions | Not established in supplied snapshot | Earlier design objectives; implementation not verified |

### 10.1 Historical discrepancies that should remain visible

- The original event section says **about 30% per turn**, while supplied `EnvEvents.cs` uses `if (rng.NextDouble() > 0.22) return null;` — approximately **22%** chance of an event, conditional on a settlement. This update does **not** silently choose one value as authoritative.
- The historical design describes event XP multipliers; the supplied event method applies domain XP changes, but does **not** show those multipliers.
- The original master text states +5/+15 milestone rewards, but the supplied `Program.cs` has no observed milestone unlock handler. The intended rule remains on record; implementation is **unverified**.
- The older dev summary references `saves/demo.json`, while the supplied `Program.cs` references `saves/phase3_demo.json`; it also depends on a `SaveGame` implementation not included in the uploaded snapshot. Current compilation and persistence behavior are **not verified**.
- The supplied `Program.cs` uses a random two-action XP mechanism per settlement as well as separate manual unit commands. This is the existing prototype interaction, **not** the finalized Direct/Guided/Emergent player-control design.
- The code's existing NI formula includes a local hotspot penalty not reproduced in the earlier design text. The new loyalty-blended candidate is a further proposal, not a completed replacement.

## 11. Open Design Decisions and Verification Questions

The following must **not** be quietly filled in by an AI assistant or developer:

1. **Autonomy:** What rules, authority boundaries, and costs govern disobedience and partial compliance in each player mode?
2. **Culture and loyalty:** What minimum representation produces meaningful settlement diversity without violating EVO-DESIGN-001? What exactly is the loyalty-to-NI aggregation method?
3. **NI:** Whether to adopt the tentative 40/60 cohesion/loyalty model, change weights, or use a different mechanism altogether.
4. **Discovery:** What is stored versus derived for evidence, breadth, depth, practical capability, and diffusion? What determines discoverability without constructing a disguised tech tree?
5. **Research steering:** How should resource allocation, institutions, autonomy, experimentation and unexpected discovery work? How should specialized research autonomy relate to overall player-control modes?
6. **Science:** How should plausible, extrapolative and speculative outcomes be evaluated without arbitrary labels or fantasy mechanics? How should risk, deployment, sustainability, and restraint affect play?
7. **Planet setup:** What valid ranges/presets should be offered for age, landmasses, geology, extinction events, and plausibility?
8. **Planet physics:** How will mass/gravity, atmosphere/pressure, biosphere history, and human-compatible conditions be generated coherently without excessive simulation complexity?
9. **Origins:** How do humans come to exist within alternative planetary and extinction histories? This was identified as unresolved, not specified.
10. **Ages:** How are meaningful eras recognized and named from actual patterns without predetermined sequence or purely cosmetic milestones?
11. **Architecture:** What existing repository files, tests, and build dependencies must be validated before claiming these systems can be integrated?
12. **Prototype drift:** Which historical numeric formulas remain provisional, especially event probability, milestones, domain-XP event treatment, and NI?

## 12. Recommended Implementation/Validation Sequence

**Recommendation, not an approved roadmap:**

1. Preserve and version this design update, then audit it against the latest local Git checkout.
2. Add short, traceable design-decision records for EVO-DESIGN-001, EKS-001, EKS-002, SPE-001 and PGF-001, recording statuses and open questions.
3. Verify baseline build, tests, simulation logs, save/load, and seeded reproducibility before substantial changes.
4. Add diagnostics for **capability vs willingness**, then prototype the three player modes on the same simulation.
5. Prototype minimal settlement cultural difference/loyalty, and evaluate whether it produces meaningful emergent behavior before expanding state.
6. Model knowledge possibility, discovery, and application through small controlled scenarios without a conventional technology tree.
7. Prototype causally linked planetary setup parameters with reproducible generation before increasing world complexity.
8. Validate interactions with comparative seeded multi-settlement / multi-civilization scenarios and document surprising outcomes.

**End of November 11–13, 2026 design update.**
