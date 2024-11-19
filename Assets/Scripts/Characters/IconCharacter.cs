using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconCharacter : IconUnit
{
    public int elixirs;
    public bool[] elixirState;

    public int classIndex;
    public int jobIndex;

    public Kin kin;

    public NarrativeChara narrativeAspect { get; private set; }

    public void GiveNarrativeAspect(NarrativeChara nc)
    {
        narrativeAspect = nc;
    }

    //we build the aspects on the copy making, so its only made on casting units and loading, instead on every list update
    //also lowers the amount of data we must save on the unitpiece file
    public IconCharacter MakeCopy()
    {
        IconCharacter copy = base.MakeCopyChara();

        copy.classIndex = classIndex;
        copy.jobIndex = jobIndex;
        copy.kin = kin;
        copy.narrativeAspect = narrativeAspect;

        //get data from type, class, job, etc
        ClassData.IconClass classData = UnitManager._instance.classes.classes[classIndex];
        int chapterNum = 1; // Mathf.CeilToInt((float)level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;
        copy.vitality = classData.chapterStats[chapterNum - 1].vitality;
        copy.defense = classData.chapterStats[chapterNum - 1].defense.ToString();
        copy.speed = classData.chapterStats[chapterNum - 1].speed;
        copy.dash = classData.chapterStats[chapterNum - 1].dash;

        copy.attack = "+" + classData.chapterStats[chapterNum - 1].attackBonus;
        copy.frayDamage = classData.chapterStats[chapterNum - 1].frayDamage.ToString();

        string damageStat = chapterNum + "d" + classData.chapterStats[chapterNum - 1].damageDie;
        int damageAdd = classData.chapterStats[chapterNum - 1].damageAdditionMultiplier;
        if (damageAdd > 0)
            damageStat += "+" + damageAdd;

        copy.damage = damageStat;
        copy.attackType = classData.chapterStats[chapterNum - 1].basicAttack;

        copy.SetFreshFlag(freshFlag);

        //check if its a first cast or not. If not, load current HP, vigor, etc. from the map file
        if (!freshFlag)
        {
            copy.currentHP = currentHP;
            copy.hp = hp;
            copy.elixirs = elixirs;
            copy.elixirState = elixirState;
            copy.size = size;
            copy.armor = armor;
        }
        else
        {
            copy.hp = classData.chapterStats[chapterNum - 1].vitality * 4;
            copy.elixirs = classData.chapterStats[chapterNum - 1].elixirs;
            copy.elixirState = new bool[elixirs];
            for(int i = 0; i < elixirs; i++)
            {
                copy.elixirState[i] = true;
            }
            copy.armor = classData.chapterStats[chapterNum - 1].armor;
            copy.vigor = 0;
            copy.addedHP = 0;
            copy.currentHP = copy.hp;
            copy.currentVigor = 0;
            copy.size = 1;
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

        //job
        for (int i = 0; i < classData.jobs[jobIndex].jobTraits.Length; i++)
        {
            ClassData.Trait traitCopy = new ClassData.Trait();
            traitCopy.traitName = classData.jobs[jobIndex].jobTraits[i].traitName;

            string auxText = classData.jobs[jobIndex].jobTraits[i].traitDescription;
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

        copy.traits = traitComp;

        return copy;
    }

    public IconCharacter MakeCopyForNewPiece()
    {
        IconCharacter copy = base.MakeCopyChara();

        copy.classIndex = classIndex;
        copy.jobIndex = jobIndex;
        copy.kin = kin;
        copy.narrativeAspect = narrativeAspect;

        //get data from type, class, job, etc
        ClassData.IconClass classData = UnitManager._instance.classes.classes[classIndex];
        int chapterNum = 1; // Mathf.CeilToInt((float)level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;
        copy.vitality = classData.chapterStats[chapterNum - 1].vitality;
        copy.defense = classData.chapterStats[chapterNum - 1].defense.ToString();
        copy.speed = classData.chapterStats[chapterNum - 1].speed;
        copy.dash = classData.chapterStats[chapterNum - 1].dash;

        copy.attack = "+" + classData.chapterStats[chapterNum - 1].attackBonus;
        copy.frayDamage = classData.chapterStats[chapterNum - 1].frayDamage.ToString();

        string damageStat = chapterNum + "d" + classData.chapterStats[chapterNum - 1].damageDie;
        int damageAdd = classData.chapterStats[chapterNum - 1].damageAdditionMultiplier;
        if (damageAdd > 0)
            damageStat += "+" + damageAdd;

        copy.damage = damageStat;
        copy.attackType = classData.chapterStats[chapterNum - 1].basicAttack;

        copy.SetFreshFlag(true); //new piece

        copy.hp = hp;
        copy.elixirs = elixirs;
        copy.elixirState = new bool[elixirs];
        for (int i = 0; i < elixirs; i++)
        {
            copy.elixirState[i] = true;
        }
        copy.size = size;
        copy.armor = armor;

        copy.vigor = 0;
        copy.addedHP = 0;
        copy.currentHP = copy.currentHP;
        copy.currentVigor = 0;

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

        //job
        for (int i = 0; i < classData.jobs[jobIndex].jobTraits.Length; i++)
        {
            ClassData.Trait traitCopy = new ClassData.Trait();
            traitCopy.traitName = classData.jobs[jobIndex].jobTraits[i].traitName;

            string auxText = classData.jobs[jobIndex].jobTraits[i].traitDescription;
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

        copy.traits = traitComp;

        return copy;
    }

    public override List<ClassData.Trait> GetTraits(bool filterThroughPhase = true)
    {
        //should return at least the class traits
        List<ClassData.Trait> sumTraits = new List<ClassData.Trait>();
        sumTraits.AddRange(UnitManager._instance.classes.classes[classIndex].classTraits);

        sumTraits.AddRange(UnitManager._instance.classes.classes[classIndex].jobs[jobIndex].jobTraits);

        return sumTraits; // base.GetTraits();
    }

    public override List<ClassData.Ability> GetAbilities(bool filterThroughPhase = true)
    {
        return base.GetAbilities();
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
