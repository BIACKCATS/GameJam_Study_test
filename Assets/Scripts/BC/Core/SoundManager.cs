using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace BC.Core
{
    /// <summary>
    /// Plays FMOD events from SFX.fspro.
    /// - MasterVolume (local, 0..1, initial 0): automated on every event's volume, so it must be set on each instance.
    /// - Attack (local, 0..1): crossfades the "Train" (0 = 100%) and "Attack" (1 = 100%) music loops.
    /// Usage: SoundManager.Instance.PlayTrain(SoundNames.Train, 0.8f);
    ///        SoundManager.Instance.PlaySound(SoundNames.UI, 0.8f);
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        private const string EventPathPrefix = "event:/";
        private const string MasterVolumeParameter = "MasterVolume";
        private const string AttackParameter = "Attack";

        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Min(0f)] private float _attackFadeInSeconds = 0.5f;
        [SerializeField, Min(0.01f)] private float _attackFadeOutSeconds = 2f;

        private EventInstance _trainInstance;
        private EventInstance _attackInstance;
        private float _attackValue;
        private float _attackTarget;
        private float _attackFadeSpeed;
        private bool _isFadingAttack;

        // Looping SFX started by PlaySound, keyed by event name, so StopSound can end them.
        private readonly Dictionary<string, EventInstance> _loopingSounds = new Dictionary<string, EventInstance>();

        public float MasterVolume => _masterVolume;
        public bool IsAttackPlaying => _attackInstance.isValid();

        /// <summary>Starts the base music loop (Attack = 0, so the train track is at 100%).</summary>
        public void PlayTrain(string songName, float masterVolume)
        {
            SetMasterVolume(masterVolume);
            StopAndRelease(ref _trainInstance);

            if (!TryCreate(songName, out _trainInstance))
            {
                return;
            }

            ApplyParameters(_trainInstance);
            _trainInstance.start();
        }

        /// <summary>
        /// Keeps the train loop running, starts the attack loop, and raises Attack 0 → 1 over the Inspector fade-in time.
        /// Called while the attack loop is still fading out, it keeps that loop and fades it back up instead of restarting it.
        /// </summary>
        public void StartAttack(string songName, float masterVolume)
        {
            StartAttack(songName, masterVolume, _attackFadeInSeconds);
        }

        /// <summary>Same as above with an explicit fade-in time. 0 = snap to 1 immediately.</summary>
        public void StartAttack(string songName, float masterVolume, float fadeInSeconds)
        {
            SetMasterVolume(masterVolume);

            if (!_attackInstance.isValid())
            {
                if (!TryCreate(songName, out _attackInstance))
                {
                    return;
                }

                ApplyParameters(_attackInstance);
                _attackInstance.start();
            }

            FadeAttackTo(1f, fadeInSeconds);
        }

        /// <summary>Lowers Attack → 0 over the Inspector fade-out time, then stops the attack loop.</summary>
        public void EndAttack()
        {
            EndAttack(_attackFadeOutSeconds);
        }

        /// <summary>Lowers Attack → 0 over <paramref name="fadeOutSeconds"/>, then stops the attack loop.</summary>
        public void EndAttack(float fadeOutSeconds)
        {
            FadeAttackTo(0f, fadeOutSeconds);
        }

        /// <summary>
        /// Plays any sound effect from <see cref="SoundNames"/>.
        /// One-shots release themselves when finished. Looping events (Turn_On_The_light, Writing, Letter_adding)
        /// keep playing until <see cref="StopSound"/>; calling PlaySound again for a loop that is already playing
        /// only updates its volume instead of stacking a second copy.
        /// </summary>
        public void PlaySound(string soundName, float masterVolume)
        {
            float volume = Mathf.Clamp01(masterVolume);

            if (_loopingSounds.TryGetValue(soundName, out EventInstance playing) && playing.isValid())
            {
                SetParameter(playing, MasterVolumeParameter, volume);
                return;
            }

            if (!TryCreate(soundName, out EventInstance instance))
            {
                return;
            }

            SetParameter(instance, MasterVolumeParameter, volume);
            instance.start();

            if (IsLooping(instance))
            {
                _loopingSounds[soundName] = instance;
            }
            else
            {
                instance.release();
            }
        }

        /// <summary>Stops a looping sound started with <see cref="PlaySound"/>. Fades out if the event has an AHDSR release.</summary>
        public void StopSound(string soundName)
        {
            if (!_loopingSounds.TryGetValue(soundName, out EventInstance instance))
            {
                return;
            }

            _loopingSounds.Remove(soundName);
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }
        }

        public bool IsSoundPlaying(string soundName)
        {
            return _loopingSounds.TryGetValue(soundName, out EventInstance instance) && instance.isValid();
        }

        /// <summary>Changes the master volume of the music and looping sounds playing now, and of every later music call.</summary>
        public void SetMasterVolume(float masterVolume)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            SetParameter(_trainInstance, MasterVolumeParameter, _masterVolume);
            SetParameter(_attackInstance, MasterVolumeParameter, _masterVolume);

            foreach (EventInstance instance in _loopingSounds.Values)
            {
                SetParameter(instance, MasterVolumeParameter, _masterVolume);
            }
        }

        public void StopAllMusic()
        {
            _isFadingAttack = false;
            _attackValue = 0f;
            _attackTarget = 0f;
            StopAndRelease(ref _trainInstance);
            StopAndRelease(ref _attackInstance);
        }

        private void Update()
        {
            if (!_isFadingAttack)
            {
                return;
            }

            SetAttackValue(Mathf.MoveTowards(_attackValue, _attackTarget, _attackFadeSpeed * Time.unscaledDeltaTime));

            if (Mathf.Approximately(_attackValue, _attackTarget))
            {
                FinishAttackFade();
            }
        }

        private void FadeAttackTo(float target, float seconds)
        {
            _attackTarget = Mathf.Clamp01(target);

            if (seconds <= 0f)
            {
                SetAttackValue(_attackTarget);
                FinishAttackFade();
                return;
            }

            _attackFadeSpeed = 1f / seconds;
            _isFadingAttack = true;
        }

        private void FinishAttackFade()
        {
            _isFadingAttack = false;

            if (_attackTarget <= 0f)
            {
                StopAndRelease(ref _attackInstance);
            }
        }

        protected override void OnDestroy()
        {
            StopAllMusic();

            foreach (EventInstance instance in _loopingSounds.Values)
            {
                EventInstance toStop = instance;
                StopAndRelease(ref toStop);
            }

            _loopingSounds.Clear();
            base.OnDestroy();
        }

        private static bool IsLooping(EventInstance instance)
        {
            return instance.getDescription(out EventDescription description) == FMOD.RESULT.OK
                && description.isOneshot(out bool isOneshot) == FMOD.RESULT.OK
                && !isOneshot;
        }

        private void SetAttackValue(float value)
        {
            _attackValue = Mathf.Clamp01(value);
            ApplyParameters(_trainInstance);
            ApplyParameters(_attackInstance);
        }

        private void ApplyParameters(EventInstance instance)
        {
            SetParameter(instance, MasterVolumeParameter, _masterVolume);
            SetParameter(instance, AttackParameter, _attackValue);
        }

        private static void SetParameter(EventInstance instance, string parameter, float value)
        {
            if (instance.isValid())
            {
                instance.setParameterByName(parameter, value, true);
            }
        }

        private static bool TryCreate(string eventName, out EventInstance instance)
        {
            string path = eventName.StartsWith(EventPathPrefix) ? eventName : EventPathPrefix + eventName;

            try
            {
                instance = RuntimeManager.CreateInstance(path);
                return true;
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning($"[SoundManager] FMOD event not found: {path} (names are case-sensitive, see SoundNames)");
                instance = default;
                return false;
            }
        }

        private static void StopAndRelease(ref EventInstance instance)
        {
            if (!instance.isValid())
            {
                return;
            }

            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
            instance.clearHandle();
        }
    }
}
