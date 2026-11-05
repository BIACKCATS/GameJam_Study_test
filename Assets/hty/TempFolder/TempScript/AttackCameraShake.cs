using UnityEngine;
using UnityEngine.Rendering;

// Lives on the effect prefab, never on the camera. Offsets exist only while rendering.
public sealed class AttackCameraShake : MonoBehaviour
{
    [Min(0)] public float strength = 0.12f;
    [Min(0.01f)] public float duration = 0.3f;
    [Min(0)] public float frequency = 18f;
    Camera target;
    float elapsed;
    bool shaking, offsetApplied;
    Vector3 appliedOffset;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginRender;
        RenderPipelineManager.endCameraRendering += EndRender;
        Camera.onPreCull += BeforeCamera;
        Camera.onPostRender += AfterCamera;
    }

    public void PlayShake()
    {
        if (!isActiveAndEnabled) return;
        StopShake();
        target = Camera.main;
        if (!target)
        {
            Debug.LogError("AttackCameraShake: No enabled camera tagged MainCamera was found.", this);
            return;
        }
        elapsed = 0;
        shaking = true;
    }

    void Update()
    {
        if (!shaking) return;
        elapsed += Time.deltaTime;
        if (elapsed >= Mathf.Max(0.01f, duration)) StopShake();
    }

    void BeforeCamera(Camera camera)
    {
        if (!shaking || camera != target || offsetApplied) return;
        float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
        float amount = Mathf.Cos(elapsed * Mathf.Max(0, frequency) * Mathf.PI * 2) * Mathf.Max(0, strength) * (1-progress) * (1-progress);
        appliedOffset = Vector3.up * amount;
        target.transform.position += appliedOffset;
        offsetApplied = true;
    }
    void AfterCamera(Camera camera) { if (camera == target) RestoreOffset(); }
    void BeginRender(ScriptableRenderContext context, Camera camera) { BeforeCamera(camera); }
    void EndRender(ScriptableRenderContext context, Camera camera) { AfterCamera(camera); }
    void RestoreOffset()
    {
        if (offsetApplied && target) target.transform.position -= appliedOffset;
        offsetApplied = false;
        appliedOffset = Vector3.zero;
    }
    public void StopShake() { RestoreOffset(); shaking = false; }
    void OnDisable()
    {
        StopShake();
        RenderPipelineManager.beginCameraRendering -= BeginRender;
        RenderPipelineManager.endCameraRendering -= EndRender;
        Camera.onPreCull -= BeforeCamera;
        Camera.onPostRender -= AfterCamera;
    }
}
