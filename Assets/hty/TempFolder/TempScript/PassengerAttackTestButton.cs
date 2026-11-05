using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class PassengerAttackTestButton : MonoBehaviour
{
    public string attackerKey = "Women";
    UnityEngine.UI.Button button;
    void Awake() { button = GetComponent<UnityEngine.UI.Button>(); }
    void OnEnable() { button.onClick.AddListener(Play); }
    void Update() { button.interactable = !PassengerAttackController.IsPlaying; }
    void OnDisable() { button.onClick.RemoveListener(Play); button.interactable = true; }
    void Play() { PassengerAttackController.PlayAttack(attackerKey); }
}
