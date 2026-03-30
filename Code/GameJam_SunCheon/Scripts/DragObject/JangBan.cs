using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JangBan : DragObj
{
    [SerializeField] private Sprite[] food;
    [SerializeField] private Image[] _foodInJangBan;
    private Dictionary<WantFoodEnum, Sprite> _foodDict;
    private bool _isComtainFood;
    public WantFoodEnum CurrentFood { get; private set; }
    private Image _myImage;
    private int _containIdx;
    [SerializeField] private int _maxContains = 3;
    protected override void OnEnable()
    {
        base.OnEnable();
        _myImage = GetComponent<Image>();
        _foodDict = new Dictionary<WantFoodEnum, Sprite>
        {
            { WantFoodEnum.Bread_Pot, food[0]},
            { WantFoodEnum.Juice, food[1]},
            { WantFoodEnum.Stew, food[2]},
            { WantFoodEnum.Bread_Cream, food[0]},
        };
        for (int i = 0; i < _foodInJangBan.Length; i++)
            _foodInJangBan[i].gameObject.SetActive(false);
        isJangBan = true;
    }


    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentFood == WantFoodEnum.None)
        {
            InGameUIManager.Instance.ShowFloatingText("음식을 놓아주세요.",Color.red);
            return;
        }
        base.OnBeginDrag(eventData);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (CurrentFood == WantFoodEnum.None) return;
        base.OnEndDrag(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        if (CurrentFood == WantFoodEnum.None) return;
        base.OnDrag(eventData);
    }
    public override void Execute(DragObj obj)
    {
        base.Execute(obj);

        bool needDestroy = false;

        var meta = obj.GetComponent<BreadMeta>();
        if (meta != null)
        {
            TrayManager.Instance
                      .GetTray(meta.originTray)
                      .Remove(meta.foodEnum);
            needDestroy = true;
        }

        if (obj.TryGetComponent(out Food food))
        {
            if (food.foodEnum == WantFoodEnum.Stew) StageTrigger.Instance.OnRemoveStew();
            _foodInJangBan[_containIdx].gameObject.SetActive(true);
            ChangeSprite(food.foodEnum);

            if (_containIdx >= _maxContains)
                _isComtainFood = true;
            else
                _containIdx++;

            CurrentFood |= food.foodEnum;
            needDestroy = true;
        }

        if (needDestroy)
        {
            // Destroy 를 한 번만, 프레임 끝에서 안전하게
            Destroy(obj.gameObject);
        }
    }
    public bool IsContain() => _isComtainFood; Vector3 prev;
    public void ChangeSprite(WantFoodEnum food)
    {
        prev = _foodInJangBan[_containIdx].rectTransform.localScale;

        if (WantFoodEnum.Juice != food)
        {
            _foodInJangBan[_containIdx].rectTransform.localScale  = prev;
            _foodInJangBan[_containIdx].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
        }
        else
        {
            _foodInJangBan[_containIdx].rectTransform.localScale /= 2;
            _foodInJangBan[_containIdx].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200f);
        }
        _foodInJangBan[_containIdx].sprite = _foodDict[food];
    }

    private void OnDestroy()
    {
        _containIdx = 0;
        var manager = GetComponentInParent<InGameManager>();
        if (manager != null)
        {
            manager.IsAlreadySpawn = false;
        }
    }
}
