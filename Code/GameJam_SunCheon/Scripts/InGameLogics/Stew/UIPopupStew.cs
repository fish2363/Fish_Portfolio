using UnityEngine;


public class UIPopupStew : UIBase 
{
    StewManager stewManager;
    [SerializeField] StewController[] stewControllers;

    public override void Opened(object[] param)
    {
        stewManager = (StewManager)param[0];
        stewManager.SetFireplace(stewControllers);

        stewControllers[0].CurState = stewControllers[0].CurState;
        stewControllers[0].SetManager(stewManager);
        stewControllers[1].CurState = stewControllers[1].CurState;
        stewControllers[1].SetManager(stewManager);
    }
}
