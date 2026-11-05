using System;
using JSH.Core;
using JSH.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Runs what a dialogue choice leads to and applies the results (one per scene; everything stays in one Scene).
    ///  - Accuse ("퇴치") on a villain   -> Eject event: VillainEjectStartedEvent, after the eject duration the
    ///                                      villain is defeated and removed from the car. No life change.
    ///  - Accuse on a passenger          -> Counterattack event (life -1).
    ///  - Persuade ("설득")               -> Timing minigame. Villain success = converted + life +1,
    ///                                      passenger success = no effect. Fail / timeout -> Counterattack (life -1),
    ///                                      villain stays unresolved (can be talked to again).
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
        private float _eventTimer;

        public bool IsActive => _target != null;
        public Talkable CurrentTarget => _target;
        public MinigameType CurrentType => _timing != null ? MinigameType.Timing : _eventType;
        public TimingMinigame ActiveTiming => _timing;

        /// <summary>A round started (Timing, Eject or Counterattack). Fired again when Timing fails into Counterattack.</summary>
        public event Action<MinigameType> RoundStarted;

        private void OnEnable() => EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoiceMade);

        private void OnDisable() => EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoiceMade);

        private void OnChoiceMade(DialogueChoiceMadeEvent evt)
        {
            if (evt.Effect != DialogueEffect.Accuse && evt.Effect != DialogueEffect.Persuade) return;
            if (IsActive || evt.Target == null) return;

            if (DialogueRunner.HasInstance) DialogueRunner.Instance.EndDialogue();
            if (evt.Effect == DialogueEffect.Persuade) Begin(evt.Target, MinigameType.Timing);
            else Begin(evt.Target, evt.Target.TryGetComponent(out Villain _) ? MinigameType.Eject : MinigameType.Counterattack);
        }

        /// <summary>
        /// Start a session directly (also used by the dialogue flow). Counterattack started here uses
        /// CounterattackReason.AccusedPassenger.
        /// </summary>
        public bool Begin(Talkable target, MinigameType type)
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
            // Only a villain persuasion pays off (converted + life +1). Passenger success = no effect.
            if (success && _villain != null)
            {
                if (LifeSystem.HasInstance) LifeSystem.Instance.Heal(1);
                _villain.Convert();
            }

            EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, MinigameType.Timing, success));

            if (success) EndSession();
            else StartCounterattack(CounterattackReason.PersuadeFailed); // same session, no input-lock flicker
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
