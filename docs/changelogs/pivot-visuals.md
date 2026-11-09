# pivot-visuals changelog

## pivot-visuals: MetroCity art for the diner lot, cars and valet
Date: 2026-11-09
Files: Scripts/Lot/{LotArtConfig,DecorLayout,SpriteTiler,LotDecor,LotCurbArt,LotLayoutBuilder,SpotBuilder}.cs, Scripts/Cars/{CarSpriteEntry,CarArtConfig,CarArtMath,CarBodyArt,CarFactory,CarTraitLook}.cs, Scripts/Valet/{ValetFacing,ValetAnimMath,ValetArtConfig,ValetAnimator}.cs, Scripts/Bootstrap/GameBootstrap.cs, Editor/{BootstrapSceneBuilder}.cs, Editor/Art/{ArtSprites,ArtAssetUtil,LotArtBuilder,CarArtBuilder,ValetArtBuilder}.cs, Tests/EditMode/{Cars/CarArtMathTests,Valet/ValetAnimMathTests,Lot/DecorLayoutTests}.cs, docs/changelogs/pivot-visuals.md
Why: the human wants to see the imported MetroCity art (chaos_redesign.md pivot; sprite names from pivot-art.md). Unity could not be launched: nothing here was compiled or run.
Tests: three new EditMode fixtures (car art maths, valet animation maths, decor layout); not run.

### Lot (step 1)
- LotDecor (new, sibling of the Lot object, same origin, built at Awake) draws: grass, a south sidewalk, a 3 unit road with the dashed Road.png tile and code-drawn crosswalk bars (the Road.png crosswalk tiles only fit crossroads), a far sidewalk, a north sidewalk, the tan green-roof house as the diner, 8 trees (Tree-Sheet) and grass tufts. Placement is the pure DecorLayout around the largest lot frame (LotGeometry.Bounds).
- The lot logic is untouched. Visual changes: ground colour = asphalt sampled from Road.png, walls = hedge green, spots = painted white bays on the asphalt (pad takes the asphalt colour), curb = cobble tiles with a yellow edge and a dark plate behind CURB. Charging bays keep their blue paint.
- Camera: GameBootstrap widens the measured lot bounds vertically with LotDecor.ExtendView so the road and diner are in frame (zoom is capped at 16 by CameraFollow; beyond that it follows the valet).
- Sprites that are auto-sliced (Tiles, Buildings) are found by a pixel inside them (LotArtBuilder: cobble 264,40; house 1147,645; tufts), not by index.

### Cars (step 2)
- CarArtConfig holds 12 regular cells and 2 EV cells of Cars-Sheet (r0c0 r0c1 r0c5 r0c6 r1c1 r1c5 r2c0 r2c1 r2c4 r2c5 r3c0 r3c5; EV r1c0 r3c4). Each entry stores the opaque size and centre, measured by the editor builder. A car id picks its sprite by a stable hash.
- CarBodyArt (child "Body", order 10) scales the sprite uniformly so its opaque length equals the CarDefinition length, centred on the car. EVs swap to an EV sprite on TicketTraitsAssigned (no tint). The root rectangle is hidden, mirrors are skipped; mud, timer fill, charge bar and letter label are unchanged. Without a CarArtConfig the old rectangles still work.
- Per-sprite rotation offset: CarSpriteEntry rotation (0 or 180 only, because the car root is scaled non-uniformly). CarArtBuilder.RotationOffset (0) is applied to every entry.

### Valet (step 3)
- ValetAnimator stacks Model (skin row 0), Outfit1 and Hairs row 0 as three SpriteRenderers (orders 20 to 22), reads Rigidbody2D.linearVelocity and shows frame = floor(movingSeconds x 10 fps) mod 6 of the facing group, or frame 0 when still. Facing is the dominant velocity axis (ValetAnimMath). No allocations in Update.
- Groups: Down cols 0-5, Right 6-11, Up 12-17, Left 18-23; flip flags in ValetArtConfig. Valet scale is 1.4 (collider radius 0.3, about the old world size).
- GameBootstrap no longer paints the valet yellow when a ValetAnimator exists.

### Build
Setup > Build Everything now also creates Data/Lot/LotArtConfig, Data/Cars/CarArtConfig and Data/Valet/ValetArtConfig and wires them (factory, lot, decor, valet animator, bootstrap).
