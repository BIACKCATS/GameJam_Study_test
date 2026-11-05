using System;
using UnityEngine;

namespace JSH.Data
{
    /// <summary>
    /// One line of dialogue. With choices the player must pick one (that ends the current talk; the next talk
    /// resumes at the chosen node). Without choices Advance() goes to NextNode in the same talk; when there is no
    /// NextNode the dialogue is finished and Outcome (if any) is triggered.
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        [SerializeField] private string _speaker = "";
        [SerializeField, TextArea(2, 4)] private string _text = "대사 1";
        [SerializeField] private DialogueChoice[] _choices = new DialogueChoice[0];
        [Tooltip("Used only when there are no choices. -1 = end of the dialogue (Outcome is triggered).")]
        [SerializeField] private int _nextNode = -1;
        [Tooltip("Used only on the final line (no choices, no next node).")]
        [SerializeField] private DialogueOutcome _outcome = DialogueOutcome.None;

        public DialogueNode() { }

        public DialogueNode(string speaker, string text, DialogueChoice[] choices, int nextNode, DialogueOutcome outcome)
        {
            _speaker = speaker;
            _text = text;
            _choices = choices ?? new DialogueChoice[0];
            _nextNode = nextNode;
            _outcome = outcome;
        }

        /// <summary>Empty = use the Talkable's display name.</summary>
        public string Speaker => _speaker;
        public string Text => _text;
        public int ChoiceCount => _choices != null ? _choices.Length : 0;
        public int NextNode => _nextNode;
        public DialogueOutcome Outcome => _outcome;

        public DialogueChoice GetChoice(int index) => _choices[index];
    }
}
