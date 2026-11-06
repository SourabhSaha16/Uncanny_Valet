using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Thin blue charge bar along an EV (design.md section 10). Created inactive; CarTraitLook activates it
    /// for EVs. Fill eases toward the latest charge and turns a lighter tint when full.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarChargeBar : MonoBehaviour
    {
        private CarId _id;
        private CarChargeBarStyle _style;
        private Transform _fill;
        private SpriteRenderer _fillRenderer;
        private IEventBus _bus;
        private Vector2 _usable;
        private float _shown;
        private float _target;

        /// <summary>Builds the (inactive) bar as a child of <paramref name="car"/>.</summary>
        public static CarChargeBar Attach(GameObject car, CarId id, Vector2 carSize, CarChargeBarStyle style, Sprite sprite)
        {
            var track = new GameObject("ChargeBar");
            track.transform.SetParent(car.transform, false);
            track.SetActive(false);
            track.transform.localPosition = new Vector3(style.OffsetX / carSize.x, 0f, 0f);
            track.transform.localScale = new Vector3(style.Size.x / carSize.x, style.Size.y / carSize.y, 1f);
            var trackRenderer = track.AddComponent<SpriteRenderer>();
            trackRenderer.sprite = sprite;
            trackRenderer.color = style.TrackColor;
            trackRenderer.sortingOrder = CarSorting.ChargeTrack;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(track.transform, false);
            var bar = track.AddComponent<CarChargeBar>();
            bar._id = id;
            bar._style = style;
            bar._fill = fill.transform;
            bar._fillRenderer = fill.AddComponent<SpriteRenderer>();
            bar._fillRenderer.sprite = sprite;
            bar._fillRenderer.sortingOrder = CarSorting.ChargeFill;
            bar._usable = new Vector2(CarChargeBarMath.UsableFraction(style.Size.x, style.Border),
                CarChargeBarMath.UsableFraction(style.Size.y, style.Border));
            return bar;
        }

        private void OnEnable()
        {
            _shown = 0f;
            _target = 0f;
            SetFull(false);
            Apply();
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            _bus.Subscribe<CarChargeChanged>(OnCharge);
            _bus.Subscribe<CarFullyCharged>(OnFull);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarChargeChanged>(OnCharge);
            _bus.Unsubscribe<CarFullyCharged>(OnFull);
            _bus = null;
        }

        private void OnCharge(CarChargeChanged e)
        {
            if (e.Car != _id) return;
            _target = Mathf.Clamp01(e.Charge01);
            SetFull(CarChargeBarMath.IsFull(_target));
        }

        private void OnFull(CarFullyCharged e)
        {
            if (e.Car != _id) return;
            _target = 1f;
            SetFull(true);
        }

        private void SetFull(bool full)
        {
            _fillRenderer.color = full ? _style.FullColor : _style.FillColor;
        }

        private void Update()
        {
            _shown = CarTimerFillMath.Smooth(_shown, _target, _style.Smoothing, Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            float height = CarChargeBarMath.FillHeight(_shown, _usable.y);
            _fill.localScale = new Vector3(_usable.x, height, 1f);
            _fill.localPosition = new Vector3(0f, CarChargeBarMath.FillCentreY(_shown, _usable.y), 0f);
        }
    }
}
