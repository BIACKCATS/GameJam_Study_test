using System;
using DG.Tweening;
using UnityEngine;

public sealed class PassengerObjectionPanel : MonoBehaviour
{
    public UnityEngine.UI.RawImage panel;
    public CanvasGroup opacity;
    [Header("Stamp")]
    [Min(1)] public float startScale = 3f;
    [Min(0.01f)] public float stampDuration = 0.22f;
    [Range(0, 0.3f)] public float reboundStrength = 0.08f;
    [Min(0.01f)] public float reboundDuration = 0.18f;
    [Min(0)] public float holdDuration = 0.45f;
    [Min(0.01f)] public float fadeDuration = 0.4f;
    public Vector2 panelSize = new Vector2(1920, 1080);
    public Vector2 endPosition = Vector2.zero;
    Sequence sequence;
    Action cancelled;

    void Awake() { Hide(); }

    public bool Play(Texture2D texture, Action completed, Action onCancelled, Action onImpact = null)
    {
        if (!isActiveAndEnabled || !panel || !opacity || !texture)
        {
            Debug.LogError("PassengerObjectionPanel: Assign an active panel, CanvasGroup and Objection texture.", this);
            return false;
        }
        Cancel();
        var rect = panel.rectTransform;
        var parent = rect.parent as RectTransform;
        if (!parent) { Debug.LogError("PassengerObjectionPanel: Panel requires a RectTransform parent.", this); return false; }
        Canvas.ForceUpdateCanvases();
        rect.sizeDelta = new Vector2(Mathf.Max(1, panelSize.x), Mathf.Max(1, panelSize.y));
        rect.anchoredPosition = endPosition;
        rect.localScale = Vector3.one * Mathf.Max(1, startScale);
        panel.texture = texture;
        panel.raycastTarget = false;
        panel.enabled = true;
        opacity.alpha = 1;
        opacity.blocksRaycasts = false;
        opacity.interactable = false;
        cancelled = onCancelled;
        sequence = DOTween.Sequence();
        // Accelerate toward the screen, then briefly rebound before settling.
        sequence.Append(rect.DOScale(1f, Mathf.Max(0.01f, stampDuration)).SetEase(Ease.InCubic));
        if(onImpact!=null)sequence.AppendCallback(()=>onImpact());
        float rebound = Mathf.Max(0.01f, reboundDuration);
        sequence.Append(rect.DOScale(1f + Mathf.Clamp(reboundStrength, 0, 0.3f), rebound * 0.4f).SetEase(Ease.OutQuad));
        sequence.Append(rect.DOScale(1f, rebound * 0.6f).SetEase(Ease.OutQuad));
        sequence.AppendInterval(Mathf.Max(0, holdDuration));
        sequence.Append(opacity.DOFade(0, Mathf.Max(0.01f, fadeDuration)).SetEase(Ease.Linear));
        sequence.OnComplete(() =>
        {
            sequence = null;
            cancelled = null;
            Hide();
            completed?.Invoke();
        });
        return true;
    }

    public void Cancel()
    {
        if (sequence != null) { sequence.Kill(false); sequence = null; }
        var callback = cancelled;
        cancelled = null;
        Hide();
        callback?.Invoke();
    }
    void Hide()
    {
        if (panel) { panel.enabled = false; panel.rectTransform.localScale = Vector3.one; }
        if (opacity) opacity.alpha = 0;
    }
    void OnDisable() { Cancel(); }
}
