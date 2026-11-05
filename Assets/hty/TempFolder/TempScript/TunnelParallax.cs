using UnityEngine;

/// <summary>Moves repeating tunnel strips left. Speeds are complete strips per second.</summary>
public sealed class TunnelParallax : MonoBehaviour
{
    public Transform track;
    public Transform pillars;
    public Transform walls;
    public Transform ceiling;
    [Header("Leftward speed (background widths per second)")]
    [Min(0)] public float trackSpeed = 0.15f;
    [Min(0)] public float pillarSpeed = 0.10f;
    [Min(0)] public float wallSpeed = 0.05f;
    [Min(0)] public float ceilingSpeed = 0.025f;
    [Min(0)] public float speedMultiplier = 1f;
    Vector3[] origins;
    Transform[] strips;
    readonly float[] offsets = new float[4];

    void OnEnable()
    {
        strips = new[] { track, pillars, walls, ceiling };
        origins = new Vector3[4];
        for (int i = 0; i < strips.Length; i++)
        {
            if (!strips[i]) { Debug.LogError("TunnelParallax: Assign all four layer references.", this); enabled = false; return; }
            origins[i] = strips[i].localPosition;
            offsets[i] = 0;
        }
    }

    void Update()
    {
        Move(0, trackSpeed); Move(1, pillarSpeed);
        Move(2, wallSpeed); Move(3, ceilingSpeed);
    }

    void Move(int index, float speed)
    {
        if (!strips[index]) return;
        offsets[index] = Mathf.Repeat(offsets[index] + Mathf.Max(0, speed) * Mathf.Max(0, speedMultiplier) * Time.deltaTime, 1f);
        strips[index].localPosition = origins[index] + Vector3.left * offsets[index];
    }

    void OnDisable()
    {
        if (strips == null || origins == null) return;
        for (int i = 0; i < strips.Length; i++)
            if (strips[i]) strips[i].localPosition = origins[i];
    }
}
