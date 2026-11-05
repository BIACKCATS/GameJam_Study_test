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

        public string DisplayName => _displayName;
        public DialogueData Dialogue => _dialogue;
        public bool CanTalk => isActiveAndEnabled && _dialogue != null && _dialogue.NodeCount > 0;
    }
}
