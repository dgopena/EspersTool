using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnippetTester : MonoBehaviour
{
    public List<ClassData.Trait> traitComp;
    // Start is called before the first frame update
    void Start()
    {
        //now we clean duplicates and update them
        List<ClassData.Trait> cleanComp = new List<ClassData.Trait>();
        List<ClassData.Trait> auxComp = new List<ClassData.Trait>(traitComp);

        while (auxComp.Count > 0)
        {
            ClassData.Trait focusTrait = new ClassData.Trait();
            focusTrait.traitName = auxComp[0].traitName;
            focusTrait.traitDescription = auxComp[0].traitDescription;

            bool foundDup = false;
            for (int t = auxComp.Count - 1; t >= 0; t--) //check and update duplicates
            {
                if (auxComp[t].traitName == focusTrait.traitName)
                {
                    if (!foundDup)
                    {
                        foundDup = true;
                        focusTrait.traitDescription = auxComp[t].traitDescription; //found last entry, therefore, the one we must consider
                    }

                    auxComp.RemoveAt(t); //remove duplicates
                }
            }

            cleanComp.Add(focusTrait);
        }

        string str = "\n------------";
        for(int i = 0; i < cleanComp.Count; i++)
        {
           str += "\n" + (i + 1) + ") " + cleanComp[i].traitName + ": " + cleanComp[i].traitDescription;
        }
        str += "------------";

        Debug.Log(str);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
