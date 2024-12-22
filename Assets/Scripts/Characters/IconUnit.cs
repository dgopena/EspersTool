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

    //stats
    public int statSTR;
    public int statINT;
    public int statDEX;
    public int statCHA;

    public string graphicImageID = "def";
    public int baseHP { get; protected set; }

    public int defense { get; protected set; }
    public int speed { get; protected set; }

    public bool freshFlag = false;

    //map (therefore instance) dependant. should, maybe, be stored as a part of the map file
    public List<Status> activeStatus { get; private set; }
    public List<Blight> activeBlights { get; private set; }
    public List<PositiveEffects> activePositiveEffects { get; private set; }

    public int currentHP { get; protected set; }
    public int addedHP { get; protected set; } //added by effects, such as faction or traits

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

    public virtual EsperCharacter MakeCopyChara()
    {
        EsperCharacter copy = new EsperCharacter();
        copy.unitID = unitID;
        copy.unitName = unitName;
        copy.level = level;
        copy.colorChoice = colorChoice;

        copy.statSTR = statSTR;
        copy.statINT = statINT;
        copy.statDEX = statDEX;
        copy.statCHA = statCHA;

        copy.graphicImageID = graphicImageID;

        if (!freshFlag) //this means is being loaded from a previous unit saved on map
        {
            copy.baseHP = baseHP;
            copy.currentHP = currentHP;
            copy.addedHP = addedHP;

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
            copy.currentHP = copy.baseHP;

            copy.activeBlights = new List<Blight>();
            copy.activeStatus = new List<Status>();
            copy.activePositiveEffects = new List<PositiveEffects>();
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
        copy.graphicImageID = graphicImageID;

        if (!freshFlag) //this means is being loaded from a previous unit saved on map
        {
            copy.baseHP = baseHP;
            copy.currentHP = currentHP;
            copy.addedHP = addedHP;

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
            copy.currentHP = copy.baseHP;

            copy.activeBlights = new List<Blight>();
            copy.activeStatus = new List<Status>();
            copy.activePositiveEffects = new List<PositiveEffects>();
        }

        return copy;
    }

    public void SetFreshFlag(bool value)
    {
        freshFlag = value;
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
        baseHP = value;
    }

    public int GetTotalHP()
    {
        return baseHP + addedHP;
    }

    public void GiveDefense(int value)
    {
        defense = value;
    }

    public void GiveSpeed(int value)
    {
        speed = value;
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

    public virtual List<ClassData.Ability> GetAbilities()
    {
        return new List<ClassData.Ability>();
    }
}
