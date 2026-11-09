# Pivot UI changelog

Entries are added by the pivot-ui agent, one per commit. Format in docs/changelogs/README.md.
Source: docs/redesign/ui_style_guide.md (Scenes 10, 11, 13) and chaos_redesign.md sections 3 and 7.

## pivot-ui: pixel UI kit, HUD, world tickets, results and title screens
Date: 2026-11-09
Files:
- Added Scripts/UI/Kit/{PixelGrid,PixelRect,PixelGlyphs,PixelTextLayout,PixelBitmap,PixelIcon,PixelIconData,PixelPalette,PixelKit,PixelCanvas,Blink,SmallNumbers,CachedText}.cs; Scripts/UI/Hud/{ClockText,AlarmLevel,Alarm,AlarmColors,StarBarLayout,ComboWobble,TicketRowOrder,WorldTicketLayout,HudPainter,KeyBoardPainter,NoticePainter,WorldTicketPainter}.cs; Scripts/UI/Screens/{UiPhase,StatLine,StarStamp,ResultsPainter,TitlePainter}.cs; Scripts/UI/{GameUi,ShiftStats}.cs.
- Moved and rewritten: Screens/ResultsContent.cs, Screens/ResultsFormatter.cs. Removed: DebugOverlay, ResultsScreen, ResultsStyles, ScoreLines, TicketRowFormatter (and the old formatter tests).
- Changed: UI/TicketBoard (CollectRows sorted by urgency, Version; AppendLines removed), UI/TicketRow (NeedsWash, NeedsCharge), Bootstrap/GameBootstrap (one GameUi replaces DebugOverlay and ResultsScreen; the first shift starts when the title is dismissed; the valet is input-gated while the title shows), Editor/BootstrapSceneBuilder (builds and wires a GameUi child).
- Tests: Tests/EditMode/UI/{PixelKitTests,HudRulesTests,ResultsScreenTests}.cs added; TicketBoardTests rewritten over rows; ResultsFormatterTests and TicketRowFormatterTests removed.
Why: ui_style_guide.md. The whole UI is drawn on a virtual 480x270 canvas scaled by the largest whole number that fits (centred, nearest-neighbour) from a generated 5x7 pixel font, a nine-slice panel and 9 code-drawn icons, all cached Point-filtered textures. HUD per Scene 10 (clock top centre, tips with coin and three-star bar top left, combo badge with wobble top right, key board bottom left, cyan pickup banner with bell, floating car tickets with patience bars). Results per Scene 11 (stamped stars with a burst on the third, stats, Mrs. Okafor placeholder box, Retry, disabled Next Lot marked Soon). Title per Scene 13 (blinking PRESS ANY KEY; the clock starts only after the key press).
Tests: added EditMode tests for the grid scale, glyph and icon tables, text layout, clock text, alarm levels, bar fill, star bar, combo wobble, ticket ordering, world ticket layout, star stamp timing, results text and shift stats. NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified.
