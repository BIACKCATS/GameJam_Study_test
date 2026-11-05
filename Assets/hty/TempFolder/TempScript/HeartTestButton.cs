using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class HeartTestButton : MonoBehaviour
{
    [Range(1, 3)] public int heartNumber = 1;
    UnityEngine.UI.Button button;
    void Awake() { button = GetComponent<UnityEngine.UI.Button>(); }
    void OnEnable() { button.onClick.AddListener(BreakHeart); }
    void OnDisable() { button.onClick.RemoveListener(BreakHeart); }
    public void BreakHeart() { HeartAnimationPlayer.PlayBreak(heartNumber); }
}
