using System;
using System.Collections.Generic;
using JSH.Core;
using JSH.Gameplay;
using UnityEngine;

namespace JSH.Juice
{
    /// <summary>
    /// Bridges gameplay events to hty's presentation prefabs (both must be active in the scene):
    ///  - CounterattackEvent on a normal passenger -> PassengerAttackController.PlayAttack(key) (헥토파스칼킥).
    ///  - VillainEjectStartedEvent (퇴치 success)   -> GongikNagaController.Play(key) (나가!).
    /// The key is picked by matching the NPC's GameObject name (case-insensitive substring, first match wins).
    /// While a presentation plays, MinigameDirector keeps the event session (input lock) open.
    /// </summary>
    public class PassengerEventPresenter : MonoBehaviour
    {
        [Serializable]
        private struct KeyRule
        {
            [Tooltip("Case-insensitive substring of the NPC GameObject name (prefab name).")]
            public string NameContains;
            [Tooltip("Key passed to the hty controller.")]
            public string Key;
        }

        [Header("Passenger -> PassengerAttackController key (Women, Grandma, student, Worker)")]
        [SerializeField] private KeyRule[] _passengerRules =
        {
            new KeyRule { NameContains = "Woman", Key = "Women" },
            new KeyRule { NameContains = "Women", Key = "Women" },
            new KeyRule { NameContains = "Grandma", Key = "Grandma" },
            new KeyRule { NameContains = "Student", Key = "student" },
            new KeyRule { NameContains = "Worker", Key = "Worker" },
        };

        [Header("Villain -> GongikNagaController key (Knight, OldMan, Danso, Zaruban, HipHop)")]
        [SerializeField] private KeyRule[] _villainRules =
        {
            new KeyRule { NameContains = "Knight", Key = "Knight" },
            new KeyRule { NameContains = "SleepyMan", Key = "OldMan" },
            new KeyRule { NameContains = "OldMan", Key = "OldMan" },
            new KeyRule { NameContains = "Danso", Key = "Danso" },
            new KeyRule { NameContains = "Jarvan", Key = "Zaruban" },
            new KeyRule { NameContains = "Zaruban", Key = "Zaruban" },
            new KeyRule { NameContains = "HipHop", Key = "HipHop" },
        };

        [Header("Hide actors during a presentation")]
        [Tooltip("Hide the player and every NPC of the current car while a presentation plays; show them again " +
                 "after it has fully finished (fade out / zoom back) and the event session closed.")]
        [SerializeField] private bool _hideActors = true;
        [Tooltip("Empty = found automatically.")]
        [SerializeField] private PlayerController2D _player;

        private readonly List<Renderer> _hiddenRenderers = new List<Renderer>(64);
        private readonly List<Renderer> _rendererBuffer = new List<Renderer>(16);
        private bool _actorsHidden;
        private Func<bool> _isPlaying;

        private void Awake() => _isPlaying = IsPresentationPlaying;

        private void OnEnable()
        {
            EventBus<CounterattackEvent>.Subscribe(OnCounterattack);
            EventBus<VillainEjectStartedEvent>.Subscribe(OnEjectStarted);
            if (MinigameDirector.HasInstance) MinigameDirector.Instance.SetEventHold(_isPlaying);
        }

        private void Start()
        {
            // MinigameDirector may wake after this component.
            if (MinigameDirector.HasInstance) MinigameDirector.Instance.SetEventHold(_isPlaying);
            if (_player == null) _player = FindAnyObjectByType<PlayerController2D>();
        }

        private void OnDisable()
        {
            EventBus<CounterattackEvent>.Unsubscribe(OnCounterattack);
            EventBus<VillainEjectStartedEvent>.Unsubscribe(OnEjectStarted);
            if (MinigameDirector.HasInstance) MinigameDirector.Instance.ClearEventHold(_isPlaying);
            ShowActors();
        }

        // Wait for the session too: an ejected villain is removed when the session finishes, so showing earlier
        // would flash it for a frame.
        private void LateUpdate()
        {
            if (!_actorsHidden || IsPresentationPlaying()) return;
            if (MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive) return;
            ShowActors();
        }

        private void HideActors()
        {
            if (!_hideActors || _actorsHidden) return;
            _hiddenRenderers.Clear();
            if (_player != null) HideUnder(_player.transform);
            TrainCar car = TrainProgress.HasInstance ? TrainProgress.Instance.CurrentCar : null;
            if (car != null)
            {
                IReadOnlyList<Talkable> npcs = car.Npcs;
                for (int i = 0; i < npcs.Count; i++)
                {
                    if (npcs[i] != null) HideUnder(npcs[i].transform);
                }
            }
            _actorsHidden = true;
        }

        private void HideUnder(Transform root)
        {
            _rendererBuffer.Clear();
            root.GetComponentsInChildren(true, _rendererBuffer);
            for (int i = 0; i < _rendererBuffer.Count; i++)
            {
                Renderer r = _rendererBuffer[i];
                if (!r.enabled) continue; // keep renderers that were already off as they were
                r.enabled = false;
                _hiddenRenderers.Add(r);
            }
        }

        private void ShowActors()
        {
            for (int i = 0; i < _hiddenRenderers.Count; i++)
            {
                if (_hiddenRenderers[i] != null) _hiddenRenderers[i].enabled = true;
            }
            _hiddenRenderers.Clear();
            _actorsHidden = false;
        }

        private static bool IsPresentationPlaying() => PassengerAttackController.IsPlaying || GongikNagaController.IsPlaying;

        private void OnCounterattack(CounterattackEvent evt)
        {
            if (evt.IsVillain || evt.Target == null) return; // 시민의 반격 only
            string key = FindKey(_passengerRules, evt.Target.name);
            if (key == null)
            {
                Debug.LogWarning($"[PassengerEventPresenter] No attack key for passenger '{evt.Target.name}'.", evt.Target);
                return;
            }
            if (PassengerAttackController.PlayAttack(key)) HideActors();
        }

        private void OnEjectStarted(VillainEjectStartedEvent evt)
        {
            if (evt.Villain == null) return;
            string key = FindKey(_villainRules, evt.Villain.name);
            if (key == null)
            {
                Debug.LogWarning($"[PassengerEventPresenter] No naga key for villain '{evt.Villain.name}'.", evt.Villain);
                return;
            }
            if (GongikNagaController.Play(key)) HideActors();
        }

        private static string FindKey(KeyRule[] rules, string objectName)
        {
            if (rules == null || string.IsNullOrEmpty(objectName)) return null;
            for (int i = 0; i < rules.Length; i++)
            {
                if (string.IsNullOrEmpty(rules[i].NameContains)) continue;
                if (objectName.IndexOf(rules[i].NameContains, StringComparison.OrdinalIgnoreCase) >= 0) return rules[i].Key;
            }
            return null;
        }
    }
}
