// JuiceManager.cs
// Orquestador central de feedback visual: Camera Shake (trauma), Hit Stop y Post-Processing URP.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace Game.Juice
{
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        [Header("Camera Shake")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float shakeDecaySpeed = 1.5f;
        [SerializeField] private float maxAngle = 5f;
        [SerializeField] private float maxOffset = 0.3f;
        [SerializeField] private float shakeFrequency = 12f;

        [Header("Post Processing (opcional)")]
        [SerializeField] private Volume postProcessVolume;

        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;
        private LensDistortion _lensDistortion;

        private float _trauma;          // 0-1, decae con el tiempo
        private Vector3 _cameraOrigin;
        private float _seed;

        private Coroutine _hitStopCoroutine;
        private Coroutine _postFxCoroutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera != null) _cameraOrigin = mainCamera.transform.localPosition;
            _seed = Random.value * 100f;

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out _chromaticAberration);
                postProcessVolume.profile.TryGet(out _vignette);
                postProcessVolume.profile.TryGet(out _lensDistortion);
            }
        }

        void Update()
        {
            if (mainCamera != null && _trauma > 0f)
                ApplyCameraShake();
        }

        // ── Camera Shake (Trauma System) ────────────────────────────────────
        public void AddTrauma(float amount) => _trauma = Mathf.Clamp01(_trauma + amount);

        private void ApplyCameraShake()
        {
            float shake = _trauma * _trauma; // cuadrático = más control en valores bajos
            float t = Time.unscaledTime * shakeFrequency;

            float offsetX = maxOffset * shake * (Mathf.PerlinNoise(_seed, t) * 2f - 1f);
            float offsetY = maxOffset * shake * (Mathf.PerlinNoise(_seed + 1f, t) * 2f - 1f);
            float rotZ    = maxAngle  * shake * (Mathf.PerlinNoise(_seed + 2f, t) * 2f - 1f);

            mainCamera.transform.localPosition = _cameraOrigin + new Vector3(offsetX, offsetY, 0f);
            mainCamera.transform.localEulerAngles = new Vector3(0f, 0f, rotZ);

            _trauma = Mathf.Max(0f, _trauma - shakeDecaySpeed * Time.unscaledDeltaTime);
            if (_trauma <= 0f)
            {
                mainCamera.transform.localPosition = _cameraOrigin;
                mainCamera.transform.localEulerAngles = Vector3.zero;
            }
        }

        // ── Hit Stop ────────────────────────────────────────────────────────
        public void TriggerHitStop(float duration = 0.08f)
        {
            if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
            _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);

            float elapsed = 0f, resumeDuration = 0.05f;
            while (elapsed < resumeDuration)
            {
                Time.timeScale = Mathf.Lerp(0f, 1f, elapsed / resumeDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = 1f;
        }

        // ── Post Processing Flash ───────────────────────────────────────────
        public void TriggerImpactPostFX(float duration = 0.3f)
        {
            if (_chromaticAberration == null && _vignette == null && _lensDistortion == null) return;
            if (_postFxCoroutine != null) StopCoroutine(_postFxCoroutine);
            _postFxCoroutine = StartCoroutine(ImpactPostFXRoutine(duration));
        }

        private IEnumerator ImpactPostFXRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float curve = Mathf.Pow(1f - t, 2f); // pico inmediato, caída suave

                if (_chromaticAberration != null) _chromaticAberration.intensity.value = curve;
                if (_vignette != null)            _vignette.intensity.value = 0.3f + curve * 0.4f;
                if (_lensDistortion != null)      _lensDistortion.intensity.value = curve * -0.3f;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (_chromaticAberration != null) _chromaticAberration.intensity.value = 0f;
            if (_vignette != null)            _vignette.intensity.value = 0.3f;
            if (_lensDistortion != null)      _lensDistortion.intensity.value = 0f;
        }

        // ── API de alto nivel ───────────────────────────────────────────────
        public void TriggerHeavyImpact(float traumaAmount = 0.6f)
        {
            AddTrauma(traumaAmount);
            TriggerHitStop(0.08f);
            TriggerImpactPostFX(0.35f);
        }
    }
}
