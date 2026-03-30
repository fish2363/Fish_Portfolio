using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChillBreadTle : DragObj
{
    private System.Type[] dropOrder = new System.Type[] { typeof(Kettle), typeof(PipingBag), typeof(Kettle) };
    private int currentStage = 0;
    [SerializeField] private Sprite[] tle;
    [SerializeField] private Image _tleImage;
    [SerializeField] private GameObject kettlePrefabs;
    private bool isUnfinished;
    public Piping CurrentPipping { get; set; }
    public int putIdx;

    public override void OnBeginDrag(PointerEventData eventData)
    {
    }

    public override void OnDrag(PointerEventData eventData)
    {
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
    }

    protected override void Awake()
    {
        base.Awake();
        AcceptDrop = true;
    }
    public void ResetPlate()
    {
        StageTrigger.Instance.BreadPut(putIdx,MiniBreadState.None);
        _tleImage.DOFade(0f, 0.1f);
        currentStage = 0;
        isUnfinished = false;
        _tleImage.sprite = tle[0];
    }
    public override void Execute(DragObj obj)
    {
        // 현재 단계와 타입 비교
        if (currentStage < dropOrder.Length && obj.GetType() == dropOrder[currentStage])
        {
            if (obj.TryGetComponent(out PipingBag bag))
            {
                CurrentPipping = bag._currentDeco;
            }
            _tleImage.DOFade(1, 1f);
            if (currentStage == 0)
            {
                Instantiate(kettlePrefabs,transform);
                StageTrigger.Instance.BreadPut(putIdx,MiniBreadState.Dough);
                isUnfinished = true;
            }
            if(currentStage == 1)
            {
                if(CurrentPipping == Piping.Cream)
                {
                    _tleImage.sprite = tle[1];
                    StageTrigger.Instance.BreadPut(putIdx, MiniBreadState.Cream);
                }
                else
                {
                    _tleImage.sprite = tle[3];
                    StageTrigger.Instance.BreadPut(putIdx, MiniBreadState.Pot);
                }
            }
            else
            {
                _tleImage.sprite = tle[currentStage];
                StageTrigger.Instance.BreadPut(putIdx, MiniBreadState.Dough);
            }
            Debug.Log($"Drop success: {obj.name} at stage {currentStage}");
            currentStage++;

            // 실제 Drop 처리 (예: 붙이기, 점수 증가 등)
            base.Execute(obj);
            obj.ResetPosition();

            // 모든 단계 완료 체크
            if (currentStage >= dropOrder.Length)
            {
                OnAlreadyBaking();
            }
        }
        else
        {
            Debug.Log($"Drop failed: {obj.name} at stage {currentStage}, wrong order");
            InGameUIManager.Instance.ShowFloatingText("잘못된 순서입니다.", Color.red);
            // 순서 틀리면 원위치 처리
            obj.ResetPosition();
        }
    }
    public bool MyCurrentBakeTle()
    {
        return isUnfinished;
    }
    private void OnAlreadyBaking()
    {
        Debug.Log("All stages completed!");
        isUnfinished = false;
        FindAnyObjectByType<UIPopupChillBread>().EnableButton();
        FindAnyObjectByType<UIPopupChillBread>().AddChillBread(this);
    }
}
