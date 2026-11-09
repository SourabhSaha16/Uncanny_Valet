using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>The top-down car sprites: regular cars and EVs. A car id always picks the same sprite.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Art Config", fileName = "CarArtConfig")]
    public sealed class CarArtConfig : ScriptableObject
    {
        [SerializeField] private CarSpriteEntry[] _cars = new CarSpriteEntry[0];
        [SerializeField, Tooltip("Sprites for EVs; a distinct look from the regular cars.")]
        private CarSpriteEntry[] _evCars = new CarSpriteEntry[0];

        /// <summary>True when at least one regular sprite is set.</summary>
        public bool HasCars => _cars != null && _cars.Length > 0;

        /// <summary>The regular sprite for a car id, or null when there are none.</summary>
        public CarSpriteEntry PickCar(int carId) => Pick(_cars, carId);

        /// <summary>The EV sprite for a car id, or null when there are none.</summary>
        public CarSpriteEntry PickEv(int carId) => Pick(_evCars, carId);

        private static CarSpriteEntry Pick(CarSpriteEntry[] entries, int carId)
        {
            var count = entries == null ? 0 : entries.Length;
            var index = CarArtMath.PickIndex(carId, count);
            return index < 0 ? null : entries[index];
        }
    }
}
