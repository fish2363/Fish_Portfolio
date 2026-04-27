using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScrollFrame : MonoBehaviour
{
    [SerializeField] private Image seal;

    public void SetPatternImage(Sprite sprite)
    {
        GetComponent<Image>().sprite = sprite;
    }

    public void Stamp(Sprite sprite, bool correctAnswer)
    {
        if (correctAnswer == false) seal.DOColor(Color.red,0.1f);

        seal.transform.localScale = new Vector3(2,2);
        seal.sprite = sprite;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(seal.DOFade(1f, 0.2f))
            .Join(seal.transform.DOScale(1.1767f, 0.3f));
    }

    public void ClearStamp()
    {
        seal.DOColor(Color.white, 0.1f);
        seal.transform.localScale = new Vector3(2, 2);
        seal.DOFade(0f, 0.2f);
    }
}
