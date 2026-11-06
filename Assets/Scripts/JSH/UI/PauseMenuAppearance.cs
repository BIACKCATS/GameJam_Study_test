using UnityEngine;

namespace JSH.UI
{
    [CreateAssetMenu(menuName = "JSH/UI/Pause Menu Appearance")]
    public sealed class PauseMenuAppearance : ScriptableObject
    {
        public Sprite returnNormal;
        public Sprite returnPressed;
        public Sprite exitNormal;
        public Sprite exitPressed;
    }
}
