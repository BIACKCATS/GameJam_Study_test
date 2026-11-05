using BC.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BC.Test
{
    /// <summary>
    /// Test only. 1: train, 2: start attack, 3: end attack, 4: UI one-shot, -/=: master volume.
    /// Needs a SoundManager in the scene.
    /// </summary>
    public class TestSoundInput : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 0.8f;
        [SerializeField] private float _volumeStep = 0.1f;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            SoundManager sound = SoundManager.Instance;
            if (keyboard == null || sound == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) sound.PlayTrain(SoundNames.Train, _masterVolume);
            if (keyboard.digit2Key.wasPressedThisFrame) sound.StartAttack(SoundNames.Attack, _masterVolume);
            if (keyboard.digit3Key.wasPressedThisFrame) sound.EndAttack();
            if (keyboard.digit4Key.wasPressedThisFrame) sound.PlaySound(SoundNames.UI, _masterVolume);

            if (keyboard.minusKey.wasPressedThisFrame) ChangeVolume(sound, -_volumeStep);
            if (keyboard.equalsKey.wasPressedThisFrame) ChangeVolume(sound, _volumeStep);
        }

        private void ChangeVolume(SoundManager sound, float delta)
        {
            _masterVolume = Mathf.Clamp01(_masterVolume + delta);
            sound.SetMasterVolume(_masterVolume);
        }
    }
}
