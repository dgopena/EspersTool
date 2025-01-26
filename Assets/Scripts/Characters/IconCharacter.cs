using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsperCharacter : EsperUnit
{
    public int[] magicArts; //(magic art)
    public int[] magicArtLevels;

    //items and equipment
    public int weaponID;
    public int[] itemInventory;
    public int[] equipmentInventory;

    //skills
    public int[] skillsIDs;

    public void SetupNewChara()
    {
        level = 0;

        magicArts = new int[0];
        magicArtLevels = new int[0];

        weaponID = 0;
        itemInventory = new int[0];
        equipmentInventory = new int[0];

        baseHP = 40;
        defense = 0;
        speed = 40;

        skillsIDs = new int[0];
    }

    //we build the aspects on the copy making, so its only made on casting units and loading, instead on every list update
    //also lowers the amount of data we must save on the unitpiece file
    public EsperCharacter MakeCopy()
    {
        EsperCharacter copy = base.MakeCopyChara();

        copy.magicArts = magicArts;
        copy.magicArtLevels = magicArtLevels;

        copy.weaponID = weaponID;
        copy.itemInventory = itemInventory;
        copy.equipmentInventory = equipmentInventory;

        copy.skillsIDs = skillsIDs;

        copy.defense = defense;
        copy.speed = defense;

        copy.SetFreshFlag(freshFlag);

        //check if its a first cast or not. If not, load current HP, vigor, etc. from the map file
        if (!freshFlag)
        {
            copy.currentHP = currentHP;
            copy.baseHP = baseHP;
        }
        else
        {
            copy.baseHP = 40;
            copy.addedHP = 0;
            copy.currentHP = copy.baseHP;
        }

        return copy;
    }

    public EsperCharacter MakeCopyForNewPiece()
    {
        EsperCharacter copy = base.MakeCopyChara();

        copy.magicArts = magicArts;
        copy.skillsIDs = skillsIDs;

        copy.defense = defense;
        copy.speed = speed;

        copy.SetFreshFlag(true); //new piece

        copy.baseHP = baseHP;

        copy.addedHP = 0;
        copy.currentHP = copy.currentHP;

        return copy;
    }

    public Tuple<string, int>[] GetBuffs(int statIndex, int cardNumber, bool attack, bool firstAction = false) //
    {
        List<Tuple<string, int>> buffSet = new List<Tuple<string, int>>();

        //attack & weapon
        if (attack && weaponID >= 0 && weaponID < UnitManager._instance.itemData.weapons.Count)
        {
            int atkMod = UnitManager._instance.itemData.weapons[weaponID].atkEffectModifier;
            buffSet.Add(new Tuple<string, int>("Weapon", atkMod));
        }

        //equipment
        for (int i = 0; i < equipmentInventory.Length; i++)
        {
            ItemsData.Equipment equip = UnitManager._instance.itemData.equipment[equipmentInventory[i]];
            if (equip.effectType == ItemsData.EquipmentEffectType.CardValueIncrease)
            {
                if (cardNumber >= equip.modRange.x && cardNumber <= equip.modRange.y)
                {
                    if (equip.modEven && equip.modOdd)
                    {
                        buffSet.Add(new Tuple<string, int>(equip.name, equip.modNumber));
                    }
                    else if (equip.modEven && cardNumber % 2 == 0)
                    {
                        buffSet.Add(new Tuple<string, int>(equip.name, equip.modNumber));
                    }
                    else if (equip.modOdd && cardNumber % 2 == 1)
                    {
                        buffSet.Add(new Tuple<string, int>(equip.name, equip.modNumber));
                    }
                    else
                    {
                        buffSet.Add(new Tuple<string, int>(equip.name, equip.modNumber));
                    }
                }
                else if (cardNumber == equip.modRange.x || cardNumber == equip.modRange.y)
                {
                    buffSet.Add(new Tuple<string, int>(equip.name, equip.modNumber));
                }
            }
            else if (equip.effectType == ItemsData.EquipmentEffectType.ActionValueIncrease)
            {
                if(equip.modNumber == statIndex)
                    buffSet.Add(new Tuple<string, int>(equip.name, 1));
            }
            else if (equip.effectType == ItemsData.EquipmentEffectType.AdvantageGain && firstAction)
            {
                if (equip.name.Equals("Meteorite Earring", StringComparison.OrdinalIgnoreCase))
                    buffSet.Add(new Tuple<string, int>(equip.name, 2));
            }
        }
        
        return buffSet.ToArray();
    }

    public bool HasAdvantage(int statIndex, bool firstAction = false)
    {
        //equipment
        for (int i = 0; i < equipmentInventory.Length; i++)
        {
            ItemsData.Equipment equip = UnitManager._instance.itemData.equipment[equipmentInventory[i]];
            if (equip.effectType == ItemsData.EquipmentEffectType.AdvantageGain)
            {
                if (statIndex == equip.modNumber)
                    return true;
                else if (statIndex < 0 && firstAction)
                {
                    if (equip.name.Equals("Emerald Earring", StringComparison.OrdinalIgnoreCase))
                        return true;
                    else if (equip.name.Equals("Meteorite Earring", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
        }

        return false;
    }
}

[System.Serializable]
public enum Kin
{
    Thrynn,
    Trogg,
    Beastfolk,
    Xixo
}
