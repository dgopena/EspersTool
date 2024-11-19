using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Foe File", menuName = "ScriptableObjects/Foe Data")]
public class FoeData : ScriptableObject
{
    public List<FoeClass> classes;

    public List<FoeClass> eliteClasses;

    public List<FoeClass> legendClasses;

    public Mob[] mobs;

    public Mob[] specialSummons;

    public enum FoeUniqueTemplates
    {
        None,
        Mob, //HP to 1, Damage to 1 or 3 if takes 2 actions, Mob trait
        Elite, //takes two turns, Elite trait
        ImperialOfficer //receives certain traits. only available under imperial faction... so some templates need to be applied after choosing faction
    }

    public enum EnemyType //restriction for sub-factions
    {
        None,
        Foe,
        Elite,
        Legend
    }

    [System.Serializable]
    public struct FoeClass
    {
        public string name;
        public string foeClassDescription;

        public ClassData.Trait[] classTraits; //to delete

        public AspectSet generalAspects;
        public AspectSet[] phaseAspects;

        public FoeStats classStats;

        [System.Serializable]
        public struct FoeStats
        {
            public string HP;
            public int speed;
            public int dash;
            public int defense;
            public string armor;
            public int frayDamage;
            public int damageDie;
            public int dieAmount;
            public string[] specialData;
        }

        public List<IconJob> jobs;
    }
}

[System.Serializable]
public class FoeFaction
{
    public string factionName;
    public string factionDescription;

    public ClassData.Trait[] generalTraits; //to delete

    public AspectSet generalAspects;
    public AspectSet[] phaseSets;

    public SubFaction[] defaults;
    public SubFaction[] heavies;
    public SubFaction[] skirmishers;
    public SubFaction[] leaders;
    public SubFaction[] artilleries;
    public SubFaction[] special;
}

[System.Serializable]
public class SubFaction
{
    public string templateName;
    public string overallTemplateName;
    public string templateDescription;

    public ClassData.Trait[] traits; // to delete

    public StatOverride[] statOverrides;
    
    [Space(5f)]
    public AspectSet[] phaseSet;

    [Space(5f)]
    public AspectSet[] specialSets;

    [Space(5f)]
    public int chapterLimitNum;
    public bool allowGreater; //if, for example, limit is 1+, then chapterLimitNum is 1 and allowGreater is true
    public ChapterSet[] chapterAspects;

    [Space(5f)]
    public bool isUnique;
    public FoeData.EnemyType typeRestriction;

}

[System.Serializable]
public class Mob
{
    public string name;
    public string description;
    public FoeData.FoeClass.FoeStats stats;
    public int factionIndex = 0; // 0 - General
    [HideInInspector]public FoeFaction[] factions;

    public AspectSet[] aspectSet;
}

[System.Serializable]
public class AspectSet
{
    public string setName;
    public string setCondition;
    public ClassData.Trait[] traits;
    public ClassData.Ability[] abilities;
}

[System.Serializable]
public class ChapterSet
{
    public int chapterLock;
    public ClassData.Trait[] traits;
    public ClassData.Ability[] abilities;
}

[System.Serializable]
public class StatOverride
{
    public enum StatType
    {
        HP,
        Speed,
        Dash,
        Defense,
        Armor,
        FrayDamage,
        DamageDie,
        DieAmount
    }

    public StatType statType;
    public int intChangedValue;
    public string stringChangedValue;
}
