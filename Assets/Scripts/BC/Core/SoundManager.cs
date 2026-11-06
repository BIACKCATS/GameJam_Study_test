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
        private readonly List<(FMOD.Sound sound, FMOD.Channel channel)> _samples = new List<(FMOD.Sound, FMOD.Channel)>();
        public void StopTrain() { StopAndRelease(ref _trainInstance); }
        public void PlaySample(TextAsset wav, float volume, float pitch = 1f)
        {
            if (!wav) { Debug.LogError("SoundManager: Missing WAV sample."); return; }
            var data = wav.bytes;
            var info = new FMOD.CREATESOUNDEXINFO { cbsize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)), length = (uint)data.Length };
            var result = RuntimeManager.CoreSystem.createSound(data, FMOD.MODE.OPENMEMORY | FMOD.MODE.CREATESAMPLE, ref info, out var sound);
            if (result != FMOD.RESULT.OK) { Debug.LogError("SoundManager: " + result); return; }
            result = RuntimeManager.CoreSystem.playSound(sound, default(FMOD.ChannelGroup), true, out var channel);
            if (result != FMOD.RESULT.OK) { sound.release(); Debug.LogError("SoundManager: " + result); return; }
            channel.setVolume(Mathf.Max(0f,volume)); channel.setPitch(Mathf.Max(0.01f,pitch)); channel.setPaused(false);
            _samples.Add((sound, channel));
        }
        // Exclusive presentation music: never overlap title, ending, or gameplay music.
        public void PlayPresentationMusic(string name)
        {
            StopAllMusic();
            StopSound(SoundNames.StartMusic);
            StopSound(SoundNames.FinalMusic);
            PlaySound(name, 0.5f);
        }
        private string _pendingGameScene;
        private int _pendingGameIndex = -1;
        public void QueueTrainForScene(string sceneName, int buildIndex)
        {
            _pendingGameScene = sceneName;
            _pendingGameIndex = buildIndex;
        }
        protected override void OnSingletonAwake()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            JSH.Core.EventBus<JSH.Gameplay.DialogueStateChangedEvent>.Subscribe(OnDialogueStateChanged);
        }
        private void OnDialogueStateChanged(JSH.Gameplay.DialogueStateChangedEvent evt)
        {
            if (evt.IsActive)
            {
                if (!TrueEndingController.IsPlaying && !GameOverSequenceController.IsPlaying)
                    StartAttack(SoundNames.Attack, MasterVolume);
            }
            else EndAttack(1f);
        }
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            bool match = !string.IsNullOrEmpty(_pendingGameScene)
                ? scene.name == System.IO.Path.GetFileNameWithoutExtension(_pendingGameScene)
                : _pendingGameIndex >= 0 && scene.buildIndex == _pendingGameIndex;
            if (!match) return;
            _pendingGameScene = null; _pendingGameIndex = -1;
            if (TrueEndingController.IsPlaying || GameOverSequenceController.IsPlaying) return;
            PlayTrain(SoundNames.Train, 0.8f);
            SetMasterVolume(0.5f);
        }
        private void LateUpdate()
        {
            if (Instance != this || CrosshairAimController.IsVisible) return;
            bool clicked = false;
#if ENABLE_INPUT_SYSTEM
            clicked = UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            clicked = Input.GetMouseButtonDown(0);
#endif
            if (clicked) PlaySound(SoundNames.UI, 0.8f);
        }
        private readonly List<(EventInstance instance, float end, float fadeStart, float amplitude)> _trimmedSounds = new List<(EventInstance, float, float, float)>();
        public void PlayTrimmedSound(string name, float volume, float trimEndSeconds, float fadeOutSeconds)
        {
            if (!TryCreate(name, out var sound)) return;
            sound.getDescription(out var description);
            if (description.getLength(out int milliseconds) != FMOD.RESULT.OK || milliseconds <= 0)
            { sound.release(); Debug.LogError("SoundManager: Cannot determine event duration: " + name); return; }
            float duration = Mathf.Max(0.01f, milliseconds / 1000f - Mathf.Max(0f,trimEndSeconds));
            float amplitude = sound.setParameterByName(MasterVolumeParameter,Mathf.Clamp01(volume),true) == FMOD.RESULT.OK ? 1f : Mathf.Clamp01(volume);
            sound.setVolume(amplitude);
            // Store base amplitude separately via the event parameter when available.
            float fade = Mathf.Clamp(fadeOutSeconds,0.001f,duration);
            sound.start();
            _trimmedSounds.Add((sound,Time.unscaledTime + duration,Time.unscaledTime + duration - fade,amplitude));
        }
        private const string EventPathPrefix = "event:/";
        private const string MasterVolumeParameter = "MasterVolume";
        private const string AttackParameter = "Attack";

        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Min(0f)] private float _attackFadeInSeconds = 0.5f;
        [SerializeField, Min(0.01f)] private float _attackFadeOutSeconds = 2f;

        private string _lastTrainName = SoundNames.Train;
        private bool _resumeTrainAfterAttack;
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
            _lastTrainName = songName;
            StopSound(SoundNames.StartMusic);
            StopSound(SoundNames.FinalMusic);
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

            _resumeTrainAfterAttack = true;
            StopTrain();
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
        /// One-shots release themselves when finished. Looping events (Turn_On_The_light, Writing, Letter_adding, StartMusic, FinalMusic)
        /// keep playing until <see cref="StopSound"/>; calling PlaySound again for a loop that is already playing
        /// only updates its volume instead of stacking a second copy.
        /// </summary>
        public void PlaySound(string soundName, float masterVolume)
        {
            PlaySound(soundName, masterVolume, null, 0f);
        }

        /// <summary>
        /// Same as above, also setting one extra local parameter before the sound starts
        /// (e.g. PlaySound(SoundNames.Firework, 0.8f, SoundNames.FireworkParameter, 1f)).
        /// For a loop that is already playing, the parameter is updated live.
        /// </summary>
        public void PlaySound(string soundName, float masterVolume, string parameterName, float parameterValue, float gain = 1f)
        {
            float volume = Mathf.Clamp01(masterVolume);
            bool hasParameter = !string.IsNullOrEmpty(parameterName);

            if (_loopingSounds.TryGetValue(soundName, out EventInstance playing) && playing.isValid())
            {
                SetParameter(playing, MasterVolumeParameter, volume);
                playing.setVolume(Mathf.Max(0f,gain));
                if (hasParameter)
                {
                    SetParameter(playing, parameterName, parameterValue);
                }

                return;
            }

            if (!TryCreate(soundName, out EventInstance instance))
            {
                return;
            }

            SetParameter(instance, MasterVolumeParameter, volume);
            instance.setVolume(Mathf.Max(0f,gain));
            if (hasParameter)
            {
                SetParameter(instance, parameterName, parameterValue);
            }

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
                instance.stop(soundName == SoundNames.StartMusic || soundName == SoundNames.FinalMusic
                    ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
            _resumeTrainAfterAttack = false;
            _isFadingAttack = false;
            _attackValue = 0f;
            _attackTarget = 0f;
            StopAndRelease(ref _trainInstance);
            StopAndRelease(ref _attackInstance);
        }

        private void Update()
        {
            for (int i = _trimmedSounds.Count - 1; i >= 0; i--)
            {
                var cue = _trimmedSounds[i];
                if (Time.unscaledTime >= cue.end)
                { var sound = cue.instance; StopAndRelease(ref sound); _trimmedSounds.RemoveAt(i); }
                else if (Time.unscaledTime >= cue.fadeStart)
                    cue.instance.setVolume(cue.amplitude * Mathf.Clamp01((cue.end-Time.unscaledTime)/(cue.end-cue.fadeStart)));
            }
            for (int i = _samples.Count - 1; i >= 0; i--)
            {
                if (_samples[i].channel.isPlaying(out bool playing) != FMOD.RESULT.OK || !playing)
                { _samples[i].sound.release(); _samples.RemoveAt(i); }
            }
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
                bool resume = _resumeTrainAfterAttack;
                _resumeTrainAfterAttack = false;
                if (resume && !TrueEndingController.IsPlaying && !GameOverSequenceController.IsPlaying
                    && !IsSoundPlaying(SoundNames.StartMusic) && !IsSoundPlaying(SoundNames.FinalMusic))
                    PlayTrain(_lastTrainName, MasterVolume);
            }
        }

        protected override void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            JSH.Core.EventBus<JSH.Gameplay.DialogueStateChangedEvent>.Unsubscribe(OnDialogueStateChanged);
            foreach (var sample in _samples) { sample.channel.stop(); sample.sound.release(); }
            _samples.Clear();
            foreach (var cue in _trimmedSounds) { var sound=cue.instance; StopAndRelease(ref sound); }
            _trimmedSounds.Clear();
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
