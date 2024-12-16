using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill File", menuName = "ScriptableObjects/Skill Data")]
public class SkillsData : ScriptableObject
{
    public List<MagicArt> magicArts;

    [System.Serializable]
    public struct MagicSkill
    {
        public string skillName;
        public int skillID;
        public string description;
        public string damage;
        public DamageTyping damageElement;
        public string effect;
    }

    public enum DamageTyping
    {
        None,
        Physical,
        Magic,
        Fire,
        Ice,
        Lightning,
        Earth,
        Light,
        Dark
    }

    [System.Serializable]
    public struct MagicArt
    {
        public string artName;

        public List<MagicSkill> noviceSkills;
        public List<MagicSkill> adeptSkills;
        public List<MagicSkill> masterSkills;
        public List<MagicSkill> grandMaster;
    }

    public MagicSkill GetSkillWithID(int skillID)
    {
        for(int m = 0; m < magicArts.Count; m++)
        {
            for(int s = 0; s < magicArts[m].noviceSkills.Count; s++)
            {
                if (magicArts[m].noviceSkills[s].skillID == skillID)
                    return magicArts[m].noviceSkills[s];
            }
            for (int s = 0; s < magicArts[m].adeptSkills.Count; s++)
            {
                if (magicArts[m].adeptSkills[s].skillID == skillID)
                    return magicArts[m].adeptSkills[s];
            }
            for (int s = 0; s < magicArts[m].masterSkills.Count; s++)
            {
                if (magicArts[m].masterSkills[s].skillID == skillID)
                    return magicArts[m].masterSkills[s];
            }
            for (int s = 0; s < magicArts[m].grandMaster.Count; s++)
            {
                if (magicArts[m].grandMaster[s].skillID == skillID)
                    return magicArts[m].grandMaster[s];
            }
        }

        return new MagicSkill();
    }
}
