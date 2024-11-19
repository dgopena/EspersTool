using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction File", menuName = "ScriptableObjects/Faction Data")]
public class FactionData : ScriptableObject
{
    public string globalFactionName;

    public FoeFaction[] foeFactions;
}
