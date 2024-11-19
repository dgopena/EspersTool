using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconUnit
{
    public int unitID { get; private set; }

    public string unitName;

    public int level; //can be chapter too, for foes

    public Color colorChoice = Color.white;
    public int headPartID { get; protected set; }
    public int bodyPartID { get; protected set; }
    public int lWeaponPartID { get; protected set; }
    public int rWeaponPartID { get; protected set; }

    public string graphicImageID = "";

    public int vitality { get; protected set; }
    public int hp { get; protected set; }
    public string textHP { get; protected set; }
    public int armor { get; protected set; }
    public string textArmor { get; protected set; }
    public int vigor { get; protected set; }

    public string defense { get; protected set; }
    public int speed { get; protected set; }

    public int dash { get; protected set; }

    public string damage { get; protected set; } //damage die. must consider additions if appliable
    public string frayDamage { get; protected set; }
    public string attack { get; protected set; }

    public string attackType { get; protected set; }

    public bool freshFlag { get; private set; }

    //map (therefore instance) dependant. should, maybe, be stored as a part of the map file
    public List<Status> activeStatus { get; private set; }
    public List<Blight> activeBlights { get; private set; }
    public List<PositiveEffects> activePositiveEffects { get; private set; }

    public int blessingTokens { get; private set; }

    public int currentHP { get; protected set; }
    public int addedHP { get; protected set; } //added by effects, such as faction or traits
    public int size { get; protected set; } //default value 1. can be modified. should be set upon setting a unit on the map
    public int currentVigor { get; protected set; }

    //total traits the unit has
    public List<ClassData.Trait> traits { get; protected set; }
    //total attacks this unit has. for now, only used for enemies (class, job, faction and template)
    public List<ClassData.Ability> attacks { get; protected set; }

    //saves if the user has manually entered these key values or not
    public bool textInHPFlag { get; protected set; }
    public bool textInArmorFlag { get; protected set; }

    public void GiveID(int id)
    {
        unitID = id;
    }

    public DateTime lastModified;

    [System.Serializable]
    public enum Status
    {
        Blind,
        Dazed,
        Hatred, //adding hatred as a status one must specify the target of said hatred
        Weakened,
        Pacified,
        Shattered,
        Stunned,
        Slashed,
        Vulnerable,
        Bloody,
        Incapacitated,
        Immobile,
        Sealed
    }

    [System.Serializable]
    public enum Blight
    {
        Burning,
        Electrified,
        Poisoned,
        Frostbite
    }

    [System.Serializable]
    public enum PositiveEffects
    {
        Counter,
        Defiance,
        Divine,
        Dodge,
        Evasion,
        Flying,
        Intangible,
        Phasing,
        Pierce,
        Regeneration,
        Skirmisher,
        Stealth,
        Sturdy,
        TrueStrike,
        Unstoppable,
        VigilanceX,
        VigorX,
        Rampart,
        Unerring
    }

    public virtual IconCharacter MakeCopyChara()
    {
        IconCharacter copy = new IconCharacter();
        copy.unitID = unitID;
        copy.unitName = unitName;
        copy.level = level;
        copy.colorChoice = colorChoice;
        copy.headPartID = headPartID;
        copy.bodyPartID = bodyPartID;
        copy.lWeaponPartID = lWeaponPartID;
        copy.rWeaponPartID = rWeaponPartID;

        copy.graphicImageID = graphicImageID;

        if (!freshFlag) //this means is being loaded from a previous unit saved on map
        {
            copy.textInHPFlag = textInHPFlag;
            copy.textInArmorFlag = textInArmorFlag;
            copy.hp = hp;
            copy.armor = armor;
            copy.currentHP = currentHP;
            copy.currentVigor = currentVigor;
            copy.addedHP = addedHP;
            copy.vigor = vigor;
            copy.size = size;

            copy.blessingTokens = blessingTokens;
            if (activeBlights != null)
                copy.activeBlights = new List<Blight>(activeBlights);
            else
                copy.activeBlights = new List<Blight>();
            if (activeStatus != null)
                copy.activeStatus = new List<Status>(activeStatus);
            else
                copy.activeStatus = new List<Status>();
            if (activePositiveEffects != null)
                copy.activePositiveEffects = new List<PositiveEffects>(activePositiveEffects);
            else
                copy.activePositiveEffects = new List<PositiveEffects>();
        }
        else
        {
            copy.currentHP = copy.hp;

            copy.activeBlights = new List<Blight>();
            copy.activeStatus = new List<Status>();
            copy.activePositiveEffects = new List<PositiveEffects>();
            copy.size = 1;
        }

        return copy;
    }

    public virtual IconFoe MakeCopyFoe()
    {
        IconFoe copy = new IconFoe();
        copy.unitID = unitID;
        copy.unitName = unitName;
        copy.level = level;
        copy.colorChoice = colorChoice;
        copy.headPartID = headPartID;
        copy.bodyPartID = bodyPartID;
        copy.lWeaponPartID = lWeaponPartID;
        copy.rWeaponPartID = rWeaponPartID;
        copy.graphicImageID = graphicImageID;

        if (!freshFlag) //this means is being loaded from a previous unit saved on map
        {
            copy.textInHPFlag = textInHPFlag;
            copy.textInArmorFlag = textInArmorFlag;
            copy.hp = hp;
            copy.armor = armor;
            copy.currentHP = currentHP;
            copy.currentVigor = currentVigor;
            copy.addedHP = addedHP;
            copy.vigor = vigor;
            copy.size = size;

            copy.blessingTokens = blessingTokens;
            if (activeBlights != null)
                copy.activeBlights = new List<Blight>(activeBlights);
            else
                copy.activeBlights = new List<Blight>();
            if (activeStatus != null)
                copy.activeStatus = new List<Status>(activeStatus);
            else
                copy.activeStatus = new List<Status>();
            if (activePositiveEffects != null)
                copy.activePositiveEffects = new List<PositiveEffects>(activePositiveEffects);
            else
                copy.activePositiveEffects = new List<PositiveEffects>();
        }
        else
        {
            copy.currentHP = copy.hp;

            copy.activeBlights = new List<Blight>();
            copy.activeStatus = new List<Status>();
            copy.activePositiveEffects = new List<PositiveEffects>();
            copy.size = 1;
        }

        return copy;
    }

    public void SetFreshFlag(bool value)
    {
        freshFlag = value;
    }

    public void CorrectTextHP(int value)
    {
        textInHPFlag = false;
        hp = value;
        currentHP = hp;
    }

    public void SetTextHP(string text)
    {
        textInHPFlag = true;
        textHP = text;
    }

    public void CorrectTextArmor(int value)
    {
        textInArmorFlag = false;
        armor = value;
    }

    public void SetTextArmor(string text)
    {
        textInArmorFlag = true;
        textArmor = text;
    }

    public void GiveAddedHP(int value)
    {
        addedHP = value;
    }

    public void GiveCurrentHP(int value)
    {
        currentHP = value;
    }

    public void SetBaseHP(int value)
    {
        hp = value;
    }

    public void SetVigor(int currentVigor, int totalVigor)
    {
        vigor = totalVigor;
        this.currentVigor = currentVigor;
    }

    public void GiveSize(int value)
    {
        size = value;
    }

    public void GiveArmorValue(int value)
    {
        armor = value;
    }

    public void SetBlessing(bool add)
    {
        blessingTokens += add ? 1 : -1;
        if(blessingTokens < 0)
            blessingTokens = 0;
    }

    public void SetBlessing(int blessCount)
    {
        blessingTokens = blessCount;
    }

    public void AddVigor(bool add)
    {
        if (add && currentVigor == vigor)
        {
            vigor += 1;
            currentVigor += 1;
        }
        else if (add && currentVigor < vigor)
            currentVigor += 1;
        else if (!add)
            currentVigor -= 1;
        
        if (currentVigor <= 0)
        {
            currentVigor = 0;
            vigor = 0;
        }
    }

    public void GiveBlightList(List<Blight> blights)
    {
        if (blights != null)
            activeBlights = new List<Blight>(blights);
        else
            activeBlights = new List<Blight>();
    }

    public void GiveStatusList(List<Status> status)
    {
        if (status != null)
            activeStatus = new List<Status>(status);
        else
            activeStatus = new List<Status>();
    }

    public void GiveEffectList(List<PositiveEffects> effects)
    {
        if (effects != null)
            activePositiveEffects = new List<PositiveEffects>(effects);
        else
            activePositiveEffects = new List<PositiveEffects>();
    }

    public void GiveGraphicPieceID(string graphID)
    {
        graphicImageID = graphID;
    }

    public void GivePartIDs(int headID, int bodyID, int lWeaponID, int rWeaponID)
    {
        headPartID = headID;
        bodyPartID = bodyID;
        lWeaponPartID = lWeaponID;
        rWeaponPartID = rWeaponID;
    }

    public virtual List<ClassData.Trait> GetTraits(bool filterThroughPhase = true)
    {
        return new List<ClassData.Trait>();
    }

    public virtual List<ClassData.Ability> GetAbilities(bool filterThroughPhase = true)
    {
        return new List<ClassData.Ability>();
    }
}
