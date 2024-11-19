using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestEffectListBackForth();
    }

    private void TestBlightListBackForth()
    {
        List<IconUnit.Blight> blights = new List<IconUnit.Blight>();
        blights.Add(IconUnit.Blight.Burning);
        blights.Add(IconUnit.Blight.Poisoned);
        blights.Add(IconUnit.Blight.Electrified);
        blights.Add(IconUnit.Blight.Burning);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(blights);

        Debug.Log(str);

        List<IconUnit.Blight> bl = pf.PassToBlightList(str);

        str = "";
        for(int i = 0; i < bl.Count; i++)
        {
            str += bl[i].ToString() + "-";
        }
        Debug.Log(str);
    }

    private void TestStatusListBackForth()
    {
        List<IconUnit.Status> status = new List<IconUnit.Status>();
        status.Add(IconUnit.Status.Blind);
        status.Add(IconUnit.Status.Incapacitated);
        status.Add(IconUnit.Status.Shattered);
        status.Add(IconUnit.Status.Hatred);
        status.Add(IconUnit.Status.Hatred);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(status);

        Debug.Log(str);

        List<IconUnit.Status> st = pf.PassToStatusList(str);

        str = "";
        for (int i = 0; i < st.Count; i++)
        {
            str += st[i].ToString() + "-";
        }
        Debug.Log(str);
    }

    private void TestEffectListBackForth()
    {
        List<IconUnit.PositiveEffects> effect = new List<IconUnit.PositiveEffects>();
        effect.Add(IconUnit.PositiveEffects.Dodge);
        effect.Add(IconUnit.PositiveEffects.Flying);
        effect.Add(IconUnit.PositiveEffects.Flying);
        effect.Add(IconUnit.PositiveEffects.Skirmisher);
        effect.Add(IconUnit.PositiveEffects.Unstoppable);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(effect);

        Debug.Log(str);

        List<IconUnit.PositiveEffects> st = pf.PassToEffectList(str);

        str = "";
        for (int i = 0; i < st.Count; i++)
        {
            str += st[i].ToString() + "-";
        }
        Debug.Log(str);
    }
}
