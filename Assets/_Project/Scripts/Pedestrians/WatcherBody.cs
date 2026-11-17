using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// A watching character: a recoloured MetroCity pedestrian standing still (facing the player) with code-drawn accessories and a gentle idle bob.
    /// It is scenery: no collider and no rigidbody, so nothing can hit it, and it takes no ticket. Built once by <see cref="Build"/>;
    /// nothing is allocated per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WatcherBody : MonoBehaviour
    {
        private const int AccessoryOrderBase = 3;

        private Transform _pivot;
        private WatcherLook _look;
        private Texture2D _texture;
        private Sprite _square;

        /// <summary>True once <see cref="Build"/> succeeded.</summary>
        public bool IsBuilt => _pivot != null;

        /// <summary>Draws the character from the look's recipe. Logs an error and builds nothing when the recipe does not fit the sprite set.</summary>
        public void Build(WatcherLook look, PedestrianConfig config)
        {
            string problem = null;
            if (config == null || !config.IsComplete) problem = "the pedestrian config is incomplete";
            else look.TryValidate(config.Skins, config.Hairs, config.Outfits, out problem);
            if (problem != null)
            {
                Debug.LogError("Watcher " + name + " cannot be drawn: " + problem, this);
                return;
            }
            _look = look;
            transform.localScale = new Vector3(config.Scale, config.Scale, 1f);
            _pivot = new GameObject("Pivot").transform;
            _pivot.SetParent(transform, false);
            var sprites = new WalkerSprites(_pivot, config);
            sprites.Show(new PedestrianLook(look.Skin, look.Hair, look.Outfit), config.GroupStart(WalkFacing.Down), config.Flip(WalkFacing.Down));
            sprites.Tint(look.SkinTint, look.OutfitTint, look.HairTint);
            BuildAccessories(sprites.BodySprite, config.SortingOrder + AccessoryOrderBase);
        }

        private void Update()
        {
            if (_pivot == null) return;
            var wave = 0.5f + 0.5f * Mathf.Sin(Time.time * _look.BobSpeed * Mathf.PI * 2f);
            _pivot.localPosition = new Vector3(0f, wave * _look.BobHeight, 0f);
        }

        private void OnDestroy()
        {
            if (_square != null) Destroy(_square);
            if (_texture != null) Destroy(_texture);
        }

        // One white 1x1 sprite shared by every accessory rectangle; each is scaled and coloured through its own renderer.
        private void BuildAccessories(Sprite body, int firstOrder)
        {
            _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = name + " Pixel" };
            _texture.SetPixel(0, 0, Color.white);
            _texture.Apply();
            _square = Sprite.Create(_texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            for (var i = 0; i < _look.Accessories.Count; i++) AddAccessory(_look.Accessories[i], body, firstOrder);
        }

        private void AddAccessory(WatcherAccessory accessory, Sprite body, int firstOrder)
        {
            var go = new GameObject(accessory.Label);
            go.transform.SetParent(_pivot, false);
            var ppu = body.pixelsPerUnit;
            var frame = body.rect.size;
            var fromPivot = new Vector2(accessory.CenterPixels.x - body.pivot.x, frame.y - accessory.CenterPixels.y - body.pivot.y);
            go.transform.localPosition = fromPivot / ppu;
            go.transform.localScale = new Vector3(accessory.SizePixels.x / ppu, accessory.SizePixels.y / ppu, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _square;
            renderer.color = accessory.Color;
            renderer.sortingOrder = firstOrder + accessory.Order;
        }
    }
}
