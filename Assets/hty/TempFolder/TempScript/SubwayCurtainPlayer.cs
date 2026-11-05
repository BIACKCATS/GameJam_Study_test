using System.Collections;
using UnityEngine;

public sealed class SubwayCurtainPlayer : MonoBehaviour
{
    public UnityEngine.UI.RawImage overlay;
    public UnityEngine.UI.Button testButton;
    public Texture2D enterSheet, blackTexture, exitSheet;
    [Header("Train movement (parent of BG and Interior)")]
    public Transform trainRoot;
    public Camera movementCamera;
    [Tooltip("Screen coverage of the curtain in each Enter frame.")]
    public float[] enterCoverage = { 0f, 1f/7f, 2f/7f, 3f/7f, 4f/7f, 5f/7f, 6f/7f, 1f };
    Vector3 originalPosition, slideVector;
    bool positionCaptured;
    [Min(0.1f)] public float framesPerSecond = 20;
    [Min(0)] public float blackHoldSeconds = 1;
    public bool IsPlaying { get; private set; }
    public string Phase { get; private set; } = "Idle";
    static SubwayCurtainPlayer instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }
    void OnEnable()
    {
        if (instance && instance != this) { Debug.LogError("SubwayCurtainPlayer: Multiple active controllers.", this); return; }
        instance = this;
        if (testButton) testButton.onClick.AddListener(Play);
    }
    void OnDisable()
    {
        if (testButton) testButton.onClick.RemoveListener(Play);
        StopAllCoroutines(); Finish();
        if (instance == this) instance = null;
    }
    /// <summary>Play the complete enter, blackout hold, and exit transition.</summary>
    public static void Play()
    {
        if (!instance || !instance.isActiveAndEnabled) { Debug.LogError("SubwayCurtainPlayer: No active controller."); return; }
        instance.Begin();
    }
    void Begin()
    {
        if (IsPlaying) { Debug.LogError("SubwayCurtainPlayer: A transition is already playing.", this); return; }
        if (!overlay || !enterSheet || !exitSheet || !blackTexture)
        { Debug.LogError("SubwayCurtainPlayer: Assign overlay and all three textures.", this); return; }
        var camera = movementCamera ? movementCamera : Camera.main;
        if (!trainRoot || !camera) { Debug.LogError("SubwayCurtainPlayer: Assign Train Root and a movement camera.", this); return; }
        originalPosition = trainRoot.position;
        positionCaptured = true;
        float depth = Vector3.Dot(originalPosition - camera.transform.position, camera.transform.forward);
        slideVector = camera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth)) - camera.ViewportToWorldPoint(new Vector3(0, 0.5f, depth));
        StartCoroutine(Sequence());
    }
    IEnumerator Sequence()
    {
        IsPlaying = true;
        if (testButton) testButton.interactable = false;
        overlay.enabled = true; overlay.raycastTarget = true;
        Phase = "Enter";
        yield return Animate(enterSheet);
        Phase = "Black";
        overlay.texture = blackTexture; overlay.uvRect = new Rect(0,0,1,1);
        // Reposition off to the right while black, then slide left into the original view.
        if (trainRoot && positionCaptured)
            trainRoot.position = originalPosition + slideVector * Coverage(7);
        yield return new WaitForSecondsRealtime(Mathf.Max(0,blackHoldSeconds));
        Phase = "Exit";
        yield return Animate(exitSheet);
        Finish();
    }
    IEnumerator Animate(Texture2D sheet)
    {
        overlay.texture = sheet;
        float frame = 0;
        while (frame < 8)
        {
            int index = Mathf.FloorToInt(frame);
            if (trainRoot && positionCaptured)
            {
                float progress = Coverage(index);
                float offset = Phase == "Enter" ? -progress : Coverage(7) - progress;
                trainRoot.position = originalPosition + slideVector * offset;
            }
            overlay.uvRect = new Rect(index % 3 / 3f, 1f - (index / 3 + 1) / 3f, 1f/3f, 1f/3f);
            yield return null;
            frame += Time.unscaledDeltaTime * Mathf.Max(0.1f,framesPerSecond);
        }
        if (trainRoot && positionCaptured)
            trainRoot.position = Phase == "Enter" ? originalPosition - slideVector * Coverage(7) : originalPosition;
    }
    float Coverage(int index) => Mathf.Clamp01(enterCoverage != null && enterCoverage.Length == 8 ? enterCoverage[index] : index / 7f);
    void Finish()
    {
        RestorePosition();
        IsPlaying = false; Phase = "Idle";
        if (overlay) { overlay.enabled = false; overlay.raycastTarget = false; }
        if (testButton) testButton.interactable = true;
    }
    void RestorePosition()
    {
        if (positionCaptured && trainRoot) trainRoot.position = originalPosition;
        positionCaptured = false;
    }
}
