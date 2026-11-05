using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class TrainSparkPlayer : MonoBehaviour
{
    public TrainMotion motion;
    public Texture2D sheet;
    public Texture2D radialSheet;
    [Range(0, 1)] public float radialChance = 0.5f;
    public Vector3 positionMin;
    public Vector3 positionMax;
    public AudioSource audioSource;
    public AudioClip[] sparkSounds;
    [Range(0, 1)] public float soundVolume = 0.35f;
    [Min(1)] public float framesPerSecond = 30;
    [Min(1)] public float pixelsPerUnit = 100;
    Sprite[] frames;
    Sprite[] directionalFrames, radialFrames;
    readonly System.Random random = new System.Random();
    public bool LastWasRadial { get; private set; }
    float Range(float a, float b) => Mathf.Lerp(a, b, (float)random.NextDouble());
    SpriteRenderer target;
    float elapsed;
    bool playing;
    public int BurstCount { get; private set; }
    [SerializeField, Min(0)] int bumpCount;
    public int BumpCount => bumpCount;
    void Awake()
    {
        target = GetComponent<SpriteRenderer>();
        directionalFrames = MakeFrames(sheet, 3, 96, new Vector2(0.1875f, 0.2916667f));
        radialFrames = MakeFrames(radialSheet, 4, 128, new Vector2(0.5f, 0.5f));
        target.enabled = false;
    }
    Sprite[] MakeFrames(Texture2D texture, int columns, int height, Vector2 pivot)
    {
        if (!texture) return null;
        var result = new Sprite[10];
        for (int i = 0; i < result.Length; i++)
            result[i] = Sprite.Create(texture, new Rect(i % columns * 128, texture.height - (i / columns + 1) * height, 128, height), pivot, Mathf.Max(1, pixelsPerUnit));
        return result;
    }
    void OnEnable() { bumpCount = 0; if (motion) motion.Bumped += CountBump; }
    void CountBump()
    {
        bumpCount++;
        if (bumpCount >= 5) PlayBurst();
    }
    void OnDisable()
    {
        if (motion) motion.Bumped -= CountBump;
        playing = false;
        if (target) target.enabled = false;
        if (audioSource) audioSource.Stop();
    }
    public void PlayBurst()
    {
        LastWasRadial = radialFrames != null && (directionalFrames == null || random.NextDouble() < radialChance);
        frames = LastWasRadial ? radialFrames : directionalFrames;
        if (frames == null) { Debug.LogError("TrainSparkPlayer: No spark animation frames assigned.", this); return; }
        transform.localPosition = new Vector3(Range(positionMin.x, positionMax.x), Range(positionMin.y, positionMax.y), Range(positionMin.z, positionMax.z));
        target.flipX = !LastWasRadial;
        elapsed = 0; playing = true; BurstCount++;
        bumpCount = 0;
        target.sprite = frames[0]; target.enabled = true;
        if (audioSource && sparkSounds != null && sparkSounds.Length > 0)
        {
            var clip = sparkSounds[random.Next(sparkSounds.Length)];
            if (clip) { audioSource.Stop(); audioSource.pitch = Range(0.9f, 1.1f); audioSource.PlayOneShot(clip, soundVolume); }
        }
    }
    void Update()
    {
        if (!playing) return;
        elapsed += Time.deltaTime;
        int index = Mathf.FloorToInt(elapsed * Mathf.Max(1, framesPerSecond));
        if (index >= frames.Length) { playing = false; target.enabled = false; return; }
        target.sprite = frames[index];
    }
    void OnDestroy()
    {
        if (directionalFrames != null) foreach (var frame in directionalFrames) if (frame) Destroy(frame);
        if (radialFrames != null) foreach (var frame in radialFrames) if (frame) Destroy(frame);
    }
}
