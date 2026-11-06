using UnityEngine;

public sealed class TrainMotion : MonoBehaviour
{
    public event System.Action Bumped;
    public Renderer background;
    public Transform interior;
    [Min(0)] public float scrollSpeed = 0.15f;
    [Min(0)] public float shakeHeight = 0.025f;
    [Min(0)] public float shakeFrequency = 7f;
    [Min(0)] public float rollDegrees = 0.12f;
    public Vector2 quietInterval = new Vector2(1.2f, 2.8f);
    [Min(0.05f)] public float bumpDuration = 0.3f;
    [Range(0, 1)] public float doubleBumpChance = 0.3f;
    [Min(0)] public float doubleBumpGap = 0.08f;
    float waitRemaining, bumpElapsed, bumpStrength;
    bool bumpActive, secondBumpPending;
    readonly System.Random random = new System.Random();
    float RandomRange(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
    float NextQuietInterval() => RandomRange(Mathf.Max(0.1f, quietInterval.x), Mathf.Max(Mathf.Max(0.1f, quietInterval.x), quietInterval.y));
    Vector3 restPosition;
    Quaternion restRotation;
    MaterialPropertyBlock properties;
    float offset;
    void OnEnable()
    {
        if (interior) { restPosition = interior.localPosition; restRotation = interior.localRotation; }
        properties = new MaterialPropertyBlock();
        bumpActive = secondBumpPending = false;
        waitRemaining = NextQuietInterval();
    }
    void Update()
    {
        offset = Mathf.Repeat(offset + scrollSpeed * Time.deltaTime, 1f);
        if (background) {
            background.GetPropertyBlock(properties);
            properties.SetFloat("_Scroll", offset);
            background.SetPropertyBlock(properties);
        }
        if (!interior) return;
        float bump = 0f;
        if (!bumpActive)
        {
            waitRemaining -= Time.deltaTime;
            if (waitRemaining <= 0f)
            {
                bumpActive = true;
                bumpElapsed = 0f;
                bumpStrength = RandomRange(0.75f, 1f);
                if (secondBumpPending) secondBumpPending = false;
                else secondBumpPending = random.NextDouble() < doubleBumpChance;
                Bumped?.Invoke();
            }
        }
        if (bumpActive)
        {
            bumpElapsed += Time.deltaTime;
            float duration = Mathf.Max(0.05f, bumpDuration);
            float progress = Mathf.Clamp01(bumpElapsed / duration);
            // A short impact settles completely before the next quiet interval.
            float envelope = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-3f * progress);
            bump = Mathf.Sin(bumpElapsed * shakeFrequency * Mathf.PI * 2f) * envelope * bumpStrength;
            if (progress >= 1f)
            {
                bump = 0f;
                bumpActive = false;
                waitRemaining = secondBumpPending ? Mathf.Max(0f, doubleBumpGap) : NextQuietInterval();
            }
        }
        interior.localPosition = restPosition + Vector3.up * (bump * shakeHeight);
        interior.localRotation = restRotation * Quaternion.Euler(0, 0, bump * rollDegrees);
    }
    void OnDisable()
    {
        if (interior) { interior.localPosition = restPosition; interior.localRotation = restRotation; }
        if (background) background.SetPropertyBlock(null);
    }
}
