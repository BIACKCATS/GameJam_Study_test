using System;
using DG.Tweening;
using UnityEngine;
using BC.Core;

// Zooms the train artwork, never the camera or screen resolution.
public sealed class GongikDoorZoom : MonoBehaviour
{
    public UnityEngine.UI.RawImage snapshot;
    public UnityEngine.UI.RawImage animatedDoor;
    public SpriteRenderer interior;
    public Rect sourceDoorPixels = new Rect(824,104,274,484);
    public Rect destinationDoorPixels = new Rect(144,47,136,182);
    [Min(0.01f)] public float zoomDuration = 1f;
    [Min(0)] public float alignedHoldDuration = 0.2f;
    public bool fillViewport = true;
    [Min(1)] public float zoomSizeMultiplier = 1.1f;
    [Min(0.01f)] public float blendDuration = 0.15f;
    [Min(0.01f)] public float returnDuration = 0.55f;
    public CanvasGroup doorOpacity;
    [Header("Background rush during zoom")]
    public TunnelParallax tunnelBackground;
    [Min(1)] public float backgroundRushMultiplier = 12f;
    [Min(0.01f)] public float accelerationDuration = 0.2f;
    [Min(1.01f)] public float railRushMultiplier = 2f;
    [Range(0,1)] public float backgroundBlackStart = 0.6f;
    [Min(0.01f)] public float backgroundBlackDuration = 0.25f;
    SpriteRenderer blackBackground;
    Sprite blackSprite;
    float originalBackgroundSpeed;
    float originalTrackSpeed;
    bool backgroundCaptured;
    Sequence sequence;
    Transform zoomTarget;
    Vector3 originalPosition, originalScale;
    Quaternion originalRotation;
    TrainMotion trainMotion;
    bool captured, motionWasEnabled;

