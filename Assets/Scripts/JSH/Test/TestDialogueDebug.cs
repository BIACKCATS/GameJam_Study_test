using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Test
{
    /// <summary>
    /// TEST ONLY - placeholder dialogue view until the real UI exists. Auto-spawns on Play (no scene reference),
    /// shows the current node top-left; keys 1..9 = Choose, Space/Enter = Advance, Esc = end.
    /// Delete the JSH/Test folder to remove it.
    /// </summary>
    public class TestDialogueDebug : MonoBehaviour
    {
        private string _log = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            GameObject go = new GameObject("[TestDialogueDebug]");
            go.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<TestDialogueDebug>();
        }

        private void OnEnable()
        {
            EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoice);
            EventBus<VillainConvertedEvent>.Subscribe(OnConverted);
            EventBus<DialogueStateChangedEvent>.Subscribe(OnState);
        }

        private void OnDisable()
        {
            EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoice);
            EventBus<VillainConvertedEvent>.Unsubscribe(OnConverted);
            EventBus<DialogueStateChangedEvent>.Unsubscribe(OnState);
        }

        private void OnState(DialogueStateChangedEvent e)
        {
            string who = e.Target != null ? e.Target.DisplayName : "-";
            _log = e.IsActive ? "Dialogue start: " + who : "Dialogue end: " + who;
            Debug.Log("[TestDialogue] " + _log);
        }

        private void OnChoice(DialogueChoiceMadeEvent e)
        {
            _log = $"Choice: node {e.NodeIndex} / choice {e.ChoiceIndex} / effect {e.Effect}";
            Debug.Log($"[TestDialogue] {_log} (target {e.Target.DisplayName}, villain={e.Target.TryGetComponent(out Villain _)})");
        }

        private void OnConverted(VillainConvertedEvent e) => Debug.Log("[TestDialogue] Villain converted: " + e.Villain.name);

        private void Update()
        {
            if (!DialogueRunner.HasInstance || !DialogueRunner.Instance.IsActive) return;
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            DialogueRunner runner = DialogueRunner.Instance;

            for (int i = 0; i < 9; i++)
            {
                if (kb[Key.Digit1 + i].wasPressedThisFrame) { runner.Choose(i); return; }
            }
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) runner.Advance();
            if (kb.escapeKey.wasPressedThisFrame) runner.EndDialogue();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 520, 320), GUI.skin.box);
            GUILayout.Label("[TEST] Dialogue  (Shift+Click: talk / 1-9: choose / Space: next / Esc: end)");
            DialogueNode node = DialogueRunner.HasInstance ? DialogueRunner.Instance.CurrentNode : null;
            if (node != null)
            {
                GUILayout.Label($"#{DialogueRunner.Instance.CurrentNodeIndex}  {node.Speaker}: {node.Text}");
                for (int i = 0; i < node.ChoiceCount; i++)
                {
                    DialogueChoice c = node.GetChoice(i);
                    string next = c.NextNode < 0 ? "end" : c.NextNode.ToString();
                    GUILayout.Label($"  {i + 1}) {c.Text}   -> {next}  [{c.Effect}]");
                }
                if (node.ChoiceCount == 0) GUILayout.Label("  (Space: next)");
            }
            else if (!DialogueRunner.HasInstance) GUILayout.Label("No DialogueRunner in scene");
            GUILayout.Label(_log);
            GUILayout.EndArea();
        }
    }
}
