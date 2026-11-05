using System.Text;
using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Test
{
    /// <summary>
    /// TEST ONLY - placeholder view for dialogue + minigames + lives until the real UI exists.
    /// Auto-spawns on Play (no scene reference). Dialogue: 1..9 choose, Space/Enter next, Esc end.
    /// Persuade minigame: Left Click = stop. Eject / Counterattack: shown as text. Delete the JSH/Test folder to remove it.
    /// F11 = show/hide this overlay (stays hidden until pressed again, also across scene reloads in the same Play session).
    /// </summary>
    public class TestDialogueDebug : MonoBehaviour
    {
        private const int BarWidth = 41;
        private static bool s_visible = true;

        private readonly StringBuilder _bar = new StringBuilder(BarWidth + 2);
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
            EventBus<DialogueStateChangedEvent>.Subscribe(OnDialogueState);
            EventBus<VillainConvertedEvent>.Subscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Subscribe(OnDefeated);
            EventBus<MinigameStateChangedEvent>.Subscribe(OnMinigameState);
            EventBus<MinigameFinishedEvent>.Subscribe(OnMinigameFinished);
            EventBus<CounterattackEvent>.Subscribe(OnCounterattack);
            EventBus<VillainEjectStartedEvent>.Subscribe(OnEject);
            EventBus<CarChangedEvent>.Subscribe(OnCarChanged);
            EventBus<LivesChangedEvent>.Subscribe(OnLives);
            EventBus<LivesDepletedEvent>.Subscribe(OnDepleted);
        }

        private void OnDisable()
        {
            EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoice);
            EventBus<DialogueStateChangedEvent>.Unsubscribe(OnDialogueState);
            EventBus<VillainConvertedEvent>.Unsubscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Unsubscribe(OnDefeated);
            EventBus<MinigameStateChangedEvent>.Unsubscribe(OnMinigameState);
            EventBus<MinigameFinishedEvent>.Unsubscribe(OnMinigameFinished);
            EventBus<CounterattackEvent>.Unsubscribe(OnCounterattack);
            EventBus<VillainEjectStartedEvent>.Unsubscribe(OnEject);
            EventBus<CarChangedEvent>.Unsubscribe(OnCarChanged);
            EventBus<LivesChangedEvent>.Unsubscribe(OnLives);
            EventBus<LivesDepletedEvent>.Unsubscribe(OnDepleted);
        }

        private void Log(string msg)
        {
            _log = msg;
            Debug.Log("[TestDialogue] " + msg);
        }

        private static string NameOf(Talkable t) => t != null ? t.DisplayName : "-";

        private void OnDialogueState(DialogueStateChangedEvent e) => Log((e.IsActive ? "Dialogue start: " : "Dialogue end: ") + NameOf(e.Target));
        private void OnChoice(DialogueChoiceMadeEvent e) => Log($"Choice: node {e.NodeIndex} / choice {e.ChoiceIndex} / effect {e.Effect} (target {NameOf(e.Target)}, villain={e.Target.TryGetComponent(out Villain _)})");
        private void OnConverted(VillainConvertedEvent e) => Log("Villain converted: " + e.Villain.name);
        private void OnDefeated(VillainDefeatedEvent e) => Log("Villain defeated: " + e.Villain.name);
        private void OnMinigameState(MinigameStateChangedEvent e) => Log($"Minigame {(e.IsActive ? "start" : "end")}: {e.Type} / {NameOf(e.Target)}");
        private void OnMinigameFinished(MinigameFinishedEvent e) => Log($"Minigame finished: {e.Type} success={e.Success} / {NameOf(e.Target)}");
        private void OnCounterattack(CounterattackEvent e) => Log($"Counterattack! {NameOf(e.Target)} (villain={e.IsVillain}, reason={e.Reason})");
        private void OnEject(VillainEjectStartedEvent e) => Log("Eject event: " + e.Villain.name);
        private void OnCarChanged(CarChangedEvent e) => Log($"Car {e.FromIndex} -> {e.ToIndex} unresolvedVillains={e.UnresolvedVillains} lifeLost={e.LifeLost} cleared={e.Cleared}");
        private void OnLives(LivesChangedEvent e) => Log($"Lives {e.Current}/{e.Max} (delta {e.Delta})");
        private void OnDepleted(LivesDepletedEvent e) => Log("Lives depleted -> bad ending");

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f11Key.wasPressedThisFrame) s_visible = !s_visible;

            if (!DialogueRunner.HasInstance || !DialogueRunner.Instance.IsActive) return;
            DialogueRunner runner = DialogueRunner.Instance;

            for (int i = 0; i < 9; i++)
            {
                if (kb[Key.Digit1 + i].wasPressedThisFrame) { runner.Choose(i); return; }
            }
            // Space/Enter (next) and mouse choices are handled by JSH.UI.DialogueView; Esc is the pause key.
        }

        private void OnGUI()
        {
            if (!s_visible) return;
            GUILayout.BeginArea(new Rect(10, 10, 560, 360), GUI.skin.box);
            GUILayout.Label("[TEST] Shift+Click: talk / 1-9: choose (fallback) / Click: minigame");
            string lives = LifeSystem.HasInstance ? $"{LifeSystem.Instance.Lives}/{LifeSystem.Instance.MaxLives}" : "(no LifeSystem)";
            GUILayout.Label("Lives: " + lives);

            if (MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive) DrawMinigame(MinigameDirector.Instance);
            else DrawDialogue();

            GUILayout.Label(_log);
            GUILayout.EndArea();
        }

        private void DrawDialogue()
        {
            DialogueNode node = DialogueRunner.HasInstance ? DialogueRunner.Instance.CurrentNode : null;
            if (node == null)
            {
                if (!DialogueRunner.HasInstance) GUILayout.Label("No DialogueRunner in scene");
                return;
            }
            GUILayout.Label($"#{DialogueRunner.Instance.CurrentNodeIndex}  {node.Speaker}: {node.Text}");
            for (int i = 0; i < node.ChoiceCount; i++)
            {
                DialogueChoice c = node.GetChoice(i);
                string next = c.NextNode < 0 ? "end" : c.NextNode.ToString();
                GUILayout.Label($"  {i + 1}) {c.Text}   -> {next}  [{c.Effect}]");
            }
            if (node.ChoiceCount == 0) GUILayout.Label("  (Space: next)");
        }

        private void DrawMinigame(MinigameDirector d)
        {
            GUILayout.Label($"Minigame: {d.CurrentType} vs {NameOf(d.CurrentTarget)}");
            if (d.CurrentType == MinigameType.Eject) GUILayout.Label("   >> Thrown off the train! (placeholder)");
            else if (d.CurrentType == MinigameType.Counterattack) GUILayout.Label("   >> COUNTERATTACK! life -1 (placeholder)");
            else if (d.ActiveTiming != null)
            {
                TimingMinigame t = d.ActiveTiming;
                GUILayout.Label(t.IsReady ? $"Time {t.TimeLeft:0.0}s   Click to STOP inside ===" : "Ready...");
                GUILayout.Label(BuildBar(t));
            }
        }

        private string BuildBar(TimingMinigame t)
        {
            int line = ToCell(t.Position);
            int min = ToCell(t.TargetMin);
            int max = ToCell(t.TargetMax);
            _bar.Clear();
            _bar.Append('[');
            for (int i = 0; i < BarWidth; i++) _bar.Append(i == line ? '|' : (i >= min && i <= max ? '=' : '-'));
            _bar.Append(']');
            return _bar.ToString();
        }

        private static int ToCell(float normalized) => Mathf.Clamp(Mathf.RoundToInt((normalized + 1f) * 0.5f * (BarWidth - 1)), 0, BarWidth - 1);
    }
}
