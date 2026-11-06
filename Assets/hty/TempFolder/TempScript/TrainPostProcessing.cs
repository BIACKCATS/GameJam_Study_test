using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public sealed class TrainPostProcessing : MonoBehaviour
{
    public Volume volume;
    public SpriteRenderer interior;
    [Min(0)] public float bloomIntensity = 0.65f;
    [Min(0)] public float bloomThreshold = 1.1f;
    [Range(0,1)] public float bloomScatter = 0.65f;
    [Range(0,10)] public float lampEmission = 3f;
    [Range(0,1)] public float vignetteIntensity = 0.28f;
    [Range(0.01f,1)] public float vignetteSmoothness = 0.45f;
    public Color vignetteColor = Color.black;
    float lampBrightness = 1f;
    public void SetLampBrightness(float brightness) { lampBrightness = Mathf.Clamp01(brightness); Apply(); }
    MaterialPropertyBlock properties;
    void OnEnable() { Apply(); }
    void Update() { Apply(); }
    void Apply()
    {
        if (!volume || !volume.sharedProfile) return;
        // Keep the Inspector's asset targets stable in edit mode. During play,
        // let Volume own its instance instead of destroying it on disable/reload.
        var profile = Application.isPlaying ? volume.profile : volume.sharedProfile;
        if (profile.TryGet<Bloom>(out var bloom)) {
            bloom.intensity.Override(bloomIntensity); bloom.threshold.Override(bloomThreshold); bloom.scatter.Override(bloomScatter);
        }
        if (profile.TryGet<Vignette>(out var vignette)) {
            vignette.intensity.Override(vignetteIntensity); vignette.smoothness.Override(vignetteSmoothness);
            vignette.color.Override(vignetteColor); vignette.center.Override(new Vector2(0.5f,0.5f));
        }
        if (interior) {
            if (properties == null) properties = new MaterialPropertyBlock();
            interior.GetPropertyBlock(properties); properties.SetFloat("_LampEmission",lampEmission * lampBrightness); properties.SetFloat("_LampBrightness",lampBrightness); interior.SetPropertyBlock(properties);
        }
    }
    void OnDisable()
    {
        if (interior) interior.SetPropertyBlock(null);
    }
}
