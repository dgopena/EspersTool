using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Culture File", menuName = "ScriptableObjects/Culture Data")]
public class CultureData : ScriptableObject
{
    public List<CultureAspect> cultures;

    [System.Serializable]
    public struct CultureAspect
    {
        public string name;
        public string description;
        public string[] values;
        public SimpleActionModifier[] modifiers;
    }

    [System.Serializable]
    public struct SimpleActionModifier
    {
        public NarrativeAction targetAction;
        public int modifier;
    }
}


