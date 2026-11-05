using System;
using UnityEngine;
using BC.Core;

public sealed class PassengerAttackController : MonoBehaviour
{
    [Serializable]
    public sealed class Attacker
    {
        public string key;
        public Sprite sprite;
        public Texture2D objectionTexture;
        [Min(0.1f)] public float height = 3.4f;
        [Tooltip("Contact point within the sprite rectangle: bottom-left (0,0), top-right (1,1).")]
        public Vector2 kickPoint = new Vector2(0.04f, 0.35f);
    }
    public SpriteRenderer victim;
    public SpriteRenderer attacker;
    public Attacker[] attackers;
    public PassengerObjectionPanel objection;
    [Header("AttackStar impact (4 columns x 3 rows, 128px cells)")]
    public SpriteRenderer impactEffect;
    public Texture2D impactSheet;
    [Min(0.1f)] public float impactFramesPerSecond = 24f;
    [Min(0.1f)] public float impactSize = 3.5f;
    public AttackCameraShake cameraShake;
    public Vector2 impactEffectOffset;
    Sprite[] impactFrames;
    [Header("Positions relative to this prefab")]
    public Vector2 impactPoint = new Vector2(-1.2f, -0.1f);
    public Vector2 approachOffset = new Vector2(9f, 0.7f);
    public Vector2 retreatOffset = new Vector2(2f, -0.4f);
    [Header("Timing (seconds)")]
    [Min(0.01f)] public float flightDuration = 0.45f;
    [Min(0)] public float impactHold = 0.18f;
    [Min(0.01f)] public float recoveryDuration = 1.4f;
    [Header("Bounce and off-screen exit")]
    [Min(0)] public float bounceHeight = 1.2f;
    public Camera exitCamera;
    [Min(0)] public float exitMargin = 0.5f;
    [Min(0)] public float victimLinger = 0.5f;
    [Min(0.01f)] public float victimFadeDuration = 0.4f;
    [Min(0)] public float arcHeight = 0.5f;
    [Header("Victim reaction")]
    [Min(0)] public float knockback = 0.35f;
    public float recoilAngle = 12f;
    public bool hideVictimWhenIdle = true;
    static PassengerAttackController instance;
    public static bool IsPlaying => instance && instance.playing;
    bool playing, hitTriggered, preparing;
    float elapsed;
    Vector3 victimRest, attackerImpact, exitOffset;
    Quaternion victimRotation;
    Color victimColor, attackerColor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }

    void OnEnable()
    {
        if (instance && instance != this)
        {
            Debug.LogError("PassengerAttackController: Only one active attack prefab is supported.", this);
            enabled = false;
            return;
        }
        if (!victim || !attacker || !victim.sprite || victim.transform.parent != transform || attacker.transform.parent != transform)
        {
            Debug.LogError("PassengerAttackController: Assign victim and attacker as direct children, and a victim sprite.", this);
            enabled = false;
            return;
        }
        instance = this;
        if (impactEffect && impactSheet && impactFrames == null)
        {
            impactFrames = new Sprite[12];
            for (int i = 0; i < 12; i++)
                impactFrames[i] = Sprite.Create(impactSheet, new Rect((i % 4) * 128, (2 - i / 4) * 128, 128, 128), new Vector2(0.5f, 0.5f), 128);
        }
        victimRest = victim.transform.localPosition;
        victimRotation = victim.transform.localRotation;
        victimColor = victim.color;
        attackerColor = attacker.color;
        ResetVisuals();
    }

    /// <summary>Requires one active PassengerAttack prefab in the scene. Keys are case-insensitive.
    /// Returns false and logs an error for invalid input, missing references, or a busy effect.</summary>
    public static bool PlayAttack(string key)
    {
        if(TrueEndingController.IsPlaying)return false;
        if (!instance || !instance.isActiveAndEnabled)
        {
            Debug.LogError("PassengerAttackController: Place an active PassengerAttack prefab in the scene first.");
            return false;
        }
        return instance.Begin(key);
    }

    bool Begin(string key)
    {
        if (GongikNagaController.IsPlaying) { Debug.LogError("PassengerAttackController: GongikNaga is playing.", this); return false; }
        Attacker selected = null;
        if (!string.IsNullOrWhiteSpace(key) && attackers != null)
            foreach (var entry in attackers)
                if (entry != null && string.Equals(entry.key, key.Trim(), StringComparison.OrdinalIgnoreCase)) { selected = entry; break; }
        if (selected == null || !selected.sprite)
        {
            Debug.LogError("PassengerAttackController: Unknown key or missing sprite: '" + key + "'. Use Women, Grandma, student, Worker.", this);
            return false;
        }
        if (playing) { Debug.LogError("PassengerAttackController: An attack is already playing. Check IsPlaying before calling again.", this); return false; }
        if (!victim || !attacker) { Debug.LogError("PassengerAttackController: Renderer reference missing.", this); return false; }
        if (!impactEffect || impactFrames == null) { Debug.LogError("PassengerAttackController: Assign AttackStar renderer and sheet.", this); return false; }
        if (!objection || !selected.objectionTexture) { Debug.LogError("PassengerAttackController: Assign Objection panel and texture for " + selected.key, this); return false; }
        var camera = exitCamera ? exitCamera : Camera.main;
        if (!camera) { Debug.LogError("PassengerAttackController: Assign Exit Camera or tag the camera MainCamera.", this); return false; }
        ResetVisuals();
        // Use the highest existing sorting layer without changing project settings.
        var top = SortingLayer.layers[0];
        foreach (var layer in SortingLayer.layers) if (layer.value > top.value) top = layer;
        victim.sortingLayerID = attacker.sortingLayerID = impactEffect.sortingLayerID = top.id;
        victim.sortingOrder = 32760; attacker.sortingOrder = 32761; impactEffect.sortingOrder = 32762;
        attacker.sprite = selected.sprite;
        float scale = Mathf.Max(0.1f, selected.height) / selected.sprite.bounds.size.y;
        attacker.transform.localScale = Vector3.one * scale;
        attacker.transform.localRotation = Quaternion.identity;
        var bounds = selected.sprite.bounds;
        Vector3 foot = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, selected.kickPoint.x), Mathf.Lerp(bounds.min.y, bounds.max.y, selected.kickPoint.y), 0);
        attackerImpact = (Vector3)impactPoint - foot * scale;
        attackerImpact.z = victimRest.z - 0.01f;
        attacker.transform.localPosition = attackerImpact;
        var worldImpact = attacker.transform.position;
        var depth = camera.WorldToViewportPoint(worldImpact).z;
        var edge = camera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth));
        var extents = attacker.bounds.extents;
        var right = camera.transform.right;
        float halfWidth = Mathf.Abs(right.x) * extents.x + Mathf.Abs(right.y) * extents.y + Mathf.Abs(right.z) * extents.z;
        float distance = Mathf.Max(0, Vector3.Dot(edge - attacker.bounds.center, right)) + halfWidth + Mathf.Max(0, exitMargin);
        exitOffset = transform.InverseTransformVector(right * distance);
        exitOffset += new Vector3(Mathf.Max(0, retreatOffset.x), retreatOffset.y, 0);
        elapsed = 0;
        playing = true;
        preparing = true;
        victim.enabled = attacker.enabled = false;
        if (!objection.Play(selected.objectionTexture, StartCombat, CancelPreparation,
            ()=>{if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.YeYeItso,0.8f);}))
        {
            CancelPreparation();
            return false;
        }
        return true;
    }

    void StartCombat()
    {
        if (!isActiveAndEnabled || !playing) return;
        preparing = false;
        if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.HecktopascalKick,0.8f);
        elapsed = 0;
        victim.enabled = attacker.enabled = true;
        RenderPose();
    }

    void CancelPreparation() { preparing = playing = false; ResetVisuals(); }

    void Update()
    {
        if (!playing || preparing) return;
        if (!victim || !attacker) { Debug.LogError("PassengerAttackController: A renderer was removed during playback.", this); playing = false; return; }
        elapsed += Time.deltaTime;
        RenderPose();
    }

    void RenderPose()
    {
        float flight = Mathf.Max(0.01f, flightDuration);
        float recovery = Mathf.Max(0.01f, recoveryDuration);
        float hold = Mathf.Max(0, impactHold);
        if (elapsed >= flight && !hitTriggered)
        {
            hitTriggered = true;
            if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.HecktopascalKicked,0.8f);
            if (cameraShake) cameraShake.PlayShake();
        }
        RenderImpact(elapsed - flight);
        if (elapsed < flight)
        {
            float t = elapsed / flight;
            attacker.transform.localPosition = attackerImpact + (Vector3)approachOffset * (1-t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * arcHeight);
            return;
        }
        float afterHit = elapsed - flight;
        float victimFadeTime = Mathf.Max(0.01f, victimFadeDuration);
        float linger = Mathf.Max(0, victimLinger);
        if (afterHit >= hold + recovery + linger + victimFadeTime) { playing = false; ResetVisuals(); return; }
        float tRecovery = Mathf.Clamp01((afterHit - hold) / recovery);
        float recoil = 1 - Mathf.SmoothStep(0, 1, tRecovery);
        victim.transform.localPosition = victimRest + Vector3.left * (knockback * recoil);
        victim.transform.localRotation = victimRotation * Quaternion.Euler(0,0,recoilAngle * recoil);
        victim.color = Color.Lerp(victimColor, new Color(1,0.5f,0.5f,victimColor.a), recoil * 0.5f);
        if (hideVictimWhenIdle)
        {
            var victimTint = victim.color;
            victimTint.a *= 1 - Mathf.Clamp01((afterHit - hold - recovery - linger) / victimFadeTime);
            victim.color = victimTint;
        }
        // Ballistic arc: rise immediately after impact, then fall away to the retreat position.
        float bounce = 4 * Mathf.Max(0, bounceHeight) * tRecovery * (1 - tRecovery);
        attacker.transform.localPosition = attackerImpact + exitOffset * tRecovery + Vector3.up * bounce;
        attacker.color = attackerColor;
    }

    void ResetVisuals()
    {
        hitTriggered = false;
        if (cameraShake) cameraShake.StopShake();
        if (impactEffect) impactEffect.enabled = false;
        if (victim) { victim.transform.localPosition = victimRest; victim.transform.localRotation = victimRotation; victim.color = victimColor; victim.enabled = !hideVictimWhenIdle; }
        if (attacker) { attacker.enabled = false; attacker.color = attackerColor; }
    }
    void RenderImpact(float timeSinceHit)
    {
        if (!impactEffect || impactFrames == null) return;
        int frame = Mathf.FloorToInt(timeSinceHit * Mathf.Max(0.1f, impactFramesPerSecond));
        impactEffect.enabled = timeSinceHit >= 0 && frame < impactFrames.Length;
        if (!impactEffect.enabled) return;
        impactEffect.sprite = impactFrames[frame];
        impactEffect.transform.localPosition = new Vector3(impactPoint.x + impactEffectOffset.x, impactPoint.y + impactEffectOffset.y, victimRest.z - 0.02f);
        impactEffect.transform.localScale = Vector3.one * Mathf.Max(0.1f, impactSize);
        impactEffect.flipX = true;
    }
    void OnDestroy()
    {
        if (impactFrames != null) foreach (var sprite in impactFrames) if (sprite) Destroy(sprite);
    }
    void OnDisable() { if (instance != this) return; playing = preparing = false; if (objection) objection.Cancel(); ResetVisuals(); instance = null; }

    [ContextMenu("Test Attack/Women")] void TestWomen() { if (Application.isPlaying) PlayAttack("Women"); }
    [ContextMenu("Test Attack/Grandma")] void TestGrandma() { if (Application.isPlaying) PlayAttack("Grandma"); }
    [ContextMenu("Test Attack/student")] void TestStudent() { if (Application.isPlaying) PlayAttack("student"); }
    [ContextMenu("Test Attack/Worker")] void TestWorker() { if (Application.isPlaying) PlayAttack("Worker"); }
}
