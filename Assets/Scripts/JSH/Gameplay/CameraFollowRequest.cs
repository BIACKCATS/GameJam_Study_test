namespace JSH.Gameplay
{
    /// <summary>
    /// Raise via EventBus&lt;CameraFollowRequest&gt;.Raise(...) to turn camera follow on/off without a reference
    /// (e.g. from a cutscene or Feel sequence). Snap = jump straight to the target when re-enabling.
    /// </summary>
    public readonly struct CameraFollowRequest
    {
        public readonly bool Enabled;
        public readonly bool Snap;

        public CameraFollowRequest(bool enabled, bool snap = false)
        {
            Enabled = enabled;
            Snap = snap;
        }
    }
}
