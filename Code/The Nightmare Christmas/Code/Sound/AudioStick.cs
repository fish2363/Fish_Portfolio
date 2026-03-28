using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AudioStick : MonoBehaviour
{
    public int band;
    public float startScale;
    public float scaleMultiplier;
    public float threshold = 10f; 

    [SerializeField] private AudioInput audioInput;
    private RectTransform _rectTransform;
    private Image _stickImage;

    private bool isHighVolume = false;

    private void Awake()
    {
       _rectTransform = GetComponent<RectTransform>();
       _stickImage = GetComponent<Image>();
    }

    private void Update()
    {
        float currentFreq = (AudioSpectrum.FreqBand[band] * scaleMultiplier) + startScale;
        float clampedScale = Mathf.Clamp(currentFreq, 0, 30);

        _rectTransform.localScale = new Vector3(_rectTransform.localScale.x, clampedScale, _rectTransform.localScale.z);

        if (currentFreq > threshold)
        {
            if (!isHighVolume)
            {
                isHighVolume = true;

                audioInput.OnBigSound?.Invoke(audioInput.entityName);

                _stickImage.DOKill();
                _stickImage.DOColor(Color.red, 0.2f); 
            }
        }
        else
        {
            if (isHighVolume)
            {
                isHighVolume = false;

                _stickImage.DOKill();
                _stickImage.DOColor(Color.white, 0.5f);
            }
        }
    }
}