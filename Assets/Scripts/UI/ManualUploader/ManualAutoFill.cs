using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManualAutoFill : MonoBehaviour
{
    public bool factionFilling = false;

    public FoeData foeData;
    public FactionData factionData;

    [SerializeField] private TextMeshProUGUI label;
    [Header("Data")]
    [SerializeField] private TraitEntry[] traitsBase;
    [SerializeField] private AbilityEntry[] abilityBase;

    private void Awake()
    {
        if (!factionFilling)
            label.text = foeData.name;
        else
            label.text = factionData.name;
    }

    public void AutoFill()
    {
        if (!factionFilling)
        {
            for (int i = 0; i < foeData.classes.Count; i++) {
                FillFoeClass(foeData.classes[i]);
            }

            for (int i = 0; i < foeData.eliteClasses.Count; i++)
            {
                FillFoeClass(foeData.eliteClasses[i]);
            }

            for (int i = 0; i < foeData.legendClasses.Count; i++)
            {
                FillFoeClass(foeData.legendClasses[i]);
            }

            for(int i=0; i < foeData.mobs.Length; i++)
            {
                FillMob(foeData.mobs[i]);
            }

            for(int i = 0; i < foeData.specialSummons.Length; i++)
            {
                FillMob(foeData.specialSummons[i]);
            }
        }
        else
        {
            for(int i = 0; i < factionData.foeFactions.Length; i++)
            {
                FillFaction(factionData.foeFactions[i]);
            }
        }
    }

    private void FillFoeClass(FoeData.FoeClass entry)
    {
        //general

        //traits general
        for(int i = 0; i < entry.generalAspects.traits.Length; i++)
        {
            ClassData.Trait t = entry.generalAspects.traits[i];

            TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

            t.traitName = foundT.traitName;
            t.traitDescription = foundT.traitEffect;

            entry.generalAspects.traits[i] = t;
        }

        //abilities general
        for(int i = 0; i < entry.generalAspects.abilities.Length; i++)
        {
            ClassData.Ability a = entry.generalAspects.abilities[i];

            AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

            a.abilityName = foundA.abilityName;
            a.isAttack = foundA.isAttack;
            a.actionCost = foundA.actionCost;
            a.abilityAspects = foundA.additionals;
            a.subCombos = foundA.subCombos;
            a.abilityEffect = foundA.effect;

            entry.generalAspects.abilities[i] = a;
        }

        //phase

        //traits phase
        for (int j = 0; j < entry.phaseAspects.Length; j++)
        {
            for (int i = 0; i < entry.phaseAspects[j].traits.Length; i++)
            {
                ClassData.Trait t = entry.phaseAspects[j].traits[i];

                TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                t.traitName = foundT.traitName;
                t.traitDescription = foundT.traitEffect;

                entry.phaseAspects[j].traits[i] = t;
            }
        }

        //abilities phase
        for (int j = 0; j < entry.phaseAspects.Length; j++)
        {
            for (int i = 0; i < entry.phaseAspects[j].abilities.Length; i++)
            {
                ClassData.Ability a = entry.phaseAspects[j].abilities[i];

                AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                a.abilityName = foundA.abilityName;
                a.isAttack = foundA.isAttack;
                a.actionCost = foundA.actionCost;
                a.abilityAspects = foundA.additionals;
                a.subCombos = foundA.subCombos;
                a.abilityEffect = foundA.effect;

                entry.phaseAspects[j].abilities[i] = a;
            }
        }

        //--------------------JOBS

        //general

        //traits general jobs
        for (int j = 0; j < entry.jobs.Count; j++)
        {
            for (int i = 0; i < entry.jobs[j].generalAspects.traits.Length; i++)
            {
                ClassData.Trait t = entry.jobs[j].generalAspects.traits[i];

                TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                t.traitName = foundT.traitName;
                t.traitDescription = foundT.traitEffect;

                entry.jobs[j].generalAspects.traits[i] = t;
            }
        }

        //abilities general jobs
        for (int j = 0; j < entry.jobs.Count; j++)
        {
            for (int i = 0; i < entry.jobs[j].generalAspects.abilities.Length; i++)
            {
                ClassData.Ability a = entry.jobs[j].generalAspects.abilities[i];

                AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                a.abilityName = foundA.abilityName;
                a.isAttack = foundA.isAttack;
                a.actionCost = foundA.actionCost;
                a.abilityAspects = foundA.additionals;
                a.subCombos = foundA.subCombos;
                a.abilityEffect = foundA.effect;

                entry.jobs[j].generalAspects.abilities[i] = a;
            }
        }

        //phase

        //traits phase jobs
        for (int j = 0; j < entry.jobs.Count; j++)
        {
            for (int p = 0; p < entry.jobs[j].phaseAspects.Length; p++)
            {
                for (int i = 0; i < entry.jobs[j].phaseAspects[p].abilities.Length; i++)
                {
                    ClassData.Trait t = entry.jobs[j].phaseAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.jobs[j].phaseAspects[p].traits[i] = t;
                }
            }
        }

        //abilities phase jobs
        for (int j = 0; j < entry.jobs.Count; j++)
        {
            for (int p = 0; p < entry.jobs[j].phaseAspects.Length; p++)
            {
                for (int i = 0; i < entry.jobs[j].phaseAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.jobs[j].phaseAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.jobs[j].phaseAspects[p].abilities[i] = a;
                }
            }
        }
    }

    private void FillMob(Mob entry)
    {
        for (int a = 0; a < entry.aspectSet.Length; a++)
        {
            //traits
            for (int i = 0; i < entry.aspectSet[a].traits.Length; i++)
            {
                ClassData.Trait t = entry.aspectSet[a].traits[i];

                TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                t.traitName = foundT.traitName;
                t.traitDescription = foundT.traitEffect;

                entry.aspectSet[a].traits[i] = t;
            }

            //abilities
            for (int i = 0; i < entry.aspectSet[a].abilities.Length; i++)
            {
                ClassData.Ability ab = entry.aspectSet[a].abilities[i];

                if (ab.docID < 0)
                    break;

                AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[ab.docID].abilities, ab.abilityID);

                ab.abilityName = foundA.abilityName;
                ab.isAttack = foundA.isAttack;
                ab.actionCost = foundA.actionCost;
                ab.abilityAspects = foundA.additionals;
                ab.subCombos = foundA.subCombos;
                ab.abilityEffect = foundA.effect;

                entry.aspectSet[a].abilities[i] = ab;
            }
        }
    }

    private void FillFaction(FoeFaction entry)
    {
        //general traits
        for (int i = 0; i < entry.generalAspects.traits.Length; i++)
        {
            ClassData.Trait t = entry.generalAspects.traits[i];

            TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

            t.traitName = foundT.traitName;
            t.traitDescription = foundT.traitEffect;

            entry.generalAspects.traits[i] = t;
        }

        //abilities general
        for (int i = 0; i < entry.generalAspects.abilities.Length; i++)
        {
            ClassData.Ability a = entry.generalAspects.abilities[i];

            AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

            a.abilityName = foundA.abilityName;
            a.isAttack = foundA.isAttack;
            a.actionCost = foundA.actionCost;
            a.abilityAspects = foundA.additionals;
            a.subCombos = foundA.subCombos;
            a.abilityEffect = foundA.effect;

            entry.generalAspects.abilities[i] = a;
        }

        //---------types

        //defaults
        for (int d = 0; d < entry.defaults.Length; d++)
        {
            //phase sets
            for(int p = 0; p < entry.defaults[d].phaseSet.Length; p++)
            {
                //traits
                for(int i = 0; i < entry.defaults[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.defaults[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.defaults[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.defaults[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.defaults[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.defaults[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.defaults[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.defaults[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.defaults[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.defaults[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.defaults[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.defaults[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.defaults[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.defaults[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.defaults[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.defaults[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.defaults[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.defaults[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.defaults[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.defaults[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }

        //heavies
        for (int d = 0; d < entry.heavies.Length; d++)
        {
            //phase sets
            for (int p = 0; p < entry.heavies[d].phaseSet.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.heavies[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.heavies[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.heavies[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.heavies[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.heavies[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.heavies[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.heavies[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.heavies[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.heavies[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.heavies[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.heavies[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.heavies[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.heavies[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.heavies[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.heavies[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.heavies[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.heavies[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.heavies[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.heavies[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.heavies[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }

        //skirmishers
        for (int d = 0; d < entry.skirmishers.Length; d++)
        {
            //phase sets
            for (int p = 0; p < entry.skirmishers[d].phaseSet.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.skirmishers[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.skirmishers[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.skirmishers[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.skirmishers[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.skirmishers[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.skirmishers[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.skirmishers[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.skirmishers[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.skirmishers[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.skirmishers[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.skirmishers[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.skirmishers[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.skirmishers[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.skirmishers[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.skirmishers[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.skirmishers[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.skirmishers[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.skirmishers[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.skirmishers[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.skirmishers[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }

        //leaders
        for (int d = 0; d < entry.leaders.Length; d++)
        {
            //phase sets
            for (int p = 0; p < entry.leaders[d].phaseSet.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.leaders[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.leaders[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.leaders[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.leaders[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.leaders[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.leaders[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.leaders[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.leaders[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.leaders[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.leaders[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.leaders[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.leaders[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.leaders[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.leaders[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.leaders[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.leaders[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.leaders[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.leaders[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.leaders[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.leaders[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }

        //artilleries
        for (int d = 0; d < entry.artilleries.Length; d++)
        {
            //phase sets
            for (int p = 0; p < entry.artilleries[d].phaseSet.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.artilleries[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.artilleries[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.artilleries[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.artilleries[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.artilleries[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.artilleries[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.artilleries[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.artilleries[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.artilleries[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.artilleries[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.artilleries[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.artilleries[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.artilleries[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.artilleries[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.artilleries[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.artilleries[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.artilleries[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.artilleries[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.artilleries[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.artilleries[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }

        //special
        for (int d = 0; d < entry.special.Length; d++)
        {
            //phase sets
            for (int p = 0; p < entry.special[d].phaseSet.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.special[d].phaseSet[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.special[d].phaseSet[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.special[d].phaseSet[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.special[d].phaseSet[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.special[d].phaseSet[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.special[d].phaseSet[p].abilities[i] = a;
                }
            }

            //special sets
            for (int p = 0; p < entry.special[d].specialSets.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.special[d].specialSets[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.special[d].specialSets[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.special[d].specialSets[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.special[d].specialSets[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.special[d].specialSets[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.special[d].specialSets[p].abilities[i] = a;
                }
            }

            //chapter aspects
            for (int p = 0; p < entry.special[d].chapterAspects.Length; p++)
            {
                //traits
                for (int i = 0; i < entry.special[d].chapterAspects[p].traits.Length; i++)
                {
                    ClassData.Trait t = entry.special[d].chapterAspects[p].traits[i];

                    TraitEntry.Trait foundT = FindTraitWithID(traitsBase[t.docID].Traits, t.traitID);

                    t.traitName = foundT.traitName;
                    t.traitDescription = foundT.traitEffect;

                    entry.special[d].chapterAspects[p].traits[i] = t;
                }

                //abilities
                for (int i = 0; i < entry.special[d].chapterAspects[p].abilities.Length; i++)
                {
                    ClassData.Ability a = entry.special[d].chapterAspects[p].abilities[i];

                    AbilityEntry.FoeAbility foundA = FindAbilityWithID(abilityBase[a.docID].abilities, a.abilityID);

                    a.abilityName = foundA.abilityName;
                    a.isAttack = foundA.isAttack;
                    a.actionCost = foundA.actionCost;
                    a.abilityAspects = foundA.additionals;
                    a.subCombos = foundA.subCombos;
                    a.abilityEffect = foundA.effect;

                    entry.special[d].chapterAspects[p].abilities[i] = a;
                }
            }
        }
    }

    private TraitEntry.Trait FindTraitWithID(TraitEntry.Trait[] coll, int ID)
    {
        for(int i = 0; i < coll.Length; i++)
        {
            if (coll[i].traitIDInList == ID)
                return coll[i];
        }

        return new TraitEntry.Trait();
    }

    private AbilityEntry.FoeAbility FindAbilityWithID(AbilityEntry.FoeAbility[] coll, int ID)
    {
        for (int i = 0; i < coll.Length; i++)
        {
            if (coll[i].abilityIDInList == ID)
                return coll[i];
        }

        return new AbilityEntry.FoeAbility();
    }


    private void FillFaction()
    {

    }
}
