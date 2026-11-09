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
        [Header("Weight (the valet on foot cannot shove a car)")]
        [SerializeField, Min(1f), Tooltip("Rigidbody2D mass; the valet is about 1, so 20 is about 20x heavier.")]
        private float _mass = 20f;
        [SerializeField, Min(0f), Tooltip("Linear drag applied while nobody is driving.")]
        private float _parkedLinearDrag = 8f;
        [SerializeField, Min(0f), Tooltip("Angular drag applied while nobody is driving.")]
        private float _parkedAngularDrag = 8f;
        [Header("Mud")]
        [SerializeField, Min(0.1f), Tooltip("How fast the mud wipe eases toward its target, per second.")]
        private float _fillSmoothing = 4f;
        [Header("Looks")]
        [SerializeField] private CarMirrorStyle _mirrors = new CarMirrorStyle();
        [SerializeField, Tooltip("Body colour of an EV (clear blue).")]
        private Color _evBodyColor = new Color(0.15f, 0.45f, 0.95f, 1f);
        [SerializeField, Tooltip("Mud overlay colour (dark brown); washing wipes it away, it never fades.")]
        private Color _mudColor = new Color(0.17f, 0.09f, 0.04f, 0.9f);
        [SerializeField] private CarAutoDriveStyle _autoDrive = new CarAutoDriveStyle();

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
        /// <summary>Rigidbody2D mass. Much heavier than the valet so shoves barely move the car.</summary>
        public float Mass => _mass;
        /// <summary>Linear drag while no driver is seated.</summary>
        public float ParkedLinearDrag => _parkedLinearDrag;
        /// <summary>Angular drag while no driver is seated.</summary>
        public float ParkedAngularDrag => _parkedAngularDrag;
        /// <summary>Mud wipe easing rate per second.</summary>
        public float FillSmoothing => _fillSmoothing;
        /// <summary>Side mirror look.</summary>
        public CarMirrorStyle Mirrors => _mirrors;
        /// <summary>Body colour used for EVs.</summary>
        public Color EvBodyColor => _evBodyColor;
        /// <summary>Self-driving arrival and departure tunables.</summary>
        public CarAutoDriveStyle AutoDrive => _autoDrive;
        /// <summary>Mud overlay colour (dark brown, constant opacity).</summary>
        public Color MudColor => _mudColor;
    }
}
