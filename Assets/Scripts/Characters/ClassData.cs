using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Class File", menuName = "ScriptableObjects/Class Data")]
public class ClassData : ScriptableObject
{
    public List<IconClass> classes;

    [System.Serializable]
    public struct IconClass
    {
        public string name;
        public Trait[] classTraits;
        public Trait specialMechanic;
        public Trait gambit;

        public List<ChapterStats> chapterStats;

        public List<IconJob> jobs;
    }

    [System.Serializable]
    public struct ChapterStats
    {
        public int vitality;
        public int elixirs;
        public int armor;
        public int defense;
        public int speed;
        public int dash;

        [Space(10f)]
        public int attackBonus;
        public int frayDamage;
        public int damageDie;
        public int damageAdditionMultiplier;
        public string basicAttack;
    }

    [System.Serializable]
    public struct Trait
    {
        public int traitID;
        public int docID;
        public string traitName;
        public string traitDescription;

        public int phaseIndex; //to be deleted
    }

    [System.Serializable]
    public struct Ability
    {
        public int abilityID;
        public int docID;
        public string abilityName;
        public string actionCost;
        public bool isAttack;
        public string[] abilityAspects;
        public int[] subCombos;
        public string abilityEffect;

        public int abilityComboDepth; //to be filled on execution

        public int phaseIndex; //to be deleted
    }
}

[System.Serializable]
public class IconJob
{
    public string name;
    public string jobDescription;

    public ClassData.Trait[] jobTraits; //to be deleted

    public AspectSet generalAspects;
    public AspectSet[] phaseAspects;
}
