using Assets.Work.CDH.Code.Weapons;
using Public.Core.Entity;
using System;
using UnityEngine;

public enum PlayerClass
{
    Surpport, // 지원
    Gunner, // 사격
    Guardian, // 보호
    Assault, //돌격
    Trickster, // 제어
}

[CreateAssetMenu(fileName = "CharacterSO", menuName = "CharacterSO")]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public string description;
    public bool isTilt;

    public PlayerClass myClass;

    public SkillDataSO defaultSkill;
    public AssistSkillSO assistSkill;

    public Sprite characterIcon;

    public StatContainer unitStat;

    public WeaponDataSO defaultWeapon;

    public GameObject visual;
    public GameObject armVisual;
}
