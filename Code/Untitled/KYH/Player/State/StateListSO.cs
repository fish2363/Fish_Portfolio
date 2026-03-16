using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateList", menuName = "SO/FSM/StateList", order = 0)]
public class StateListSO : ScriptableObject
{
    public List<StateSO> states;
}
