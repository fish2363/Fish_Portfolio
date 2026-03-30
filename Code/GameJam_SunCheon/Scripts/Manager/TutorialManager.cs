using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;

[Serializable]
public class TutorialStep
{
    [Header("이 단계에서 이동할 위치")]
    public RectTransform pos;

    [Header("이 단계에서 눌러야 할 버튼")]
    public Button targetButton;

    [Header("특정 메서드가 호출될 때까지 대기 여부")]
    public bool waitForMethod;
}

public class TutorialManager : MonoBehaviour
{
    [Header("튜토리얼 단계 리스트")]
    [SerializeField] private TutorialStep[] tutorialSteps;

    [Header("가이드 포인터 (따라다닐 오브젝트)")]
    [SerializeField] private RectTransform guidePointer;

    [Header("이동 관련 옵션")]
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    [SerializeField] private InGameManager _gameManager;
    private int _currentIndex = 0;
    private bool _isLocked = false;

    private void Start()
    {
        if (tutorialSteps == null || tutorialSteps.Length == 0 || guidePointer == null)
        {
            Debug.LogWarning("튜토리얼 세팅이 안 되어 있음!");
            return;
        }

        MoveToCurrent();
    }

    private void MoveToCurrent()
    {
        if (_currentIndex >= tutorialSteps.Length)
        {
            // 모든 목표를 다 돌면 자기 자신 제거
            Destroy(gameObject);
            return;
        }

        TutorialStep step = tutorialSteps[_currentIndex];

        if (step.pos == null || step.targetButton == null)
        {
            Debug.LogWarning($"{_currentIndex}번 튜토리얼 스텝이 비어있음!");
            _currentIndex++;
            MoveToCurrent(); // 다음으로 스킵
            return;
        }
        // 기존 리스너 제거 후 새로운 버튼에 리스너 등록
        step.targetButton.onClick.RemoveAllListeners();
        step.targetButton.onClick.AddListener(HandleButtonClick);
        step.pos.gameObject.SetActive(true);

        // 만약 메서드 대기를 해야 한다면 포인터 숨기고 잠금
        if (step.waitForMethod)
        {
            _isLocked = true;
            tutorialSteps[_currentIndex-1].pos.gameObject.SetActive(false);
            guidePointer.gameObject.SetActive(false);
            return;
        }
        else if(_currentIndex >= 1)
            tutorialSteps[_currentIndex - 1].pos.gameObject.SetActive(false);

        // 포인터 이동
        guidePointer.gameObject.SetActive(true);
        guidePointer.DOMove(step.pos.position, moveTime).SetEase(moveEase);
    }

    private void HandleButtonClick()
    {
        if (_isLocked) return; // 잠겨 있으면 무시

        _currentIndex++;
        MoveToCurrent();
    }

    public void SpawnHuman()
    {
        _gameManager.TutorialStartGame();
    }

    /// <summary>
    /// 외부에서 특정 메서드가 호출됐을 때 언락
    /// </summary>
    public void UnlockStep()
    {
        if (!_isLocked) return;

        _isLocked = false;

        // 포인터 다시 보이게 하고 이동
        TutorialStep step = tutorialSteps[_currentIndex];
        if (step.pos != null)
        {
            guidePointer.gameObject.SetActive(true);
            guidePointer.DOMove(step.pos.position, moveTime).SetEase(moveEase);
        }
    }
}
