using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using static ClassData;

public class IconFoe : IconUnit
{
    public FoeType type;
    public int classIndex;
    public int jobIndex;

    public bool isDefaultFactionEntry = false;
    public int factionIndex = -1; //by default no faction
    public int subFactionIndex;

    public int templateIndex = -1; //by default no template
    public int subTemplate = 0; // 0 - None, 1 - Mob, 2 - Elite
    public int currentPhase { get; private set; } //only appliable to legends

    public IconFoe MakeCopy()
    {
        IconFoe copy = base.MakeCopyFoe();
        copy.type = type;
        copy.classIndex = classIndex;
        copy.jobIndex = jobIndex;
        copy.factionIndex = factionIndex;
        copy.subFactionIndex = subFactionIndex;
        copy.templateIndex = templateIndex;
        copy.subTemplate = subTemplate;
        copy.isDefaultFactionEntry = (classIndex == 4);

        copy.SetFreshFlag(freshFlag);

        FoeData.FoeClass.FoeStats stats = BuildStatsSet();

        if (freshFlag)
        {
            int auxParse = 0;
            if (int.TryParse(stats.HP, out auxParse))
            {
                copy.hp = auxParse;
                copy.textInHPFlag = false;
            }
            else
            {
                copy.textHP = stats.HP;
                copy.textInHPFlag = true;
            }

            copy.currentHP = copy.hp;
            copy.currentVigor = 0;
        }
        else
        {
            copy.hp = hp;
            copy.textInHPFlag = textInHPFlag;
            copy.textHP = textHP;
        }

        copy.defense = stats.defense.ToString();
        copy.speed = stats.speed;
        copy.dash = stats.dash;
        copy.frayDamage = stats.frayDamage.ToString();

        copy.damage = stats.dieAmount + "D" + stats.damageDie;

        copy.traits = GetTraits();

        copy.attacks = GetAbilities();

        return copy;
    }

    public List<string> GetFoeData()
    {
        List<string> dataLines = new List<string>();

        if (type == FoeType.Mob || type == FoeType.SpecialSummon)
            dataLines.Add(type == FoeType.Mob ? UnitManager._instance.foes.mobs[classIndex].name : UnitManager._instance.summons.specialSummons[classIndex].name);
        else
        {
            string titleLabel = "";

            if (subTemplate > 0)
            {
                if (subTemplate == 1)
                {
                    titleLabel += "Heavy ";
                    if (classIndex == 1)
                        titleLabel += "Skirmisher ";
                    else if (classIndex == 2)
                        titleLabel += "Leader ";
                    else if (classIndex == 3)
                        titleLabel += "Artillery ";

                    titleLabel += "Mob\n";

                    dataLines.Add(titleLabel);
                }
                else if (subTemplate == 2)
                {
                    titleLabel += "Elite ";

                    titleLabel += "Heavy";
                    if (classIndex == 1)
                        titleLabel += "Skirmisher";
                    else if (classIndex == 2)
                        titleLabel += "Leader";
                    else if (classIndex == 3)
                        titleLabel += "Artillery";

                    dataLines.Add(titleLabel);
                }
            }
            else
            {
                if (type == FoeType.Foe)
                {
                    titleLabel = "Heavy Foe\n";
                    if (classIndex == 1)
                        titleLabel = "Skirmisher Foe\n";
                    else if (classIndex == 2)
                        titleLabel = "Leader Foe\n";
                    else if (classIndex == 3)
                        titleLabel = "Artillery Foe\n";

                    dataLines.Add(titleLabel);
                }
                else if (type == FoeType.Elite)
                {
                    titleLabel = "Elite Heavy\n";
                    if (classIndex == 1)
                        titleLabel = "Elite Skirmisher\n";
                    else if (classIndex == 2)
                        titleLabel = "Elite Leader\n";
                    else if (classIndex == 3)
                        titleLabel = "Elite Artillery\n";

                    dataLines.Add(titleLabel);
                }
                else if (type == FoeType.Legend)
                {
                    titleLabel = "Legendary Heavy\n";
                    if (classIndex == 1)
                        titleLabel = "Legendary Skirmisher\n";
                    else if (classIndex == 2)
                        titleLabel = "Legendary Leader\n";
                    else if (classIndex == 3)
                        titleLabel = "Legendary Artillery\n";

                    dataLines.Add(titleLabel);
                }
            }

            titleLabel = "<i>";

            //first, title label
            if (factionIndex < 0)
            {
                if (type == FoeType.Foe)
                    titleLabel += UnitManager._instance.foes.classes[classIndex].jobs[jobIndex].name;
                else if (type == FoeType.Elite)
                    titleLabel += UnitManager._instance.foes.eliteClasses[classIndex].name;
                else if (type == FoeType.Legend)
                    titleLabel += UnitManager._instance.foes.legendClasses[classIndex].name;

                dataLines.Add(titleLabel);
            }
            else
            {
                titleLabel += GetFaction().factionName + " " + GetFactionJob().templateName + "</i>";

                dataLines.Add(titleLabel);
            }
        }

        return dataLines;
    }

