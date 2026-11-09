# Pivot crowd changelog

Source: docs/redesign/ui_style_guide.md, "Revision 3" (binding).

## pivot-crowd: short plates, pickup bell, walking patrons, face-down delivery, headlights, closer zoom, audio mix
Date: 2026-11-09
Files:
- Patrons: PlateFormat (2 letters + space + 1 digit, 3610 plates), PlatePool (documented duplicate fallback when exhausted), new CurbCall (stay over: send the patron to the curb, request the pickup on PatronReachedCurb or after a fallback timeout), Ticket (Summoned/SummonedMinute), TicketRegistry (CurbWalkTimeoutMinutes, PatronReachedCurb), PatronDeparture (delivered car waits for PatronBoarded or a game-minute timeout, or ShiftEnded), PatronConfig (CurbWalkTimeoutMinutes 40, BoardingTimeoutMinutes 6), PatronSpawner.
- Core (additive): events PatronSummoned, PatronReachedCurb, PatronBoarded (TicketEvents), CurbFacingChanged (CarEvents); contracts ICurbFacing, IPatronLocator, IPedestrianPaths.
- Lot: new CurbFacingRule (pure), PedestrianPathMap (pure routes: road crosswalk, strip, north sidewalk, door, curb standing spots), CurbZone (facing condition and hint events), StopZone (Accepts/OnForget hooks), LotConfig (curb facing tolerance 35), WalkwayStyle (curb arrows, standing spots, door), LotWalkwayArt (public Arrow, ChevronDown), LotCurbArt (down arrows and chevrons on the curb), WalkwayLayout.StripX, LotDecor (Paths replaces WalkRoutes).
- Pedestrians: replaced the looping walkers with a pooled state machine: PedestrianPhase, PedestrianBrain, PathFollower, WalkerRouter, PatronWalkers, PedestrianCrowd, PedestrianWalker, PedestrianConfig (ambient flow and patron pool tunables). Removed RouteWalker and PedestrianPlan.RouteFor/StartFraction.
- Cars: new CarHeadlights, CarHeadlightMath, CarFlashConfig headlight tunables, CarSorting.Glow/Headlight, CarFactory.
- UI: PlateLayout/PlatePainter (narrow word gap, car-width plates), WorldTicketPainter (plate as wide as the car), KeyBoardPainter (bell and flashing highlight via new PickupMark, narrower panel), new CurbHintTracker and Hud/CurbHintPainter (PLEASE PARK IT PROPERLY), GameUi, NoticePainter (banner removed), TicketBoard (PickupCount and AppendPickups removed), PixelPalette (PanelCyan removed). Removed PickupBannerFormatter.
- Audio: AudioConfig defaults sfx 0.4, music 0.2 (asset updated).
- Bootstrap: CameraFit (pure), CameraFollow (fit tightness 0.87, play area never cropped), GameBootstrap (registers IPedestrianPaths and IPatronLocator, Begin()).
- Tests: added PlateTests (3610 plates, exhaustion), PlateUiTests, PickupMarkTests, CurbFacingRuleTests, PedestrianPathMapTests, CarHeadlightMathTests, PedestrianBrainTests, PathFollowerTests, PatronWalkTimingTests, CameraFitTests; removed PickupBannerTests, RouteWalkerTests and the old route-plan tests.
Why: Revision 3. Timing change: a patron is no longer "ready" the moment the stay ends. The stay (game minutes) still ends the same way, but it only summons the patron; the pickup (and its patience) starts when the patron stands at the curb, so pickups now come the walk time later (about 20 to 30 s real time at the default clock). Without a crowd the pickup is requested after CurbWalkTimeoutMinutes. Existing registry tests keep the old instant behaviour because the timeout defaults to 0 outside PatronSpawner.
Tests: NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files are not committed.
