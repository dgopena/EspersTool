using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status File", menuName = "ScriptableObjects/Status Data")]
public class StatusData : ScriptableObject
{
    public ImageBlight[] displayBlights;

    [System.Serializable]
    public struct ImageBlight
    {
        public Sprite image;
        public IconUnit.Blight blight;
        public string blightDescription;
    }

    public ImageStatus[] displayStatus;

    [System.Serializable]
    public struct ImageStatus
    {
        public Sprite image;
        public IconUnit.Status status;
        public string statusDescription;
    }

    public ImageEffect[] displayEffects;


    [System.Serializable]
    public struct ImageEffect
    {
        public Sprite image;
        public IconUnit.PositiveEffects effect;
        public string effectDescription;
    }
}
