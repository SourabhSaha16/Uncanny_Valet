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
        [SerializeField, Min(0.1f), Tooltip("Slowest walking speed, world units per second.")]
        private float _minSpeed = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("Fastest walking speed, world units per second.")]
        private float _maxSpeed = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Size of a pedestrian (the valet is 1.4).")]
        private float _scale = 1.3f;
        [SerializeField, Tooltip("Sorting order of the body layer; outfit and hair sit just above. Stays under the cars.")]
        private int _sortingOrder = 4;

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
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => Mathf.Max(_minSpeed, _maxSpeed);
        public float Scale => _scale;
        public int SortingOrder => _sortingOrder;
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
