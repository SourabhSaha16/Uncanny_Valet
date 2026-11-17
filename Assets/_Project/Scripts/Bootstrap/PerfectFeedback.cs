using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Sits on the Powers object beside the meter and activation controllers. Once the bus exists it links PerfectPark and
    /// PerfectDropOff to the camera kick (the PERFECT! pop and the ding live in the UI and Audio modules).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PerfectFeedback : MonoBehaviour
    {
        [SerializeField] private CameraFollow _camera;

        private PerfectKickLink _link;
        private ShakeLink _shakeLink;
        private ThunderKickLink _thunderKick;
        private ThunderBoostLink _thunderBoost;

        private void Start()
        {
            if (_camera == null)
            {
                Debug.LogError("PerfectFeedback on " + name + " has no CameraFollow assigned; there will be no camera kick.", this);
                return;
            }
            _link = new PerfectKickLink(Services.Get<IEventBus>(), Services.Get<ICarFactory>(), _camera.Kick);
            _shakeLink = new ShakeLink(Services.Get<IEventBus>(), Services.Get<ITuning>(), _camera.Shake);
            _thunderKick = new ThunderKickLink(Services.Get<IEventBus>(), _camera.Kick);
            _thunderBoost = new ThunderBoostLink(Services.Get<IEventBus>(), Services.Get<ICarFactory>(),
                () => Services.TryGet<IPowerView>(out var view) ? view : null);
        }

        private void OnDestroy()
        {
            _link?.Dispose();
            _shakeLink?.Dispose();
            _thunderKick?.Dispose();
            _thunderBoost?.Dispose();
        }
    }
}
