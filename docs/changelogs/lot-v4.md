# lot-v4 changelog

## lot-v4: drop cars returned from the results screen
Date: 2026-11-10
Files: Scripts/UI/{ShiftStats,GameUi}.cs, Scripts/UI/Screens/ResultsFormatter.cs, Tests/EditMode/UI/ResultsScreenTests.cs
Why: human request (design.md A5 results screen): the CARS RETURNED stat and the counter that only fed it are removed; the other stats stay.
Tests: ResultsScreenTests updated (three stat lines, ShiftStats without the returned counter). Run in the full EditMode pass at the end of the series.

## lot-v4: traffic defaults from the F2 tuning
Date: 2026-11-10
Files: Scripts/Traffic/{TrafficConfig,TrafficModule}.cs, Scripts/Bootstrap/TuningSeeder.cs, Data/Traffic/TrafficConfig.asset, Tests/EditMode/Integration/TuningSeederTests.cs
Why: human tuned traffic in F2 (design.md A5): ambient 13 cars per minute, ambient speed scale 1.25, entry car speed 7.3, exit car speed 5 (units per second, as the panel shows). New TrafficConfig fields EntrySpeed and ExitSpeed hold the start values; TuningSeeder seeds the keys from them and TrafficModule applies them at build. CruiseSpeed and RouteSpeed stay the scale-1 references (and still drive ambient cars), so the panel value is the real speed in units per second.
Tests: TuningSeederTests asserts the four defaults.
