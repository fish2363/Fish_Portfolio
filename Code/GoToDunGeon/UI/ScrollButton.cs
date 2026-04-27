using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI scrollName;

    private ScrollUI ScrollUI;
    private ScrollSO currentScrollSO;
    private UIManager uIManager;

    public void Init(ScrollSO currentSO, ScrollUI scroll , UIManager uIManager)
    {
        currentScrollSO = currentSO;
        ScrollUI = scroll;
        this.uIManager = uIManager;
        GetComponent<Button>().onClick.AddListener(ClickButton);
    }

    public void ChangeScrollAppearance(string name, Sprite sprite)
    {
        scrollName.text = name;
        icon.sprite = sprite;
    }

    private void ClickButton()
    {
        ScrollUI.ShowScroll(currentScrollSO,uIManager.InGameManager.OnScrollSelected);
        SizeUp();
        uIManager.OnCancelUIClick += SizeDown;
        uIManager.OnUseUIClick += SizeDown;
    }

    public void CancelButton()
    {
        SizeDown();
    }

    #region 버튼 크기 관련
    private void SizeUp()
    {
        Debug.Log("Size커짐");
    }

    private void SizeDown()
    {
        Debug.Log("Size작아짐");
        uIManager.OnCancelUIClick -= SizeDown;
        uIManager.OnUseUIClick -= SizeDown;
    }
    #endregion
}
