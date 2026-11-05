using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Only the light is rendered through URP Bloom; the existing game camera is untouched.
[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public sealed class GameOverLightBloom : MonoBehaviour
{
    public Shader additiveShader;
    [Min(1)] public float lightEmission = 3f;
    [Min(0)] public float bloomIntensity = 1.2f;
    [Min(0)] public float bloomThreshold = 1f;
    [Range(0,1)] public float bloomScatter = 0.7f;
    [Range(256,2048)] public int textureWidth = 1024;
    UnityEngine.UI.RawImage image;
    Texture originalTexture;
    Material originalMaterial, sourceMaterial, compositeMaterial;
    GameObject rig;
    RenderTexture output;
    Sprite sprite;
    VolumeProfile profile;
    Bloom bloom;
    UnityEngine.UI.RawImage fullscreenImage;
    Camera bloomCamera;
    Transform lightTransform;
    bool originalEnabled;
    readonly Vector3[] corners=new Vector3[4];
    void OnEnable()
    {
        image=GetComponent<UnityEngine.UI.RawImage>();
        if(!UniversalRenderPipeline.asset || !UniversalRenderPipeline.asset.supportsHDR || !additiveShader || !image.texture)
        {Debug.LogError("GameOverLightBloom: URP HDR, additive shader and light texture are required.",this);return;}
        originalTexture=image.texture;originalMaterial=image.material;
        originalEnabled=image.enabled;
        var source=originalTexture as Texture2D;
        if(!source){Debug.LogError("GameOverLightBloom: Source must be Texture2D.",this);return;}
        sourceMaterial=new Material(additiveShader);compositeMaterial=new Material(additiveShader);
        float aspect=(float)Screen.width/Mathf.Max(1,Screen.height);
        output=new RenderTexture(Mathf.Clamp(textureWidth,256,2048),Mathf.RoundToInt(Mathf.Clamp(textureWidth,256,2048)/aspect),24,RenderTextureFormat.ARGBHalf);
        output.name="GameOverLightBloomHDR";output.Create();
        rig=new GameObject("GameOverBloomCamera");rig.transform.position=new Vector3(10000,10000,-10);
        rig.layer=31;
        var camera=rig.AddComponent<Camera>();bloomCamera=camera;camera.orthographic=true;camera.orthographicSize=1f;
        camera.aspect=aspect;camera.nearClipPlane=0.1f;camera.farClipPlane=20;camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(0,0,0,1);camera.allowHDR=true;camera.cullingMask=1<<31;camera.targetTexture=output;camera.depth=-100;
        var data=rig.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;data.volumeLayerMask=1<<31;data.volumeTrigger=rig.transform;
        var lightGo=new GameObject("HDRLight");lightGo.layer=31;lightGo.transform.SetParent(rig.transform,false);lightGo.transform.localPosition=new Vector3(0,0,10);
        lightTransform=lightGo.transform;
        var renderer=lightGo.AddComponent<SpriteRenderer>();sprite=Sprite.Create(source,new Rect(0,0,source.width,source.height),Vector2.one*0.5f,source.height);
        renderer.sprite=sprite;renderer.sharedMaterial=sourceMaterial;
        var collider=rig.AddComponent<BoxCollider>();collider.isTrigger=true;collider.size=Vector3.one*4;
        var volume=rig.AddComponent<Volume>();volume.isGlobal=false;volume.priority=1000;volume.weight=1;
        profile=ScriptableObject.CreateInstance<VolumeProfile>();bloom=profile.Add<Bloom>(true);volume.sharedProfile=profile;
        var composite=new GameObject("FullscreenLightBloom",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));
        var rect=(RectTransform)composite.transform;rect.SetParent(image.canvas.rootCanvas.transform,false);
        rect.SetSiblingIndex(image.transform.GetSiblingIndex()+1);
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        fullscreenImage=composite.GetComponent<UnityEngine.UI.RawImage>();fullscreenImage.raycastTarget=false;
        fullscreenImage.texture=output;fullscreenImage.material=compositeMaterial;image.enabled=false;
        UpdateLayout();
        Apply();
    }
    void LateUpdate(){UpdateLayout();Apply();}
    void UpdateLayout()
    {
        if(!bloomCamera||!fullscreenImage)return;
        float aspect=(float)Screen.width/Mathf.Max(1,Screen.height);
        int width=Mathf.Clamp(textureWidth,256,2048),height=Mathf.Max(1,Mathf.RoundToInt(width/aspect));
        if(output.width!=width||output.height!=height)
        {
            bloomCamera.targetTexture=null;output.Release();output.width=width;output.height=height;output.Create();bloomCamera.targetTexture=output;
        }
        bloomCamera.aspect=aspect;
        image.rectTransform.GetWorldCorners(corners);
        var uiCamera=image.canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:image.canvas.worldCamera;
        Vector2 a=RectTransformUtility.WorldToScreenPoint(uiCamera,corners[0]);
        Vector2 b=RectTransformUtility.WorldToScreenPoint(uiCamera,corners[2]);
        Vector2 center=(a+b)*0.5f;
        lightTransform.localPosition=new Vector3((center.x/Screen.width-0.5f)*2*aspect,(center.y/Screen.height-0.5f)*2,10);
        float sourceAspect=(float)originalTexture.width/originalTexture.height;
        lightTransform.localScale=new Vector3((b.x-a.x)/Screen.width*2*aspect/sourceAspect,(b.y-a.y)/Screen.height*2,1);
        fullscreenImage.color=image.color;
    }
    void Apply()
    {
        if(!sourceMaterial || !bloom)return;
        sourceMaterial.SetFloat("_Emission",Mathf.Max(1,lightEmission));
        bloom.intensity.Override(Mathf.Max(0,bloomIntensity));bloom.threshold.Override(Mathf.Max(0,bloomThreshold));
        bloom.scatter.Override(Mathf.Clamp01(bloomScatter));bloom.highQualityFiltering.Override(true);
    }
    void OnDisable()
    {
        if(image && originalTexture){image.texture=originalTexture;image.material=originalMaterial;image.enabled=originalEnabled;}
        if(fullscreenImage){fullscreenImage.gameObject.SetActive(false);Destroy(fullscreenImage.gameObject);fullscreenImage=null;}
        if(rig){rig.SetActive(false);Destroy(rig);}
        if(output){output.Release();Destroy(output);}
        if(sprite)Destroy(sprite);
        if(sourceMaterial)Destroy(sourceMaterial);if(compositeMaterial)Destroy(compositeMaterial);
        if(profile){foreach(var component in profile.components)if(component)Destroy(component);Destroy(profile);}
        rig=null;output=null;sprite=null;profile=null;bloom=null;sourceMaterial=null;compositeMaterial=null;originalTexture=null;
    }
}
