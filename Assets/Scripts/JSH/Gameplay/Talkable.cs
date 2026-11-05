using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Marks a passenger/villain as talkable (Shift + Left Click). Needs a Collider2D on the same GameObject
    /// so TalkTargetSelector can hit it.
    /// Dialogue progress lives here: one talk runs the whole tree from ResumeNode (the root; DialogueRunner no
    /// longer moves it). Once the dialogue is finished (end reached / outcome event) the NPC can't be talked to again,
    /// except a failed passenger persuasion, which calls ResetDialogue(). Pooled: progress resets on spawn.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Talkable : MonoBehaviour, IPoolable
    {
        [SerializeField] private string _displayName = "승객";
        [SerializeField] private DialogueData _dialogue;
        [Tooltip("Bust portrait shown in the dialogue window (Art/JSH/Sprites/Portrait). Empty = DialogueView fallback portrait.")]
        [SerializeField] private Sprite _portrait;
        [Header("Minigame override (empty = MinigameDirector default)")]
        [SerializeField] private TimingSettings _timingOverride;

        private int _resumeNode = DialogueData.StartNode;
        private bool _isDialogueFinished;

        public string DisplayName => _displayName;
        public DialogueData Dialogue => _dialogue;
        public Sprite Portrait => _portrait;
        public TimingSettings TimingOverride => _timingOverride;
        public int ResumeNode => _resumeNode;
        public bool IsDialogueFinished => _isDialogueFinished;
        public bool CanTalk => isActiveAndEnabled && !_isDialogueFinished && _dialogue != null && _dialogue.IsValidNode(_resumeNode);

        /// <summary>The next talk starts at this node. Unused by DialogueRunner now (talks always run the whole tree); kept for API stability.</summary>
        public void SetResumeNode(int nodeIndex) => _resumeNode = nodeIndex;

        /// <summary>Disable further talks.</summary>
        public void FinishDialogue() => _isDialogueFinished = true;

        /// <summary>Start over from the first node and allow talking again.</summary>
        public void ResetDialogue()
        {
            _resumeNode = DialogueData.StartNode;
            _isDialogueFinished = false;
        }

        public void OnSpawned() => ResetDialogue();

        public void OnDespawned() { }
    }
}
