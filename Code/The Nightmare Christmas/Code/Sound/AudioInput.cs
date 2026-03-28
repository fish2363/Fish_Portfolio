using System;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioInput : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup micMixerGroup;

    public Action<string> OnBigSound;
    public string entityName = "PlayerCharacter";

    private string currentDevice;

    private void Awake()
    {
        if (audioSource == null) return;
        audioSource.outputAudioMixerGroup = micMixerGroup;
        audioSource.loop = true; 
    }

    public void ChangeMicrophone(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName)) return;

        if (Microphone.IsRecording(currentDevice))
            Microphone.End(currentDevice);

        currentDevice = deviceName;

        audioSource.clip = Microphone.Start(currentDevice, true, 10, AudioSettings.outputSampleRate);

        audioSource.Play();
        Debug.Log($"마이크 활성화: {currentDevice}");
    }
}