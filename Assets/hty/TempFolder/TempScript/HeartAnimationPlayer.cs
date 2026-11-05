using UnityEngine;

public sealed class HeartAnimationPlayer : MonoBehaviour
{
    public UnityEngine.UI.RawImage[] hearts = new UnityEngine.UI.RawImage[3];
    public Texture2D shimmerSheet;
    public Texture2D breakSheet;
    [Min(0.1f)] public float shimmerFps = 6f;
    [Min(0.1f)] public float breakFps = 18f;
    [Header("Heart Break Audio")]
    public AudioSource breakAudioSource;
    public AudioClip breakSound;
    [Range(0f, 1f)] public float breakSoundVolume = 0.4f;
    static HeartAnimationPlayer instance;
    readonly int[] states = new int[3]; // 0: shimmer, 1: breaking, 2: hidden
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
    public static void PlayBreak(int heartNumber)
    {
        if (heartNumber < 1 || heartNumber > 3) { Debug.LogError($"HeartAnimationPlayer.PlayBreak: Invalid number {heartNumber}; expected 1..3."); return; }
        if (!instance || !instance.ready || !instance.isActiveAndEnabled)
        { Debug.LogError("HeartAnimationPlayer.PlayBreak: No active, configured controller in the scene."); return; }
        int index = heartNumber - 1;
        if (instance.states[index] != 0)
        { Debug.LogError($"HeartAnimationPlayer.PlayBreak: Heart {heartNumber} is already breaking or hidden.", instance); return; }
        if (!instance.hearts[index]) { Debug.LogError($"HeartAnimationPlayer: Heart {heartNumber} reference is missing.", instance); return; }
        instance.states[index] = 1; instance.frames[index] = 0; instance.Render(index);
        if (!instance.breakAudioSource || !instance.breakSound)
            Debug.LogError("HeartAnimationPlayer: Assign Break Audio Source and Break Sound to play the heart break sound.", instance);
        else if (!instance.breakAudioSource.isActiveAndEnabled)
            Debug.LogError("HeartAnimationPlayer: Break Audio Source is disabled.", instance);
        else
            instance.breakAudioSource.PlayOneShot(instance.breakSound, instance.breakSoundVolume);
    }

    void Update()
    {
        if (!ready || instance != this) return;
        for (int i = 0; i < 3; i++)
        {
            if (states[i] == 2) continue;
            if (!hearts[i]) { Debug.LogError($"HeartAnimationPlayer: Heart {i + 1} was removed.", this); states[i] = 2; continue; }
            frames[i] += Time.deltaTime * Mathf.Max(0.1f, states[i] == 0 ? shimmerFps : breakFps);
            if (states[i] == 0) frames[i] = Mathf.Repeat(frames[i], 10);
            else if (frames[i] >= 10) states[i] = 2;
            Render(i);
        }
    }

    void Render(int index)
    {
        var heart = hearts[index];
        heart.enabled = states[index] != 2;
        if (!heart.enabled) return;
        heart.texture = states[index] == 0 ? shimmerSheet : breakSheet;
        int frame = Mathf.Clamp(Mathf.FloorToInt(frames[index]), 0, 9);
        heart.uvRect = new Rect((frame % 4) / 4f, 1f - (frame / 4 + 1) / 3f, 0.25f, 1f / 3f);
    }
    void OnDisable() { if (instance == this) instance = null; ready = false; }
}
