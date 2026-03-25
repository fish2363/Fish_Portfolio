using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptureTutorialObject : MonoBehaviour
{
    [Flags]
    public enum TutorialEventEnum : short
    {
        Null = 0,
        Time = 1,
        Click = 2,
    }


    [System.Serializable]
    public struct TutorialContentStruct
    {
        [TextArea(2, 3)] public string text;
        public TutorialEventEnum nextOverType;
        public CanvasGroup contents;
        public RectTransform btnGoalTrm;
        public float delay;
    }

    public event Action OnEndEvent = null;
    [SerializeField] private bool _isEnable = false;
    [SerializeField] private bool _isChecking = false;
    [SerializeField] private bool _isTransition = false;

    [SerializeField] private TextMeshProUGUI _textDisplay;
    [SerializeField] private Button _selectGuideButton;
    [SerializeField] private Image _selectGuidePointer;
    private Vector3 _selectGuideStartPosition;

    [SerializeField] private TutorialContentStruct[] _tutorialContentList;


    [Header("Additional Setting")]
    [SerializeField] private bool _isWithFade = true;
    [SerializeField] private float _fadeTime = 0.5f;
    [Space(10)]
    [SerializeField] private bool _isWithBtnInterpolation = true;
    [SerializeField] private float _btnInterpolationTime = 0.5f;
    [SerializeField] private Ease _btnInterpolationEase = Ease.Linear;


    private int _currentContentIdx = 0;
    private TutorialContentStruct _previousContent;
    private TutorialContentStruct _currentContent;
    private float _lastTimeStamp;


    private void Awake()
    {
        _selectGuideStartPosition
           = _selectGuidePointer.transform.position;
        Initialize();
    }

    [ContextMenu("Initialize")]
    private void Initialize()
    {
        _selectGuideButton.onClick.AddListener(HandleSelectGuideButtonClick);

        _currentContentIdx = 0;

        _previousContent = new TutorialContentStruct();
        _currentContent = _tutorialContentList[0];

        EnableAction();
    }

    private void EnableAction()
    {
        _selectGuideButton.gameObject.SetActive(true);
        _selectGuidePointer.gameObject.SetActive(true);

        if (_isWithFade)
        {
            _currentContent.contents.DOFade(1, _fadeTime);

            DOVirtual.DelayedCall(_fadeTime, () =>
            {
                _isChecking = true;
                _isEnable = true;
                _lastTimeStamp = Time.time;
                _currentContent.contents.interactable = true;
                _currentContent.contents.blocksRaycasts = true;
            });
        }
        else
        {
            _currentContent.contents.alpha = 1;

            _isChecking = true;
            _isEnable = true;
            _lastTimeStamp = Time.time;
            _currentContent.contents.interactable = true;
            _currentContent.contents.blocksRaycasts = true;
        }

        _selectGuideButton.image.color = Color.white;
        _selectGuidePointer.transform.position
         = _selectGuideStartPosition;

        _textDisplay.text = _currentContent.text;
    }

    private void HandleSelectGuideButtonClick()
    {
        if ((int)(_currentContent.nextOverType & TutorialEventEnum.Click) > 0)
        {
            OnCheckSuccess();
        }
    }

    private void OnCheckSuccess()
    {
        _isChecking = false;
        _lastTimeStamp = Time.time;
    }

    private void Update()
    {
        if (!_isEnable) return;

        if (_isChecking)
        {
            CheckCondition();
        }
        else
        {
            if (_isTransition) return;
            TransitionToNextContent();
        }
    }

    private void CheckCondition()
    {
        if ((_currentContent.nextOverType & TutorialEventEnum.Time) == 0) return;
        if (_lastTimeStamp + _currentContent.delay < Time.time)
        {
            OnCheckSuccess();
        }
    }

    private void TransitionToNextContent()
    {

        while (_currentContentIdx + 1 != _tutorialContentList.Length
           && _tutorialContentList[_currentContentIdx + 1].nextOverType == TutorialEventEnum.Null)
        {
            ++_currentContentIdx;
        }

        if (_currentContentIdx + 1 == _tutorialContentList.Length)
        {
            _isEnable = false;
            DisableAction();
            //³¡³µÀ»¶§
            gameObject.transform.root.gameObject.SetActive(false);
            return;
        }

        _previousContent = _currentContent;
        _currentContent = _tutorialContentList[++_currentContentIdx];

        UpdateContent();


        if (_isWithFade)
        {
            _isTransition = true;
            DOVirtual.DelayedCall(_fadeTime, () =>
            {
                _isChecking = true;
                _isTransition = false;
            });
        }
        else
        {
            _isChecking = true;
        }
    }


    private void UpdateContent()
    {
        if (_previousContent.nextOverType != TutorialEventEnum.Null)
        {
            if (_isWithFade)
            {
                _previousContent.contents.DOFade(0, _fadeTime);
                _currentContent.contents.DOFade(1, _fadeTime);
            }
            else
            {
                _previousContent.contents.alpha = 0;
                _currentContent.contents.alpha = 1;
            }

            bool isExistsNextGoalPosition
               = _currentContent.btnGoalTrm;
            if (_isWithBtnInterpolation && isExistsNextGoalPosition)
            {
                _selectGuideButton.interactable = false;

                _selectGuidePointer.transform
               .DOMove(_currentContent.btnGoalTrm.position,
                   _btnInterpolationTime).SetEase(_btnInterpolationEase)
               .OnComplete(() => _selectGuideButton.interactable = true);
            }
            else
            {
                _selectGuidePointer.transform.position
               = _currentContent.btnGoalTrm.position;
            }


            _previousContent.contents.interactable = false;
            _previousContent.contents.blocksRaycasts = false;

            _currentContent.contents.interactable = true;
            _currentContent.contents.blocksRaycasts = true;

            _textDisplay.text = _currentContent.text;
        }
    }


    private void DisableAction()
    {
        OnEndEvent?.Invoke();
        _selectGuideButton.onClick.RemoveListener(HandleSelectGuideButtonClick);
        _selectGuideButton.gameObject.SetActive(false);
        _selectGuidePointer.gameObject.SetActive(false);

        if (_isWithFade)
        {
            _tutorialContentList[_tutorialContentList.Length - 1]
               .contents.DOFade(0, _fadeTime);
            _textDisplay.text = "";
        }
        else
        {
            _tutorialContentList[_tutorialContentList.Length - 1]
               .contents.alpha = 0;
            _textDisplay.text = "";
        }
    }




}
