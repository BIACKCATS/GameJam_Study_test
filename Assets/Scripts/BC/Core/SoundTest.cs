using UnityEngine;
using BC.Core;

public class SoundTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayTrain(SoundNames.Train, 0.8f);    // 평상시 음악 시작
        SoundManager.Instance.StartAttack(SoundNames.Attack, 0.8f); // 공격 시작
        SoundManager.Instance.EndAttack();                          // 공격 끝 (인스펙터에 설정한 시간, 기본 2초)
        SoundManager.Instance.EndAttack(3f);                        // 3초에 걸쳐 돌아오게
        SoundManager.Instance.PlaySound(SoundNames.UI, 0.8f);       // 효과음 1회
        SoundManager.Instance.SetMasterVolume(0.5f);                // 전체 볼륨만 변경

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
