using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>The validation rules of a <see cref="PropRecipe"/>. Pure, so the tests and the setup tools share them.</summary>
    public static class PropRecipeRules
    {
        /// <summary>True when the recipe can be drawn (or has a sprite override of the right size).</summary>
        public static bool TryValidate(PropRecipe recipe, out string problem)
        {
            problem = Check(recipe);
            return problem == null;
        }

        private static string Check(PropRecipe recipe)
        {
            if (recipe == null) return "the recipe is missing";
            var size = recipe.SizeUnits;
            if (size.x < 1 || size.y < 1 || size.x > PropRecipe.MaxUnits || size.y > PropRecipe.MaxUnits)
                return recipe.name + ": the size must be 1 to " + PropRecipe.MaxUnits + " grid units each way";
            if (recipe.Solid && (recipe.ColliderSize.x <= 0f || recipe.ColliderSize.y <= 0f)) return recipe.name + ": a solid prop needs a collider size";
            if (recipe.SpriteOverride != null) return CheckOverride(recipe);
            if (recipe.Layers.Count == 0) return recipe.name + ": no layers and no sprite override";
            for (var i = 0; i < recipe.Layers.Count; i++)
            {
                var problem = CheckLayer(recipe, recipe.Layers[i]);
                if (problem != null) return problem;
            }
            return null;
        }

        private static string CheckOverride(PropRecipe recipe)
        {
            var rect = recipe.SpriteOverride.rect;
            return Mathf.RoundToInt(rect.width) == recipe.SizePixels.x && Mathf.RoundToInt(rect.height) == recipe.SizePixels.y
                ? null : recipe.name + ": the sprite override is not " + recipe.SizePixels.x + " by " + recipe.SizePixels.y + " pixels";
        }

        private static string CheckLayer(PropRecipe recipe, PropLayer layer)
        {
            if (layer == null || layer.Shapes.Count == 0) return recipe.name + ": a layer has no shapes";
            var canvas = new RectInt(Vector2Int.zero, recipe.SizePixels);
            for (var i = 0; i < layer.Shapes.Count; i++)
            {
                var area = layer.Shapes[i].Area;
                if (area.width < 1 || area.height < 1) return recipe.name + ": layer " + layer.Name + " shape " + i + " is empty";
                if (area.xMin < 0 || area.yMin < 0 || area.xMax > canvas.xMax || area.yMax > canvas.yMax)
                    return recipe.name + ": layer " + layer.Name + " shape " + i + " leaves the picture";
            }
            if (layer.Animation == PropAnimation.None) return null;
            if (layer.Speed == 0f) return recipe.name + ": layer " + layer.Name + " moves but its speed is 0";
            if (layer.Animation == PropAnimation.Blink && layer.Speed < 0f) return recipe.name + ": layer " + layer.Name + " blinks with a negative period";
            return null;
        }
    }
}
