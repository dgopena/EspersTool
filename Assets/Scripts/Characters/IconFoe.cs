using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class EsperFoe : IconUnit
{
    public FoeType type;
    public int classIndex;

    //ATK mods
    // public int statModSTR;
    // public int statModINT;
    // public int statModDEX;
    // public int statModCHA;

    public int[] abilityIDs;

    public string description;

    private string atkModRaw = "";
    private string[] atkModStats;
    private string[] atckModOperations;

    public EsperFoe MakeCopy()
    {
        EsperFoe copy = base.MakeCopyFoe();
        copy.type = type;
        copy.classIndex = classIndex;

        copy.SetFreshFlag(freshFlag);
        
        // copy.statModSTR = statModSTR;
        // copy.statModINT = statModINT;
        // copy.statModDEX = statModDEX;
        // copy.statModCHA = statModCHA;

        copy.description = description;

        FoeData.FoeClass.FoeStats stats = BuildStatsSet();

        if (freshFlag)
        {
            copy.currentHP = copy.baseHP;
        }
        else
        {
            copy.baseHP = baseHP;
        }

        copy.defense = defense;
        copy.speed = speed;

        copy.abilityIDs = abilityIDs;

        copy.atkModRaw = atkModRaw;
        copy.atkModStats = atkModStats;
        copy.atckModOperations = atckModOperations;

        return copy;
    }

    public void SetupNewFoe()
    {
        unitName = "";
        description = "";
        abilityIDs = Array.Empty<int>();
        colorChoice = Color.white;
        statSTR = 0;
        statINT = 0;
        statDEX = 0;
        statCHA = 0;
        // statModSTR = 0;
        // statModINT = 0;
        // statModDEX = 0;
        // statModCHA = 0;
        baseHP = 20;
        defense = 0;
        speed = 40;
        atkModStats = Array.Empty<string>();
        atckModOperations = Array.Empty<string>();
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

    public int GetATKMod()
    {
        if (atkModStats.Length != (atckModOperations.Length + 1))
        {
            return int.MinValue;
        }

        int result = 0;
        for (int i = 0; i < atkModStats.Length; i++)
        {
            int num = 0;
            if (atkModStats[i] == "STR")
                num = statSTR;
            else if (atkModStats[i] == "INT")
                num = statINT;
            else if (atkModStats[i] == "DEX")
                num = statDEX;
            else if (atkModStats[i] == "CHA")
                num = statCHA;
            else if(int.TryParse(atkModStats[i], out int numResult))
                num = numResult;
            
            if (i == 0)
            {
                result = num;
            }
            else
            {
                if (atckModOperations[i - 1] == "+")
                    result += num;
                else if (atckModOperations[i - 1] == "-")
                    result -= num;
                else if (atckModOperations[i - 1] == "*")
                    result *= num;
                else if (atckModOperations[i - 1] == "/")
                {
                    float div = (float)result / (float)num;
                    result = Mathf.RoundToInt(div);
                }
            }
        }
        
        return result;
    }

    public string GetATKModString()
    {
        if (atkModStats.Length != (atckModOperations.Length + 1))
        {
            return "";
        }

        string modText = "";
        for (int i = 0; i < atkModStats.Length; i++)
        {
            string stat = atkModStats[i];
            
            if (i == 0)
            {
                modText = stat;
            }
            else
            {
                modText += atckModOperations[i - 1] + stat;
            }
        }
        
        return modText;
    }
    
    public void GiveATKMod(string[] stats, string[] operations)
    {
        atkModStats = stats;
        atckModOperations = operations;
    }
    
    //return false if was able to parse it
    public bool GiveATKModString(string atkModEntry)
    {
        atkModEntry = Regex.Replace(atkModEntry, @"\s+", ""); //remove all spaces
        atkModEntry = atkModEntry.ToUpper(); //to upper case

        string auxAtkModRaw = atkModEntry;
        
        List<string> statEntries = new List<string>();
        List<string> operationEntries = new List<string>();
        
        bool stop = false;
        int escapeCounter = 100;
        while (!stop)
        {
            int splitIndex = ClosestSplitIndex(atkModEntry);

            if (splitIndex == -1)
                stop = true;

            string stat = "";
            if(!stop)
                stat = atkModEntry.Substring(0, splitIndex);
            else
                stat = atkModEntry;
            
            statEntries.Add(stat);
            
            if (!stop)
            {
                atkModEntry = atkModEntry.Substring(splitIndex);
                if (atkModEntry.Length == 0)
                    stop = true;
                else
                {
                    string op = atkModEntry.Substring(0, 1);
                    atkModEntry = atkModEntry.Substring(1);

                    operationEntries.Add(op);
                }
            }
            
            escapeCounter--;
            if (escapeCounter <= 0)
                stop = true;
        }

        if (ValidateStatEntries(statEntries) && ValidateOpEntries(operationEntries))
        {
            atkModRaw = auxAtkModRaw;
            
            GiveATKMod(statEntries.ToArray(), operationEntries.ToArray());
            return true;
        }
        
        return false;
    }

    private int ClosestSplitIndex(string text)
    {
        int closestIndex = int.MaxValue;
        int index = text.IndexOf('+');
        if (index >= 0 && index < closestIndex)
            closestIndex = index;
        index = text.IndexOf('-');
        if (index >= 0 && index < closestIndex)
            closestIndex = index;
        index = text.IndexOf('*');
        if (index >= 0 && index < closestIndex)
            closestIndex = index;
        index = text.IndexOf('/');
        if (index >= 0 && index < closestIndex)
            closestIndex = index;

        if (closestIndex == int.MaxValue)
            closestIndex = -1;

        return closestIndex;
    }

    private bool ValidateStatEntries(List<string> stats)
    {
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i] != "STR" && stats[i] != "INT" && stats[i] != "DEX" && stats[i] != "CHA")
            {
                if (!int.TryParse(stats[i], out int num))
                    return false;
            }
        }

        return true;
    }

    private bool ConvertToStatEntries(List<string> stats)
    {
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i] != "STR" && stats[i] != "INT" && stats[i] != "DEX" && stats[i] != "CHA")
            {
                if (!int.TryParse(stats[i], out int num))
                    return false;
            }
        }

        return true;
    }
    
    private bool ValidateOpEntries(List<string> ops)
    {
        for(int i = 0; i < ops.Count; i++)
        {
            if (ops[i] != "+" && ops[i] != "-" && ops[i] != "*" && ops[i] != "/")
                return false;
        }

        return true;
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
