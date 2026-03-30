using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BreadType
{
    Cook,
    Burn
}

public class FinishBread : DragObj
{
    public Piping currentPiping;
    public BreadType breadType;
    [SerializeField] private Sprite[] _bread;

    public void Initialized(BreadType bread,Piping piping)
    {
        breadType = bread;
        isFood = breadType == BreadType.Cook;
        currentPiping = piping;
        if (breadType == BreadType.Burn)
            GetComponent<Image>().sprite = _bread[1];
        else
            GetComponent<Image>().sprite = _bread[0];
    }

}
