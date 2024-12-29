using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Foe Presets File", menuName = "ScriptableObjects/Foe Presets")]
public class FoePresets : ScriptableObject
{
    public List<FoePreset> presets;

    public List<FoeAbility> abilites;
    
    [System.Serializable]
    public struct FoePreset
    {
        public string presetName;

        public string foeDescription;
        public int[] abilityIds;

        public string ATKMod;
        
        public int STRStat;
        public int INTStat;
        public int DEXStat;
        public int CHAStat;
        public int HPStat;
        public int DEFStat;
    }

    [System.Serializable]
    public struct FoeAbility
    {
        public string abilityName;
        public string abilityDescription;
    }
}
