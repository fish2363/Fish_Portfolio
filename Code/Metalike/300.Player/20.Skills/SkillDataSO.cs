using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    public string displayName;
    public string displayDescription;
    public float cooldown;

    public Sprite skillIcon;
}