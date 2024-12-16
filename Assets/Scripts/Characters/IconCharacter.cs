using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsperCharacter : IconUnit
{
    public int[] magicArts; //(magic art)
    public int[] magicArtLevels;

    //items and equipment
    public int weaponID;
    public int[] itemInventory;
    public int[] equipmentInventory;

    //skills
    public int[] skillsIDs;

    //aether

    //fate

    //hand

    public void SetupNewChara()
    {
        level = 0;

        magicArts = new int[0];
        magicArtLevels = new int[0];

        weaponID = 0;
        itemInventory = new int[0];
        equipmentInventory = new int[0];

        hp = 40;
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
            copy.hp = hp;
        }
        else
        {
            copy.hp = 40;
            copy.addedHP = 0;
            copy.currentHP = copy.hp;
        }

        return copy;
    }

    public EsperCharacter MakeCopyForNewPiece()
    {
        EsperCharacter copy = base.MakeCopyChara();

        copy.magicArts = magicArts;
        copy.skillsIDs = skillsIDs;

        //get data from type, class, job, etc
        ClassData.IconClass classData = UnitManager._instance.classes.classes[magicArts[0]];
        int chapterNum = 1; // Mathf.CeilToInt((float)level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;
        copy.defense = defense;
        copy.speed = speed;

        copy.SetFreshFlag(true); //new piece

        copy.hp = hp;

        copy.addedHP = 0;
        copy.currentHP = copy.currentHP;

        return copy;
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
