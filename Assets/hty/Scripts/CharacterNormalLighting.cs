using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Hty.CharacterLighting
{
    public sealed class CharacterNormalLighting : MonoBehaviour
    {
        public SpriteRenderer character;
        public Sprite[] frames;
        public Light2D movingLight;
        public Camera testCamera;
        public bool normals = true, animate = true, slowOrbit;
        public int frame;
        public int direction;
        public float orbitAngle;
        public int renderedUpdates;
        static readonly FieldInfo Quality = typeof(Light2D).GetField("m_NormalMapQuality", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly Vector3[] Directions = { Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        float clock;

        public void SetState(int f, int d, bool normal, float angle = -1)
        {
            if (!character || !movingLight || frames == null || frames.Length != 6 || Quality == null)
                throw new InvalidOperationException("HTY rig requires six sprites, renderer, light and URP17 m_NormalMapQuality field");
            frame = Mathf.Clamp(f, 0, frames.Length - 1);
            direction = d; normals = normal;
            character.sprite = frames[frame];
            var requested = normal ? Light2D.NormalMapQuality.Accurate : Light2D.NormalMapQuality.Disabled;
            if (movingLight.normalMapQuality != requested) Quality.SetValue(movingLight, requested);
            movingLight.transform.position = angle >= 0
                ? new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 1.35f
                : Directions[Mathf.Clamp(d, 0, 3)] * 1.35f;
        }
        void Start() { Application.runInBackground = true; SetState(frame, direction, normals); }
        void Update()
        {
            renderedUpdates++;
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.wasPressedThisFrame) { direction = 0; slowOrbit = false; }
                if (k.rightArrowKey.wasPressedThisFrame) { direction = 1; slowOrbit = false; }
                if (k.upArrowKey.wasPressedThisFrame) { direction = 2; slowOrbit = false; }
                if (k.downArrowKey.wasPressedThisFrame) { direction = 3; slowOrbit = false; }
                if (k.nKey.wasPressedThisFrame) normals = !normals;
                if (k.oKey.wasPressedThisFrame) slowOrbit = !slowOrbit;
                if (k.spaceKey.wasPressedThisFrame) animate = !animate;
                if (k.periodKey.wasPressedThisFrame) { animate = false; frame = (frame + 1) % frames.Length; }
                if (k.commaKey.wasPressedThisFrame) { animate = false; frame = (frame + frames.Length - 1) % frames.Length; }
            }
            clock += Time.unscaledDeltaTime;
            if (animate && clock >= .15f) { frame = (frame + 1) % frames.Length; clock = 0; }
            if (slowOrbit) orbitAngle = Mathf.Repeat(orbitAngle + Time.unscaledDeltaTime * .35f, Mathf.PI * 2);
            SetState(frame, direction, normals, slowOrbit ? orbitAngle : -1);
        }
        void OnGUI()
        {
            GUI.Label(new Rect(12, 10, 950, 70), "HTY / Febucci sample character | TEST 160 x 120, PPU 16\nArrows: light direction | O: slow orbit | N: normals " + (normals ? "ON (Accurate)" : "OFF (Disabled)") +
                " | Space: animation | , / . : frame " + frame + "\nBoth states retain Sprite-Lit material, identical light color/intensity/radius; diffuse relighting, no specular highlight model.");
        }
    }
}
