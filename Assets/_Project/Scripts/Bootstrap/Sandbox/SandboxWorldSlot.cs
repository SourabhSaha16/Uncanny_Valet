using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UncannyValet.Run;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// The inactive template of one sandbox world: a root holding the lot builder and the decor (or the hand-made lot), set up for its lot definition by the
    /// scene builder. The sandbox clones it to show the world, so the lot builds from scratch (mud, props, lighting) every time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SandboxWorldSlot : MonoBehaviour
    {
        [SerializeField] private LotDefinition _definition;
        [SerializeField] private LotLayoutBuilder _lot;
        [SerializeField] private LotDecor _decor;
        [SerializeField, Tooltip("The hand-made lot of a Handmade definition (an instance of its prefab); then the lot builder and the decor stay empty.")]
        private HandmadeLot _handmadeLot;

        private ILotSource _source;

        public LotDefinition Definition => _definition;
        public LotLayoutBuilder Lot => _lot;
        public LotDecor Decor => _decor;
        public HandmadeLot HandmadeLot => _handmadeLot;

        /// <summary>The lot of this world as the bootstrap sees it; null when the slot has no lot. Built on first use.</summary>
        public ILotSource Source => _source ?? (_source = LotSources.Of(_handmadeLot, _lot, _decor));
    }
}
