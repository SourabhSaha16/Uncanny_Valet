using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Builds and removes cars. Cars are generated coloured rectangles (placeholder art).</summary>
    public sealed class CarFactory : MonoBehaviour, ICarFactory
    {
        [SerializeField] private CarDefinition _defaultDefinition;

        [SerializeField, Range(0.1f, 1f)] private float _labelHeight = 0.55f;
        [SerializeField] private Color _labelColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        private readonly Dictionary<CarId, CarDriver> _cars = new Dictionary<CarId, CarDriver>();
        private Sprite _sprite;

        /// <summary>Optional bus for the publishSpawned overload. Set by the bootstrap.</summary>
        public IEventBus Bus { get; set; }

        public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees)
            => Spawn(_defaultDefinition, id, position, rotationDegrees, default, false);

        /// <summary>Spawns from a definition. Publishes CarSpawned only when publishSpawned is true.</summary>
        public IDrivable Spawn(CarDefinition definition, CarId id, Vector2 position,
            float rotationDegrees, TicketId ticket, bool publishSpawned)
        {
            if (_cars.ContainsKey(id)) Despawn(id);
            if (definition == null) definition = _defaultDefinition;
            if (definition == null) definition = ScriptableObject.CreateInstance<CarDefinition>();

            var car = Build(id, definition, position, rotationDegrees);
            _cars[id] = car;
            if (publishSpawned && Bus != null) Bus.Publish(new CarSpawned(id, ticket));
            return car;
        }

        public bool TryGet(CarId id, out IDrivable car)
        {
            var found = _cars.TryGetValue(id, out var driver);
            car = found ? driver : null;
            return found;
        }

        public void Despawn(CarId id)
        {
            if (!_cars.TryGetValue(id, out var car)) return;
            _cars.Remove(id);
            if (car != null) Destroy(car.gameObject);
        }

        private CarDriver Build(CarId id, CarDefinition def, Vector2 position, float rotation)
        {
            var go = new GameObject(id.ToString());
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
            go.transform.localScale = new Vector3(def.Size.x, def.Size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSprite();
            renderer.color = def.Color;
            renderer.sortingOrder = CarSorting.Body;

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.mass = def.Mass;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<BoxCollider2D>().size = Vector2.one;

            var driver = go.AddComponent<CarDriver>();
            driver.Initialize(id, def);
            go.AddComponent<CarInteraction>();
            AttachLooks(go, id, def, renderer);
            CarTimerFill.Attach(go, id, def, GetSprite());
            CarLabel.Attach(go, id, def.Size, _labelHeight, _labelColor);
            return driver;
        }

        private void AttachLooks(GameObject go, CarId id, CarDefinition def, SpriteRenderer body)
        {
            var sprite = GetSprite();
            CarMirrors.Attach(go, def.Size, def.Mirrors, sprite);
            var bar = CarChargeBar.Attach(go, id, def.Size, def.ChargeBar, sprite);
            var mud = CarMudOverlay.Attach(go, id, def, sprite);
            CarTraitLook.Attach(go, id, def, body, bar, mud);
        }

        private Sprite GetSprite()
        {
            if (_sprite != null) return _sprite;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _sprite;
        }
    }
}
