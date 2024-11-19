using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trait File", menuName = "ScriptableObjects/Trait Data")]
public class TraitEntry : ScriptableObject
{
    public Trait[] Traits;

    [System.Serializable]
    public struct Trait
    {
        public string traitName;
        public string traitEffect;
        public int traitIDInList;
    }
}
