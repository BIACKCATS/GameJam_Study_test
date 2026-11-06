using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class CrosshairFocusOverlay : MonoBehaviour
{
    public CrosshairAimController controller;
    public UnityEngine.UI.RawImage panel;
    [Range(0, 1)] public float darkness = 0.4f;
    [Min(1)] public float radius = 336f;
    [Min(0)] public float edgeSoftness = 16f;
    [Min(0)] public float fadeDuration = 0.15f;
    Material runtimeMaterial;
    Material originalMaterial;
    float visibility;

    void Awake()
    {
        if (!controller || !controller.crosshair || !panel || !panel.material)
        {
            Debug.LogError("CrosshairFocusOverlay: Assign controller, panel and material.", this);
            enabled = false;
            return;
        }
        originalMaterial = panel.material;
        runtimeMaterial = new Material(originalMaterial);
        panel.material = runtimeMaterial;
        panel.raycastTarget = false;
        panel.enabled = false;
    }

    void LateUpdate()
    {
        if((GameOverSequenceController.IsPlaying || TrueEndingController.IsPlaying))return;
        if (!runtimeMaterial || !controller || !controller.crosshair || !panel) return;
        bool visible = controller.isActiveAndEnabled && controller.crosshair.isActiveAndEnabled;
        float target = visible && CrosshairAimController.IsAiming ? 1f : 0f;
        visibility = fadeDuration <= 0 ? target : Mathf.MoveTowards(visibility, target, Time.unscaledDeltaTime / fadeDuration);
        if (!visible) visibility = 0;
        panel.enabled = visibility > 0;
        var rect = panel.rectTransform.rect;
        Vector2 center = panel.rectTransform.InverseTransformPoint(controller.crosshair.rectTransform.position);
        runtimeMaterial.SetVector("_Size", new Vector4(rect.width, rect.height, 0, 0));
        runtimeMaterial.SetVector("_Center", new Vector4(center.x - rect.xMin, center.y - rect.yMin, 0, 0));
        runtimeMaterial.SetFloat("_Radius", Mathf.Max(1, radius) * controller.sizeMultiplier);
        runtimeMaterial.SetFloat("_Softness", Mathf.Max(0.01f, edgeSoftness));
        runtimeMaterial.SetFloat("_Darkness", Mathf.Clamp01(darkness) * visibility);
    }

    void OnDisable() { visibility = 0; if (panel) panel.enabled = false; }
    void OnDestroy()
    {
        if (panel) panel.material = originalMaterial;
        if (runtimeMaterial) Destroy(runtimeMaterial);
    }
}
