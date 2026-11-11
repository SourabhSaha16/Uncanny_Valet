# Pending tasks (atomic)

Last updated 2026-11-10. Each task is one small, separately shippable change. Owner = the agent that does it; the lead merges.
Source of truth for design is `docs/design.md`; this file is only the to-do list. Tick or delete a line when it ships.

## A. Play-test verification (human, then report back)

Nothing below has been played end to end; only browser smoke tests were done.

- [ ] A1. Hard-refresh the live site and confirm the latest deploy (`1e5ac34`) is what loads.
- [ ] A2. Patrons face the VALET PICKUP spot while waiting at the curb (every patron, every spawn side).
- [ ] A3. CAR SLOTS status words switch at the right moments (Parked, Washing, Washed, Charging, Charged, Pick Up, Cust. Angry).
- [ ] A4. Plate shows only once the car is parked in a spot or station.
- [ ] A5. Patron mood bubble is legible and sits well over the patron (green, yellow, red).
- [ ] A6. Plate flash speeds read as slow, faster, very fast (1.5, 3, 7 Hz).
- [ ] A7. Exit car clears the crosswalk and the waiting patrons.
- [ ] A8. Patron car entering the queue column and leaving by the curb route looks right at default traffic numbers.
- [ ] A9. Pedestrian count (F2) starts at a comfortable level.
- [ ] A10. Report anything that feels wrong in a play-test (design checkpoint 2).

## B. Known bugs and gaps from the last rounds

- [ ] B1. Cars can drive into the gap west of the pickup zone. Owner: lot. Add a blocker or fence; regression test.
- [ ] B2. Patron car is invisible to ambient traffic after it turns into the queue route. Owner: traffic.
- [ ] B3. A ticket closing mid-entry falls back to a straight drive-out. Owner: traffic. Make it finish or abort the trip cleanly.
- [ ] B4. Car body tint pulse still uses the old cyan, amber and orange-red, not the new green, yellow, red. Decision first (B4a), then build (B4b).
  - [ ] B4a. Human decides: match the new emotion colours, or turn the pulse off.
  - [ ] B4b. Owner: cars (plus a Core emotion rule or mirrored config so cars need not reference UI).
- [ ] B5. Angry state starts at 85% patience. Human decides: keep, or start only at expiry. Owner: ui-status if changed.
- [ ] B6. Camera crops the road and the key board panel covers it. Decide camera zoom versus showing the road. Owner: integration.
- [ ] B7. Hedge-green walls versus the asphalt surround: decide the colour. Owner: lot.
- [ ] B8. Diner reads as a flat red roof plus a front strip; refine the building art. Owner: lot.
- [ ] B9. "Cust. Angry" does not fit the status column and shows as "Angry"; widen the column or shorten the label. Owner: integration (UI).

## C. Patron Director

- [ ] C1. Step 4: `ReturnScheduler` pure rule with EditMode tests (spec: `docs/redesign/patron_director_spec.md`).
- [ ] C2. Step 4: wire the scheduler into the Director shell and add its F2 keys to `TuningKey` and `TuningCatalog`.
- [ ] C3. Add the return-scheduler rows to the F3 overlay.
- [ ] C4. Extend the soak test to cover returns.

## D. Redesign backlog (planned in `docs/design.md` A8)

