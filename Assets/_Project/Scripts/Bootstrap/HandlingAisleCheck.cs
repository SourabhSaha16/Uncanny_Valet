using UncannyValet.Cars;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Fails loudly when the handling config's measured aisle width no longer equals the lot's aisle width (the turning
    /// radius is derived from it). The editor setup copies the value; Cars cannot reference Lot, so this is the guard.
    /// </summary>
    public sealed class HandlingAisleCheck : MonoBehaviour
    {
        private const float Tolerance = 1e-3f;

        [SerializeField] private LotConfig _lot;
        [SerializeField] private CarHandlingConfig _handling;

        /// <summary>True when the two widths differ by more than a rounding error.</summary>
        public static bool Differs(float lotAisleWidth, float measuredAisleWidth)
            => Mathf.Abs(lotAisleWidth - measuredAisleWidth) > Tolerance;

        private void Start()
        {
            if (_lot == null || _handling == null)
            {
                Debug.LogError("HandlingAisleCheck on " + name + " is missing the LotConfig or the CarHandlingConfig: rebuild with Setup/Build Everything.", this);
                return;
            }
            if (Differs(_lot.AisleWidth, _handling.MeasuredAisleWidth))
                Debug.LogError("HandlingAisleCheck on " + name + ": CarHandlingConfig.MeasuredAisleWidth " + _handling.MeasuredAisleWidth
                    + " differs from LotConfig.AisleWidth " + _lot.AisleWidth + ". Rebuild with Setup/Build Everything to resync the turning radius.", this);
        }
    }
}
