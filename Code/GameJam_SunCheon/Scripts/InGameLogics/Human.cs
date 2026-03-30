using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Human : DragObj
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _text; // 손님 대사
    [SerializeField] private RectTransform _dialogueBox; // 손님 대사
    [SerializeField] private Image _image;          // 단계별 이미지 표시

    private HumanSO _myType;
    [SerializeField]private WantMenu _wantMenu;
    private InGameManager _gameManager;

    private float _limitTime;
    private float _elapsedTime = 0f;
    private bool _isStarted;

    private int _currentStage = 0;
    private bool isGone;

    private RectTransform _rect;

    protected override void Awake()
    {
        AcceptDrop = true;
    }

    // 초기화
    public void Initialize(HumanSO info, InGameManager gameManager)
    {
        _myType = info;
        _gameManager = gameManager;

        if (_myType.visual.Length > 0)
            _image.sprite = _myType.visual[0];

        _wantMenu = info.GetRandomDialogue();
        StartCoroutine(DialogueDialogue());
        _limitTime = info.waitTime;

        _rect = GetComponent<RectTransform>();

        _isStarted = true;
        _elapsedTime = 0f;
        _currentStage = 0;
    }
    public IEnumerator DialogueDialogue()
    {
        yield return new WaitForSeconds(0.4f);
        _dialogueBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        // DOTween으로 0 -> 56.7f 애니메이션 (0.5초)
        DOTween.To(
            () => _dialogueBox.rect.height,
            h => _dialogueBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h),
            56.7f,
            0.2f
        ).SetEase(Ease.InOutCubic);

        // 텍스트 설정
        _text.text = _wantMenu.dialogue;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
    }

    public override void OnDrag(PointerEventData eventData)
    {
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);
        if(obj.TryGetComponent(out JangBan food))
        {
            OnServing(food.CurrentFood);
            Destroy(obj.gameObject);
        }
    }

    // 손님에게 음식을 제공했을 때
    public void OnServing(WantFoodEnum getFood)
    {
        bool isHappy = _wantMenu.foodEnum == getFood;
        Satisfied(isHappy);
    }

    // 만족 여부 처리
    private void Satisfied(bool isHappy)
    {
        if (isHappy)
        {
            if(_currentStage == 0)
                GameManager.Instance.CurScore+=2;
            else
                GameManager.Instance.CurScore++;
            Debug.Log("점수 추가");
        }
        else
        {
            GameManager.Instance.CurScore--;
            Debug.Log("점수 깎임");
        }

        GetOut();
    }

    private void Update()
    {
        if (!_isStarted) return;

        _elapsedTime += Time.deltaTime;

        int stage = GetStage(_elapsedTime, _limitTime);

        if (stage != _currentStage)
        {
            _currentStage = stage;
            if (_myType.visual.Length > _currentStage)
                _image.sprite = _myType.visual[_currentStage];

            Debug.Log($"단계 변경: {_currentStage}, 경과 {_elapsedTime:F1}s");
        }

        // 제한 시간 초과 시 불만족 처리
        if (_elapsedTime >= _limitTime && !isGone)
        {
            isGone = true;
            Satisfied(false);
        }
    }

    // UI 이동 (퇴장)
    private void GetOut()
    {
        _gameManager.RemoveHuman(this);

        if (_rect != null)
        {
            // anchoredPosition 기준으로 화면 밖으로 이동
            _rect.DOAnchorPosX(-2500f, 3f).SetEase(Ease.OutCubic)
                 .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // elapsedTime → 단계 반환 (0~2)
    private int GetStage(float value, float max)
    {
        if (value <= max / 3f)
            return 0;
        else if (value <= (max * 2f / 3f))
            return 1;
        else
            return 2;
    }

}
