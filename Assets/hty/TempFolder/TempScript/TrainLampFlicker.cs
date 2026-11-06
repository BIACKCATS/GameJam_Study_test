using UnityEngine;

public sealed class TrainLampFlicker : MonoBehaviour
{
    public TrainPostProcessing lighting;
    public Vector2 intervalSeconds = new Vector2(3f, 7f);
    public Vector2 offDurationSeconds = new Vector2(0.06f, 0.14f);
    [Range(0f, 1f)] public float dimBrightness = 0.12f;
    [Range(0f, 1f)] public float doubleFlickerChance = 0.3f;
    [Min(0.01f)] public float doubleFlickerGap = 0.1f;
    int phase;
    float remaining;
    bool doubleFlicker;

    void OnEnable()
    {
        if (!lighting) lighting = GetComponentInParent<TrainPostProcessing>();
        ResetCycle();
    }

    void ResetCycle()
    {
        phase = 0;
        remaining = Sample(intervalSeconds);
        if (lighting) lighting.SetLampBrightness(1f);
    }

    static float Sample(Vector2 range) => Random.Range(
        Mathf.Max(0.01f, Mathf.Min(range.x, range.y)),
        Mathf.Max(0.01f, Mathf.Max(range.x, range.y)));

    void Update()
    {
        if (!lighting) return;
        if (GameOverSequenceController.IsPlaying || TrueEndingController.IsPlaying)
        {
            if (phase != 0) ResetCycle();
            return;
        }
        remaining -= Time.deltaTime;
        if (remaining > 0f) return;
        switch (phase)
        {
            case 0: TriggerFlicker(); break;
            case 1:
                lighting.SetLampBrightness(1f);
                if (doubleFlicker) { phase = 2; remaining = Mathf.Max(0.01f, doubleFlickerGap); }
                else ResetCycle();
                break;
            case 2:
                lighting.SetLampBrightness(dimBrightness);
                phase = 3; remaining = Sample(offDurationSeconds);
                break;
            default: ResetCycle(); break;
        }
    }

    [ContextMenu("Test Flicker (Play Mode)")]
    public void TriggerFlicker()
    {
        if (!Application.isPlaying || !lighting || !isActiveAndEnabled ||
            GameOverSequenceController.IsPlaying || TrueEndingController.IsPlaying) return;
        doubleFlicker = Random.value < doubleFlickerChance;
        phase = 1; remaining = Sample(offDurationSeconds);
        lighting.SetLampBrightness(dimBrightness);
    }

    void OnDisable() { if (lighting) lighting.SetLampBrightness(1f); }
}
