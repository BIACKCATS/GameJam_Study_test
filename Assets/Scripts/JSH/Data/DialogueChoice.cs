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
        [SerializeField] private DialogueEffect _effect = DialogueEffect.None;

        public string Text => _text;
        public int NextNode => _nextNode;
        public DialogueEffect Effect => _effect;
    }
}
