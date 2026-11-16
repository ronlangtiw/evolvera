# Evolvera — Design Decision Log

**Consolidated:** 2026-11-16  
**Source:** Dedicated Player Direction and Civilization Autonomy design discussion.  
**Purpose:** Preserve confirmed decisions, distinguish proposals from commitments, and record replaced directions. IDs are documentation labels, not code identifiers.

## Status definitions

- **Confirmed:** Explicitly chosen or accepted by the user.
- **Design consequence:** Needed to make a confirmed rule internally consistent; implementation still to be worked out.
- **Proposed:** Discussed recommendation not explicitly adopted.
- **Deferred:** Recognized work intentionally left for later.
- **Superseded:** Earlier proposal replaced by a later decision.

## Confirmed decisions

| ID | Topic | Decision |
| --- | --- | --- |
| PD-01 | Three modes | Direct, Guided and Emergent are alternative **permanent player roles** chosen at game start. |
| PD-02 | Role coexistence | Government leadership, advisors and public influencers **all exist in every civilization**; unselected roles are AI-operated. |
| PD-03 | Direct identity | Player represents the government leader and issues commands; sufficient resources/knowledge do not guarantee obedience when serious cultural or political opposition exists. |
| PD-04 | Guided identity | Player represents a counselor/strategist advising a separately simulated leader. |
| PD-05 | Emergent identity | Player represents influential individuals or groups steering public opinion and collective aspirations, not a direct commander. |
| PD-06 | Representative turnover | Historical persons/groups representing the player change over the years; the player's selected role does not. |
| PD-07 | Contextual succession | Simulation determines successor eligibility and outcome; player preparation, relationships and authority may affect the transition. |
| PD-08 | Continuity through setbacks | Player always retains a role/perspective even with negligible effective authority after a crisis. Civilization history and consequences persist. |
| HAI-01 | Actor origins | Actors and institutions arise from civilization simulation; player cannot directly create/control their personalities or the population of loyal NPCs. |
| HAI-02 | Heterogeneity | Popular figures may encourage like-minded NPCs, but ideological uniformity is uncommon; strong leaders do not guarantee strong successors. |
| HAI-03 | Lightweight background system | Keep actors/institutions small, aggregated and simulated only enough to make succession and transitions interesting. |
| GP-01 | Map world | World map generated at game start, with terrain, resources, territory, units and structures. |
| GP-02 | Unit-centered actions | Units (individuals or groups) interact with map objects; players can select a unit then a target, or target then unit/action. |
| GP-03 | Shared interface | All three roles use the **same interface**, but the context-dependent **available actions/expressions differ**. |
| GP-04 | Intent examples | Direct: 'Harvest/mine resources'; Guided: 'We need more resource from this area'; Emergent: 'Let's build a walled city' as a societal vision, not a direct build order. |
| GP-05 | Actions and capability | Actions chiefly depend on people (**who**), process (**how**), and tools (**with what**), with resource/capacity constraints and improvements from experience/equipment. |
| GP-06 | Built environment | Temporary/permanent structures, settlements/towns/cities/capitals, internal facilities, monuments, housing and industry; actions can create or improve objects. |
| GP-07 | Research | Resources may be allocated to study, experimentation and discovering or improving practices; organic progress rather than rigid technology trees. |
| GP-08 | Mobile start | Starting population is mobile; decisions about camping and permanent settlement arise in play. |
| GP-09 | Discovery model | Mixed discovery: player-directed investigations and incidental/autonomous learning. |
| GP-10 | Capacity-based activity | Available people, resources, tools and capability constrain concurrent actions; no fixed action-point allowance per turn. |
| TM-01 | Base time unit | One day is the fundamental world-time unit. |
| TM-02 | Multi-day actions | Actions can take multiple days, with concurrent progress when capacity permits. |
| TM-03 | Single-player turn end | The next player-action completion sets the turn boundary, unless an interruption happens first. |
| TM-04 | AI time behavior | AI actions progress and complete in simulation but do not end a player's turn unless an interruption event occurs. |
| TM-05 | Multiplayer boundary | Earliest relevant human-player action completion across all players ends the shared turn, barring an earlier interruption. |
| TM-06 | Shared-turn options | Players whose activities are unfinished at another player's turn boundary may interrupt or continue their own work. |
| TM-07 | Estimates | Durations presented to players are **estimates**, growing more precise with expertise/familiarity; keep estimation mechanics simple. |
| MP-01 | Multi-civilization world | Multiple civilizations exist in **every** game, including single-player. |
| MP-02 | Starting civ count | World size determines starting civilization allocations; exact sizing formula is not set. |
| MP-03 | Player allocation | **One human player per civilization**, no shared human-controlled civilization. |
| MP-04 | AI civilization allocations | Every civilization without a human player is computer-controlled during the game. |
| MP-05 | Competition and cooperation | Human players compete via separate civilizations, while their civilizations may cooperate. |
| AI-01 | AI skill creation setting | Opponent AI skill is configurable at game creation. |
| AI-02 | Default and variation | Global AI skill default with optional variation among computer-controlled civilizations. |
| AI-03 | Behavioral diversity requirement | Equal-skill AI civilizations must not all behave identically; detailed play-style design deferred. |

## Design consequences to preserve when implementing

