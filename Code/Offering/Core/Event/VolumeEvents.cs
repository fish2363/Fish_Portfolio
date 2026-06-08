using System.Collections.Generic;
using UnityEngine;

public struct VinetteBlinkInEvent : IEvent
{
    public Color warningColor;
    public float minIntensity;
    public float maxIntensity;
    public float blinkSpeed;

    public VinetteBlinkInEvent(Color warningColor,float minIntensity, float maxIntensity, float blinkSpeed)
    {
        this.warningColor = warningColor;
        this.minIntensity = minIntensity;
        this.maxIntensity = maxIntensity;
        this.blinkSpeed = blinkSpeed;
    }
}
public struct VignettePulseEvent : IEvent
{
    public Color color;
    public float peakIntensity;
    public float fadeIn;       
    public float hold;         
    public float fadeOut;

    public VignettePulseEvent(Color warningColor, float peakIntensity, float fadeIn, float hold, float fadeOut)
    {
        this.color = warningColor;
        this.peakIntensity = peakIntensity;
        this.fadeIn = fadeIn;
        this.hold = hold;
        this.fadeOut = fadeOut;
    }
}

public struct VinetteResetEvent : IEvent
{

}

public struct PPDistortionApplyEvent : IEvent
{
    public string key;
    public int priority;
    public float chromaticIntensity;
    public float lensIntensity;
    public float lensXMultiplier;
    public float lensYMultiplier;
    public Vector2 lensCenter;
    public float lensScale;

    public PPDistortionApplyEvent(string key, int priority)
    {
        this.key = key;
        this.priority = priority;
        chromaticIntensity = 0.5f;
        lensIntensity = -0.256f;
        lensXMultiplier = 1f;
        lensYMultiplier = 1f;
        lensCenter = new Vector2(0.5f, 0.5f);
        lensScale = 1f;
    }

    public PPDistortionApplyEvent(
        string key,
        int priority,
        float chromaticIntensity,
        float lensIntensity,
        float lensXMultiplier = 1f,
        float lensYMultiplier = 1f,
        float lensScale = 1f)
    {
        this.key = key;
        this.priority = priority;
        this.chromaticIntensity = chromaticIntensity;
        this.lensIntensity = lensIntensity;
        this.lensXMultiplier = lensXMultiplier;
        this.lensYMultiplier = lensYMultiplier;
        lensCenter = new Vector2(0.5f, 0.5f);
        this.lensScale = lensScale;
    }
}

public struct PPDistortionCancelEvent : IEvent
{
    public string key;

    public PPDistortionCancelEvent(string key)
    {
        this.key = key;
    }
}
