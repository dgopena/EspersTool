using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Bonds File", menuName = "ScriptableObjects/Bonds Data")]
public class BondsData : ScriptableObject
{
    public List<Bond> bonds;

    [System.Serializable]
    public struct Bond
    {
        public string name;
        public CultureData.SimpleActionModifier[] modifier;
        public string[] ideals;
        [Space(20f)]
        public int effort;
        public string secondWind;
        public string stressSpecial;
        public int strain;
    }
}
