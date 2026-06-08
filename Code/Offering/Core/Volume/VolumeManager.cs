using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public interface IVolumeService : IGameService
{
    void SetVignette(Color color, float intensity);
}

    public class VolumeManager : MonoBehaviour, IVolumeService
    {
        [Header("URP Volume")]
        public Volume volume;

        private Vignette vignette;
        private ChromaticAberration chromaticAberration;
        private LensDistortion lensDistortion;
        private Coroutine vignetteRoutine;

        private float baseIntensity;
        private float baseChromaticIntensity;
        private float baseLensIntensity;
        private float baseLensXMultiplier;
        private float baseLensYMultiplier;
        private Vector2 baseLensCenter;
        private float baseLensScale;
        private bool cached;
        private int ppOrder;

        private readonly Dictionary<string, PPDistortionLayer> distortionLayers = new();

        public bool IsInitialized { get; private set; }

        public UniTask InitAsync()
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            SubscribeEvents();
            Cache();

            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void Release()
        {
            StopVignetteRoutine();
            UnsubscribeEvents();
            distortionLayers.Clear();
            ppOrder = 0;
            cached = false;
            IsInitialized = false;
        }

        private void OnDestroy()
        {
            StopVignetteRoutine();
            UnsubscribeEvents();   // Release 미호출 대비 (중복 해제는 무해)
        }

        private void SubscribeEvents()
        {
            Bus<VinetteBlinkInEvent>.OnEvent += StartVignetteBlink;
            Bus<VinetteResetEvent>.OnEvent += ResetVignette;
            Bus<VignettePulseEvent>.OnEvent += PlayVignettePulse;
            Bus<PPDistortionApplyEvent>.OnEvent += ApplyPPDistortion;
            Bus<PPDistortionCancelEvent>.OnEvent += CancelPPDistortion;
        }

        private void UnsubscribeEvents()
        {
            Bus<VinetteBlinkInEvent>.OnEvent -= StartVignetteBlink;
            Bus<VinetteResetEvent>.OnEvent -= ResetVignette;
            Bus<VignettePulseEvent>.OnEvent -= PlayVignettePulse;
            Bus<PPDistortionApplyEvent>.OnEvent -= ApplyPPDistortion;
            Bus<PPDistortionCancelEvent>.OnEvent -= CancelPPDistortion;
        }

        private void Cache()
        {
            cached = false;
            vignette = null;
            chromaticAberration = null;
            lensDistortion = null;

            if (volume == null || volume.profile == null) return;
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);

            if (vignette == null && chromaticAberration == null && lensDistortion == null) return;

            if (vignette != null)
            {
                baseIntensity = vignette.intensity.value;
                vignette.intensity.overrideState = true;
                vignette.color.overrideState = true;
            }

            if (chromaticAberration != null)
            {
                baseChromaticIntensity = chromaticAberration.intensity.value;
                chromaticAberration.intensity.overrideState = true;
            }

            if (lensDistortion != null)
            {
                baseLensIntensity = lensDistortion.intensity.value;
                baseLensXMultiplier = lensDistortion.xMultiplier.value;
                baseLensYMultiplier = lensDistortion.yMultiplier.value;
                baseLensCenter = lensDistortion.center.value;
                baseLensScale = lensDistortion.scale.value;

                lensDistortion.intensity.overrideState = true;
                lensDistortion.xMultiplier.overrideState = true;
                lensDistortion.yMultiplier.overrideState = true;
                lensDistortion.center.overrideState = true;
                lensDistortion.scale.overrideState = true;
            }

            cached = true;
        }

        public void SetVignette(Color color, float intensity)
        {
            if (!cached) Cache();
            if (vignette == null) return;

            vignette.color.value = color;
            vignette.intensity.value = Mathf.Clamp01(intensity);
        }

        private void ResetVignette(VinetteResetEvent evt)
        {
            if (this == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (!cached) Cache();
            if (vignette == null) return;

            StopVignetteRoutine();
            vignette.intensity.value = baseIntensity;
        }

        private void StartVignetteBlink(VinetteBlinkInEvent evt)
        {
            if (this == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (!cached) Cache();
            if (vignette == null) return;

            StopVignetteRoutine();

            vignette.color.value = evt.warningColor;
            vignetteRoutine = StartCoroutine(VignetteBlinkRoutine(
                Mathf.Clamp01(evt.minIntensity),
                Mathf.Clamp01(evt.maxIntensity),
                Mathf.Max(0.01f, evt.blinkSpeed)
            ));
        }

        private void PlayVignettePulse(VignettePulseEvent evt)
        {
            if (this == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (!cached) Cache();
            if (vignette == null) return;

            StopVignetteRoutine();

            float peak = Mathf.Clamp01(evt.peakIntensity);
            float fadeIn = Mathf.Max(0f, evt.fadeIn);
            float hold = Mathf.Max(0f, evt.hold);
            float fadeOut = Mathf.Max(0f, evt.fadeOut);

            vignette.color.value = evt.color;
            vignetteRoutine = StartCoroutine(VignettePulseRoutine(peak, fadeIn, hold, fadeOut));
        }

        private void ApplyPPDistortion(PPDistortionApplyEvent evt)
        {
            if (this == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (!cached) Cache();
            if (chromaticAberration == null && lensDistortion == null) return;
            if (string.IsNullOrEmpty(evt.key)) return;

            float chromaticIntensity = evt.chromaticIntensity;
            float lensIntensity = evt.lensIntensity;
            float lensXMultiplier = evt.lensXMultiplier;
            float lensYMultiplier = evt.lensYMultiplier;
            Vector2 lensCenter = evt.lensCenter;
            float lensScale = evt.lensScale;

            if (chromaticIntensity == 0f && lensIntensity == 0f && lensXMultiplier == 0f &&
                lensYMultiplier == 0f && lensCenter == Vector2.zero && lensScale == 0f)
            {
                chromaticIntensity = 0.5f;
                lensIntensity = -0.256f;
                lensXMultiplier = 1f;
                lensYMultiplier = 1f;
                lensCenter = new Vector2(0.5f, 0.5f);
                lensScale = 1f;
            }

            distortionLayers[evt.key] = new PPDistortionLayer
            {
                priority = evt.priority,
                order = ++ppOrder,
                chromaticIntensity = Mathf.Clamp01(chromaticIntensity),
                lensIntensity = Mathf.Clamp(lensIntensity, -1f, 1f),
                lensXMultiplier = lensXMultiplier,
                lensYMultiplier = lensYMultiplier,
                lensCenter = lensCenter,
                lensScale = lensScale
            };

            RefreshPPDistortion();
        }

        private void CancelPPDistortion(PPDistortionCancelEvent evt)
        {
            if (this == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (string.IsNullOrEmpty(evt.key)) return;
            if (!distortionLayers.Remove(evt.key)) return;

            RefreshPPDistortion();
        }

        private void StopVignetteRoutine()
        {
            if (vignetteRoutine != null)
            {
                StopCoroutine(vignetteRoutine);
                vignetteRoutine = null;
            }
        }

        private IEnumerator VignetteBlinkRoutine(float minIntensity, float maxIntensity, float blinkSpeed)
        {
            while (true)
            {
                float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
                vignette.intensity.value = Mathf.Lerp(minIntensity, maxIntensity, t);
                yield return null;
            }
        }

        private IEnumerator VignettePulseRoutine(float peakIntensity, float fadeIn, float hold, float fadeOut)
        {
            float start = baseIntensity;
            float t = 0f;

            if (fadeIn > 0f)
            {
                while (t < fadeIn)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / fadeIn);
                    vignette.intensity.value = Mathf.Lerp(start, peakIntensity, k);
                    yield return null;
                }
            }
            vignette.intensity.value = peakIntensity;

            if (hold > 0f)
                yield return new WaitForSecondsRealtime(hold);

            t = 0f;
            if (fadeOut > 0f)
            {
                while (t < fadeOut)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / fadeOut);
                    vignette.intensity.value = Mathf.Lerp(peakIntensity, baseIntensity, k);
                    yield return null;
                }
            }

            vignette.intensity.value = baseIntensity;
            vignetteRoutine = null;
        }

        private void RefreshPPDistortion()
        {
            if (!cached) Cache();

            if (distortionLayers.Count == 0)
            {
                ResetPPDistortion();
                return;
            }

            PPDistortionLayer topLayer = default;
            bool hasLayer = false;

            foreach (PPDistortionLayer layer in distortionLayers.Values)
            {
                if (!hasLayer || layer.priority > topLayer.priority ||
                    (layer.priority == topLayer.priority && layer.order > topLayer.order))
                {
                    topLayer = layer;
                    hasLayer = true;
                }
            }

            if (!hasLayer)
            {
                ResetPPDistortion();
                return;
            }

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = topLayer.chromaticIntensity;

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = topLayer.lensIntensity;
                lensDistortion.xMultiplier.value = topLayer.lensXMultiplier;
                lensDistortion.yMultiplier.value = topLayer.lensYMultiplier;
                lensDistortion.center.value = topLayer.lensCenter;
                lensDistortion.scale.value = topLayer.lensScale;
            }
        }

        private void ResetPPDistortion()
        {
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = baseChromaticIntensity;

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = baseLensIntensity;
                lensDistortion.xMultiplier.value = baseLensXMultiplier;
                lensDistortion.yMultiplier.value = baseLensYMultiplier;
                lensDistortion.center.value = baseLensCenter;
                lensDistortion.scale.value = baseLensScale;
            }
        }

        private struct PPDistortionLayer
        {
            public int priority;
            public int order;
            public float chromaticIntensity;
            public float lensIntensity;
            public float lensXMultiplier;
            public float lensYMultiplier;
            public Vector2 lensCenter;
            public float lensScale;
        }
    }