    public bool Play(Action revealDoor, Action completed)
    {
        Cancel();
        var camera=Camera.main;
        if(!interior){var train=FindFirstObjectByType<TrainMotion>();if(train && train.interior)interior=train.interior.GetComponent<SpriteRenderer>();}
        if(!camera || !interior || !interior.sprite || !animatedDoor || !doorOpacity)
        {Debug.LogError("GongikDoorZoom: Missing MainCamera, train interior, door image or CanvasGroup.",this);return false;}
        Canvas.ForceUpdateCanvases();
        if(fillViewport)
        {
            var viewport=((RectTransform)animatedDoor.canvas.transform).rect;
            float fit=Mathf.Max(viewport.width/426f,viewport.height/307f)*Mathf.Max(1,zoomSizeMultiplier);
            animatedDoor.rectTransform.sizeDelta=new Vector2(426f*fit,307f*fit);
            animatedDoor.rectTransform.anchoredPosition=Vector2.zero;
            Canvas.ForceUpdateCanvases();
        }
        zoomTarget=interior.transform;
        trainMotion=interior.GetComponentInParent<TrainMotion>();
        motionWasEnabled=trainMotion && trainMotion.enabled;
        // TrainMotion writes the interior's pose each Update; suspend it during this effect.
        if(motionWasEnabled)trainMotion.enabled=false;
        originalPosition=zoomTarget.localPosition;originalRotation=zoomTarget.localRotation;originalScale=zoomTarget.localScale;captured=true;
        var sprite=interior.sprite;
        Vector3 WorldPoint(Vector2 pixel)
        {
            var local=(pixel-sprite.rect.position-sprite.pivot)/sprite.pixelsPerUnit;
            return zoomTarget.TransformPoint(local);
        }
        Vector2 ScreenPoint(Vector2 pixel)
        {
            var d=animatedDoor.rectTransform;var r=d.rect;
            return RectTransformUtility.WorldToScreenPoint(null,d.TransformPoint(new Vector3(r.xMin+pixel.x/426f*r.width,r.yMin+pixel.y/307f*r.height)));
        }
        var a=WorldPoint(sourceDoorPixels.min);var b=WorldPoint(sourceDoorPixels.max);
        float depth=camera.WorldToScreenPoint((a+b)*0.5f).z;
        var sa=ScreenPoint(destinationDoorPixels.min);var sb=ScreenPoint(destinationDoorPixels.max);
        var ta=camera.ScreenToWorldPoint(new Vector3(sa.x,sa.y,depth));var tb=camera.ScreenToWorldPoint(new Vector3(sb.x,sb.y,depth));
        float x=Mathf.Abs((tb.x-ta.x)/(b.x-a.x)), y=Mathf.Abs((tb.y-ta.y)/(b.y-a.y));
        if(float.IsNaN(x)||float.IsInfinity(x)||float.IsNaN(y)||float.IsInfinity(y)||depth<=0)
        {Debug.LogError("GongikDoorZoom: Invalid doorway alignment bounds.",this);Cancel();return false;}
        // Match doorway height with a uniform multiplier: preserve the artwork's original aspect.
        float uniformScale=y;
        var delta=(a+b)*0.5f-zoomTarget.position;
        var destination=(ta+tb)*0.5f-new Vector3(delta.x*uniformScale,delta.y*uniformScale,delta.z);
        var scale=new Vector3(originalScale.x*uniformScale,originalScale.y*uniformScale,originalScale.z);
        doorOpacity.alpha=0;
        PrepareBlackBackground(camera);
        if(!animatedDoor.GetComponentInChildren<GongikWindowBlackout>(true))
        {
            var mask=new GameObject("BlackSideWindows",typeof(RectTransform),typeof(GongikWindowBlackout));
            var rect=(RectTransform)mask.transform;
            rect.SetParent(animatedDoor.transform,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
        if(!tunnelBackground)tunnelBackground=FindFirstObjectByType<TunnelParallax>();
        if(tunnelBackground)
        {
            originalBackgroundSpeed=tunnelBackground.speedMultiplier;
            originalTrackSpeed=tunnelBackground.trackSpeed;
            backgroundCaptured=true;
        }
        sequence=DOTween.Sequence();
        if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.PassingDark,0.8f);
        sequence.Append(zoomTarget.DOScale(scale,Mathf.Max(0.01f,zoomDuration)).SetEase(Ease.InOutCubic));
        sequence.Join(zoomTarget.DOMove(destination,Mathf.Max(0.01f,zoomDuration)).SetEase(Ease.InOutCubic));
        sequence.Insert(Mathf.Max(0.01f,zoomDuration)*backgroundBlackStart,
            blackBackground.DOFade(1,Mathf.Min(Mathf.Max(0.01f,backgroundBlackDuration),Mathf.Max(0.01f,zoomDuration)*(1-backgroundBlackStart)+alignedHoldDuration)).SetEase(Ease.Linear));
        if(tunnelBackground)
        {
            float duration=Mathf.Max(0.01f,zoomDuration);
            float brakeStart=duration*Mathf.Clamp01(backgroundBlackStart);
            float acceleration=Mathf.Min(Mathf.Max(0.01f,accelerationDuration),Mathf.Max(0.01f,brakeStart));
            float rush=originalBackgroundSpeed*Mathf.Max(1,backgroundRushMultiplier);
            float rail=Mathf.Max(originalTrackSpeed,Mathf.Max(tunnelBackground.pillarSpeed,
                Mathf.Max(tunnelBackground.wallSpeed,tunnelBackground.ceilingSpeed)))*Mathf.Max(1.01f,railRushMultiplier);
            sequence.Insert(0,DOVirtual.Float(0,duration,duration,time=>
            {
                float ramp=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/acceleration));
                float speed=Mathf.Lerp(originalBackgroundSpeed,rush,ramp);
                if(time>=brakeStart)
                    speed=rush*Mathf.Pow(1-Mathf.Clamp01((time-brakeStart)/Mathf.Max(0.01f,duration-brakeStart)),3);
                tunnelBackground.speedMultiplier=speed;
                tunnelBackground.trackSpeed=Mathf.Lerp(originalTrackSpeed,rail,ramp);
            }).SetEase(Ease.Linear));
        }
        sequence.AppendInterval(Mathf.Max(0,alignedHoldDuration));
        sequence.AppendCallback(()=>revealDoor?.Invoke());
        sequence.Append(doorOpacity.DOFade(1,Mathf.Max(0.01f,blendDuration)));
        sequence.OnComplete(()=>{sequence=null;completed?.Invoke();});
        return true;
    }
    public void Return(Action completed)
    {
        if(!captured || !zoomTarget){completed?.Invoke();return;}
        sequence=DOTween.Sequence();
        sequence.Append(doorOpacity.DOFade(0,Mathf.Max(0.01f,blendDuration)));
        sequence.Append(zoomTarget.DOScale(originalScale,Mathf.Max(0.01f,returnDuration)).SetEase(Ease.InOutCubic));
        sequence.Join(zoomTarget.DOLocalMove(originalPosition,Mathf.Max(0.01f,returnDuration)).SetEase(Ease.InOutCubic));
        if(blackBackground)sequence.Join(blackBackground.DOFade(0,Mathf.Max(0.01f,returnDuration)));
        sequence.OnComplete(()=>{sequence=null;RestoreInterior();completed?.Invoke();});
    }
    void RestoreInterior()
    {
        if(captured && zoomTarget){zoomTarget.localPosition=originalPosition;zoomTarget.localRotation=originalRotation;zoomTarget.localScale=originalScale;}
        if(captured && trainMotion && motionWasEnabled)trainMotion.enabled=true;
        captured=false;zoomTarget=null;trainMotion=null;motionWasEnabled=false;
    }
    void RestoreBackground()
    {
        if(backgroundCaptured && tunnelBackground)
        {
            tunnelBackground.speedMultiplier=originalBackgroundSpeed;
            tunnelBackground.trackSpeed=originalTrackSpeed;
        }
        backgroundCaptured=false;
    }
    void PrepareBlackBackground(Camera camera)
    {
        if(!blackBackground)
        {
            var go=new GameObject("NagaBlackBackground");go.transform.SetParent(transform,false);
            blackBackground=go.AddComponent<SpriteRenderer>();
            blackSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*0.5f,1);
            blackBackground.sprite=blackSprite;
        }
        float depth=camera.WorldToScreenPoint(interior.transform.position).z;
        var a=camera.ViewportToWorldPoint(new Vector3(0,0,depth));
        var b=camera.ViewportToWorldPoint(new Vector3(1,1,depth));
        blackBackground.transform.position=(a+b)*0.5f;
        blackBackground.transform.rotation=camera.transform.rotation;
        blackBackground.transform.localScale=new Vector3(Mathf.Abs(b.x-a.x)*1.1f,Mathf.Abs(b.y-a.y)*1.1f,1);
        blackBackground.sortingLayerID=interior.sortingLayerID;
        blackBackground.sortingOrder=interior.sortingOrder-1;
        blackBackground.color=new Color(0,0,0,0);
        blackBackground.enabled=true;
    }
    public void Cancel()
    {
        if(sequence!=null){sequence.Kill(false);sequence=null;}
        RestoreBackground();
        RestoreInterior();
        if(blackBackground){blackBackground.color=new Color(0,0,0,0);blackBackground.enabled=false;}
        if(snapshot){snapshot.enabled=false;snapshot.texture=null;snapshot.rectTransform.localScale=Vector3.one;snapshot.rectTransform.anchoredPosition=Vector2.zero;}
        if(doorOpacity)doorOpacity.alpha=0;
    }
    void OnDisable(){Cancel();}
    void OnDestroy(){if(blackSprite)Destroy(blackSprite);}
}
