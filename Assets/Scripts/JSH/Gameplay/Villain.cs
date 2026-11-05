using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Villain state. Add next to Talkable on villain prefabs; normal passengers have Talkable only.
    /// "Is this person a villain?" = TryGetComponent(out Villain _).
    /// Resolved = converted (persuaded) or defeated (accused -> ejected). Unresolved villains cost a life at the car exit.
    /// Pooled by TrainCar: the state resets every time it is spawned (IPoolable.OnSpawned).
    /// </summary>
    [RequireComponent(typeof(Talkable))]
    public class Villain : MonoBehaviour, IPoolable
    {
        private Talkable _talkable;
        private bool _isConverted;
        private bool _isDefeated;

        public bool IsConverted => _isConverted;
        public bool IsDefeated => _isDefeated;
        public bool IsResolved => _isConverted || _isDefeated;

        private void Awake() => TryGetComponent(out _talkable);

        private void OnEnable() => EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoiceMade);

        private void OnDisable() => EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoiceMade);

        // Direct "Convert" dialogue effect (no minigame). Accuse/Persuade go through MinigameDirector.
        private void OnChoiceMade(DialogueChoiceMadeEvent evt)
        {
            if (evt.Target != _talkable) return;
            if (evt.Effect == DialogueEffect.Convert) Convert();
        }

        public void OnSpawned()
        {
            _isConverted = false;
            _isDefeated = false;
        }

        public void OnDespawned() { }

        public void Convert()
        {
            if (IsResolved) return;
            _isConverted = true;
            EventBus<VillainConvertedEvent>.Raise(new VillainConvertedEvent(this));
        }

        public void Defeat()
        {
            if (IsResolved) return;
            _isDefeated = true;
            EventBus<VillainDefeatedEvent>.Raise(new VillainDefeatedEvent(this));
        }
    }
}
