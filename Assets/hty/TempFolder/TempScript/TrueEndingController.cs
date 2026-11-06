using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using BC.Core;
using UnityEngine.SceneManagement;

public sealed class TrueEndingController : MonoBehaviour
{
    public GameObject presentation;
    public CanvasGroup fadeGroup;
    [Tooltip("주인공, 단소, 오래된 남자, 힙합 소년, 자르반84세, 기사 순서")]
    public RectTransform[] characters = new RectTransform[6];
    public RectTransform fireworkLayer;
    public Texture2D[] fireworkSheets;
    [Header("Lineup / Jump")]
    [Min(1)] public float characterSpacing = 235;
    public float floorY = -310;
    [Min(0)] public float jumpHeight = 48;
    [Min(0.1f)] public float jumpPeriod = 1.15f;
    [Min(0.05f)] public float jumpDuration = 0.45f;
    [Min(0)] public float fadeDuration = 0.65f;
    [Header("Fireworks")]
    public Vector2 fireworkInterval = new Vector2(0.7f,1f);
    public Vector2 flightDuration = new Vector2(0.65f,0.95f);
    public Vector2 burstSize = new Vector2(220,360);
    public Vector2 fireworkSizeMultiplier = new Vector2(0.5f,2.5f);
    public Vector2 flightDistanceMultiplier = new Vector2(0.8f,1.6f);
    public Vector2 targetX = new Vector2(-760,760);
    public Vector2 targetY = new Vector2(40,210);
    [Min(1)] public float framesPerSecond = 16;
    [Header("Navigation")]
    public string titleSceneName = "Title";
    static TrueEndingController instance;
    readonly List<RectTransform> lineup = new List<RectTransform>();
    readonly List<TrueEndingFirework> pool = new List<TrueEndingFirework>();
    List<Tween> pausedTweens;
    bool previousAudioPause;
    readonly Dictionary<Animator,float> pausedAnimators = new Dictionary<Animator,float>();
    float elapsed, nextFirework, previousTimeScale;
    bool running, previousCursorVisible, returning;
    CursorLockMode previousCursorLock;
    public static bool IsPlaying => instance && instance.running;
    public int VisibleCharacterCount => lineup.Count;
    public int FireworksLaunched { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic(){instance=null;}
    void OnEnable()
    {
        if(instance && instance!=this){Debug.LogError("TrueEndingController: Only one active controller is allowed.",this);enabled=false;return;}
        instance=this;
        if(presentation)presentation.SetActive(false);
    }

    /// <summary>순서: 단소, 오래된 남자, 힙합 소년, 자르반84세, 기사. 주인공은 항상 등장.</summary>
    public static bool PlayTrueEnding(bool danso, bool oldMan, bool hipHop, bool zaruban, bool knight)
    {
        if(!instance || !instance.isActiveAndEnabled){Debug.LogError("TrueEndingController: Add the TrueEndingPresentation prefab to the scene.");return false;}
        return instance.Begin(new[]{true,danso,oldMan,hipHop,zaruban,knight});
    }
    public static void StopTrueEnding(){if(instance)instance.StopPresentation();}
    bool Begin(bool[] included)
    {
        if(!presentation || !fadeGroup || !fireworkLayer || characters==null || characters.Length!=6 || fireworkSheets==null || fireworkSheets.Length<2)
        {Debug.LogError("TrueEndingController: Missing presentation, six character slots, or firework sheets.",this);return false;}
        foreach(var item in characters)if(!item){Debug.LogError("TrueEndingController: Missing character reference.",this);return false;}
        foreach(var sheet in fireworkSheets)if(!sheet){Debug.LogError("TrueEndingController: Missing firework texture.",this);return false;}
        StopPresentation();
        if(GameOverSequenceController.IsPlaying)GameOverSequenceController.StopGameOver();
        previousTimeScale=Time.timeScale;previousCursorVisible=Cursor.visible;previousCursorLock=Cursor.lockState;
        pausedTweens=DOTween.PlayingTweens();
        if(pausedTweens!=null)foreach(var tween in pausedTweens)tween.Pause();
        previousAudioPause=AudioListener.pause;AudioListener.pause=true;
        pausedAnimators.Clear();
        foreach(var animator in Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
            if(animator.isActiveAndEnabled && !animator.transform.IsChildOf(transform))
            {pausedAnimators.Add(animator,animator.speed);animator.speed=0;}
        Time.timeScale=0;Cursor.visible=true;Cursor.lockState=CursorLockMode.None;
        running=true;returning=false;elapsed=0;FireworksLaunched=0;nextFirework=RandomInterval();
        if(SoundManager.Instance) SoundManager.Instance.PlayPresentationMusic(SoundNames.FinalMusic);
        lineup.Clear();
        // Keep the protagonist near the middle of the selected cast.
        var selected=new List<RectTransform>();
        for(int i=1;i<characters.Length;i++){characters[i].gameObject.SetActive(included[i]);if(included[i])selected.Add(characters[i]);}
        selected.Insert(selected.Count/2,characters[0]);characters[0].gameObject.SetActive(true);lineup.AddRange(selected);
        for(int i=0;i<lineup.Count;i++)lineup[i].anchoredPosition=new Vector2((i-(lineup.Count-1)*0.5f)*characterSpacing,floorY);
        presentation.SetActive(true);fadeGroup.alpha=fadeDuration<=0?1:0;
        return true;
    }
    void Update()
    {
        if(!running)return;
        float dt=Time.unscaledDeltaTime;elapsed+=dt;
        fadeGroup.alpha=fadeDuration<=0?1:Mathf.SmoothStep(0,1,elapsed/fadeDuration);
        float period=Mathf.Max(0.1f,jumpPeriod),duration=Mathf.Clamp(jumpDuration,0.05f,period);
        for(int i=0;i<lineup.Count;i++)
        {
            float phase=Mathf.Repeat(elapsed-i*0.07f,period);
            float t=phase/duration;
            float y=phase<duration?4*t*(1-t)*jumpHeight:0;
            lineup[i].anchoredPosition=new Vector2((i-(lineup.Count-1)*0.5f)*characterSpacing,floorY+y);
        }
        nextFirework-=dt;
        if(nextFirework<=0){LaunchFirework();LaunchFirework();nextFirework=RandomInterval();}
    }
    float RandomInterval()=>Random.Range(Mathf.Max(0.05f,fireworkInterval.x),Mathf.Max(Mathf.Max(0.05f,fireworkInterval.x),fireworkInterval.y));
    void LaunchFirework()
    {
        TrueEndingFirework fx=null;
        foreach(var item in pool)if(item&&!item.gameObject.activeSelf){fx=item;break;}
        if(!fx)
        {
            if(pool.Count>=12)return;
            var go=new GameObject("CelebrationFirework",typeof(RectTransform),typeof(TrueEndingFirework));
            go.transform.SetParent(fireworkLayer,false);fx=go.GetComponent<TrueEndingFirework>();pool.Add(fx);
        }
        var destination=new Vector2(Random.Range(targetX.x,targetX.y),Random.Range(targetY.x,targetY.y));
        var origin=new Vector2(Mathf.Clamp(destination.x+Random.Range(-240,240),-850,850),floorY-40);
        float sizeScale=RandomMultiplier(fireworkSizeMultiplier);
        destination=origin+(destination-origin)*RandomMultiplier(flightDistanceMultiplier);
        fx.Launch(fireworkSheets,origin,destination,Random.Range(Mathf.Max(0.1f,flightDuration.x),Mathf.Max(0.1f,flightDuration.y)),Random.Range(burstSize.x,burstSize.y),Mathf.Max(1,framesPerSecond),sizeScale);
        FireworksLaunched++;
        if(SoundManager.Instance) SoundManager.Instance.PlaySound(SoundNames.Firework,0.64f,SoundNames.FireworkParameter,1f);
    }
    static float RandomMultiplier(Vector2 range)=>Random.Range(Mathf.Max(0.01f,Mathf.Min(range.x,range.y)),Mathf.Max(0.01f,Mathf.Max(range.x,range.y)));
    void StopPresentation()
    {
        foreach(var item in pool)if(item)item.gameObject.SetActive(false);
        if(presentation)presentation.SetActive(false);
        if(!running)return;
        running=false;
        if(SoundManager.Instance) SoundManager.Instance.StopSound(SoundNames.FinalMusic);
        Time.timeScale=previousTimeScale;Cursor.visible=previousCursorVisible;Cursor.lockState=previousCursorLock;
        AudioListener.pause=previousAudioPause;
        foreach(var item in pausedAnimators)if(item.Key)item.Key.speed=item.Value;
        pausedAnimators.Clear();
        if(pausedTweens!=null)foreach(var tween in pausedTweens)if(tween.IsActive()&&!tween.IsComplete())tween.Play();
        pausedTweens=null;lineup.Clear();
    }
    public void ReturnToTitle()
    {
        if(returning)return;
        if(string.IsNullOrWhiteSpace(titleSceneName)||!Application.CanStreamedLevelBeLoaded(titleSceneName))
        {Debug.LogError("TrueEndingController: Title scene must be enabled in the build scene list: "+titleSceneName,this);return;}
        returning=true;StopPresentation();
        JSH.Gameplay.EndingStats.ClearLast();
        if(JSH.Core.GameManager.HasInstance)JSH.Core.GameManager.Instance.GoToTitle();
        if(JSH.Core.SceneLoader.HasInstance)JSH.Core.SceneLoader.Instance.Load(titleSceneName);
        else SceneManager.LoadScene(titleSceneName);
    }
    void OnDisable(){if(instance==this){StopPresentation();instance=null;}}
    [ContextMenu("Test/All Characters")] public void TestAll(){if(Application.isPlaying)PlayTrueEnding(true,true,true,true,true);else Debug.LogError("Enter Play mode first.",this);}
    [ContextMenu("Test/Selected Characters")] public void TestSelected(){if(Application.isPlaying)PlayTrueEnding(true,false,true,false,true);else Debug.LogError("Enter Play mode first.",this);}
    [ContextMenu("Test/Protagonist Only")] public void TestMainOnly(){if(Application.isPlaying)PlayTrueEnding(false,false,false,false,false);else Debug.LogError("Enter Play mode first.",this);}
    [ContextMenu("Test/Stop")] public void TestStop(){StopTrueEnding();}
}