| ID | Consequence |
| --- | --- |
| DC-01 | Player role and current representative are different entities; succession changes representatives, not the selected mode. |
| DC-02 | One shared world clock and one simulation state are needed for synchronized multiplayer. |
| DC-03 | The turn boundary is determined by **actual** completion of human action(s); the estimate is a provisional display, not guaranteed completion time. |
| DC-04 | All activities progress during elapsed time. Unfinished actions retain progress and available capacity is not double-spent. |
| DC-05 | AI-controlled government, advisers and influencers continue operating inside each human-associated civilization. |
| DC-06 | Information and actions are filtered through the selected role, while the same world objects and physical rules exist underneath. |

## Proposed implementation details (not confirmed)

| ID | Proposal |
| --- | --- |
| PR-01 | Model finite activity stages as proposed/evaluated/committed/executing/resolved, with persistent reservations. |
| PR-02 | Distinguish routine, significant and critical events; only interrupt for meaningful decisions or major transitions. |
| PR-03 | Use compact authority/legitimacy, advisor credibility and influencer reach measures to govern responses. |
| PR-04 | Use small pools of historically significant NPCs, summarized institutions and event-driven updates rather than individual life simulation. |
| PR-05 | Retain civilization-level familiarity by broad action class; show a bounded-error duration estimate that narrows with practice. Exact error rates not decided. |
| PR-06 | Use a fallback review/advance mechanism when no human finite action is scheduled. |
| PR-07 | Single-player core can use the same intent model as server-authoritative multiplayer. |
| PR-08 | Add manageable initial actions: move, explore, gather/hunt, build, experiment, organize, interact; specific menu taxonomy is not finalized. |
| PR-09 | Starting tribe example of 36 people / 24 available worker equivalents / 12 food units; labor example 12 food, 6 shelter/materials, 6 exploration. Values are illustrative. |
| PR-10 | AI skill should emphasize decision quality without hidden economic bonuses; final difficulty tiers, weighting and overrides remain open. |

## Deferred design work

| ID | Topic | What remains |
| --- | --- | --- |
| DF-01 | AI behavioral diversity | How distinct strategies arise among 7+ equal-skill AI civilizations; parameterization and tests. |
| DF-02 | Autonomy reaction algorithms | Specific command refusal, persuasion, influence spread, legitimacy and rebellion calculations. |
| DF-03 | Early action balance | Actual action durations, capacities, productivity, experience gains, tool effects and environmental risks. |
| DF-04 | Event mechanics | Detection/reporting, interruption priority, response deadlines, tied simultaneous events and persistence. |
| DF-05 | Time edge cases | No finite player action; estimated versus actual completion; repeating operations; multiplayer deliberate one-day stalling. |
| DF-06 | AI configuration | Global difficulty variation weights, per-civilization overrides, stable versus adaptive skill. |
| DF-07 | World allocation | Map scale, placement, viable territory and population density formula for starting civilization count. |
| DF-08 | Win conditions | Competitive end conditions vs long-term historical development. |
| DF-09 | Multiplayer operations | Ready-up, disconnects, asynchronous play, fog of war, authoritative networking and save/resume. |
| DF-10 | Historical actors | Minimal data structure, event generation and continuity; keep deliberately lightweight. |

## Superseded proposals / historical clarifications

| Earlier idea | Current direction |
| --- | --- |
| Direct Mode automatically obeys every mechanically valid command | Direct commands can face refusal, organized opposition, rebellion and political consequences. |
| Each mode has only its corresponding social role in the civilization | All three roles exist in all civilizations; the player selects one, AI simulates the others. |
| Player role might switch after political changes | Mode/role is fixed for the complete game; historical representative changes. |
| Guaranteed continuity means regaining power quickly | Participation remains available even if authority or reach nearly vanishes. |
| Historical actors require rich individual life/family simulation | Lightweight aggregated actors and institutions only. |
| Fixed three player directions per turn | No fixed action quota; capacity, work and tools determine feasible concurrent activities. |
| Player chooses adaptive 3-, 7-, 30-day turn interval | Time advances to next actual **human action** completion or qualifying interrupt. Base unit day. |
| Estimated duration is the turn's guaranteed length | Estimates improve through familiarity but can be wrong; actual completion governs scheduling. |
| Shared-civilization human multiplayer | Exactly one human player allocated per civilization. |
| A single-player scenario might contain only the player's civilization | Every game begins with multiple civilizations; unused allocations are AI-run. |
| Prototype uses two random settlement actions each turn | Prototype only; intended design uses map objects/units and capacity-based multi-day actions. |

## Existing implementation versus desired design

The uploaded project includes a C#/.NET 8 console prototype with map/terrain/resources, unit movement, settlements, domain calculations, events and simple expansion. It does **not** yet implement fixed-role Direct/Guided/Emergent play, contextual succession, action-duration estimates, action-based shared turn boundaries, authoritative multiplayer or multiple fully autonomous AI civilizations. Treat this register as **design documentation**, not evidence that features have been coded.

## Next design focus

Define a first playable catalog of unit/world-object actions: required people, available processes, equipment, resources, work duration estimation, outcomes, capacity requirements and event interrupt criteria. Then map the same world action to Direct commands, Guided recommendations and Emergent aspirations using one interface.