    public FoeData.FoeClass.FoeStats BuildStatsSet()
    {
        FoeData.FoeClass.FoeStats stats = new FoeData.FoeClass.FoeStats();

        if (type == FoeType.Mob || type == FoeType.SpecialSummon) //also manage here the special summons
        {
            if (type == FoeType.Mob)
            {
                Mob classData = UnitManager._instance.foes.mobs[classIndex];
                stats = classData.stats;
            }
            else if(type == FoeType.SpecialSummon)
            {
                Mob classData = UnitManager._instance.summons.specialSummons[classIndex];
                stats = classData.stats;
            }
        }
        else
        {
            int intHPValue = -1;
            string stringHPValue = "";

            if(factionIndex < 0) //if no faction, then its a normal job.
            {
                List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                if (type == FoeType.Elite)
                    classList = UnitManager._instance.foes.eliteClasses;
                else if (type == FoeType.Legend)
                    classList = UnitManager._instance.foes.legendClasses;

                FoeData.FoeClass classData = classList[classIndex];
                stats = classData.classStats;

                if (templateIndex < 0) //if no template, then no overrides
                {
                    return stats;
                }

                FoeTemplate tData = UnitManager._instance.templates.templates[templateIndex];
                for(int i = 0; i < tData.statOverrides.Length; i++)
                {
                    if (tData.statOverrides[i].stringChangedValue.Length > 0)
                    {
                        //string type override
                        if (tData.statOverrides[i].statType == StatOverride.StatType.HP)
                            stats.HP = tData.statOverrides[i].stringChangedValue;
                        else
                            Debug.LogError("Found a non HP text override in " + tData.templateName);
                    }
                    else
                    {
                        if (tData.statOverrides[i].statType == StatOverride.StatType.HP)
                            stats.HP = tData.statOverrides[i].intChangedValue.ToString();
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.Speed)
                            stats.speed = tData.statOverrides[i].intChangedValue;
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.Dash)
                            stats.dash = tData.statOverrides[i].intChangedValue;
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.Defense)
                            stats.defense = tData.statOverrides[i].intChangedValue;
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.FrayDamage)
                            stats.frayDamage = tData.statOverrides[i].intChangedValue;
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.DamageDie)
                            stats.damageDie = tData.statOverrides[i].intChangedValue;
                        else if (tData.statOverrides[i].statType == StatOverride.StatType.DieAmount)
                            stats.dieAmount = tData.statOverrides[i].intChangedValue;
                    }
                }

                if (int.TryParse(stats.HP, out int HPInt))
                {
                    intHPValue = HPInt;
                }
                else
                    stringHPValue = stats.HP;
            }
            else //some factions have stat overrides
            {
                List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                //elite and legend factions are unique type enemies. the index will give us all necessary overrides, but still, we better have the base ones anyway

                FoeData.FoeClass classData = classList[classIndex];
                stats = classData.classStats;

                FactionData factionSet = UnitManager._instance.factions[factionIndex];

                int factionSetIndex = 0;
                if(factionIndex > 8) //folk faction has 6 sub sets
                {
                    factionSetIndex = factionIndex - 8;
                }
                FoeFaction factionData = factionSet.foeFactions[factionSetIndex];

                SubFaction sbf = new SubFaction();
                if (classIndex == 0)
                    sbf = factionData.heavies[subFactionIndex];
                else if (classIndex == 1)
                    sbf = factionData.skirmishers[subFactionIndex];
                else if (classIndex == 2)
                    sbf = factionData.leaders[subFactionIndex];
                else if (classIndex == 3)
                    sbf = factionData.artilleries[subFactionIndex];
                else
                    sbf = factionData.defaults[subFactionIndex];

                //we apply said overrides
                for (int i = 0; i < sbf.statOverrides.Length; i++)
                {
                    if (sbf.statOverrides[i].stringChangedValue.Length > 0)
                    {
                        //string type override
                        if (sbf.statOverrides[i].statType == StatOverride.StatType.HP)
                            stats.HP = sbf.statOverrides[i].stringChangedValue;
                        else
                            Debug.LogError("Found a non HP text override in " + sbf.templateName);
                    }
                    else
                    {
                        if (sbf.statOverrides[i].statType == StatOverride.StatType.HP)
                            stats.HP = sbf.statOverrides[i].intChangedValue.ToString();
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.Speed)
                            stats.speed = sbf.statOverrides[i].intChangedValue;
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.Dash)
                            stats.dash = sbf.statOverrides[i].intChangedValue;
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.Defense)
                            stats.defense = sbf.statOverrides[i].intChangedValue;
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.FrayDamage)
                            stats.frayDamage = sbf.statOverrides[i].intChangedValue;
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.DamageDie)
                            stats.damageDie = sbf.statOverrides[i].intChangedValue;
                        else if (sbf.statOverrides[i].statType == StatOverride.StatType.DieAmount)
                            stats.dieAmount = sbf.statOverrides[i].intChangedValue;
                    }
                }

                if (int.TryParse(stats.HP, out int HPInt))
                {
                    intHPValue = HPInt;
                }
                else
                    stringHPValue = stats.HP;
            }

            //sub template overrides
            if(intHPValue < 0)
            {
                //string option
                if (subTemplate == 2 && type != FoeType.Elite) //elite
                    stats.HP = "(" + stringHPValue + ") x2";
                else if(subTemplate == 1 && type != FoeType.Elite) //mob
                    stats.HP = "1";
            }
            else
            {
                //int option
                if (subTemplate == 2 && type != FoeType.Elite) //elite
                    stats.HP = (intHPValue * 2).ToString();
                else if (subTemplate == 1 && type != FoeType.Elite) //mob
                    stats.HP = "1";
            }
        }

        return stats;
    }

    public override List<ClassData.Trait> GetTraits(bool filterThroughPhase = true)
    {
        //we build the trait list
        List<ClassData.Trait> traitComp = new List<ClassData.Trait>();

        if (type == FoeType.Mob || type == FoeType.SpecialSummon)
        {
            FoeData.FoeClass.FoeStats stats = type == FoeType.Mob ? UnitManager._instance.foes.mobs[classIndex].stats : UnitManager._instance.summons.specialSummons[classIndex].stats;
            if (stats.specialData.Length > 0)
            {
                ClassData.Trait spec = new ClassData.Trait();
                spec.traitName = "Special Traits";

                string classTraits = "";

                for (int i = 0; i < stats.specialData.Length; i++)
                {
                    classTraits += "\n\n<i>·" + stats.specialData[i] + "</i>";
                }

                traitComp.Add(spec);
            }

            AspectSet[] miniSet = type == FoeType.Mob ? UnitManager._instance.foes.mobs[classIndex].aspectSet : UnitManager._instance.summons.specialSummons[classIndex].aspectSet;

            for (int i = 0; i < miniSet.Length; i++)
            {
                for (int t = 0; t < miniSet[i].traits.Length; t++) {
                    ClassData.Trait traitCopy = new ClassData.Trait();
                    traitCopy.traitName = miniSet[i].traits[t].traitName;

                    string auxText = miniSet[i].traits[t].traitDescription;
                    string traitDescription = "";

                    traitDescription = MiscTools.GetLineJumpedForm(auxText);
                    traitCopy.traitDescription = traitDescription;

                    traitComp.Add(traitCopy);
                }
            }
        }
        else
        {
            //sub template
            if (subTemplate == 1 && type != FoeType.Mob)
            {
                ClassData.Trait stTrait = new Trait();
                stTrait.traitName = "Mob Template";
                stTrait.traitDescription = "\n·This character doesn’t trigger slay effects\n·1 point in an encounter budget gets 5 mobs of the same type</i>";
                stTrait.phaseIndex = 0;
                traitComp.Add(stTrait);
            }
            else if (subTemplate == 2 && type != FoeType.Elite)
            {
                ClassData.Trait stTrait = new Trait();
                stTrait.traitName = "Elite Template";
                stTrait.traitDescription = "\n·This foe takes 2 turns\n·The foe takes up 2 points in an encounter budget";
                stTrait.phaseIndex = 0;
                traitComp.Add(stTrait);
            }

            if (factionIndex < 0) //no faction, therefore basic job. get traits from the base form and elite legend sub job as normal
            {
                List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                if (type == FoeType.Elite)
                    classList = UnitManager._instance.foes.eliteClasses;
                else if (type == FoeType.Legend)
                    classList = UnitManager._instance.foes.legendClasses;

                FoeData.FoeClass classData = classList[classIndex];

                List<Trait> sumTraits = new List<Trait>();

                //general
                for (int i = 0; i < classData.generalAspects.traits.Length; i++)
                {
                    sumTraits.Add(classData.generalAspects.traits[i]);
                }

                //phases
                if (classData.phaseAspects.Length > 0)
                {
                    for(int i = 0; i < classData.phaseAspects.Length; i++)
                    {
                        if (filterThroughPhase && i != currentPhase)
                            continue;

                        for(int t = 0; t < classData.phaseAspects[i].traits.Length; t++)
                        {
                            sumTraits.Add(classData.phaseAspects[i].traits[t]);
                        }
                    }
                }

                //job
                if(classData.jobs.Count > 0) //if no job, the no job traits
                {
                    IconJob unitJob = classData.jobs[jobIndex];

                    //general
                    for (int i = 0; i < unitJob.generalAspects.traits.Length; i++)
                    {
                        sumTraits.Add(unitJob.generalAspects.traits[i]);
                    }

                    //phases
                    if (unitJob.phaseAspects.Length > 0)
                    {
                        for (int i = 0; i < unitJob.phaseAspects.Length; i++)
                        {
                            if (filterThroughPhase && i != currentPhase)
                                continue;

                            for (int t = 0; t < unitJob.phaseAspects[i].traits.Length; t++)
                            {
                                sumTraits.Add(unitJob.phaseAspects[i].traits[t]);
                            }
                        }
                    }
                }

                //template
                if (templateIndex >= 0) //if no template, then no additional traits
                {
                    FoeTemplate tData = UnitManager._instance.templates.templates[templateIndex];

                    for (int i = 0; i < tData.templateAspects.Length; i++)
                    {
                        for (int t = 0; t < tData.templateAspects[i].traits.Length; t++)
                        {
                            sumTraits.Add(tData.templateAspects[i].traits[t]);
                        }
                    }
                }

                traitComp.AddRange(sumTraits);
            }
            else
            {
                //faction unit. the still get traits from the base classes
                List<Trait> sumTraits = new List<Trait>();

                FactionData factionSet = UnitManager._instance.factions[factionIndex];

                int factionSetIndex = 0;
                if (factionIndex > 8) //folk faction has 6 sub sets
                {
                    factionSetIndex = factionIndex - 8;
                }
                FoeFaction factionData = factionSet.foeFactions[factionSetIndex];
                SubFaction sbf = new SubFaction();
                if (isDefaultFactionEntry)
                    sbf = factionData.defaults[subFactionIndex];
                else if (classIndex == 0)
                    sbf = factionData.heavies[subFactionIndex];
                else if (classIndex == 1)
                    sbf = factionData.skirmishers[subFactionIndex];
                else if (classIndex == 2)
                    sbf = factionData.leaders[subFactionIndex];
                else if (classIndex == 3)
                    sbf = factionData.artilleries[subFactionIndex];

                if (!sbf.isUnique)
                {
                    List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                    if (type == FoeType.Elite)
                        classList = UnitManager._instance.foes.eliteClasses;
                    else if (type == FoeType.Legend)
                        classList = UnitManager._instance.foes.legendClasses;

                    FoeData.FoeClass classData = classList[classIndex];

                    //general
                    for (int i = 0; i < classData.generalAspects.traits.Length; i++)
                    {
                        sumTraits.Add(classData.generalAspects.traits[i]);
                    }

                    //phases
                    if (classData.phaseAspects.Length > 0)
                    {
                        for (int i = 0; i < classData.phaseAspects.Length; i++)
                        {
                            if (filterThroughPhase && i != currentPhase)
                                continue;

                            for (int t = 0; t < classData.phaseAspects[i].traits.Length; t++)
                            {
                                sumTraits.Add(classData.phaseAspects[i].traits[t]);
                            }
                        }
                    }
                }

                //faction
                List<ClassData.Trait> factionTraits = GetFactionTraits();
                for(int t = 0; t < factionTraits.Count; t++)
                {
                    traitComp.Add(factionTraits[t]);
                }

                traitComp.AddRange(sumTraits);
            }
        }

        //now we clean duplicates, negatives, and update them
        List<ClassData.Trait> cleanComp = new List<ClassData.Trait>();
        List<ClassData.Trait> auxComp = new List<ClassData.Trait>(traitComp);

        while (auxComp.Count > 0) 
        {
            ClassData.Trait focusTrait = new ClassData.Trait();
            focusTrait.traitName = auxComp[0].traitName;
            focusTrait.traitDescription = auxComp[0].traitDescription;

            bool foundDup = false;
            for (int t = auxComp.Count - 1; t >= 0; t--) //check and update duplicates
            {
                if (auxComp[t].traitName.Equals(focusTrait.traitName, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!foundDup)
                    {
                        foundDup = true;
                        focusTrait.traitDescription = auxComp[t].traitDescription; //found last entry, therefore, the one we must consider
                    }

                    auxComp.RemoveAt(t); //remove duplicates
                }
            }

            cleanComp.Add(focusTrait);
        }

        //we check the negative ones
        auxComp = new List<Trait>(cleanComp);
        List<int> negs = new List<int>();
        for(int i = 0; i < auxComp.Count; i++)
        {
            if (auxComp[i].traitID < 0)
                negs.Add(auxComp[i].traitID);
        }

        cleanComp = new List<Trait>();
        for(int i = 0; i < auxComp.Count; i++)
        {
            bool delFlag = false;
            for (int n = 0; n < negs.Count; n++)
            {
                if(auxComp[i].traitID == -1 * negs[n])
                {
                    delFlag = true;
                    break;
                }
            }

            if (!delFlag)
                cleanComp.Add(auxComp[i]);
        }

        return cleanComp;
    }

    //automatically builds the attack set. only considers phase value if the type corresponds to a legend
    public override List<ClassData.Ability> GetAbilities(bool filterThroughPhase = true)
    {
        //build attack set here
        List<ClassData.Ability> attackComp = new List<ClassData.Ability>();

        if (type == FoeType.Mob || type == FoeType.SpecialSummon)
        {
            AspectSet[] miniSet = type == FoeType.Mob ? UnitManager._instance.foes.mobs[classIndex].aspectSet : UnitManager._instance.summons.specialSummons[classIndex].aspectSet;

            for (int i = 0; i < miniSet.Length; i++)
            {
                for (int t = 0; t < miniSet[i].abilities.Length; t++)
                {
                    ClassData.Ability abilityCopy = new ClassData.Ability();
                    abilityCopy.isAttack = miniSet[i].abilities[t].isAttack;
                    abilityCopy.abilityID = miniSet[i].abilities[t].abilityID;
                    abilityCopy.abilityName = miniSet[i].abilities[t].abilityName;
                    abilityCopy.docID = miniSet[i].abilities[t].docID;
                    abilityCopy.actionCost = miniSet[i].abilities[t].actionCost;
                    abilityCopy.abilityEffect = miniSet[i].abilities[t].abilityEffect; // MiscTools.GetLineJumpedForm(miniSet[i].abilities[t].abilityEffect);
                    abilityCopy.subCombos = miniSet[i].abilities[t].subCombos;

                    attackComp.Add(abilityCopy);
                }
            }
        }
        else
        {
            if (factionIndex < 0) //no faction, therefore basic job. get abilites from the base form and elite legend sub job as normal
            {
                List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                if (type == FoeType.Elite)
                    classList = UnitManager._instance.foes.eliteClasses;
                else if (type == FoeType.Legend)
                    classList = UnitManager._instance.foes.legendClasses;

                FoeData.FoeClass classData = classList[classIndex];

                List<Ability> sumAbilities = new List<Ability>();

                //general
                for (int i = 0; i < classData.generalAspects.abilities.Length; i++)
                {
                    sumAbilities.Add(classData.generalAspects.abilities[i]);
                }

                //phases
                if (classData.phaseAspects.Length > 0)
                {
                    for (int i = 0; i < classData.phaseAspects.Length; i++)
                    {
                        if (filterThroughPhase && i != currentPhase)
                            continue;

                        for (int t = 0; t < classData.phaseAspects[i].abilities.Length; t++)
                        {
                            sumAbilities.Add(classData.phaseAspects[i].abilities[t]);
                        }
                    }
                }

                //job
                if (classData.jobs.Count > 0) //if no job, the no job traits
                {
                    IconJob unitJob = classData.jobs[jobIndex];

                    //general
                    for (int i = 0; i < unitJob.generalAspects.abilities.Length; i++)
                    {
                        sumAbilities.Add(unitJob.generalAspects.abilities[i]);
                    }

                    //phases
                    if (unitJob.phaseAspects.Length > 0)
                    {
                        for (int i = 0; i < unitJob.phaseAspects.Length; i++)
                        {
                            if (filterThroughPhase && i != currentPhase)
                                continue;

                            for (int t = 0; t < unitJob.phaseAspects[i].abilities.Length; t++)
                            {
                                sumAbilities.Add(unitJob.phaseAspects[i].abilities[t]);
                            }
                        }
                    }
                }

                //template
                if (templateIndex >= 0) //if no template, then no additional abilities
                {
                    FoeTemplate tData = UnitManager._instance.templates.templates[templateIndex];

                    for (int i = 0; i < tData.templateAspects.Length; i++)
                    {
                        for (int t = 0; t < tData.templateAspects[i].abilities.Length; t++)
                        {
                            sumAbilities.Add(tData.templateAspects[i].abilities[t]);
                        }
                    }
                }

                attackComp.AddRange(sumAbilities);
            }
            else
            {
                List<Ability> sumAbilities = new List<Ability>();

                FactionData factionSet = UnitManager._instance.factions[factionIndex];

                int factionSetIndex = 0;
                if (factionIndex > 8) //folk faction has 6 sub sets
                {
                    factionSetIndex = factionIndex - 8;
                }
                FoeFaction factionData = factionSet.foeFactions[factionSetIndex];
                SubFaction sbf = new SubFaction();
                if (isDefaultFactionEntry)
                    sbf = factionData.defaults[subFactionIndex];
                else if (classIndex == 0)
                    sbf = factionData.heavies[subFactionIndex];
                else if (classIndex == 1)
                    sbf = factionData.skirmishers[subFactionIndex];
                else if (classIndex == 2)
                    sbf = factionData.leaders[subFactionIndex];
                else if (classIndex == 3)
                    sbf = factionData.artilleries[subFactionIndex];

                //faction unit. they still get abilities from the base classes unless they're unique
                if (!sbf.isUnique)
                {
                    List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                    if (type == FoeType.Elite)
                        classList = UnitManager._instance.foes.eliteClasses;
                    else if (type == FoeType.Legend)
                        classList = UnitManager._instance.foes.legendClasses;

                    FoeData.FoeClass classData = classList[classIndex];

                    //general
                    for (int i = 0; i < classData.generalAspects.abilities.Length; i++)
                    {
                        sumAbilities.Add(classData.generalAspects.abilities[i]);
                    }

                    //phases
                    if (classData.phaseAspects.Length > 0)
                    {
                        for (int i = 0; i < classData.phaseAspects.Length; i++)
                        {
                            if (filterThroughPhase && i != currentPhase)
                                continue;

                            for (int t = 0; t < classData.phaseAspects[i].abilities.Length; t++)
                            {
                                sumAbilities.Add(classData.phaseAspects[i].abilities[t]);
                            }
                        }
                    }
                }

                //faction
                List<ClassData.Ability> factionAbs = GetFactionAbilities(filterThroughPhase);
                for (int t = 0; t < factionAbs.Count; t++)
                {
                    attackComp.Add(factionAbs[t]);
                }

                attackComp.AddRange(sumAbilities);
            }
        }

        //now we clean duplicates, negatives, and update them
        List<ClassData.Ability> cleanComp = new List<ClassData.Ability>();
        List<ClassData.Ability> auxComp = new List<ClassData.Ability>(attackComp);

        while (auxComp.Count > 0)
        {
            ClassData.Ability focusAbility = auxComp[0];

            bool foundDup = false;
            for (int t = auxComp.Count - 1; t >= 0; t--) //check and update duplicates
            {
                if (auxComp[t].abilityName.Equals(focusAbility.abilityName, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!foundDup)
                    {
                        foundDup = true;
                        focusAbility = auxComp[t]; //found last entry, therefore, the one we must consider
                    }

                    auxComp.RemoveAt(t); //remove duplicates
                }
            }

            cleanComp.Add(focusAbility);
        }

        //we check the negative ones
        auxComp = new List<Ability>(cleanComp);
        List<int> negs = new List<int>();
        for (int i = 0; i < auxComp.Count; i++)
        {
            if (auxComp[i].abilityID < 0)
                negs.Add(auxComp[i].abilityID);
        }

        cleanComp = new List<Ability>();
        for (int i = 0; i < auxComp.Count; i++)
        {
            bool delFlag = false;
            for (int n = 0; n < negs.Count; n++)
            {
                if (auxComp[i].abilityID == -1 * negs[n])
                {
                    delFlag = true;
                    break;
                }
            }

            if (!delFlag)
                cleanComp.Add(auxComp[i]);
        }

        //we order through action cost
        cleanComp = ArrangeThroughActionCost(cleanComp);

        //finally, we add the combo information to the actions
        cleanComp = AppendComboAbilities(cleanComp);

        /*
        string deb = "Final:\n";
        for (int i = 0; i < cleanComp.Count; i++)
        {
            deb += cleanComp[i].abilityName + "\n";
        }
        */

        return cleanComp;
    }

    public void ChangePhase(int value, bool update = true)
    {
        currentPhase = value;

        if (update)
        {
            traits = GetTraits();
            attacks = GetAbilities();
        }
    }

    public FoeFaction GetFaction()
    {
        //in faction cases, if the faction is a unique then the base class traits need to be ignored. otherwise they need to be incorporated into the list, considering the chapter too
        FactionData factionSet = UnitManager._instance.factions[factionIndex];

        int factionSetIndex = 0;
        if (factionIndex > 8) //folk faction has 6 sub sets
        {
            factionSetIndex = factionIndex - 8;
        }
        FoeFaction factionData = factionSet.foeFactions[factionSetIndex];

        return factionData;
    }

    public SubFaction GetFactionJob()
    {
        FoeFaction factionData = GetFaction();

        if (subFactionIndex < 0)
            return null;

        SubFaction sbf = new SubFaction();
        if (isDefaultFactionEntry)
            sbf = factionData.defaults[subFactionIndex];
        else if (classIndex == 0)
            sbf = factionData.heavies[subFactionIndex];
        else if (classIndex == 1)
            sbf = factionData.skirmishers[subFactionIndex];
        else if (classIndex == 2)
            sbf = factionData.leaders[subFactionIndex];
        else if (classIndex == 3)
            sbf = factionData.artilleries[subFactionIndex];

        return sbf;
    }

    public List<Trait> GetFactionTraits(bool filterThroughPhase = true)
    {
        List<Trait> traitComp = new List<Trait>();

        FoeFaction factionData = GetFaction();

        //we first add the faction's general traits
        for (int i = 0; i < factionData.generalAspects.traits.Length; i++)
        {
            traitComp.Add(factionData.generalAspects.traits[i]);
        }

        //then we add the faction's phase sets
        for (int p = 0; p < factionData.phaseSets.Length; p++)
        {
            for(int t  = 0; t < factionData.phaseSets[p].traits.Length; t++)
            {
                if (filterThroughPhase && p != currentPhase)
                    continue;

                traitComp.Add(factionData.phaseSets[p].traits[t]);
            }
        }

        SubFaction sbf = GetFactionJob();

        //we add the traits of the specified faction job. legendary traits will be taken account by checking the currentphase
        //then we add the faction's phase sets
        for (int p = 0; p < sbf.phaseSet.Length; p++)
        {
            for (int t = 0; t < sbf.phaseSet[p].traits.Length; t++)
            {
                if (filterThroughPhase && p != currentPhase)
                    continue;

                traitComp.Add(sbf.phaseSet[p].traits[t]);
            }
        }


        //we add additional traits depending on chapter status
        for (int c = 0; c < sbf.chapterAspects.Length; c++)
        {
            if (sbf.chapterAspects[c].chapterLock == level)
            {
                for (int i = 0; i < sbf.chapterAspects[c].traits.Length; i++)
                {
                    traitComp.Add(sbf.chapterAspects[c].traits[i]);
                }
            }
        }

        //finally, we add any possible special sets
        for (int s = 0; s < sbf.specialSets.Length; s++)
        {
            for (int t = 0; t < sbf.specialSets[s].traits.Length; t++)
            {
                traitComp.Add(sbf.specialSets[s].traits[t]);
            }
        }

        return traitComp;
    }

    public int HasTraitSize()
    {
        List<Trait> foeTraits = traits;
        if (foeTraits == null || foeTraits.Count == 0)
            foeTraits = GetTraits();

        if (foeTraits == null || foeTraits.Count == 0)
            return 1;

        for(int i = 0; i < foeTraits.Count; i++)
        {
            if (foeTraits[i].traitName.Equals("Regular", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (foeTraits[i].traitName.Equals("Big", StringComparison.OrdinalIgnoreCase))
                return 2;
            if (foeTraits[i].traitName.Equals("Giant", StringComparison.OrdinalIgnoreCase))
                return 3;
        }

        return 1;
    }

    public List<Ability> GetFactionAbilities(bool filterThroughPhase = true)
    {
        List<Ability> abilityComp = new List<Ability>();

        //in faction cases, if the faction is a unique then the base class abilites need to be ignored. otherwise they need to be incorporated into the list, considering the chapter too
        FoeFaction factionData = GetFaction();

        //we first add the faction's general traits
        for (int i = 0; i < factionData.generalAspects.abilities.Length; i++)
        {
            abilityComp.Add(factionData.generalAspects.abilities[i]);
        }

        //then we add the faction's phase sets
        for (int p = 0; p < factionData.phaseSets.Length; p++)
        {
            for (int t = 0; t < factionData.phaseSets[p].abilities.Length; t++)
            {
                if (filterThroughPhase && p != currentPhase)
                    continue;

                abilityComp.Add(factionData.phaseSets[p].abilities[t]);
            }
        }

        SubFaction sbf = GetFactionJob();

        //we add the abilities of the specified faction job. legendary abilities will be taken account by checking the currentphase
        //then we add the faction's phase sets
        for (int p = 0; p < sbf.phaseSet.Length; p++)
        {
            for (int t = 0; t < sbf.phaseSet[p].abilities.Length; t++)
            {
                if (filterThroughPhase && p != currentPhase)
                    continue;

                abilityComp.Add(sbf.phaseSet[p].abilities[t]);
            }
        }


        //we add additional abilities depending on chapter status
        for (int c = 0; c < sbf.chapterAspects.Length; c++)
        {
            if (sbf.chapterAspects[c].chapterLock == level)
            {
                for (int i = 0; i < sbf.chapterAspects[c].abilities.Length; i++)
                {
                    abilityComp.Add(sbf.chapterAspects[c].abilities[i]);
                }
            }
        }

        //finally, we add any possible special sets
        for (int s = 0; s < sbf.specialSets.Length; s++)
        {
            for (int t = 0; t < sbf.specialSets[s].abilities.Length; t++)
            {
                abilityComp.Add(sbf.specialSets[s].abilities[t]);
            }
        }

        return abilityComp;
    }

    public List<Ability> ArrangeThroughActionCost(List<Ability> inputList)
    {
        List<Ability> orderedList = new List<Ability>();

        List<Ability> roundActions = new List<Ability>();
        List<Ability> freeActions = new List<Ability>();
        List<Ability> numActions = new List<Ability>();

        for(int i = 0; i < inputList.Count; i++)
        {
            if (inputList[i].actionCost.Equals("Round", StringComparison.OrdinalIgnoreCase))
                roundActions.Add(inputList[i]);

            else if (inputList[i].actionCost.Equals("free", StringComparison.OrdinalIgnoreCase))
                freeActions.Add(inputList[i]);

            else
                numActions.Add(inputList[i]);
        }

        orderedList.AddRange(roundActions);
        orderedList.AddRange(freeActions);
        orderedList.AddRange(numActions);

        return orderedList;
    }

    public List<Ability> AppendComboAbilities(List<Ability> inputList)
    {
        //we find the combo abilities if any. said abilities must be copied so the combo depth info is not lost or overwritten
        List<Ability> comboList = new List<Ability>();

        for(int i = 0; i < inputList.Count; i++)
        {
            comboList.Add(inputList[i]);
            if (inputList[i].subCombos.Length > 0)
            {
                for(int a = 0; a < inputList[i].subCombos.Length; a++)
                {
                    int abilityID = inputList[i].subCombos[a];

                    int depthCount = 1;

                    while (true)
                    {
                        Ability comboAb = PieceManager._instance.PieceDisplayPanel.FindAbility(inputList[i].docID, abilityID);
                        comboAb.abilityComboDepth = depthCount;

                        comboList.Add(comboAb);
                        if (comboAb.subCombos.Length == 0)
                            break;
                        else
                        {
                            abilityID = comboAb.subCombos[0];
                            depthCount++;
                        }
                    }
                }
            } 
        }

        return comboList;
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
