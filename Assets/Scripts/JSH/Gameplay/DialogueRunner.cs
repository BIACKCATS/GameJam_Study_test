using System;
using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// UI-agnostic dialogue state machine (one per scene). UI subscribes to the C# events and calls
    /// Choose(index) / Advance(). Gameplay systems listen to EventBus DialogueStateChangedEvent / DialogueChoiceMadeEvent /
    /// DialogueOutcomeReachedEvent.
    /// Step-by-step trees: a talk starts at Talkable.ResumeNode and ends as soon as a choice is picked (the next talk
    /// resumes at the chosen node). Reading the final line finishes the Talkable's dialogue and raises its Outcome.
    /// </summary>
    public class DialogueRunner : Singleton<DialogueRunner>
    {
        private Talkable _target;
        private DialogueData _data;
        private int _nodeIndex = -1;

        public bool IsActive => _target != null;
        public Talkable CurrentTarget => _target;
        public int CurrentNodeIndex => _nodeIndex;
        public DialogueNode CurrentNode => IsActive && _data.IsValidNode(_nodeIndex) ? _data.GetNode(_nodeIndex) : null;

        /// <summary>Dialogue opened with this target.</summary>
        public event Action<Talkable> Started;
        /// <summary>A node is now displayed (speaker, text, choices). Show the window/choices here.</summary>
        public event Action<DialogueNode> NodeShown;
        /// <summary>Dialogue closed.</summary>
        public event Action<Talkable> Ended;

        public bool StartDialogue(Talkable target)
        {
            if (IsActive || target == null || !target.CanTalk) return false;

            _target = target;
            _data = target.Dialogue;
            Started?.Invoke(_target);
            EventBus<DialogueStateChangedEvent>.Raise(new DialogueStateChangedEvent(true, _target));
            GoTo(target.ResumeNode);
            return true;
        }

        /// <summary>Pick a choice on the current node (0-based).</summary>
        public void Choose(int choiceIndex)
        {
            DialogueNode node = CurrentNode;
            if (node == null || choiceIndex < 0 || choiceIndex >= node.ChoiceCount) return;

            DialogueChoice choice = node.GetChoice(choiceIndex);
            Talkable target = _target;
            int nodeIndex = _nodeIndex;
            int next = choice.NextNode;

            EventBus<DialogueChoiceMadeEvent>.Raise(new DialogueChoiceMadeEvent(target, nodeIndex, choiceIndex, choice.Effect));
            if (_target != target) return; // a listener ended/restarted the dialogue

            // This talk ends here; the next talk continues from the chosen branch.
            if (_data.IsValidNode(next)) target.SetResumeNode(next);
            else target.FinishDialogue();
            EndDialogue();
        }

        /// <summary>Continue a node that has no choices (next line, or finish the dialogue on the final line).</summary>
        public void Advance()
        {
            DialogueNode node = CurrentNode;
            if (node == null || node.ChoiceCount > 0) return;
            if (_data.IsValidNode(node.NextNode))
            {
                GoTo(node.NextNode);
                return;
            }

            Talkable target = _target;
            DialogueOutcome outcome = node.Outcome;
            target.FinishDialogue();
            EndDialogue();
            if (outcome != DialogueOutcome.None)
                EventBus<DialogueOutcomeReachedEvent>.Raise(new DialogueOutcomeReachedEvent(target, outcome));
        }

        public void EndDialogue()
        {
            if (!IsActive) return;
            Talkable target = _target;
            _target = null;
            _data = null;
            _nodeIndex = -1;
            Ended?.Invoke(target);
            EventBus<DialogueStateChangedEvent>.Raise(new DialogueStateChangedEvent(false, target));
        }

        private void GoTo(int index)
        {
            if (!_data.IsValidNode(index))
            {
                EndDialogue();
                return;
            }
            _nodeIndex = index;
            NodeShown?.Invoke(_data.GetNode(index));
        }

        protected override void OnDestroy()
        {
            EndDialogue();
            base.OnDestroy();
        }
    }
}
