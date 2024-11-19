using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Foe Template File", menuName = "ScriptableObjects/Foe Template Data")]
public class TemplateData : ScriptableObject
{
    public FoeTemplate[] templates;

    public FoeTemplate[] subTemplates;
}

[System.Serializable]
public class FoeTemplate
{
    public string templateName;
    public StatOverride[] statOverrides;
    public ClassData.Trait[] templateTraits;
    public ClassData.Ability[] templateAttacks;

    public AspectSet[] templateAspects;
}
