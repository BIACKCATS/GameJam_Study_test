using UnityEngine;
using BC.Core;
using DG.Tweening;

// Instantiates the TempFolder variant; never edits the shared JSH prefab or scripts.
public sealed class GameOverEndingMessages : MonoBehaviour
{
    public GameObject presentationPrefab;
    [Min(0.01f)] public float messagePopDuration = 0.35f;
    public Ease messagePopEase = Ease.OutBack;
    GameObject instance;
    GameObject header;
    JSH.UI.EndingMessageSequence sequence;
    Sequence headerPop;
    Vector3 bubbleScale, titleScale;
    public bool Begin()
    {
        Stop();
        if(!presentationPrefab){Debug.LogError("GameOverEndingMessages: Assign the ending message variant.",this);return false;}
        instance=Instantiate(presentationPrefab,transform);
        instance.SetActive(true);
        header=instance.transform.Find("Header").gameObject;
        bubbleScale=header.transform.Find("HeaderBubble").localScale;
        titleScale=header.transform.Find("HeaderBubble/TitleText").localScale;
        sequence=instance.GetComponentInChildren<JSH.UI.EndingMessageSequence>(true);
        header.SetActive(false);
        instance.GetComponent<JSH.UI.EndingRestartPrompt>().Play();
        return true;
    }
    public void ShowRightBubble()
    {
        if(!header || header.activeSelf)return;
        if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.MessageSendingAlarm,0.8f);
        header.SetActive(true);
        header.transform.Find("HeaderBubble/Dots").gameObject.SetActive(false);
        var content=header.transform.Find("HeaderBubble/TitleText");
        content.gameObject.SetActive(true);
        headerPop?.Kill();
        var bubble=header.transform.Find("HeaderBubble");
        bubble.localScale=Vector3.zero;
        content.localScale=Vector3.zero;
        // Match the Web label's simultaneous bubble/text pop, preserving the mirrored layout.
        float duration=Mathf.Max(0.01f,messagePopDuration);
        headerPop=DOTween.Sequence().SetUpdate(true);
        headerPop.Join(bubble.DOScale(bubbleScale,duration).SetEase(messagePopEase));
        headerPop.Join(content.DOScale(titleScale,duration).SetEase(messagePopEase));
    }
    public void ShowLeftBubble(){if(sequence)sequence.Play();}
    public void Stop()
    {
        headerPop?.Kill();headerPop=null;
        if(instance){instance.SetActive(false);Destroy(instance);}
        instance=null;header=null;sequence=null;
    }
    void OnDisable(){Stop();}
}
