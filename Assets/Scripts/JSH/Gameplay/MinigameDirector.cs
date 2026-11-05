using System;
using JSH.Core;
using JSH.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Runs what a dialogue outcome (DialogueOutcomeReachedEvent) leads to and applies the results
    /// (one per scene; everything stays in one Scene). The Talkable's dialogue is already finished (no more talks)
    /// unless stated otherwise:
    ///  - Embrace ("포용"/설득) -> Timing minigame (CurrentGoal = Embrace).
    ///      villain:   success = converted + life +1. Fail / timeout = Counterattack (life -1), stays unresolved.
    ///      passenger: success = no effect. Fail / timeout = no life change, dialogue restarts from the beginning.
    ///  - Defeat ("퇴치") -> Timing minigame (CurrentGoal = Defeat).
    ///      villain:   success = Eject event (VillainEjectStartedEvent, then defeated + removed from the car).
    ///                 Fail / timeout = Counterattack (life -1), stays unresolved.
    ///      passenger: success or fail = Counterattack (life -1) - accusing an innocent always backfires.
    /// Legacy choice effects (DialogueEffect.Persuade / Accuse) map to Embrace / Defeat.
    /// Eject / Counterattack are timed placeholders until their presentation exists: the session (input lock)
    /// lasts the configured seconds, presentation listens to the start events.
    /// Input: Left Click = stop the Timing line. UI reads ActiveTiming each frame and listens to the events.
    /// </summary>
    public class MinigameDirector : Singleton<MinigameDirector>
    {
        [SerializeField] private TimingSettings _defaultTiming;
        [Tooltip("Seconds the eject event keeps the session active (scaled time) before the villain is removed.")]
        [SerializeField, Min(0f)] private float _ejectDuration = 1.5f;
        [Tooltip("Seconds the counterattack event keeps the session active (scaled time).")]
        [SerializeField, Min(0f)] private float _counterattackDuration = 1f;

        private Talkable _target;
        private Villain _villain;
        private TimingMinigame _timing;
        private MinigameType _eventType = MinigameType.None;
        private DialogueOutcome _goal = DialogueOutcome.None;
        private float _eventTimer;

        public bool IsActive => _target != null;
        public Talkable CurrentTarget => _target;
        public MinigameType CurrentType => _timing != null ? MinigameType.Timing : _eventType;
        public TimingMinigame ActiveTiming => _timing;
        /// <summary>What the current Timing minigame decides: Embrace (persuade) or Defeat (퇴치). None outside Timing sessions.</summary>
        public DialogueOutcome CurrentGoal => _goal;

        /// <summary>A round started (Timing, Eject or Counterattack). Fired again when Timing fails into Counterattack.</summary>
        public event Action<MinigameType> RoundStarted;

        private void OnEnable()
        {
            EventBus<DialogueOutcomeReachedEvent>.Subscribe(OnOutcomeReached);
            EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoiceMade);
        }

        private void OnDisable()
        {
            EventBus<DialogueOutcomeReachedEvent>.Unsubscribe(OnOutcomeReached);
            EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoiceMade);
        }

        private void OnOutcomeReached(DialogueOutcomeReachedEvent evt) => Resolve(evt.Target, evt.Outcome);

        // Legacy: hand-made DialogueData whose choices carry Persuade / Accuse effects.
        private void OnChoiceMade(DialogueChoiceMadeEvent evt)
        {
            DialogueOutcome outcome = evt.Effect == DialogueEffect.Persuade ? DialogueOutcome.Embrace
                : evt.Effect == DialogueEffect.Accuse ? DialogueOutcome.Defeat
                : DialogueOutcome.None;
            if (outcome == DialogueOutcome.None || IsActive || evt.Target == null) return;

            if (DialogueRunner.HasInstance) DialogueRunner.Instance.EndDialogue();
            evt.Target.FinishDialogue();
            Resolve(evt.Target, outcome);
        }

        /// <summary>Start what a dialogue outcome leads to (see class summary).</summary>
        public bool Resolve(Talkable target, DialogueOutcome outcome)
        {
            if (outcome == DialogueOutcome.None || target == null) return false;
            return Begin(target, MinigameType.Timing, outcome);
        }

        /// <summary>
        /// Start a session directly (also used by the dialogue flow). Counterattack started here uses
        /// CounterattackReason.AccusedPassenger. timingGoal only matters for Timing.
        /// </summary>
        public bool Begin(Talkable target, MinigameType type, DialogueOutcome timingGoal = DialogueOutcome.Embrace)
        {
            if (IsActive || target == null || type == MinigameType.None) return false;
            target.TryGetComponent(out Villain villain);
            if (type == MinigameType.Timing && ResolveTimingSettings(target) == null)
            {
                Debug.LogError("[MinigameDirector] Missing Timing settings (assign the default on the director).", this);
                return false;
            }
            if (type == MinigameType.Eject && villain == null)
            {
                Debug.LogError($"[MinigameDirector] Eject needs a Villain: {target.name}.", target);
                return false;
            }

            _target = target;
            _villain = villain;
            _goal = type == MinigameType.Timing ? (timingGoal == DialogueOutcome.Defeat ? DialogueOutcome.Defeat : DialogueOutcome.Embrace)
                : DialogueOutcome.None;
            EventBus<MinigameStateChangedEvent>.Raise(new MinigameStateChangedEvent(true, type, _target));

            if (type == MinigameType.Timing) StartTiming();
            else if (type == MinigameType.Eject) StartEject();
            else StartCounterattack(CounterattackReason.AccusedPassenger);
            return true;
        }

        private void Update()
        {
            if (!IsActive) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return; // pause-menu clicks must not hit

            float dt = Time.deltaTime;
            if (_timing != null)
            {
                _timing.Tick(dt);
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) _timing.Stop();
                if (_timing.IsFinished) FinishTiming();
            }
            else if (_eventType != MinigameType.None)
            {
                _eventTimer -= dt;
                if (_eventTimer <= 0f) FinishEvent();
            }
        }

        private void StartTiming()
        {
            _eventType = MinigameType.None;
            _timing = new TimingMinigame(ResolveTimingSettings(_target));
            RoundStarted?.Invoke(MinigameType.Timing);
        }

        private void StartEject()
        {
            _timing = null;
            _eventType = MinigameType.Eject;
            _eventTimer = _ejectDuration;
            RoundStarted?.Invoke(MinigameType.Eject);
            EventBus<VillainEjectStartedEvent>.Raise(new VillainEjectStartedEvent(_villain));
        }

        private void StartCounterattack(CounterattackReason reason)
        {
            _timing = null;
            _eventType = MinigameType.Counterattack;
            _eventTimer = _counterattackDuration;
            RoundStarted?.Invoke(MinigameType.Counterattack);
            EventBus<CounterattackEvent>.Raise(new CounterattackEvent(_target, _villain != null, reason));
            if (LifeSystem.HasInstance) LifeSystem.Instance.Damage(1);
        }

        private void FinishTiming()
        {
            bool success = _timing.Success;

            if (_goal == DialogueOutcome.Defeat)
            {
                EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, MinigameType.Timing, success));
                if (_villain == null) StartCounterattack(CounterattackReason.AccusedPassenger); // passenger: always backfires
                else if (success) StartEject(); // same session, no input-lock flicker
                else StartCounterattack(CounterattackReason.DefeatFailed);
                return;
            }

            // Embrace: only a villain persuasion pays off (converted + life +1). Passenger success = no effect.
            if (success && _villain != null)
            {
                if (LifeSystem.HasInstance) LifeSystem.Instance.Heal(1);
                _villain.Convert();
            }

            EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, MinigameType.Timing, success));

            if (success) EndSession();
            else if (_villain != null) StartCounterattack(CounterattackReason.PersuadeFailed);
            else
            {
                _target.ResetDialogue(); // passenger: no penalty, talk again from the beginning
                EndSession();
            }
        }

        private void FinishEvent()
        {
            MinigameType type = _eventType;
            if (type == MinigameType.Eject && _villain != null)
            {
                _villain.Defeat();
                EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, type, true));
                RemoveFromCar(_target);
            }
            else
            {
                EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, type, false));
            }
            EndSession();
        }

        private static void RemoveFromCar(Talkable npc)
        {
            bool removed = TrainProgress.HasInstance && TrainProgress.Instance.CurrentCar != null
                           && TrainProgress.Instance.CurrentCar.RemoveNpc(npc);
            if (!removed) npc.gameObject.SetActive(false); // hand-placed NPC outside the car's pool
        }

        /// <summary>Abort without applying results (e.g. ending screen).</summary>
        public void Cancel() => EndSession();

        private void EndSession()
        {
            if (!IsActive) return;
            Talkable target = _target;
            MinigameType type = CurrentType;
            _target = null;
            _villain = null;
            _timing = null;
            _eventType = MinigameType.None;
            _goal = DialogueOutcome.None;
            EventBus<MinigameStateChangedEvent>.Raise(new MinigameStateChangedEvent(false, type, target));
        }

        private TimingSettings ResolveTimingSettings(Talkable t) => t.TimingOverride != null ? t.TimingOverride : _defaultTiming;

        protected override void OnDestroy()
        {
            EndSession();
            base.OnDestroy();
        }
    }
}
