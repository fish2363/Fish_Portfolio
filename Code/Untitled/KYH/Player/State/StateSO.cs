using UnityEngine;

[CreateAssetMenu(fileName = "StateSO", menuName = "SO/FSM/State", order = 0)]
public class StateSO : ScriptableObject
{
    public string stateName;
    public string className;
    public AnimParamSO animParam;
}