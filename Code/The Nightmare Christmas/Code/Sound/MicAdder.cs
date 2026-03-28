using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Dark;

public class MicAdder : MonoBehaviour
{
    [SerializeField] private HorizontalSelector micSelector;
    [SerializeField] private AudioInput targetAudioInput;

    private List<string> currentDevices = new List<string>();
    private string selectedDevice = "";
    WaitForSeconds waitTime = new(2f);

    private void Start()
    {
        micSelector.onValueChanged.AddListener(OnMicSelected);

        UpdateMicDevices();
        StartCoroutine(DeviceCheckRoutine());
    }

    private IEnumerator DeviceCheckRoutine()
    {
        while (true)
        {
            if (DevicesChanged())
            {
                UpdateMicDevices();
            }
            yield return waitTime;
        }
    }

    private bool DevicesChanged()
    {
        string[] devices = Microphone.devices;

        if (devices.Length != currentDevices.Count) return true;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i] != currentDevices[i]) return true;
        }

        return false;
    }

    public void UpdateMicDevices()
    {
        string[] devices = Microphone.devices;

        currentDevices.Clear();
        currentDevices.AddRange(devices);
        micSelector.itemList.Clear();

        foreach (string device in devices)
        {
            micSelector.itemList.Add(new HorizontalSelector.Item { itemTitle = device });
        }

        micSelector.SetupSelector();

        if (devices.Length > 0)
        {
            selectedDevice = devices[micSelector.index];
            ApplySelectedDevice(selectedDevice);
        }
    }

    private void OnMicSelected(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < currentDevices.Count)
        {
            selectedDevice = currentDevices[selectedIndex];
            ApplySelectedDevice(selectedDevice);
        }
    }

    private void ApplySelectedDevice(string deviceName)
    {
        if (targetAudioInput != null)
        {
            targetAudioInput.ChangeMicrophone(deviceName);
        }
        else
        {
            Debug.LogWarning("연결된 AudioInput 컴포넌트가 없습니다!");
        }
    }
}