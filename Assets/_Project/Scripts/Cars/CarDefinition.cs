using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Handling stats and looks for one kind of car (design.md section 8). No runtime state.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Definition", fileName = "CarDefinition")]
    public sealed class CarDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Sedan";
        [SerializeField, Min(0.1f)] private float _topSpeed = 8f;
        [SerializeField, Min(0.1f)] private float _acceleration = 6f;
        [SerializeField, Min(0.1f)] private float _braking = 12f;
        [SerializeField, Min(0f)] private float _coastDrag = 3f;
        [SerializeField, Min(1f)] private float _turnRate = 120f;
        [SerializeField, Min(0f)] private float _grip = 6f;
        [SerializeField, Range(0.1f, 1f)] private float _reverseFactor = 0.4f;
        [SerializeField] private Vector2 _size = new Vector2(1f, 2f);
        [SerializeField] private Color _color = new Color(0.8f, 0.2f, 0.2f, 1f);

        public string DisplayName => _displayName;
        /// <summary>Max forward speed, units per second.</summary>
        public float TopSpeed => _topSpeed;
        /// <summary>Speed gained per second under throttle.</summary>
        public float Acceleration => _acceleration;
        /// <summary>Deceleration when throttle opposes motion.</summary>
        public float Braking => _braking;
        /// <summary>Deceleration with no throttle or no driver.</summary>
        public float CoastDrag => _coastDrag;
        /// <summary>Steering at full speed, degrees per second.</summary>
        public float TurnRate => _turnRate;
        /// <summary>Sideways velocity decay rate; higher is grippier.</summary>
        public float Grip => _grip;
        /// <summary>Reverse top speed as a fraction of TopSpeed.</summary>
        public float ReverseFactor => _reverseFactor;
        /// <summary>Width (x) and length (y) in world units.</summary>
        public Vector2 Size => _size;
        public Color Color => _color;
    }
}
