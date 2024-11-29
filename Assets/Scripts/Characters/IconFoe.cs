using System.Collections.Generic;

public class IconFoe : IconUnit
{
    public FoeType type;
    public int classIndex;

    //ATK mods
    public int statModSTR;
    public int statModINT;
    public int statModDEX;
    public int statModCHA;

    public int currentPhase { get; private set; } //only appliable to legends

    public List<ClassData.Ability> abilities;

    public string description;

    public IconFoe MakeCopy()
    {
        IconFoe copy = base.MakeCopyFoe();
        copy.type = type;
        copy.classIndex = classIndex;

        copy.SetFreshFlag(freshFlag);

        copy.statModSTR = statModSTR;
        copy.statModINT = statModINT;
        copy.statModDEX = statModDEX;
        copy.statModCHA = statModCHA;

        copy.description = description;

        FoeData.FoeClass.FoeStats stats = BuildStatsSet();

        if (freshFlag)
        {
            int auxParse = 0;
            if (int.TryParse(stats.HP, out auxParse))
            {
                copy.hp = auxParse;
            }

            copy.currentHP = copy.hp;
        }
        else
        {
            copy.hp = hp;
        }

        copy.defense = stats.defense.ToString();
        copy.speed = stats.speed;

        copy.abilities = GetAbilities();

        return copy;
    }

    public List<string> GetFoeData()
    {
        List<string> dataLines = new List<string>();

        return dataLines;
    }

    public FoeData.FoeClass.FoeStats BuildStatsSet()
    {
        FoeData.FoeClass.FoeStats stats = new FoeData.FoeClass.FoeStats();



        return stats;
    }

    //automatically builds the attack set. only considers phase value if the type corresponds to a legend
    public override List<ClassData.Ability> GetAbilities()
    {
        return base.GetAbilities();
    }
}

[System.Serializable]
public enum FoeType
{
    Foe,
    Mob,
    Elite,
    Legend,
    SpecialSummon
}
