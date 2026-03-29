using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using System.Collections.Generic;

public class ESCManager : MonoBehaviour
{
    private bool isOn;
    [SerializeField] CanvasGroup escCanvas;

    [SerializeField] private BroAudioType _bgm;
    [SerializeField] private BroAudioType _sfx;
    [SerializeField] private BroAudioType _main;

    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;


    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private List<Resolution> resolutions=new();
    private bool _isInitialized;
    public static ESCManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetupResolutionDropdown();
        SetupFullscreenToggle(fullscreenToggle.isOn);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionSelected);
        fullscreenToggle.onValueChanged.AddListener(SetupFullscreenToggle);
    }
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            ESC();
    }

    void SetupResolutionDropdown()
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution r = Screen.resolutions[i];
            float aspect = (float)r.width / r.height;
            float refreshRate = r.refreshRateRatio.numerator / (float)r.refreshRateRatio.denominator;

            if (Mathf.Abs(aspect - (16f / 9f)) < 0.1f && r.width >= 1280)
            {
                resolutions.Add(r);
            }
        }
        resolutionDropdown.ClearOptions();
        resolutions.Reverse();

        var options = resolutions.Select(r => $"{r.width} x {r.height}").ToList();
        resolutionDropdown.AddOptions(options);

        _isInitialized = true;
    }

    void SetupFullscreenToggle(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    void OnResolutionSelected(int index)
    {
        if (!_isInitialized) return;

        bool isFullscreen = fullscreenToggle.isOn;
        Resolution res = resolutions[index];

        Screen.SetResolution(res.width, res.height, isFullscreen);
    }

    

    public void ESC()
    {
        isOn = !isOn;
        float force = isOn ? 1 : 0;
        escCanvas.DOFade(force, 0.2f);
        escCanvas.blocksRaycasts = isOn;
        escCanvas.interactable = isOn;
    }


    public void BGM(float volume)
    {
        _bgmSlider.value = volume;
        BroAudio.SetVolume(_bgm, volume);
    }

    public void SFX(float volume)
    {
        _sfxSlider.value = volume;
        BroAudio.SetVolume(_sfx, volume);
    }

    public void Master(float volume)
    {
        _masterSlider.value = volume;
        BroAudio.SetVolume(_main, volume);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터 모드에서 실행 중이면 Play 모드를 끔
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
