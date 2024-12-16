using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability File", menuName = "ScriptableObjects/Ability Data")]
public class AbilityData : ScriptableObject
{
    public List<MonsterAbility> abilities;

    [System.Serializable]
    public struct MonsterAbility
    {
        public string name;
        public string description;
    }
}
