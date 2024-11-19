using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability File", menuName = "ScriptableObjects/Ability Data")]
public class AbilityEntry : ScriptableObject
{
    public FoeAbility[] abilities;

    [System.Serializable]
    public struct FoeAbility
    {
        public string abilityName;
        public string actionCost;
        public bool isAttack;
        public string[] additionals;
        public int[] subCombos;
        public string effect;
        public int abilityIDInList;
    }
}
