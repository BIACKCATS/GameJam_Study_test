using System;
using JSH.Core;
using JSH.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Runs the minigames chosen in dialogue and applies their results (one per scene, e.g. on [Managers]).
    ///  - Accuse ("퇴치")   -> Reflex (no time limit).
    ///  - Persuade ("설득") -> Timing; villain success = converted + life +1, passenger success = no effect,
    ///    fail or timeout -> Reflex.
    ///  - Reflex: villain open = defeated; villain guarded = villain counterattack + life -1 + defeated;
    ///    passenger (any state) = passenger counterattack + life -1.
    /// Input: Left Click (Hit / Stop). UI reads ActiveReflex / ActiveTiming each frame and listens to the events.
    /// </summary>
    public class MinigameDirector : Singleton<MinigameDirector>
    {
        [SerializeField] private ReflexSettings _defaultReflex;
        [SerializeField] private TimingSettings _defaultTiming;

        private Talkable _target;
        private Villain _villain;
        private ReflexMinigame _reflex;
        private TimingMinigame _timing;

        public bool IsActive => _target != null;
        public Talkable CurrentTarget => _target;
        public MinigameType CurrentType => _reflex != null ? MinigameType.Reflex : _timing != null ? MinigameType.Timing : MinigameType.None;
        public ReflexMinigame ActiveReflex => _reflex;
        public TimingMinigame ActiveTiming => _timing;

        /// <summary>A round started (Reflex or Timing). Fired again when Timing fails over to Reflex.</summary>
        public event Action<MinigameType> RoundStarted;
        /// <summary>Reflex Guard/Open changed (true = guarding).</summary>
        public event Action<bool> ReflexGuardChanged;

        private void OnEnable() => EventBus<DialogueChoiceMadeEvent>.Subscribe(OnChoiceMade);

        private void OnDisable() => EventBus<DialogueChoiceMadeEvent>.Unsubscribe(OnChoiceMade);

        private void OnChoiceMade(DialogueChoiceMadeEvent evt)
        {
            if (evt.Effect != DialogueEffect.Accuse && evt.Effect != DialogueEffect.Persuade) return;
            if (IsActive || evt.Target == null) return;

            if (DialogueRunner.HasInstance) DialogueRunner.Instance.EndDialogue();
            Begin(evt.Target, evt.Effect == DialogueEffect.Accuse ? MinigameType.Reflex : MinigameType.Timing);
        }

        /// <summary>Start a minigame session directly (also used by the dialogue flow).</summary>
        public bool Begin(Talkable target, MinigameType type)
        {
            if (IsActive || target == null || type == MinigameType.None) return false;
            if (ResolveReflexSettings(target) == null || ResolveTimingSettings(target) == null)
            {
                Debug.LogError("[MinigameDirector] Missing Reflex/Timing settings (assign defaults on the director).", this);
                return false;
            }

            _target = target;
            target.TryGetComponent(out _villain);
            EventBus<MinigameStateChangedEvent>.Raise(new MinigameStateChangedEvent(true, type, _target));
            StartRound(type);
            return true;
        }

        private void Update()
        {
            if (!IsActive) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return; // pause-menu clicks must not hit

            bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            float dt = Time.deltaTime;

            if (_reflex != null)
            {
                if (_reflex.Tick(dt)) ReflexGuardChanged?.Invoke(_reflex.IsGuarding);
                if (click) _reflex.Hit();
                if (_reflex.IsFinished) FinishReflex();
            }
            else if (_timing != null)
            {
                _timing.Tick(dt);
                if (click) _timing.Stop();
                if (_timing.IsFinished) FinishTiming();
            }
        }

        private void StartRound(MinigameType type)
        {
            if (type == MinigameType.Reflex)
            {
                _timing = null;
                _reflex = new ReflexMinigame(ResolveReflexSettings(_target), _villain != null);
            }
            else
            {
                _reflex = null;
                _timing = new TimingMinigame(ResolveTimingSettings(_target));
            }
            RoundStarted?.Invoke(type);
        }

        private void FinishReflex()
        {
            ReflexResult result = _reflex.Result;
            switch (result)
            {
                case ReflexResult.VillainDefeatedOpen:
                    if (_villain != null) _villain.Defeat();
                    break;
                case ReflexResult.VillainDefeatedGuarded:
                    // Order matters for feedback: counterattack -> life -1 -> defeated.
                    if (_villain != null) EventBus<VillainCounterattackEvent>.Raise(new VillainCounterattackEvent(_villain));
                    DamagePlayer();
                    if (_villain != null) _villain.Defeat();
                    break;
                case ReflexResult.PassengerHit:
                    DamagePlayer(); // once, regardless of guard state
                    EventBus<PassengerCounterattackEvent>.Raise(new PassengerCounterattackEvent(_target, _reflex.HitWhileGuarding));
                    break;
            }

            bool success = result == ReflexResult.VillainDefeatedOpen || result == ReflexResult.VillainDefeatedGuarded;
            EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, MinigameType.Reflex, success, result));
            EndSession();
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

            EventBus<MinigameFinishedEvent>.Raise(new MinigameFinishedEvent(_target, MinigameType.Timing, success, ReflexResult.None));

            if (success) EndSession();
            else StartRound(MinigameType.Reflex); // persuasion failed -> Reflex, same session
        }

        /// <summary>Abort without applying results (e.g. scene change, cutscene).</summary>
        public void Cancel() => EndSession();

        private void EndSession()
        {
            if (!IsActive) return;
            Talkable target = _target;
            MinigameType type = CurrentType;
            _target = null;
            _villain = null;
            _reflex = null;
            _timing = null;
            EventBus<MinigameStateChangedEvent>.Raise(new MinigameStateChangedEvent(false, type, target));
        }

        private static void DamagePlayer()
        {
            if (LifeSystem.HasInstance) LifeSystem.Instance.Damage(1);
        }

        private ReflexSettings ResolveReflexSettings(Talkable t) => t.ReflexOverride != null ? t.ReflexOverride : _defaultReflex;

        private TimingSettings ResolveTimingSettings(Talkable t) => t.TimingOverride != null ? t.TimingOverride : _defaultTiming;

        protected override void OnDestroy()
        {
            EndSession();
            base.OnDestroy();
        }
    }
}