Car controller (spec: `docs/redesign/car_handling_spec.md`, build order in its section 8; one step at a time per file set):
- [x] D1. Step 1 (cars): measure the aisle width and car collider size, set R_min and MaxTurnRate, add the PlayMode test assembly and the bay-entry PlayMode test.
- [ ] D2. Step 2 (cars + Core input): `DriftModel`, `SteeringModel`, `IDriveModifier` seam, Drift input (Shift, right shoulder), `CarHandlingConfig` and its F2 keys.
- [ ] D3. Step 3 (cars/fx + audio): minimal `DecalPool`, skid marks, tyre chirp and squeal.
- [ ] D3a. Step 4 (lot): `ParkJudge`, `ParkJudgeRunner`, `PerfectPark` event, pop, ding, camera kick, +$2 on the ticket, results stat.
- [ ] D3b. Step 5 (valet): dash on foot, `BoostRules`, `ValetBoost`, barge-through, boost feedback, results stat.
- [ ] D3c. Step 6 (cars): EV stat overrides in `CarDefinition`.
- [x] D3d. Aisle 4.6 passes the pull-out rule; no widening needed.
- [ ] D3e. Human decision: keep the real 1 x 2 car (spec assumed about 1.8 x 4; bay slack 0.8 per side), or resize cars, sprites and the queue?
- [ ] D3f. Step 2 must keep `CarHandlingConfig.MeasuredAisleWidth` in sync with `LotConfig.AisleWidth` (cars cannot reference Lot) and enable the 7 ignored PlayMode bay cases.

Damage:
- [ ] D4. Impact damage rule (pure class, tests).
- [ ] D5. Screen shake on impact.
- [ ] D6. Smoke and debris effects.

Pedestrians:
- [ ] D7. Honk input and pedestrian reaction.
- [ ] D8. Tumble on contact.
- [ ] D9. Courtesy tips for careful driving near pedestrians.

Kev, the rookie AI valet:
- [ ] D10. Task board data and rules.
- [ ] D11. Kev movement and task execution.
- [ ] D12. Swap-valet button.

Later content:
- [ ] D13. Faults (overheating, on fire) as new `CarStatus` rules.
- [ ] D14. Mechanic bay.
- [ ] D15. Fire spread.
- [ ] D16. More venues.
- [ ] D17. ParkCorp showdowns.
- [ ] D18. World map.

## E. Content and polish

- [ ] E1. Title screen: add the valet, Kev and the key-tag dot on the i.
- [ ] E2. Replace the Mrs. Okafor placeholder box with art.
- [ ] E3. More car variety (only 14 civilian sprites exist; tint and mirroring cover it today).
- [ ] E4. Curb arrows and headlights are hard to see; tune visibility.

## F. Open design questions (human decides)

- [ ] F1. Keep or rename the title.
- [ ] F2. Single-valet hard mode: yes or no.
- [ ] F3. Cliff car rule.
- [ ] F4. Local co-op input.

## G. Housekeeping

- [ ] G8. Play-test the nine difficulty dials at extreme positions (Intensity 10, Mercy 0, Dirty or EV 0, Ramp 0); the anchors are first guesses and the soak test was not rerun at non-default dials.
- [ ] G9. Decide which rows stay visible without Advanced (now only the dials plus Game minutes per second and Shift length); Traffic and Pedestrians are Advanced-only.
- [ ] G10. Optional: mark derived versus hand-edited Advanced rows in the F2 panel.

- [ ] G3. Split `Tests/EditMode/UI/TicketBoardTests.cs` (314 lines, limit 200). Owner: integration.
- [ ] G4. Remove unused `TicketRow.ShowsInfoBox`, `ShowsDirtMeter`, `ShowsChargeMeter`, `ChargeShown`, `Dirt01` and their tests. Owner: integration.
- [ ] G5. Check `CarAutoDrive._finalRotation` after a queue retarget (stays the first slot's rotation). Owner: cars.
- [ ] G6. Find the real cause of the failed arrival spawn (pacer now retries); consider a "spawner blocked" hold reason in the F3 overlay.
- [ ] G7. Play-test: slot tint colours, droplet/plug icon beside the plate, camera 0.78, pedestrian bounce feel and boing loudness, walkers vs cars and valet.

- [ ] G1. Optional: install the `mattpocock-skills` plugin (`claude plugin install mattpocock-skills@claude-plugins-official`), then decide which skills clash with `CLAUDE.md`.
- [ ] G2. Update `docs/status.md` with a short line pointing to this file.
