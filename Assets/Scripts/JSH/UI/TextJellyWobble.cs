using TMPro;
using UnityEngine;

namespace JSH.UI
{
    /// <summary>
    /// Keeps a TMP text softly wobbling like jelly: each glyph squashes / stretches and its corners drift.
    /// Vertex-only (does not touch the Transform), so it can run together with scale / position tweens.
    /// Add it to the TMP object; disable it to stop (the mesh is restored).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextJellyWobble : MonoBehaviour
    {
        [Tooltip("Squash / stretch amount (0.1 = 10%).")]
        [SerializeField, Range(0f, 0.5f)] private float _squash = 0.1f;
        [SerializeField, Min(0f)] private float _squashSpeed = 3f;
        [Tooltip("Corner drift in text units.")]
        [SerializeField, Min(0f)] private float _wobble = 2.5f;
        [SerializeField, Min(0f)] private float _wobbleSpeed = 4f;
        [Tooltip("Phase offset between characters (multi-letter texts ripple).")]
        [SerializeField] private float _charPhase = 0.6f;
        [SerializeField] private bool _unscaledTime = true;

        private TMP_Text _text;

        private void Awake() => TryGetComponent(out _text);

        private void OnDisable()
        {
            if (_text != null)
            {
                _text.ForceMeshUpdate();
            }
        }

        private void LateUpdate()
        {
            if (_text == null)
            {
                return;
            }

            _text.ForceMeshUpdate();
            TMP_TextInfo info = _text.textInfo;
            float t = _unscaledTime ? Time.unscaledTime : Time.time;

            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo ci = info.characterInfo[i];
                if (!ci.isVisible)
                {
                    continue;
                }

                Vector3[] v = info.meshInfo[ci.materialReferenceIndex].vertices;
                int vi = ci.vertexIndex;
                Vector3 center = (v[vi] + v[vi + 2]) * 0.5f;
                float s = Mathf.Sin(t * _squashSpeed + i * _charPhase) * _squash;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 p = v[vi + j] - center;
                    p.x *= 1f + s;
                    p.y *= 1f - s;
                    float w = t * _wobbleSpeed + j * 1.7f + i * _charPhase;
                    p.x += Mathf.Sin(w) * _wobble;
                    p.y += Mathf.Cos(w * 1.3f) * _wobble;
                    v[vi + j] = center + p;
                }
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
