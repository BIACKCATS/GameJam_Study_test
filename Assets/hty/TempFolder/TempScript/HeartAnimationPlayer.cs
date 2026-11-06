using UnityEngine;
using BC.Core;

public sealed class HeartAnimationPlayer : MonoBehaviour
{
    public UnityEngine.UI.RawImage[] hearts = new UnityEngine.UI.RawImage[3];
    public Texture2D shimmerSheet;
    public Texture2D breakSheet;
    public Texture2D healSheet;
    [Min(0.1f)] public float healFps = 15f;
    [Min(0.1f)] public float shimmerFps = 6f;
    [Min(0.1f)] public float breakFps = 18f;
    [Header("Heart Break Audio")]
    public AudioSource breakAudioSource;
    public AudioClip breakSound;
    public TextAsset healSound;
    [Range(0f, 1f)] public float breakSoundVolume = 0.4f;
    static HeartAnimationPlayer instance;
    readonly int[] states = new int[3]; // 0: shimmer, 1: breaking, 2: hidden, 3: healing
    readonly float[] frames = new float[3];
    bool ready;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }

    void OnEnable()
    {
        if (instance && instance != this) { Debug.LogError("HeartAnimationPlayer: Multiple active controllers.", this); return; }
        if (hearts == null || hearts.Length != 3 || !hearts[0] || !hearts[1] || !hearts[2] || !shimmerSheet || !breakSheet)
        { Debug.LogError("HeartAnimationPlayer: Assign exactly three hearts and both animation sheets.", this); return; }
        instance = this; ready = true;
        for (int i = 0; i < 3; i++) Render(i);
    }

    /// <summary>Break one heart, numbered 1 to 3 from left to right.</summary>
    public static void PlayBreak(int heartNumber) { TryPlayBreak(heartNumber); }
    public static void PlayBreakSound()
    {
        if(SoundManager.Instance) SoundManager.Instance.PlaySound(SoundNames.HealthLose,instance ? instance.breakSoundVolume : 0.4f,null,0f,3f);
    }
    public static bool TryPlayBreak(int heartNumber, bool playSound = true)
    {
        if(TrueEndingController.IsPlaying)return false;
        if (heartNumber < 1 || heartNumber > 3) { Debug.LogError($"HeartAnimationPlayer.PlayBreak: Invalid number {heartNumber}; expected 1..3."); return false; }
        if (!instance || !instance.ready || !instance.isActiveAndEnabled)
        { Debug.LogError("HeartAnimationPlayer.PlayBreak: No active, configured controller in the scene."); return false; }
        int index = heartNumber - 1;
        if (instance.states[index] != 0)
        { Debug.LogError($"HeartAnimationPlayer.PlayBreak: Heart {heartNumber} is already breaking or hidden.", instance); return false; }
        if (!instance.hearts[index]) { Debug.LogError($"HeartAnimationPlayer: Heart {heartNumber} reference is missing.", instance); return false; }
        instance.states[index] = 1; instance.frames[index] = 0; instance.Render(index);
        if(playSound) PlayBreakSound();
        return true;
    }

    /// <summary>Restore a broken heart (1..3), then resume its shimmer.</summary>
    public static void PlayHeal(int heartNumber)
    {
        if(TrueEndingController.IsPlaying)return;
        if (heartNumber < 1 || heartNumber > 3) { Debug.LogError($"HeartAnimationPlayer.PlayHeal: Invalid number {heartNumber}; expected 1..3."); return; }
        if (!instance || !instance.ready || !instance.isActiveAndEnabled)
        { Debug.LogError("HeartAnimationPlayer.PlayHeal: No active configured controller."); return; }
        int index = heartNumber - 1;
        if (!instance.healSheet || !instance.hearts[index]) { Debug.LogError("HeartAnimationPlayer.PlayHeal: Missing heal sheet or heart reference.", instance); return; }
        if (instance.states[index] == 0 || instance.states[index] == 3)
        { Debug.LogError($"HeartAnimationPlayer.PlayHeal: Heart {heartNumber} is already healthy or healing.", instance); return; }
        instance.states[index] = 3; instance.frames[index] = 0; instance.Render(index);
        if(SoundManager.Instance) SoundManager.Instance.PlaySample(instance.healSound,4.8f);
    }

    void Update()
    {
        if (!ready || instance != this) return;
        for (int i = 0; i < 3; i++)
        {
            if (states[i] == 2) continue;
            if (!hearts[i]) { Debug.LogError($"HeartAnimationPlayer: Heart {i + 1} was removed.", this); states[i] = 2; continue; }
            frames[i] += Time.deltaTime * Mathf.Max(0.1f, states[i] == 0 ? shimmerFps : states[i] == 3 ? healFps : breakFps);
            if (states[i] == 0) frames[i] = Mathf.Repeat(frames[i], 10);
            else if (frames[i] >= 10) { states[i] = states[i] == 3 ? 0 : 2; frames[i] = 0; }
            Render(i);
        }
    }

    void Render(int index)
    {
        var heart = hearts[index];
        heart.enabled = states[index] != 2;
        if (!heart.enabled) return;
        heart.texture = states[index] == 0 ? shimmerSheet : states[index] == 3 ? healSheet : breakSheet;
        int frame = Mathf.Clamp(Mathf.FloorToInt(frames[index]), 0, 9);
        heart.uvRect = new Rect((frame % 4) / 4f, 1f - (frame / 4 + 1) / 3f, 0.25f, 1f / 3f);
    }
    void OnDisable() { if (instance == this) instance = null; ready = false; }
}
