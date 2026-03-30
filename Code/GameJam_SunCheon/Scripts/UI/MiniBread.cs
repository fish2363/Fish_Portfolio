using UnityEngine;
using UnityEngine.UI;

public enum MiniBreadState
{
    None,
    Dough,
    Burn,
    Pot,
    Cream
}

public class MiniBread : MonoBehaviour
{
    private Image visual;
    [SerializeField,Header("도우,탐,팥,크림")]
    private Sprite[] visuals;

    private void Awake()
    {
        visual = GetComponent<Image>();
    }
    
    public void SetVisual(MiniBreadState miniBread)
    {
        switch(miniBread)
        {
            case MiniBreadState.Dough:
                visual.enabled = true;
                visual.sprite = visuals[0];
                break;
            case MiniBreadState.Burn:
                visual.enabled = true;

                visual.sprite = visuals[1];

                break;

            case MiniBreadState.Pot:
                visual.enabled = true;

                visual.sprite = visuals[2];

                break;

            case MiniBreadState.Cream:
                visual.enabled = true;

                visual.sprite = visuals[3];
                break;
        }
    }
}
