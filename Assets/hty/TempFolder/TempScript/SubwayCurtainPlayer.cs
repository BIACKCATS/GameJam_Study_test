using System.Collections;
using UnityEngine;

public sealed class SubwayCurtainPlayer : MonoBehaviour
{
    public UnityEngine.UI.RawImage overlay;
    public UnityEngine.UI.Button testButton;
    public Texture2D enterSheet, blackTexture, exitSheet;
    [Min(0.1f)] public float framesPerSecond = 20;
    [Min(0)] public float blackHoldSeconds = 1;
    public bool IsPlaying { get; private set; }
    public string Phase { get; private set; } = "Idle";
    void OnEnable() { if (testButton) testButton.onClick.AddListener(Play); }
    void OnDisable()
    {
        if (testButton) testButton.onClick.RemoveListener(Play);
        StopAllCoroutines(); Finish();
    }
    public void Play()
    {
        if (IsPlaying) return;
        if (!overlay || !enterSheet || !exitSheet || !blackTexture)
        { Debug.LogError("SubwayCurtainPlayer: Assign overlay and all three textures.", this); return; }
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
            overlay.uvRect = new Rect(index % 3 / 3f, 1f - (index / 3 + 1) / 3f, 1f/3f, 1f/3f);
            yield return null;
            frame += Time.unscaledDeltaTime * Mathf.Max(0.1f,framesPerSecond);
        }
    }
    void Finish()
    {
        IsPlaying = false; Phase = "Idle";
        if (overlay) { overlay.enabled = false; overlay.raycastTarget = false; }
        if (testButton) testButton.interactable = true;
    }
}
