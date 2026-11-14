using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The mud patch overlap rule, the world-space field, the pixel look and the builder that registers the patches.</summary>
    public sealed class MudPatchTests
    {
        [TearDown]
        public void TearDown() => Services.Reset();

        [Test]
        public void Patch_InsideEdgeOutside()
        {
            var patch = new MudPatch(new Vector2(2f, 1f), 3f);
            Assert.IsTrue(patch.Contains(new Vector2(2f, 1f)));
            Assert.IsTrue(patch.Contains(new Vector2(3.4f, 1f)));
            Assert.IsTrue(patch.Contains(new Vector2(3.5f, 1f)), "the rim counts as inside");
            Assert.IsFalse(patch.Contains(new Vector2(3.6f, 1f)));
            Assert.IsFalse(patch.Contains(new Vector2(3.2f, 2.2f)), "corner of the square is outside the round puddle");
        }

        [Test]
        public void Field_MovesPatchesToWorldSpace_AndFindsAnyOfThem()
        {
            var field = new MudPatchField(new[] { new MudPatch(Vector2.zero, 2f), new MudPatch(new Vector2(10f, 0f), 2f) }, new Vector2(100f, 50f));
            Assert.AreEqual(2, field.Count);
            Assert.IsTrue(field.IsInside(new Vector2(100f, 50f)));
            Assert.IsTrue(field.IsInside(new Vector2(110f, 50.5f)));
            Assert.IsFalse(field.IsInside(Vector2.zero), "lot space is not world space");
            Assert.IsFalse(field.IsInside(new Vector2(105f, 50f)));
        }

        [Test]
        public void Pixels_CentreIsDark_CornerIsEmpty_AndEveryVariantHasARimAndARipple()
        {
            const int n = MudBlobArt.Pixels;
            for (var variant = 0; variant < MudBlobPixels.Variants; variant++)
            {
                Assert.AreEqual(MudTone.None, MudBlobPixels.Sample(0, 0, n, variant));
                var rim = 0;
                var ripple = 0;
                for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    var tone = MudBlobPixels.Sample(x, y, n, variant);
                    if (tone == MudTone.Rim) rim++;
                    if (tone == MudTone.Ripple) ripple++;
                }
                Assert.Greater(rim, 0, "variant " + variant);
                Assert.Greater(ripple, 0, "variant " + variant);
            }
        }

        [Test]
        public void Art_CachesOneSpriteForEachVariant_AndItIsOneUnitWide()
        {
            var art = new MudBlobArt();
            var first = art.Get(0);
            Assert.AreSame(first, art.Get(0));
            Assert.AreSame(first, art.Get(MudBlobPixels.Variants), "variants wrap around");
            Assert.AreNotSame(first, art.Get(1));
            Assert.AreEqual(1f, first.bounds.size.x, 1e-4f);
            Assert.AreEqual(FilterMode.Point, first.texture.filterMode);
        }

        [Test]
        public void Builder_BuildsOneSpritePerPatchUnderTheLot_AndRegistersTheField()
        {
            var lot = new GameObject("Lot");
            lot.transform.position = new Vector3(5f, 0f, 0f);
            try
            {
                var field = MudPatchBuilder.Build(lot.transform, new[] { new MudPatch(new Vector2(1f, 1f), 3f), new MudPatch(new Vector2(-4f, 2f), 2.5f) });
                Assert.IsNotNull(field);
                Assert.AreEqual(2, lot.transform.Find("Mud Patches").childCount);
                Assert.AreEqual(2, lot.GetComponentsInChildren<SpriteRenderer>().Length);
                Assert.AreEqual(3f, lot.transform.Find("Mud Patches/Mud Patch 0").localScale.x);
                Assert.IsTrue(Services.TryGet<IMudPatches>(out var registered));
                Assert.IsTrue(registered.IsInside(new Vector2(6f, 1f)));
            }
            finally { Object.DestroyImmediate(lot); }
        }

        [Test]
        public void Builder_WithoutPatches_BuildsAndRegistersNothing()
        {
            var lot = new GameObject("Lot");
            try
            {
                Assert.IsNull(MudPatchBuilder.Build(lot.transform, new MudPatch[0]));
                Assert.AreEqual(0, lot.transform.childCount);
                Assert.IsFalse(Services.TryGet<IMudPatches>(out _));
            }
            finally { Object.DestroyImmediate(lot); }
        }
    }
}
