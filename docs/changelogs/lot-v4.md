# lot-v4 changelog

## lot-v4: drop cars returned from the results screen
Date: 2026-11-10
Files: Scripts/UI/{ShiftStats,GameUi}.cs, Scripts/UI/Screens/ResultsFormatter.cs, Tests/EditMode/UI/ResultsScreenTests.cs
Why: human request (design.md A5 results screen): the CARS RETURNED stat and the counter that only fed it are removed; the other stats stay.
Tests: ResultsScreenTests updated (three stat lines, ShiftStats without the returned counter). Run in the full EditMode pass at the end of the series.
