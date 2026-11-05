using UnityEngine;
[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class GongikNagaTestButton : MonoBehaviour
{
    public string villainKey="Knight";
    UnityEngine.UI.Button button;
    void Awake(){button=GetComponent<UnityEngine.UI.Button>();}
    void OnEnable(){button.onClick.AddListener(Play);}
    void Update(){button.interactable=!GongikNagaController.IsPlaying && !PassengerAttackController.IsPlaying;}
    void Play(){GongikNagaController.Play(villainKey);}
    void OnDisable(){button.onClick.RemoveListener(Play);button.interactable=true;}
}
