using System.Collections.Generic;
using UnityEngine;

public class SequenceUIController : MonoBehaviour
{
    [SerializeField] private GameEventChannelSO inGameEvent;
    private List<ESequenceCharacter> selectedOrder = new();


    public void Confirm()
    {
        inGameEvent.RaiseEvent(new RunSequenceEvent().Initialize(selectedOrder));
    }
}
