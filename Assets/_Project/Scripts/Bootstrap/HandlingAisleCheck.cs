using UncannyValet.Cars;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Warns when the handling config's measured aisle width no longer equals the lot's, or when the tightest turn
    /// radius (set directly) is too wide to enter a bay from the aisle. The editor setup copies the width; Cars cannot reference Lot.
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
                Debug.LogWarning("HandlingAisleCheck on " + name + ": CarHandlingConfig.MeasuredAisleWidth " + _handling.MeasuredAisleWidth
                    + " differs from LotConfig.AisleWidth " + _lot.AisleWidth + ". Rebuild with Setup/Build Everything to resync it.", this);
            if (!TurningRadius.CanEnterBay(_lot.AisleWidth, _handling.RMin, _handling.RadiusMargin))
                Debug.LogWarning("HandlingAisleCheck on " + name + ": the tightest turn radius " + _handling.RMin
                    + " is too wide to swing from the aisle (" + _lot.AisleWidth + ") into a bay.", this);
        }
    }
}
