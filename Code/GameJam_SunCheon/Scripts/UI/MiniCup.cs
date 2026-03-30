using UnityEngine;
using UnityEngine.UI;

public class MiniCup : MonoBehaviour
{
    private Image visual;
    [SerializeField, Header("빈,채우는,채워진,넘친,완료")]
    private Sprite[] visuals;

    private void Awake()
    {
        visual = GetComponent<Image>();
    }

    public void SetVisual(JuiceState miniBread)
    {
        switch (miniBread)
        {
            case JuiceState.Set:
                visual.enabled = true;
                visual.sprite = visuals[0];
                break;
            case JuiceState.Filling:
                visual.enabled = true;

                visual.sprite = visuals[1];

                break;

            case JuiceState.End:
                visual.enabled = true;

                visual.sprite = visuals[2];

                break;

            case JuiceState.Over:
                visual.enabled = true;

                visual.sprite = visuals[3];
                break;
            case JuiceState.Pilled:
                visual.enabled = true;

                visual.sprite = visuals[4];
                break;
            case JuiceState.None:
                visual.enabled = false;
                break;
            default:

                break;
        }
    }
}
