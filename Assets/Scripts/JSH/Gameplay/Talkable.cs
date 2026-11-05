using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Marks a passenger/villain as talkable (Shift + Left Click). Needs a Collider2D on the same GameObject
    /// so TalkTargetSelector can hit it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Talkable : MonoBehaviour
    {
        [SerializeField] private string _displayName = "승객";
        [SerializeField] private DialogueData _dialogue;
        [Tooltip("Shown in the dialogue window's hexagon frame. Empty = DialogueView fallback portrait.")]
        [SerializeField] private Sprite _portrait;
        [Header("Minigame override (empty = MinigameDirector default)")]
        [SerializeField] private TimingSettings _timingOverride;

        public string DisplayName => _displayName;
        public DialogueData Dialogue => _dialogue;
        public Sprite Portrait => _portrait;
        public TimingSettings TimingOverride => _timingOverride;
        public bool CanTalk => isActiveAndEnabled && _dialogue != null && _dialogue.NodeCount > 0;
    }
}
