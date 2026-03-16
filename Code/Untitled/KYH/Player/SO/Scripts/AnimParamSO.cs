using UnityEngine;

[CreateAssetMenu(fileName = "ParamSO", menuName = "SO/Animator/Param",order = 0)]
public class AnimParamSO : ScriptableObject
{
    public string parameterName;
    public int hashValue;
    [TextArea]
    public string description;

    private void OnValidate()
    {
        hashValue = Animator.StringToHash(parameterName);
    }
}
