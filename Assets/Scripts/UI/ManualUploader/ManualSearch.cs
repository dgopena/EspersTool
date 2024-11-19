using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

public class ManualSearch : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField requestEntry;
    [SerializeField] private GameObject resultPrefab;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Data")]
    [SerializeField] private TraitEntry[] traitsBase;
    [SerializeField] private AbilityEntry[] abilityBase;

    private struct MatchEntry
    {
        public float distanceFromInput;
        public int entryID;
        public int docID;
        public string name;
        public bool isTrait;
    }

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SearchMatches();
        }
    }

    public void SearchMatches()
    {
        int matchListSize = 5;

        MatchEntry[] topMatches = new MatchEntry[matchListSize];

        for(int i = 0; i < topMatches.Length; i++)
        {
            MatchEntry entr = new MatchEntry();
            entr.distanceFromInput = float.MaxValue;

            topMatches[i] = entr;
        }

        string toMatch = requestEntry.text;

        if (toMatch == null || toMatch.Length == 0)
            return;

        //clean result list
        for(int i = resultPrefab.transform.parent.childCount - 1; i >= 1; i--)
        {
            DestroyImmediate(resultPrefab.transform.parent.GetChild(i).gameObject);
        }

        descriptionText.text = "";

        //first, check traits
        for(int i = 0; i < traitsBase.Length; i++)
        {
            TraitEntry traitList = traitsBase[i];

            for(int t = 0; t < traitList.Traits.Length; t++)
            {
                string compName = traitList.Traits[t].traitName;

                float distance = ComputeDistance(toMatch, compName);

                for(int m = 0; m < topMatches.Length; m++)
                {
                    if (distance < topMatches[m].distanceFromInput)
                    {
                        MatchEntry entr = new MatchEntry();
                        entr.isTrait = true;
                        entr.distanceFromInput = distance;
                        entr.docID = i;
                        entr.entryID = t;
                        entr.name = compName;

                        topMatches = AddInIndex(topMatches, entr, m);

                        break;
                    }
                }
            }
        }

        //then check abilities
        for (int i = 0; i < abilityBase.Length; i++)
        {
            AbilityEntry abilityList = abilityBase[i];

            for (int a = 0; a < abilityList.abilities.Length; a++)
            {
                string compName = abilityList.abilities[a].abilityName;

                float distance = ComputeDistance(toMatch, compName);

                for (int m = 0; m < topMatches.Length; m++)
                {
                    if (distance < topMatches[m].distanceFromInput)
                    {
                        MatchEntry entr = new MatchEntry();
                        entr.isTrait = false;
                        entr.distanceFromInput = distance;
                        entr.docID = i;
                        entr.entryID = a;
                        entr.name = compName;

                        topMatches = AddInIndex(topMatches, entr, m);

                        break;
                    }
                }
            }
        }

        //build result list
        for (int i = 0; i < topMatches.Length; i++)
        {
            GameObject pref = Instantiate<GameObject>(resultPrefab, resultPrefab.transform.parent);

            pref.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = topMatches[i].isTrait ? "Trait" : "Ability";
            pref.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = topMatches[i].entryID.ToString();
            pref.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = topMatches[i].name;
            pref.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = "[" + topMatches[i].docID + "]-" + (topMatches[i].isTrait ? traitsBase[topMatches[i].docID].name : abilityBase[topMatches[i].docID].name);

            string description = topMatches[i].isTrait ? traitsBase[topMatches[i].docID].Traits[topMatches[i].entryID].traitEffect : abilityBase[topMatches[i].docID].abilities[topMatches[i].entryID].effect;

            description = description.Replace("\\n", "<br>");

            pref.GetComponent<HoldButton>().onDown.AddListener(delegate {
                descriptionText.text = description;
            });

            pref.transform.SetAsLastSibling();
            pref.SetActive(true);
        }
    }

    private MatchEntry[] AddInIndex(MatchEntry[] list, MatchEntry nuEntry, int index)
    {
        if (index > list.Length)
            return null;

        MatchEntry[] ret = new MatchEntry[list.Length];
        for (int i = 0; i < index; i++)
        {
            ret[i] = list[i];
        }

        ret[index] = nuEntry;

        for(int i = index; i < list.Length - 1; i++)
        {
            ret[i + 1] = list[i];
        }

        return ret;
    }

    //Damereau-Levenshein Distance
    public static int ComputeDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 1; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }
}
