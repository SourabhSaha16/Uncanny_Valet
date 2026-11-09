using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Builds and removes cars. Cars use the top-down sprites of the CarArtConfig, or coloured rectangles without one.</summary>
    public sealed class CarFactory : MonoBehaviour, ICarFactory
    {
        [SerializeField] private CarDefinition _defaultDefinition;

        [SerializeField, Tooltip("Top-down car sprites. Empty: cars are coloured rectangles.")]
        private CarArtConfig _art;

        [SerializeField, Tooltip("Body tint and pickup flash tunables.")]
        private CarFlashConfig _flash;

        private readonly Dictionary<CarId, CarDriver> _cars = new Dictionary<CarId, CarDriver>();
        private Sprite _sprite;
        private CarFlashConfig _fallbackFlash;

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

        public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees)
        {
            var definition = _defaultDefinition != null ? _defaultDefinition : ScriptableObject.CreateInstance<CarDefinition>();
            Vector2 heading = Quaternion.Euler(0f, 0f, rotationDegrees) * Vector2.up;
            Vector2 start = stopPosition - heading * definition.AutoDrive.EntryDistance;
            var car = (CarDriver)Spawn(definition, id, start, rotationDegrees, default, false);
            car.GetComponent<CarAutoDrive>().BeginArriving(definition, stopPosition);
            return car;
        }

        public void DriveAwayAndDespawn(CarId id)
        {
            if (!_cars.TryGetValue(id, out var car) || car == null) { Despawn(id); return; }
            var drive = car.GetComponent<CarAutoDrive>();
            if (drive.IsDeparting) return;
            drive.BeginDeparting(car.Definition, () => Despawn(id));
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
            go.AddComponent<CarAutoDrive>();
            var art = AttachBodyArt(go, id, def, renderer);
            AttachLooks(go, id, def, renderer, art);
            CarKeyTag.Attach(go, id, art, FlashConfig());
            return driver;
        }

        private CarFlashConfig FlashConfig()
        {
            if (_flash != null) return _flash;
            if (_fallbackFlash != null) return _fallbackFlash;
            Debug.LogError("CarFactory has no CarFlashConfig; using defaults. Rebuild with Setup/Build Everything.", this);
            _fallbackFlash = ScriptableObject.CreateInstance<CarFlashConfig>();
            return _fallbackFlash;
        }

        private CarBodyArt AttachBodyArt(GameObject go, CarId id, CarDefinition def, SpriteRenderer placeholder)
        {
            if (_art == null || !_art.HasCars) return null;
            placeholder.enabled = false; // the sprite child replaces the coloured rectangle
            return CarBodyArt.Attach(go, _art, id, def.Size);
        }

        private void AttachLooks(GameObject go, CarId id, CarDefinition def, SpriteRenderer body, CarBodyArt art)
        {
            var sprite = GetSprite();
            if (art == null) CarMirrors.Attach(go, def.Size, def.Mirrors, sprite); // sprites have their own mirrors
            var mud = CarMudOverlay.Attach(go, id, def, sprite);
            CarTraitLook.Attach(go, id, def, body, mud, art);
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
