namespace UncannyValet.Valet
{
    /// <summary>Pure helper: blocks an action for the rest of one frame.</summary>
    public sealed class FrameGuard
    {
        private int _blockedFrame = -1;

        public void Block(int frame) => _blockedFrame = frame;

        public bool IsBlocked(int frame) => frame == _blockedFrame;
    }
}
