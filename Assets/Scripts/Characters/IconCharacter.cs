using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconCharacter : IconUnit
{
    public int classIndex; //(magic art)

    //items and equipment

    //skills
    public int[] skillsIDs;

    //aether

    //fate

    //hand

    //we build the aspects on the copy making, so its only made on casting units and loading, instead on every list update
    //also lowers the amount of data we must save on the unitpiece file
    public IconCharacter MakeCopy()
    {
        IconCharacter copy = base.MakeCopyChara();

        copy.classIndex = classIndex;

        //get data from type, class, job, etc
        ClassData.IconClass classData = UnitManager._instance.classes.classes[classIndex];
        int chapterNum = 1; // Mathf.CeilToInt((float)level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;
        copy.defense = classData.chapterStats[chapterNum - 1].defense.ToString();
        copy.speed = classData.chapterStats[chapterNum - 1].speed;

        string damageStat = chapterNum + "d" + classData.chapterStats[chapterNum - 1].damageDie;
        int damageAdd = classData.chapterStats[chapterNum - 1].damageAdditionMultiplier;
        if (damageAdd > 0)
            damageStat += "+" + damageAdd;

        copy.SetFreshFlag(freshFlag);

        //check if its a first cast or not. If not, load current HP, vigor, etc. from the map file
        if (!freshFlag)
        {
            copy.currentHP = currentHP;
            copy.hp = hp;
        }
        else
        {
            copy.hp = classData.chapterStats[chapterNum - 1].vitality * 4;
            copy.addedHP = 0;
            copy.currentHP = copy.hp;
        }

        List<ClassData.Trait> traitComp = new List<ClassData.Trait>();

        //we build the trait list
        //class
        for (int i = 0; i < classData.classTraits.Length; i++)
        {
            ClassData.Trait traitCopy = new ClassData.Trait();
            traitCopy.traitName = classData.classTraits[i].traitName;

            string auxText = classData.classTraits[i].traitDescription;
            string traitDescription = "";

            for (int c = 0; c < auxText.Length; c++)
            {
                if (((int)auxText[c] == 8226) || ((int)auxText[c] == 183))
                {
                    string sub = auxText.Substring(0, c) + "\n ";
                    auxText = auxText.Substring(c);
                    auxText = sub + auxText;
                    c += 3;
                }
            }

            traitDescription = auxText;
            traitCopy.traitDescription = traitDescription;

            traitComp.Add(traitCopy);
        }

        return copy;
    }

    public IconCharacter MakeCopyForNewPiece()
    {
        IconCharacter copy = base.MakeCopyChara();

        copy.classIndex = classIndex;
        copy.skillsIDs = skillsIDs;

        //get data from type, class, job, etc
        ClassData.IconClass classData = UnitManager._instance.classes.classes[classIndex];
        int chapterNum = 1; // Mathf.CeilToInt((float)level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;
        copy.defense = classData.chapterStats[chapterNum - 1].defense.ToString();
        copy.speed = classData.chapterStats[chapterNum - 1].speed;

        string damageStat = chapterNum + "d" + classData.chapterStats[chapterNum - 1].damageDie;
        int damageAdd = classData.chapterStats[chapterNum - 1].damageAdditionMultiplier;
        if (damageAdd > 0)
            damageStat += "+" + damageAdd;

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
