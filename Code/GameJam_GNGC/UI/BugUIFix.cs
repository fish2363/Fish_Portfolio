using UnityEngine;
using UnityEngine.UI;

public class BugUIFix : MonoBehaviour
{
    private Image renderer;
    [SerializeField]
    public Image fixLongSprite;
    [SerializeField]
    public Image fixEarthSprite;

    private void Awake()
    {
        renderer = GetComponent<Image>();
    }

    public void FixLong()
    {
        renderer.enabled = false;
        fixLongSprite.gameObject.SetActive(true);
    }

    public void FixEarth()
    {
        renderer.enabled = false;
        fixEarthSprite.gameObject.SetActive(true);
    }
}
