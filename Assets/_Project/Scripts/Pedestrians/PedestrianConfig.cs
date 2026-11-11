using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Everything tunable about the walking crowd: how many, how fast, how big, and the layered character sprites
    /// (body, outfit, hair; each a flat list of rows of <see cref="Columns"/> sprites). The routes come from the lot at
    /// runtime, because they depend on its layout. Read only at runtime; the editor builder fills the sprites.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Pedestrian Config", fileName = "PedestrianConfig")]
    public sealed class PedestrianConfig : ScriptableObject
    {
        [Header("Crowd")]
        [SerializeField, Range(6, 10), Tooltip("Pedestrians created once at the start of the night.")]
        private int _count = 8;
        [SerializeField, Range(0, 10), Tooltip("Ambient pedestrians active when the game starts (the F2 panel changes it live). Capped at Count.")]
        private int _startActive = 3;
        [SerializeField, Min(0.1f), Tooltip("Slowest walking speed, world units per second.")]
        private float _minSpeed = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("Fastest walking speed, world units per second.")]
        private float _maxSpeed = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Size of a pedestrian (the valet is 1.4).")]
        private float _scale = 1.3f;
        [SerializeField, Tooltip("Sorting order of the body layer; outfit and hair sit just above. Stays under the cars.")]
        private int _sortingOrder = 4;

        [Header("Ambient flow")]
        [SerializeField, Tooltip("Seconds an ambient person stays in the diner (min, max).")]
        private Vector2 _insideSeconds = new Vector2(5f, 14f);
        [SerializeField, Tooltip("Seconds an ambient person stays out of sight before appearing at the road side again (min, max).")]
        private Vector2 _respawnSeconds = new Vector2(1f, 7f);
        [SerializeField, Range(0f, 0.9f), Tooltip("At the start of the night an ambient person may begin up to this share of the way along their walk, so the street is not empty.")]
        private float _openingProgressMax = 0.7f;

        [Header("Patrons")]
        [SerializeField, Range(1, 12), Tooltip("Most patrons that can be on foot at once (pooled).")]
        private int _patronPoolSize = 6;
        [SerializeField, Min(0.1f), Tooltip("Walking speed of patrons, world units per second.")]
        private float _patronSpeed = 1.7f;

        [Header("Collision")]
        [SerializeField, Range(0.05f, 0.6f), Tooltip("Radius of a pedestrian's circle collider, world units.")]
        private float _colliderRadius = 0.22f;
        [SerializeField, Min(0.05f), Tooltip("How far ahead (beyond one step) a walker looks for a car, the valet, a wall or another person.")]
        private float _lookAhead = 0.35f;
        [SerializeField, Tooltip("Seconds a blocked walker waits before walking on through the blocker (min, max; each person gets a fixed value in between so two people facing each other do not give up together).")]
        private Vector2 _waitSeconds = new Vector2(1.5f, 3f);
        [SerializeField, Min(0f), Tooltip("Seconds a walker that gave up waiting walks through (not solid), then looks ahead again.")]
        private float _passSeconds = 1.5f;
        [SerializeField, Min(0f), Tooltip("Blockers are ignored this close to the end of a route, so people can reach the diner door and board their car.")]
        private float _arrivalRadius = 1.4f;
        [SerializeField, Tooltip("A person standing still (a patron waiting at the curb) keeps their collider. Off by default so a waiting patron never stops a car from reaching the pickup zone.")]
        private bool _standingSolid;

        [Header("Bounce (a car or the valet bumps a person)")]
        [SerializeField, Range(0f, 1f), Tooltip("Peak push-back of a full-strength bump, world units. Keep it tiny.")]
        private float _bounceDistance = 0.2f;
        [SerializeField, Range(0.05f, 1f), Tooltip("Seconds the bounce lasts (quick out, slower settle).")]
        private float _bounceSeconds = 0.25f;
        [SerializeField, Min(0f), Tooltip("Seconds after a bounce starts before the same person can bounce again.")]
        private float _bounceCooldown = 0.6f;
        [SerializeField, Min(0f), Tooltip("Impact speed below which a touch is ignored (no bounce, no sound).")]
        private float _bounceMinSpeed = 0.4f;
        [SerializeField, Min(0.1f), Tooltip("Impact speed that gives a full-strength bump (bigger distance, louder sound).")]
        private float _bounceFullSpeed = 5f;

        [Header("Stumble (the boosted valet runs through a person)")]
        [SerializeField, Min(0.1f), Tooltip("Seconds of the stumble when the F2 value is missing.")]
        private float _stumbleSeconds = 0.4f;
        [SerializeField, Range(0f, 2f), Tooltip("Peak push away from the valet, world units.")]
        private float _stumbleDistance = 0.7f;
        [SerializeField, Range(0f, 3f), Tooltip("Full turns the person spins during the stumble.")]
        private float _stumbleSpins = 1f;

        [Header("Animation")]
        [SerializeField, Min(1)] private int _columns = 24;
        [SerializeField, Min(1)] private int _framesPerGroup = 6;
        [SerializeField, Min(1f), Tooltip("Walk-cycle frames per second.")] private float _framesPerSecond = 10f;
        [SerializeField, Min(0)] private int _downStart;
        [SerializeField, Min(0)] private int _rightStart = 6;
        [SerializeField, Min(0)] private int _upStart = 12;
        [SerializeField, Min(0)] private int _leftStart = 18;
        [SerializeField, Tooltip("Mirror the Right group (use if it shows the walker facing left).")] private bool _flipRight;
        [SerializeField, Tooltip("Mirror the Left group (use if it shows the walker facing right).")] private bool _flipLeft;

        [Header("Sprites (rows of Columns sprites)")]
        [SerializeField] private Sprite[] _model = new Sprite[0];
        [SerializeField] private Sprite[] _outfit = new Sprite[0];
        [SerializeField] private Sprite[] _hair = new Sprite[0];

        public int Count => _count;
        public int StartActive => Mathf.Min(_startActive, _count);
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => Mathf.Max(_minSpeed, _maxSpeed);
        public float Scale => _scale;
        public int SortingOrder => _sortingOrder;
        public Vector2 InsideSeconds => _insideSeconds;
        public Vector2 RespawnSeconds => _respawnSeconds;
        public float OpeningProgressMax => _openingProgressMax;
        public int PatronPoolSize => _patronPoolSize;
        public float PatronSpeed => _patronSpeed;
        public float ColliderRadius => _colliderRadius;
        public float LookAhead => _lookAhead;
        public float PassSeconds => _passSeconds;
        public float ArrivalRadius => _arrivalRadius;
        public bool StandingSolid => _standingSolid;
        public float BounceDistance => _bounceDistance;
        public float BounceSeconds => _bounceSeconds;
        public float BounceCooldown => _bounceCooldown;
        public float BounceMinSpeed => _bounceMinSpeed;
        public float BounceFullSpeed => _bounceFullSpeed;
        public float StumbleSeconds => _stumbleSeconds;
        public float StumbleDistance => _stumbleDistance;
        public float StumbleSpins => _stumbleSpins;

        /// <summary>Seconds a person waits when blocked; <paramref name="patience01"/> (0..1) picks a value in the configured range.</summary>
        public float WaitFor(float patience01)
            => Mathf.Lerp(Mathf.Min(_waitSeconds.x, _waitSeconds.y), Mathf.Max(_waitSeconds.x, _waitSeconds.y), Mathf.Clamp01(patience01));

        public int Columns => _columns;
        public int FramesPerGroup => _framesPerGroup;
        public float FramesPerSecond => _framesPerSecond;

        /// <summary>Skin rows available.</summary>
        public int Skins => _model.Length / _columns;
        /// <summary>Hair rows available.</summary>
        public int Hairs => _hair.Length / _columns;
        /// <summary>Outfit rows available.</summary>
        public int Outfits => _outfit.Length / _columns;

        /// <summary>True when every layer holds at least one whole row.</summary>
        public bool IsComplete => Skins > 0 && Hairs > 0 && Outfits > 0 && _columns >= _leftStart + _framesPerGroup;

        public Sprite Model(PedestrianLook look, int column) => _model[look.Skin * _columns + column];
        public Sprite Outfit(PedestrianLook look, int column) => _outfit[look.Outfit * _columns + column];
        public Sprite Hair(PedestrianLook look, int column) => _hair[look.Hair * _columns + column];

        /// <summary>First sheet column of the walk cycle for a facing.</summary>
        public int GroupStart(WalkFacing facing)
        {
            switch (facing)
            {
                case WalkFacing.Right: return _rightStart;
                case WalkFacing.Up: return _upStart;
                case WalkFacing.Left: return _leftStart;
                default: return _downStart;
            }
        }

        /// <summary>True when the sprites of this facing must be mirrored.</summary>
        public bool Flip(WalkFacing facing)
            => (facing == WalkFacing.Right && _flipRight) || (facing == WalkFacing.Left && _flipLeft);
    }
}
