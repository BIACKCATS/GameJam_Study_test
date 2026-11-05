using System;
using UnityEngine;

namespace JSH.Data
{
    /// <summary>One line of dialogue. With choices the player must pick one; without choices Advance() goes to NextNode.</summary>
    [Serializable]
    public class DialogueNode
    {
        [SerializeField] private string _speaker = "승객";
        [SerializeField, TextArea(2, 4)] private string _text = "대사 1";
        [SerializeField] private DialogueChoice[] _choices = new DialogueChoice[0];
        [Tooltip("Used only when there are no choices. -1 = end the dialogue.")]
        [SerializeField] private int _nextNode = -1;

        public string Speaker => _speaker;
        public string Text => _text;
        public int ChoiceCount => _choices != null ? _choices.Length : 0;
        public int NextNode => _nextNode;

        public DialogueChoice GetChoice(int index) => _choices[index];
    }
}
