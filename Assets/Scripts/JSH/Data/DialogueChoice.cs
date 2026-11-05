using System;
using UnityEngine;

namespace JSH.Data
{
    [Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string _text = "선택지";
        [Tooltip("Node index to go to after this choice. -1 = end the dialogue.")]
        [SerializeField] private int _nextNode = -1;
        [Tooltip("Legacy. CSV dialogues use DialogueNode.Outcome on the final line instead.")]
        [SerializeField] private DialogueEffect _effect = DialogueEffect.None;

        public DialogueChoice() { }

        public DialogueChoice(string text, int nextNode)
        {
            _text = text;
            _nextNode = nextNode;
        }

        public string Text => _text;
        public int NextNode => _nextNode;
        public DialogueEffect Effect => _effect;
    }
}
