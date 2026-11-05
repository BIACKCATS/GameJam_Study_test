using UnityEngine;

namespace JSH.Data
{
    /// <summary>A whole conversation. Node 0 is the start. Design target: 2 choices per choice node.</summary>
    [CreateAssetMenu(fileName = "Dialogue_New", menuName = "JSH/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public const int StartNode = 0;
        public const int ExpectedChoiceCount = 2;

        [SerializeField] private DialogueNode[] _nodes = new DialogueNode[0];

        public int NodeCount => _nodes != null ? _nodes.Length : 0;

        public bool IsValidNode(int index) => index >= 0 && index < NodeCount;

        public DialogueNode GetNode(int index) => _nodes[index];

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < NodeCount; i++)
            {
                DialogueNode node = _nodes[i];
                if (node == null) continue;
                int count = node.ChoiceCount;
                if (count != 0 && count != ExpectedChoiceCount)
                    Debug.LogWarning($"[DialogueData] {name} node {i}: {count} choices (design expects {ExpectedChoiceCount}).", this);
                for (int c = 0; c < count; c++)
                {
                    int next = node.GetChoice(c).NextNode;
                    if (next != -1 && !IsValidNode(next))
                        Debug.LogWarning($"[DialogueData] {name} node {i} choice {c}: nextNode {next} does not exist.", this);
                }
            }
        }
#endif
    }
}
