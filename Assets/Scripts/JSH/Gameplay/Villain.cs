using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Villain state. Add next to Talkable on villain prefabs; normal passengers have Talkable only.
    /// "Is this person a villain?" = TryGetComponent(out Villain _).
    /// </summary>
    [RequireComponent(typeof(Talkable))]
    public class Villain : MonoBehaviour
    {
        private Talkable _talkable;
        private bool _isConverted;

        public bool IsConverted => _isConverted;

        private void Awake() => TryGetComponent(out _talkable);

        private void OnEnable() => EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoiceMade);

        private void OnDisable() => EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoiceMade);

        private void OnChoiceMade(DialogueChoiceMadeEvent evt)
        {
            if (evt.Target != _talkable) return;
            if (evt.Effect == DialogueEffect.Convert) Convert();
        }

        public void Convert()
        {
            if (_isConverted) return;
            _isConverted = true;
            EventBus<VillainConvertedEvent>.Raise(new VillainConvertedEvent(this));
        }
    }
}
