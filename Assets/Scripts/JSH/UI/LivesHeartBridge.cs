using JSH.Core;
using JSH.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace JSH.UI
{
    /// <summary>
    /// Connects LifeSystem to the teammate's heart HUD (HeartAnimationPlayer in TempFolderStage).
    /// Data only: all heart visuals/sounds belong to the teammate's component.
    ///  - Life lost   -> HeartAnimationPlayer.PlayBreak(n) for each lost heart, rightmost first (n = 1..N, left to right)
    ///  - Life gained -> On Heart Restored(n) event only (the teammate wires their restore animation here)
    /// A heart broken on screen is not broken again until it is restored: while nothing listens to
    /// On Heart Restored, healed hearts stay hidden (HeartAnimationPlayer cannot restore yet).
    /// </summary>
    public sealed class LivesHeartBridge : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _heartCount = 3;
        [Tooltip("Heart number 1..N (left to right) that should reappear. Wire the teammate's restore animation here.")]
        [SerializeField] private UnityEvent<int> _onHeartRestored = new UnityEvent<int>();

        private bool[] _broken;
        private int _shownLives;

        private void Awake() => _broken = new bool[_heartCount];

        private void OnEnable() => EventBus<LivesChangedEvent>.Subscribe(OnLivesChanged);

        private void OnDisable() => EventBus<LivesChangedEvent>.Unsubscribe(OnLivesChanged);

        // After every Awake (LifeSystem sets its count in Awake). Hearts start full.
        private void Start()
        {
            _shownLives = _heartCount;
            if (LifeSystem.HasInstance) ShowLives(LifeSystem.Instance.Lives);
        }

        private void OnLivesChanged(LivesChangedEvent evt) => ShowLives(evt.Current);

        private void ShowLives(int lives)
        {
            int target = Mathf.Clamp(lives, 0, _heartCount);
            for (int n = _shownLives; n > target; n--) BreakHeart(n);
            for (int n = _shownLives + 1; n <= target; n++) RestoreHeart(n);
            _shownLives = target;
        }

        private void BreakHeart(int heartNumber)
        {
            if (_broken[heartNumber - 1]) return;
            _broken[heartNumber - 1] = true;
            HeartAnimationPlayer.PlayBreak(heartNumber);
        }

        private void RestoreHeart(int heartNumber)
        {
            if (_onHeartRestored.GetPersistentEventCount() == 0) return; // no restore animation yet: keep it hidden
            _broken[heartNumber - 1] = false;
            _onHeartRestored.Invoke(heartNumber);
        }
    }
}
