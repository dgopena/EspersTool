using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeChara : MonoBehaviour
{
    [System.Serializable]
    public struct ActionModifier
    {
        public NarrativeAction targetAction;
        public int dotModifier;
        public int cultureModifier;
        public int bondModifier;
    }

    public int cultureIndex { get; private set; }
    public int bondIndex { get; private set; }
    public int startActionChoiceIndex { get; private set; }

    public List<ActionModifier> actionValues;
    private bool valuesInitialized = false;

    private void InitializeActionValues()
    {
        actionValues = new List<ActionModifier>();

        for(int i = 0; i < 10; i++)
        {
            ActionModifier nuVal = new ActionModifier();
            nuVal.targetAction = (NarrativeAction)i;
            nuVal.bondModifier = 0;
            nuVal.cultureModifier = 0;
            nuVal.dotModifier = 0;

            actionValues.Add(nuVal);
        }

        valuesInitialized = true;
    }

    public void UpdateBaseActionValues()
    {
        for (int i = 0; i < actionValues.Count; i++)
        {
            ActionModifier aModded = actionValues[i];
            aModded.cultureModifier = 0;
            aModded.bondModifier = 0;
            actionValues[i] = aModded;
        }

        CultureData.CultureAspect asp = UnitManager._instance.cultures.cultures[cultureIndex];
        for (int i = 0; i < asp.modifiers.Length; i++)
        {
            CultureData.SimpleActionModifier cMod = asp.modifiers[i];

            int cModIndex = (int)cMod.targetAction;
            ActionModifier cModded = actionValues[cModIndex];
            cModded.cultureModifier = cMod.modifier;
            actionValues[cModIndex] = cModded;
        }

        BondsData.Bond bond = UnitManager._instance.bonds.bonds[bondIndex];
        CultureData.SimpleActionModifier mod = bond.modifier[startActionChoiceIndex];
        int modIndex = (int)mod.targetAction;
        ActionModifier modded = actionValues[modIndex];
        modded.bondModifier = mod.modifier;
        actionValues[modIndex] = modded;
    }

    public void SetCulture(int index)
    {
        if (!valuesInitialized)
            InitializeActionValues();

        for (int i = 0; i < actionValues.Count; i++)
        {
            ActionModifier modded = actionValues[i];
            modded.cultureModifier = 0;
            actionValues[i] = modded;
        }

        cultureIndex = index;

        CultureData.CultureAspect asp = UnitManager._instance.cultures.cultures[index];
        for(int i = 0; i < asp.modifiers.Length; i++)
        {
            CultureData.SimpleActionModifier mod = asp.modifiers[i];

            int modIndex = (int)mod.targetAction;
            ActionModifier modded = actionValues[modIndex];
            modded.cultureModifier = mod.modifier;
            actionValues[modIndex] = modded;
        }
    }

    public void SetBond(int index)
    {
        if (!valuesInitialized)
            InitializeActionValues();

        for (int i = 0; i < actionValues.Count; i++)
        {
            ActionModifier bModded = actionValues[i];
            bModded.bondModifier = 0;
            actionValues[i] = bModded;
        }

        bondIndex = index;

        BondsData.Bond bond = UnitManager._instance.bonds.bonds[bondIndex];
        bond = UnitManager._instance.bonds.bonds[index];
        CultureData.SimpleActionModifier mod = bond.modifier[startActionChoiceIndex];
        int modIndex = (int)mod.targetAction;
        ActionModifier modded = actionValues[modIndex];
        modded.bondModifier = mod.modifier;
        actionValues[modIndex] = modded;
    }

    public void SetStartActionIndex(int index)
    {
        startActionChoiceIndex = index;
    }

    public void CleanDotModifiers()
    {
        if (!valuesInitialized)
            InitializeActionValues();

        else
        {
            for(int i = 0; i < actionValues.Count; i++)
            {
                ActionModifier mod = actionValues[i];
                mod.dotModifier = 0;
                actionValues[i] = mod;
            }
        }
    }

    public void ChangeDotModifier(int index, int value)
    {
        ActionModifier auxMod = actionValues[index];
        auxMod.dotModifier = value;
        actionValues[index] = auxMod;
    }
}

public enum NarrativeAction
{
    Sneak,
    Excel,
    Sense,
    Charm,
    Command,
    Tinker,
    Study,
    Traverse,
    Smash,
    Endure
}
