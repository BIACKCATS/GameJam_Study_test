using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace JSH.Core
{
    /// <summary>
    /// BGM crossfade (2 sources) + pooled SFX voices + saved volume settings.
    /// Usage: AudioManager.Instance.PlaySfx(clip, 1f, 0.1f);
    /// </summary>
    public sealed class AudioManager : Singleton<AudioManager>
    {
        private const string MasterKey = "Audio.Master";
        private const string BgmKey = "Audio.Bgm";
        private const string SfxKey = "Audio.Sfx";

        [SerializeField] private AudioMixerGroup _bgmGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField, Min(1)] private int _sfxVoices = 16;
        [SerializeField, Min(0f)] private float _defaultBgmFade = 0.5f;

        private readonly AudioSource[] _bgmSources = new AudioSource[2];
        private AudioSource[] _sfxSources;
        private int _activeBgm;
        private int _nextSfx;
        private float _bgmClipVolume = 1f;
        private Coroutine _bgmFade;

        public float MasterVolume { get; private set; }
        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public AudioClip CurrentBgm => _bgmSources[_activeBgm].clip;

        private float BgmTarget => MasterVolume * BgmVolume * _bgmClipVolume;

        protected override void OnSingletonAwake()
        {
            MasterVolume = SaveSystem.GetFloat(MasterKey, 1f);
            BgmVolume = SaveSystem.GetFloat(BgmKey, 0.7f);
            SfxVolume = SaveSystem.GetFloat(SfxKey, 1f);

            for (int i = 0; i < _bgmSources.Length; i++)
            {
                _bgmSources[i] = CreateSource(_bgmGroup, true);
            }

            _sfxSources = new AudioSource[_sfxVoices];
            for (int i = 0; i < _sfxVoices; i++)
            {
                _sfxSources[i] = CreateSource(_sfxGroup, false);
            }
        }

        // ---------- BGM ----------

        public void PlayBgm(AudioClip clip, float volume = 1f, float fadeTime = -1f)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource current = _bgmSources[_activeBgm];
            if (current.clip == clip && current.isPlaying)
            {
                return;
            }

            _bgmClipVolume = volume;
            _activeBgm = 1 - _activeBgm;
            AudioSource next = _bgmSources[_activeBgm];
            next.clip = clip;
            next.volume = 0f;
            next.Play();

            StartBgmFade(current, next, fadeTime);
        }

        public void StopBgm(float fadeTime = -1f)
        {
            StartBgmFade(_bgmSources[_activeBgm], null, fadeTime);
        }

        private void StartBgmFade(AudioSource from, AudioSource to, float fadeTime)
        {
            if (_bgmFade != null)
            {
                StopCoroutine(_bgmFade);
            }

            _bgmFade = StartCoroutine(BgmFadeRoutine(from, to, fadeTime < 0f ? _defaultBgmFade : fadeTime));
        }

        private IEnumerator BgmFadeRoutine(AudioSource from, AudioSource to, float duration)
        {
            float fromStart = from.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / duration;
                from.volume = fromStart * (1f - k);
                if (to != null)
                {
                    to.volume = BgmTarget * k;
                }

                yield return null;
            }

            from.volume = 0f;
            from.Stop();
            if (to != null)
            {
                to.volume = BgmTarget;
            }

            _bgmFade = null;
        }

        // ---------- SFX ----------

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchVariance = 0f)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = GetFreeSfxSource();
            source.clip = clip;
            source.volume = MasterVolume * SfxVolume * volume;
            source.pitch = pitchVariance > 0f ? 1f + Random.Range(-pitchVariance, pitchVariance) : 1f;
            source.Play();
        }

        private AudioSource GetFreeSfxSource()
        {
            int count = _sfxSources.Length;
            for (int i = 0; i < count; i++)
            {
                int index = (_nextSfx + i) % count;
                if (!_sfxSources[index].isPlaying)
                {
                    _nextSfx = (index + 1) % count;
                    return _sfxSources[index];
                }
            }

            // All busy: steal the oldest voice (round-robin).
            AudioSource stolen = _sfxSources[_nextSfx];
            _nextSfx = (_nextSfx + 1) % count;
            return stolen;
        }

        // ---------- Volume ----------

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            SaveSystem.SetFloat(MasterKey, MasterVolume);
            ApplyBgmVolume();
        }

        public void SetBgmVolume(float value)
        {
            BgmVolume = Mathf.Clamp01(value);
            SaveSystem.SetFloat(BgmKey, BgmVolume);
            ApplyBgmVolume();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            SaveSystem.SetFloat(SfxKey, SfxVolume);
        }

        private void ApplyBgmVolume()
        {
            if (_bgmFade == null && _bgmSources[_activeBgm].isPlaying)
            {
                _bgmSources[_activeBgm].volume = BgmTarget;
            }
        }

        private AudioSource CreateSource(AudioMixerGroup group, bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }
    }
}
