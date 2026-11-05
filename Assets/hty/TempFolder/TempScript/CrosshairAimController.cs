using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class CrosshairAimController : MonoBehaviour
{
    public UnityEngine.UI.RawImage crosshair;
    public UnityEngine.UI.Button testButton;
    public Texture2D animationSheet;
    public Texture2D clickSheet;
    public Texture2D dismissSheet;
    [Min(0.1f)] public float dismissFramesPerSecond = 20;
    [Min(0.1f)] public float clickFramesPerSecond = 24;
    [Min(0.1f)] public float framesPerSecond = 16;
    [Min(0.1f)] public float sizeMultiplier = 1;
    public bool escapeToStop = true;
    static CrosshairAimController instance;
    public static bool IsAiming => instance && instance.aiming && !instance.closing;
    public static Vector2 AimScreenPosition => instance ? instance.aimPosition : Vector2.zero;
    bool aiming, previousVisible;
    CursorLockMode previousLock;
    float frame;
    bool clicking;
    bool closing;
    float dismissFrame;
    float clickFrame;
    int aimStartedFrame;
    readonly System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> uiHits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    Vector2 aimPosition;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }
    void OnEnable()
    {
        if(instance && instance!=this) { Debug.LogError("CrosshairAimController: Multiple active controllers.",this); return; }
        instance=this;
        if(crosshair) { crosshair.enabled=false; crosshair.raycastTarget=false; }
        if(testButton) testButton.onClick.AddListener(ToggleAim);
    }
    /// <summary>Show the animated crosshair and hide the system cursor. Repeated calls are ignored.</summary>
    public static void StartAim()
    {
        if(!instance || !instance.isActiveAndEnabled) { Debug.LogError("CrosshairAimController: No active controller."); return; }
        instance.Begin();
    }
    /// <summary>Play the dismiss animation, then hide the crosshair and restore the cursor.</summary>
    public static void StopAim() { if(instance) instance.End(); }
    void ToggleAim() { if(IsAiming) StopAim(); else StartAim(); }
    void Begin()
    {
        if(aiming && !closing) return;
        if(!crosshair || !animationSheet) { Debug.LogError("CrosshairAimController: Assign crosshair and animation sheet.",this); return; }
        if(!aiming) { previousVisible=Cursor.visible; previousLock=Cursor.lockState; }
        aiming=true; closing=false; frame=0; clicking=false; aimStartedFrame=Time.frameCount;
        Cursor.lockState=CursorLockMode.Confined; Cursor.visible=false;
        crosshair.texture=animationSheet; crosshair.enabled=true;
        UpdateVisual();
    }
    void Update()
    {
        if(!aiming) return;
        if(closing)
        {
            if(clicking)
            {
                clickFrame+=Time.unscaledDeltaTime*Mathf.Max(0.1f,clickFramesPerSecond);
                if(clickFrame>=8) { clicking=false; dismissFrame=0; }
            }
            else dismissFrame+=Time.unscaledDeltaTime*Mathf.Max(0.1f,dismissFramesPerSecond);
            if(!clicking && dismissFrame>=12) { EndImmediately(); return; }
            UpdateVisual(); return;
        }
#if ENABLE_INPUT_SYSTEM
        if(escapeToStop && Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) { End(); return; }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if(escapeToStop && Input.GetKeyDown(KeyCode.Escape)) { End(); return; }
#endif
        bool pressed=false;
#if ENABLE_INPUT_SYSTEM
        pressed=Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        pressed=Input.GetMouseButtonDown(0);
#endif
        if(clicking)
        {
            clickFrame+=Time.unscaledDeltaTime*Mathf.Max(0.1f,clickFramesPerSecond);
            if(clickFrame>=8) { clicking=false; frame=13; }
        }
        else frame=Mathf.Min(13,frame+Time.unscaledDeltaTime*Mathf.Max(0.1f,framesPerSecond));
        UpdateVisual();
        if(pressed && Time.frameCount>aimStartedFrame && !PointerOverUI()) PlayClick();
    }
    bool PointerOverUI()
    {
        var system=UnityEngine.EventSystems.EventSystem.current;
        if(!system) return false;
        var pointer=new UnityEngine.EventSystems.PointerEventData(system) { position=aimPosition };
        uiHits.Clear(); system.RaycastAll(pointer,uiHits);
        foreach(var hit in uiHits) if(hit.module is UnityEngine.UI.GraphicRaycaster) return true;
        return false;
    }
    public static void PlayClick()
    {
        if(!instance || !instance.aiming || instance.closing) return;
        if(!instance.clickSheet) { Debug.LogError("CrosshairAimController: Assign Click Sheet.",instance); return; }
        instance.clicking=true; instance.clickFrame=0; instance.UpdateVisual();
    }
    void UpdateVisual()
    {
        Vector2 mouse=new Vector2(Screen.width*0.5f,Screen.height*0.5f);
#if ENABLE_INPUT_SYSTEM
        if(Mouse.current!=null) mouse=Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
        mouse=Input.mousePosition;
#endif
        if(!closing) aimPosition=new Vector2(Mathf.Clamp(mouse.x,0,Screen.width),Mathf.Clamp(mouse.y,0,Screen.height));
        var parent=crosshair.rectTransform.parent as RectTransform;
        if(parent && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,aimPosition,null,out var local)) crosshair.rectTransform.anchoredPosition=local;
        crosshair.rectTransform.sizeDelta=new Vector2(1920,1080)*sizeMultiplier;
        int index=Mathf.FloorToInt(clicking ? clickFrame : closing ? dismissFrame : frame);
        int rows=clicking ? 3 : closing ? 4 : 5;
        crosshair.texture=clicking ? clickSheet : closing ? dismissSheet : animationSheet;
        crosshair.uvRect=new Rect(index%3/3f,1f-(index/3+1)/(float)rows,1f/3f,1f/rows);
    }
    void End()
    {
        if(!aiming || closing) return;
        if(!dismissSheet) { Debug.LogError("CrosshairAimController: Assign Dismiss Sheet.",this); EndImmediately(); return; }
        closing=true; dismissFrame=0;
        UpdateVisual();
    }
    void EndImmediately()
    {
        if(!aiming) return;
        aiming=false; clicking=false; closing=false;
        if(crosshair) crosshair.enabled=false;
        Cursor.lockState=previousLock; Cursor.visible=previousVisible;
    }
    void OnApplicationFocus(bool focused) { if(!focused) EndImmediately(); }
    void OnDisable()
    {
        if(testButton) testButton.onClick.RemoveListener(ToggleAim);
        EndImmediately(); if(instance==this) instance=null;
    }
}
