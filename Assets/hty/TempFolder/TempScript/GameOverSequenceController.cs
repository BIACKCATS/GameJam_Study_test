using UnityEngine;
using UnityEngine.Events;

public sealed class GameOverSequenceController : MonoBehaviour
{
    public GameObject presentation;
    public UnityEngine.UI.RawImage character, lightOverlay, rightTyping, leftTyping, rightBubble, leftBubble;
    public UnityEngine.UI.Text rightText, leftText;
    public GameOverEndingMessages endingMessages;
    [Min(0)] public float characterFadeDuration = 0.8f;
    [TextArea] public string rightMessage = "";
    [TextArea] public string leftMessage = "";
    [Header("Light flicker (seconds)")]
    public Vector2 flickerInterval = new Vector2(0.8f,1.85f);
    [Range(0,1)] public float doubleFlickerChance = 0.3f;
    [Min(0.01f)] public float flickerOffDuration = 0.08f;
    [Min(0.01f)] public float doubleFlickerGap = 0.08f;
    [Range(0,1)] public float lightOpacity = 0.65f;
    [Header("Message timing")]
    [Min(0)] public float rightTypingDelay = 0f;
    [Min(0)] public float rightBubbleDelay = 1.2f;
    [Min(0)] public float leftTypingDelay = 0.5f;
    [Min(0)] public float leftBubbleDelay = 0.5f;
    [Min(0.1f)] public float typingFramesPerSecond = 24f;
    public Color rightBubbleColor = new Color(0.3f,1f,0.45f,1);
    public Color leftBubbleColor = Color.white;
    [Header("Connect additional game logic here")]
    public UnityEvent onStarted = new UnityEvent();
    public UnityEvent onRightBubbleShown = new UnityEvent();
    public UnityEvent onLeftTypingStarted = new UnityEvent();
    public UnityEvent onSequenceCompleted = new UnityEvent();
    public UnityEvent onStopped = new UnityEvent();
    static GameOverSequenceController instance;
    public static bool IsPlaying => instance && instance.running;
    bool running;
    float elapsed, flickerTimer;
    int stage, flickerPhase;
    bool doubleFlicker;
    float previousTimeScale;
    bool worldPaused, previousCursorVisible;
    CursorLockMode previousCursorLock;
    System.Collections.Generic.List<DG.Tweening.Tween> pausedTweens;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic(){instance=null;}
    void OnEnable()
    {
        if(instance && instance!=this){Debug.LogError("GameOverSequenceController: Only one active controller is supported.",this);enabled=false;return;}
        instance=this;
        ResetPresentation();
    }
    public static bool PlayGameOver()
    {
        if(TrueEndingController.IsPlaying)return false;
        if(!instance || !instance.isActiveAndEnabled){Debug.LogError("GameOverSequenceController: Place an active GameOverPresentation prefab in the scene.");return false;}
        return instance.Begin();
    }
    public static void StopGameOver()
    {
        if(!instance){Debug.LogError("GameOverSequenceController: No active controller.");return;}
        bool wasRunning=instance.running;
        instance.ResetPresentation();
        if(wasRunning)instance.InvokeSafely(instance.onStopped);
    }
    bool Begin()
    {
        if(running){Debug.LogError("GameOverSequenceController: Already playing.",this);return false;}
        if(!presentation||!character||!lightOverlay||!rightTyping||!leftTyping||!rightBubble||!leftBubble||
            !character.texture||!lightOverlay.texture||!rightTyping.texture||!leftTyping.texture||!rightBubble.texture||!leftBubble.texture)
        {Debug.LogError("GameOverSequenceController: Missing presentation/image/texture reference.",this);return false;}
        ResetPresentation();
        running=true;elapsed=0;stage=0;flickerPhase=0;flickerTimer=NextInterval();
        PauseWorld();
        presentation.SetActive(true);
        if(endingMessages&&!endingMessages.Begin()){ResetPresentation();return false;}
        SetLight(true);
        rightBubble.color=rightBubbleColor;leftBubble.color=leftBubbleColor;
        if(rightText)rightText.text=rightMessage;
        if(leftText)leftText.text=leftMessage;
        InvokeSafely(onStarted);
        if(running)RenderTimeline();
        return true;
    }
    void Update()
    {
        if(!running)return;
        elapsed+=Time.unscaledDeltaTime;
        RenderTimeline();
        if(running)TickFlicker(Time.unscaledDeltaTime);
    }
    void RenderTimeline()
    {
        var characterColor=character.color;
        characterColor.a=characterFadeDuration<=0?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/characterFadeDuration));
        character.color=characterColor;
        float rightStart=Mathf.Max(0,rightTypingDelay);
        float rightEnd=rightStart+Mathf.Max(0,rightBubbleDelay);
        float leftStart=rightEnd+Mathf.Max(0,leftTypingDelay);
        float leftEnd=leftStart+Mathf.Max(0,leftBubbleDelay);
        rightTyping.enabled=elapsed>=rightStart&&elapsed<rightEnd;
        leftTyping.enabled=elapsed>=leftStart&&elapsed<leftEnd;
        AnimateTyping(rightTyping,elapsed-rightStart);AnimateTyping(leftTyping,elapsed-leftStart);
        if(stage<1&&elapsed>=rightEnd){stage=1;if(endingMessages)endingMessages.ShowRightBubble();else rightBubble.gameObject.SetActive(true);InvokeSafely(onRightBubbleShown);if(!running)return;}
        if(stage<2&&elapsed>=leftStart){stage=2;InvokeSafely(onLeftTypingStarted);if(!running)return;}
        if(stage<3&&elapsed>=leftEnd){stage=3;if(endingMessages)endingMessages.ShowLeftBubble();else leftBubble.gameObject.SetActive(true);InvokeSafely(onSequenceCompleted);}
    }
    void AnimateTyping(UnityEngine.UI.RawImage image,float time)
    {
        int frame=Mathf.FloorToInt(Mathf.Max(0,time)*Mathf.Max(0.1f,typingFramesPerSecond))%36;
        image.uvRect=new Rect(frame%5/5f,1-(frame/5+1)/8f,1f/5,1f/8);
    }
    float NextInterval()=>Random.Range(Mathf.Max(0.01f,flickerInterval.x),Mathf.Max(Mathf.Max(0.01f,flickerInterval.x),flickerInterval.y));
    void TickFlicker(float delta)
    {
        flickerTimer-=delta;
        if(flickerTimer>0)return;
        switch(flickerPhase)
        {
            case 0: doubleFlicker=Random.value<Mathf.Clamp01(doubleFlickerChance);SetLight(false);flickerPhase=1;flickerTimer=Mathf.Max(0.01f,flickerOffDuration);break;
            case 1: SetLight(true);flickerPhase=doubleFlicker?2:0;flickerTimer=doubleFlicker?Mathf.Max(0.01f,doubleFlickerGap):NextInterval();break;
            case 2: SetLight(false);flickerPhase=3;flickerTimer=Mathf.Max(0.01f,flickerOffDuration);break;
            default: SetLight(true);flickerPhase=0;flickerTimer=NextInterval();break;
        }
    }
    void SetLight(bool on){var c=lightOverlay.color;c.a=on?lightOpacity:0;lightOverlay.color=c;}
    void ResetPresentation()
    {
        running=false;
        if(endingMessages)endingMessages.Stop();
        ResumeWorld();
        if(presentation)presentation.SetActive(false);
        if(rightTyping)rightTyping.enabled=false;if(leftTyping)leftTyping.enabled=false;
        if(rightBubble)rightBubble.gameObject.SetActive(false);if(leftBubble)leftBubble.gameObject.SetActive(false);
    }
    void PauseWorld()
    {
        if(worldPaused)return;
        previousTimeScale=Time.timeScale;
        previousCursorVisible=Cursor.visible;previousCursorLock=Cursor.lockState;
        worldPaused=true;
        pausedTweens=DG.Tweening.DOTween.PlayingTweens();
        if(pausedTweens!=null)foreach(var tween in pausedTweens)DG.Tweening.TweenExtensions.Pause(tween);
        Time.timeScale=0;
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
    }
    void ResumeWorld()
    {
        if(!worldPaused)return;
        Time.timeScale=previousTimeScale;
        if(pausedTweens!=null)
        {
            foreach(var tween in pausedTweens)
                if(DG.Tweening.TweenExtensions.IsActive(tween)&&!DG.Tweening.TweenExtensions.IsComplete(tween))DG.Tweening.TweenExtensions.Play(tween);
            pausedTweens=null;
        }
        Cursor.lockState=previousCursorLock;Cursor.visible=previousCursorVisible;
        worldPaused=false;
    }
    void InvokeSafely(UnityEvent callback)
    {
        try{callback?.Invoke();}catch(System.Exception ex){Debug.LogError("GameOverSequenceController callback: "+ex,this);}
    }
    void OnDisable(){if(instance==this){ResetPresentation();instance=null;}}
    [ContextMenu("Test/Play Game Over")] public void TestPlay(){if(Application.isPlaying)PlayGameOver();else Debug.LogError("Enter Play mode to test Game Over.",this);}
    [ContextMenu("Test/Stop Game Over")] public void TestStop(){if(Application.isPlaying)StopGameOver();}
}
