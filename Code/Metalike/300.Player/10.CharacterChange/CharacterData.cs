using Assets.Work.CDH.Code.Weapons;
using Public.Core.Entity;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string description;
    public bool isTilt;

    public SkillDataSO defaultSkill;

    public Sprite characterIcon;

    public StatContainer unitStat;

    public WeaponDataSO defaultWeapon;

    public GameObject visual;
    public GameObject armVisual;
}
