# pivot-art changelog

## pivot-art: import MetroCity art and add pixel import postprocessor
Date: 2026-11-09
Files: Assets/_Project/Art/ThirdParty/MetroCity/** (29 PNGs + CREDITS.md), Assets/_Project/Editor/Import/{ArtPostprocessor,SlicePlanner,SlicePlan,SpriteSheetBuilder}.cs, docs/changelogs/pivot-art.md
Why: chaos_redesign.md pivot steps 4 and 5. Art extracted from art-src/MetroCity/*.rar (originals untouched; "MetroCity 2.0 4.rar" ignored as a duplicate). Native pixel grid is 32 px, so the project PPU is 32.
Tests: none run (Unity could not be launched). Slice rules are the pure class SlicePlanner; no EditMode test added yet.

### Import settings (all PNGs under Assets/_Project/Art/)
Sprite, PPU 32, Point filter, uncompressed, no mipmaps, clamp, max size 4096 (Building.png is 4096x768).
Re-import an asset to re-apply. The postprocessor lives in the existing UncannyValet.Editor assembly.

### Verified source sizes
Tiles.png 400x288 (irregular atlas, not a clean 32 grid), Road.png 320x320 (10x10 of 32), Tree-Sheet.png 192x64 (4 trees, 48x64 each),
Cars-Sheet.png 1152x512 (9x4 of 128), FireTruck.png 640x160 (4 of 160x160), Character Model.png 768x192 (24x6 of 32),
Hairs.png 768x256 (24x8), Outfit1-6.png 768x32 (24x1), Suit.png 768x128, Suit1.png and Pack20/Hair.png 768x160 (24 columns, 4-5 rows),
Hair1-7.png and Shadow.png 32x32 (single), Environment/Buildings/Building.png 4096x768, Stations/ PNGs single images.

### Sprite names (deterministic)
- Grid sheets: `{file}_r{row}_c{col}`, row 0 = TOP row, col 0 = left. Example `Cars-Sheet_r1_c5`, `Character Model_r0_c3`. Pivot centre.
- Auto-sliced sheets (Tiles.png, Buildings/Building.png): `{file}_{index:00}`, ordered top to bottom (64 px bands) then left to right. Pivot bottom centre.
- Single images (Hair1-7, Shadow, Outfits are grids, Stations/*): sprite name = file name. Stations pivot bottom centre.

### Cars (Art/ThirdParty/MetroCity/Cars)
Cars-Sheet: 9 columns x 4 rows, 128 px cells, each cell is one view of one vehicle (colour variants).
By inspection (a colour tint is baked into the outline of each variant):
- Top-down (tall, use for gameplay): r0 c0,c1,c2(ambulance),c5,c6; r1 c0,c1,c5(green),c6(police); r2 c0,c1,c4,c5,c6(ambulance); r3 c0,c1(police),c4,c5.
- Side/front views (wide, for menus or garage art): all remaining cells; ambulance side is r1 c4 and r3 c8.
Top-down sprites appear to face up (nose toward the top of the cell); the lead should confirm in Unity and set the car rotation offset once.
FireTruck.png: 4 cells of 160: c0 side, c1 top-down, c2 side (mirrored), c3 top-down (alternate colouring).
No damage frames exist; use tint, smoke and shake.

### Characters (Art/ThirdParty/MetroCity/Characters)
All sheets are 32 px cells, 24 columns. Character Model has 6 rows = 6 skin/body variants (row 0 top). Hair, Outfit and Suit sheets use the same column layout so layers can be drawn at the same position:
- cols 0-5: facing down (front), 6 frames
- cols 6-11: side view, first direction
- cols 12-17: facing up (back), 6 frames
- cols 18-23: side view, other direction
Each 6-frame group is a run cycle; frame 0 of a group can serve as the idle pose. Which side group is left or right, and the exact idle frame, were judged from a downscaled preview and must be confirmed in Unity (flip with SpriteRenderer.flipX if both sides look the same).
Hairs.png: 8 hair styles/colours as rows (each row = same 24 columns). Hair is only animated facing forward per the pack notes. Outfits: Outfit1-6, one row each. Suits/Suit.png, Suit1.png and Hair/Pack20/Hair.png are the older 2.0 layers.
Layer by stacking SpriteRenderers (model, outfit, hair) and switching all three to the same r/c frame.

### Environment
- Tiles.png (auto-sliced): grass patches, cobbles, fences, signs, props.
- Road/Road.png (32 px grid): road, crosswalks and kerb pieces; sliced as r{row}_c{col}.
- Trees/Tree-Sheet.png: `Tree-Sheet_r0_c0..c3`, 48x64.
- Buildings/Building.png (auto): 9 buildings with baked shadows. Any may be used for the diner; the tan building with green roof (second row, second from left) is a good fit.
- Stations/: Building.png (176x320 tower house), FireStation.png, PoliceStation.png as single sprites.
