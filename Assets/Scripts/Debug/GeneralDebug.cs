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
        List<EsperUnit.Blight> blights = new List<EsperUnit.Blight>();
        blights.Add(EsperUnit.Blight.Burning);
        blights.Add(EsperUnit.Blight.Poisoned);
        blights.Add(EsperUnit.Blight.Electrified);
        blights.Add(EsperUnit.Blight.Burning);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(blights);

        Debug.Log(str);

        List<EsperUnit.Blight> bl = pf.PassToBlightList(str);

        str = "";
        for(int i = 0; i < bl.Count; i++)
        {
            str += bl[i].ToString() + "-";
        }
        Debug.Log(str);
    }

    private void TestStatusListBackForth()
    {
        List<EsperUnit.Status> status = new List<EsperUnit.Status>();
        status.Add(EsperUnit.Status.Blind);
        status.Add(EsperUnit.Status.Incapacitated);
        status.Add(EsperUnit.Status.Shattered);
        status.Add(EsperUnit.Status.Hatred);
        status.Add(EsperUnit.Status.Hatred);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(status);

        Debug.Log(str);

        List<EsperUnit.Status> st = pf.PassToStatusList(str);

        str = "";
        for (int i = 0; i < st.Count; i++)
        {
            str += st[i].ToString() + "-";
        }
        Debug.Log(str);
    }

    private void TestEffectListBackForth()
    {
        List<EsperUnit.PositiveEffects> effect = new List<EsperUnit.PositiveEffects>();
        effect.Add(EsperUnit.PositiveEffects.Dodge);
        effect.Add(EsperUnit.PositiveEffects.Flying);
        effect.Add(EsperUnit.PositiveEffects.Flying);
        effect.Add(EsperUnit.PositiveEffects.Skirmisher);
        effect.Add(EsperUnit.PositiveEffects.Unstoppable);

        PieceFile pf = new PieceFile();

        string str = pf.PassToString(effect);

        Debug.Log(str);

        List<EsperUnit.PositiveEffects> st = pf.PassToEffectList(str);

        str = "";
        for (int i = 0; i < st.Count; i++)
        {
            str += st[i].ToString() + "-";
        }
        Debug.Log(str);
    }
}
