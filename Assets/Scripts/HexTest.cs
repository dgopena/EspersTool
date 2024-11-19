using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int e = 156166545;
        string hexValue = "AFFFFF"; // e.ToString("X6");

        Debug.Log(hexValue);

        int back = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

        Debug.Log(back.ToString());
    }

}
