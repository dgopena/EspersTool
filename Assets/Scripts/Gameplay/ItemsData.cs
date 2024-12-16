using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Items File", menuName = "ScriptableObjects/Items Data")]
public class ItemsData : ScriptableObject
{
    public List<Weapon> weapons;

    public enum RangeEnum
    {
        Close,
        Near,
        Far
    }

    [System.Serializable]
    public struct Weapon
    {
        public string name;
        public RangeEnum range;
        public int atkEffectModifier;
    }

    public List<Item> items;

    public enum ItemType
    {
        HPHeal,
        StatIncrease,
        Other
    }

    public enum RarityType
    {
        Common,
        Uncommon,
        Rare
    }

    [System.Serializable]
    public struct Item
    {
        public string name;
        public ItemType type;
        public int modNumber;
        public string effect;
        public int price;
        public RarityType rarity;
        public int size;
    }

    public List<Equipment> equipment;

    public enum EquipmentType
    {
        Bangle,
        Ring,
        Chain,
        Pendant,
        Charm,
        Earring
    }

    public enum EquipmentEffectType
    {
        CardValueIncrease,
        FateDrawIncrease,
        ActionValueIncrease,
        HPAdd,
        CarryingAdd,
        AdvantageGain,
        Other
    }

    [System.Serializable]
    public struct Equipment
    {
        public string name;
        public EquipmentType type;
        public EquipmentEffectType effectType;
        public Vector2 modRange;
        public int modNumber;
        public bool modEven;
        public bool modOdd;
        public string effect;
        public int price;
        public RarityType rarity;
        public int size;
    }
}
