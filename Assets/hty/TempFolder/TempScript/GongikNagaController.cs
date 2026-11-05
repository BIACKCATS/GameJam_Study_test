using UnityEngine;

public sealed class GongikNagaController : MonoBehaviour
{
    public PassengerObjectionPanel stamp;
    public Texture2D stampTexture;
    public GameObject doorStage;
    public UnityEngine.UI.RawImage door;
    public UnityEngine.UI.RawImage[] villains;
    public string[] villainKeys = { "Knight", "OldMan", "Danso", "Zaruban", "HipHop" };
    public string testVillain = "Knight";
    public GongikDoorZoom doorZoom;
    [Header("Doom ground impact")]
    public UnityEngine.UI.RawImage groundCrack;
    public UnityEngine.UI.RawImage groundDebris;
    [Min(0.1f)] public float groundImpactFramesPerSecond = 20f;
    public Vector2 groundImpactSize = new Vector2(480,384);
    [Min(0.01f)] public float landingBounceDuration = 0.18f;
    [Min(0)] public float landingBounceHeight = 45f;
    [Range(0,0.4f)] public float landingSquash = 0.18f;
    [Min(0.01f)] public float openingDuration = 0.75f;
    [Min(0.01f)] public float openHoldDuration = 1f;
    [Min(0.01f)] public float closingDuration = 0.75f;
    [Tooltip("Villain and Doom fade immediately after landing, in seconds.")]
    [UnityEngine.Serialization.FormerlySerializedAs("closingFadeDuration")]
    [Min(0.01f)] public float landingFadeDuration = 0.5f;
    [Header("Throw positions in canvas units")]
    public Vector2 throwStart = new Vector2(-300, -450);
    public bool throwFromScreenBottom = true;
    [Min(0)] public float bottomStartPadding = 20f;
    public Vector2 throwEnd = new Vector2(-60, 20);
    [Range(0.1f, 1)] public float throwWindow = 0.58f;
    public float clockwiseAngle = 40f;
    [Min(0)] public float throwArc = 180f;
    [Range(0.01f, 1)] public float distantScale = 0.12f;
    static GongikNagaController instance;
    public static bool IsPlaying => instance && instance.playing;
    bool playing, doorsRunning;
    float elapsed;
    int selectedVillain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }
    void OnEnable()
    {
        if (instance && instance != this) { Debug.LogError("GongikNagaController: Only one active prefab is supported.", this); enabled = false; return; }
        instance = this;
        Cleanup();
    }
    /// <summary>Plays the stamp and ejects one villain. Keys are case-insensitive.
    /// Returns false and logs an error if input/references are invalid or another event is busy.</summary>
    public static bool Play(string villainKey)
    {
        if(TrueEndingController.IsPlaying)return false;
        if (!instance || !instance.isActiveAndEnabled) { Debug.LogError("GongikNagaController: Place an active GongikNaga prefab in the scene."); return false; }
        return instance.Begin(villainKey);
    }
    bool Begin(string villainKey)
    {
        if (playing || PassengerAttackController.IsPlaying) { Debug.LogError("GongikNagaController: Another passenger event is playing.", this); return false; }
        if (!stamp || !stampTexture || !doorStage || !door || !door.texture || villains == null || villains.Length == 0)
        { Debug.LogError("GongikNagaController: Missing stamp, door or Villin references.", this); return false; }
        if (villainKeys == null || villainKeys.Length != villains.Length) { Debug.LogError("GongikNagaController: Villain keys and images must have matching counts.", this); return false; }
        int selected = -1;
        for (int i = 0; i < villainKeys.Length; i++)
            if (!string.IsNullOrWhiteSpace(villainKey) && string.Equals(villainKeys[i], villainKey.Trim(), System.StringComparison.OrdinalIgnoreCase)) { selected = i; break; }
        if (selected < 0) { Debug.LogError("GongikNagaController: Invalid key '" + villainKey + "'. Use Knight, OldMan, Danso, Zaruban, HipHop.", this); return false; }
        if (!villains[selected] || !villains[selected].texture) { Debug.LogError("GongikNagaController: Missing image for " + villainKey, this); return false; }
        Cleanup();
        selectedVillain = selected;
        playing = true;
        if (!doorZoom) { Debug.LogError("GongikNagaController: Assign Door Zoom.",this); Cleanup();return false; }
        if (!stamp.Play(stampTexture, StartZoom, Cleanup)) { Cleanup(); return false; }
        return true;
    }
    void StartZoom()
    {
        if(!playing || !isActiveAndEnabled)return;
        if(!doorZoom.Play(()=>{door.uvRect=new Rect(0,5f/6,1f/6,1f/6);doorStage.SetActive(true);},StartDoors))Cleanup();
    }
    void StartDoors()
    {
        if (!playing || !isActiveAndEnabled) return;
        doorsRunning = true; elapsed = 0; doorStage.SetActive(true); RenderDoor();
    }
    void Update() { if (doorsRunning) { elapsed += Time.deltaTime; RenderDoor(); } }
    void RenderDoor()
    {
        float opening = Mathf.Max(0.01f, openingDuration), hold = Mathf.Max(0.01f, openHoldDuration), closing = Mathf.Max(0.01f, closingDuration);
        if (elapsed >= opening + hold + closing) { doorsRunning=false;doorZoom.Return(Cleanup);return; }
        int frame;
        if (elapsed < opening) frame = Mathf.Min(8, Mathf.FloorToInt(elapsed / opening * 9));
        else if (elapsed < opening + hold) frame = 9 + Mathf.Min(17, Mathf.FloorToInt((elapsed - opening) / hold * 18));
        else frame = 27 + Mathf.Min(8, Mathf.FloorToInt((elapsed - opening - hold) / closing * 9));
        door.uvRect = new Rect(frame % 6 / 6f, 1 - (frame / 6 + 1) / 6f, 1f / 6, 1f / 6);
        float progress = (elapsed - opening) / hold;
        float window = Mathf.Clamp(throwWindow, 0.1f, 1);
        float alpha = 1f - Mathf.Clamp01((elapsed - opening - hold * window) / Mathf.Max(0.01f, landingFadeDuration));
        RenderGroundImpact(elapsed - opening - hold * window, alpha);
        for (int i = 0; i < villains.Length; i++)
        {
            float t = progress / window;
            var image = villains[i];
            if (!image) continue;
            SetAlpha(image, alpha);
            image.enabled = i == selectedVillain && t >= 0 && alpha > 0;
            if (!image.enabled) continue;
            var rect = image.rectTransform;
            float flight=Mathf.Clamp01(t);
            var start=throwFromScreenBottom ? ScreenBottomStart(image) : throwStart;
            float travel=throwFromScreenBottom ? 1-(1-flight)*(1-flight) : flight*flight;
            rect.anchoredPosition = Vector2.Lerp(start, throwEnd, travel) + Vector2.up * (Mathf.Sin(flight * Mathf.PI) * throwArc);
            rect.localEulerAngles = new Vector3(0, 0, -clockwiseAngle);
            float size=Mathf.Lerp(1,Mathf.Max(0.01f,distantScale),flight);
            rect.localScale=Vector3.one*size;
            if(t>=1)
            {
                float bounce=Mathf.Clamp01((progress-window)*hold/Mathf.Max(0.01f,landingBounceDuration));
                rect.anchoredPosition=throwEnd+Vector2.up*(Mathf.Sin(bounce*Mathf.PI)*landingBounceHeight*(1-bounce));
                float squash=landingSquash*(1-bounce);
                rect.localScale=new Vector3(size*(1+squash),size*(1-squash),1);
            }
        }
    }
    Vector2 ScreenBottomStart(UnityEngine.UI.RawImage image)
    {
        var rect=image.rectTransform;
        var parent=rect.parent as RectTransform;
        var canvas=image.canvas;
        var camera=canvas && canvas.renderMode!=RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if(!parent || !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(Screen.width*0.5f,0),camera,out var bottom))
            return throwStart;
        var rotation=Quaternion.Euler(0,0,-clockwiseAngle);
        var bounds=rect.rect;
        float top=float.NegativeInfinity;
        foreach(var corner in new[]{new Vector2(bounds.xMin,bounds.yMin),new Vector2(bounds.xMin,bounds.yMax),new Vector2(bounds.xMax,bounds.yMin),new Vector2(bounds.xMax,bounds.yMax)})
            top=Mathf.Max(top,(rotation*(Vector3)corner).y);
        float anchorOffset=rect.localPosition.y-rect.anchoredPosition.y;
        return new Vector2(throwStart.x,bottom.y-anchorOffset-top-Mathf.Max(0,bottomStartPadding));
    }
    void Cleanup()
    {
        playing = doorsRunning = false;
        if(doorZoom)doorZoom.Cancel();
        if (doorStage) doorStage.SetActive(false);
        if(groundCrack){groundCrack.enabled=false;SetAlpha(groundCrack,1);}
        if(groundDebris){groundDebris.enabled=false;SetAlpha(groundDebris,1);}
        if (villains != null) foreach (var v in villains) if (v) { v.enabled = false; SetAlpha(v,1); v.rectTransform.localScale = Vector3.one; }
    }
    static void SetAlpha(UnityEngine.UI.RawImage image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
    void RenderGroundImpact(float sinceLanding, float alpha)
    {
        int frame=Mathf.Clamp(Mathf.FloorToInt(sinceLanding*Mathf.Max(0.1f,groundImpactFramesPerSecond)),0,11);
        UpdateGroundLayer(groundCrack,sinceLanding,frame,alpha);
        UpdateGroundLayer(groundDebris,sinceLanding,frame,alpha);
    }
    void UpdateGroundLayer(UnityEngine.UI.RawImage image,float sinceLanding,int frame,float alpha)
    {
        if(!image)return;
        SetAlpha(image,alpha);
        image.enabled=sinceLanding>=0 && alpha>0;
        if(!image.enabled)return;
        image.uvRect=new Rect(frame%4/4f,1-(frame/4+1)/3f,1f/4,1f/3);
        image.rectTransform.pivot=new Vector2(0.5f,0.375f);
        image.rectTransform.anchoredPosition=throwEnd;
        image.rectTransform.sizeDelta=new Vector2(Mathf.Max(1,groundImpactSize.x),Mathf.Max(1,groundImpactSize.y));
    }
    void OnDisable() { if (instance != this) return; if (stamp) stamp.Cancel(); Cleanup(); instance = null; }
    [ContextMenu("Test GongikNaga")]
    public void TestPlay() { if (Application.isPlaying) Play(testVillain); }
}
