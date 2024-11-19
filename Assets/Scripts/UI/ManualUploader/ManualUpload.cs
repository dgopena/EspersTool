using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManualUpload : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField rawInput;
    [SerializeField] private TMP_InputField nameLabel;
    [SerializeField] private TMP_InputField costLabel;
    [SerializeField] private Toggle attackLabel;
    [SerializeField] private RectTransform additionalsPrefab;
    [SerializeField] private TextMeshProUGUI effectDisplay;

    private List<GameObject> additionals;

    [Header("Settings")]
    [SerializeField] private string[] cursiveTheseWords;

    public enum EntryTypes
    {
        IconAbility,
        FoeAbility,
        Trait
    }

    [Header("Entry Control")]
    [SerializeField] private EntryTypes currentExpectedEntry = EntryTypes.FoeAbility;
    public TraitEntry currentTraitList;
    public AbilityEntry currentAbilityList;

    private void Awake()
    {
        additionals = new List<GameObject>();

        //CheckAbilityIDs();
    }

    public void Process()
    {
        if (currentExpectedEntry == EntryTypes.FoeAbility)
        {
            //grab raw text from the manual, transform it into the expected format of abilities
            string manualEntry = rawInput.text;

            manualEntry = CleanLineBreaks(manualEntry, " ");

            Debug.Log(manualEntry);

            int indexOfPar = manualEntry.IndexOf('(');
            int indexOfEndPar = manualEntry.IndexOf(')');

            string abName = manualEntry.Substring(0, indexOfPar - 1);
            nameLabel.SetTextWithoutNotify(abName);

            attackLabel.SetIsOnWithoutNotify(false);

            string parenthesis = manualEntry.Substring(indexOfPar + 1, indexOfEndPar - indexOfPar - 1);

            List<string> aspects = new List<string>();
            string auxPar = parenthesis;
            while (true)
            {
                int commaIndex = auxPar.IndexOf(",");
                if (commaIndex < 0)
                {
                    aspects.Add(auxPar.Trim());

                    break;
                }

                string rem = auxPar.Substring(0, auxPar.IndexOf(","));
                aspects.Add(rem.Trim());

                auxPar = auxPar.Substring(auxPar.IndexOf(",") + 1);
            }

            for (int i = 0; i < additionals.Count; i++)
            {
                DestroyImmediate(additionals[i]);
            }

            additionals.Clear();

            for (int i = 0; i < aspects.Count; i++)
            {
                if (aspects[i].Contains("action", System.StringComparison.OrdinalIgnoreCase))
                {
                    string val = aspects[i].Substring(0, aspects[i].IndexOf(' '));
                    costLabel.SetTextWithoutNotify(val);
                }
                else if (aspects[i].Contains("interrupt", System.StringComparison.OrdinalIgnoreCase))
                {
                    costLabel.SetTextWithoutNotify("interrupt");
                }
                else if (aspects[i].Contains("attack", System.StringComparison.OrdinalIgnoreCase))
                {
                    attackLabel.SetIsOnWithoutNotify(true);
                }
                else
                {
                    GameObject nuAdd = Instantiate<GameObject>(additionalsPrefab.gameObject, additionalsPrefab.parent);
                    nuAdd.transform.SetAsLastSibling();
                    nuAdd.transform.GetChild(1).GetComponent<TMP_InputField>().SetTextWithoutNotify(aspects[i].Trim());
                    nuAdd.SetActive(true);

                    additionals.Add(nuAdd);
                }
            }

            //process the effect part
            string effectPar = manualEntry.Substring(manualEntry.IndexOf(':') + 2);

            string editedEffect = "";

            string auxTxt = effectPar;

            int loopedOut = 100;
            while (true)
            {
                int quoteIndex = auxTxt.IndexOf(':');
                if (quoteIndex < 0)
                {
                    editedEffect += auxTxt;
                    break;
                }

                string entry = auxTxt;

                string afterHalf = auxTxt.Substring(quoteIndex + 1);
                int endIndex = afterHalf.IndexOf(':');
                if (endIndex >= 0)
                {
                    string halfAux = afterHalf.Substring(0, endIndex);
                    endIndex = halfAux.LastIndexOf('.') + quoteIndex + 2;
                    entry = auxTxt.Substring(0, endIndex + 1);
                }

                string fHalf = entry.Substring(0, quoteIndex + 1);
                string sHalf = entry.Substring(quoteIndex + 2);

                entry = "<b>" + fHalf + " </b>" + sHalf;

                if (editedEffect.Length != 0)
                    entry = "\n" + entry.Trim();

                editedEffect += entry;

                if (endIndex >= 0)
                    auxTxt = auxTxt.Substring(endIndex + 1);
                else
                    break;

                loopedOut -= 1;
                if (loopedOut < 0)
                    break;
            }

            editedEffect = ApplyCursives(editedEffect);
            editedEffect = ApplyBreaks(editedEffect);
            effectDisplay.text = editedEffect;
        }
        else if(currentExpectedEntry == EntryTypes.Trait)
        {
            //grab raw text from the manual, transform it into the expected format of abilities
            string manualEntry = rawInput.text;

            manualEntry = CleanLineBreaks(manualEntry, " ");

            Debug.Log(manualEntry);

            int indexOfPar = manualEntry.IndexOf(':');
            string abName = manualEntry.Substring(0, indexOfPar);
            nameLabel.SetTextWithoutNotify(abName);

            //process the effect part
            string effectPar = manualEntry.Substring(manualEntry.IndexOf(':') + 2);

            string editedEffect = "";

            string auxTxt = effectPar;

            int loopedOut = 100;
            while (true)
            {
                int quoteIndex = auxTxt.IndexOf(':');
                if (quoteIndex < 0)
                {
                    editedEffect += auxTxt;
                    break;
                }

                string entry = auxTxt;

                string afterHalf = auxTxt.Substring(quoteIndex + 1);
                int endIndex = afterHalf.IndexOf(':');
                if (endIndex >= 0)
                {
                    string halfAux = afterHalf.Substring(0, endIndex);
                    endIndex = halfAux.LastIndexOf('.') + quoteIndex + 2;
                    entry = auxTxt.Substring(0, endIndex + 1);
                }

                string fHalf = entry.Substring(0, quoteIndex + 1);
                string sHalf = entry.Substring(quoteIndex + 2);

                entry = "<b>" + fHalf + " </b>" + sHalf;

                if (editedEffect.Length != 0)
                    entry = "\n" + entry.Trim();

                editedEffect += entry;

                if (endIndex >= 0)
                    auxTxt = auxTxt.Substring(endIndex + 1);
                else
                    break;

                loopedOut -= 1;
                if (loopedOut < 0)
                    break;
            }

            editedEffect = ApplyCursives(editedEffect);
            editedEffect = ApplyBreaks(editedEffect);
            effectDisplay.text = editedEffect;
        }
    }

    public void AddToCurrentList()
    {
        //adds the ability to the current list
        if(currentExpectedEntry == EntryTypes.FoeAbility)
        {
            AbilityEntry.FoeAbility nuAb = new AbilityEntry.FoeAbility();
            nuAb.abilityName = nameLabel.text;
            nuAb.actionCost = costLabel.text;
            nuAb.isAttack = attackLabel.isOn;

            string[] abilityAdditionals = new string[additionals.Count];
            for(int i = 0; i < additionals.Count; i++)
            {
                abilityAdditionals[i] = additionals[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            }
            nuAb.additionals = abilityAdditionals;

            nuAb.subCombos = new int[0];

            nuAb.effect = effectDisplay.text;

            List<AbilityEntry.FoeAbility> abs = new List<AbilityEntry.FoeAbility>(currentAbilityList.abilities);

            nuAb.abilityIDInList = abs.Count;
            abs.Add(nuAb);

            currentAbilityList.abilities = abs.ToArray();
        }
        else if(currentExpectedEntry == EntryTypes.Trait)
        {
            TraitEntry.Trait nuTr = new TraitEntry.Trait();
            nuTr.traitName = nameLabel.text;
            nuTr.traitEffect = effectDisplay.text;

            List<TraitEntry.Trait> trts = new List<TraitEntry.Trait>(currentTraitList.Traits);

            nuTr.traitIDInList = trts.Count;
            trts.Add(nuTr);

            currentTraitList.Traits = trts.ToArray();
        }
    }

    public string ApplyCursives(string entry)
    {
        for (int i = 0; i < cursiveTheseWords.Length; i++)
        {
            int hook = entry.IndexOf(cursiveTheseWords[i], System.StringComparison.OrdinalIgnoreCase);
            if(hook >= 0)
            {
                string aux = entry;

                string earlyHalf = aux.Substring(0, hook);
                string word = aux.Substring(hook);
                int spaceIndex = word.IndexOf(' ');
                string laterHalf = "";
                if (spaceIndex >= 0) {
                    string auxWord = word.Substring(0, spaceIndex);
                    laterHalf = word.Substring(spaceIndex);
                    word = auxWord;
                }

                entry = earlyHalf + "<i>" + word + "</i>" + laterHalf;
            }
        }

        return entry;
    }

    public string ApplyBreaks(string entry)
    {
        string outGo = "";
        for(int i = 0; i < entry.Length; i++)
        {
            if(entry[i] == '•')
            {
                outGo += "\n";
            }
            outGo += entry[i];
        }

        return outGo;
    }

    public static string CleanLineBreaks(string entry, string replaceWith)
    {
        while (true)
        {
            int idx = entry.IndexOf("\n");
            if (idx < 0)
                return entry;

            string fHalf = entry.Substring(0, idx - 1);
            string sHalf = entry.Substring(idx + 1);
            entry = fHalf + replaceWith + sHalf;
        }
    }

    private void CheckAbilityIDs()
    {
        for (int g = 0; g < currentAbilityList.abilities.Length; g++)
        {
            List<string> abFounds = new List<string>();

            for (int i = 0; i < currentAbilityList.abilities.Length; i++)
            {
                if (currentAbilityList.abilities[i].abilityIDInList == g)
                {
                    abFounds.Add(currentAbilityList.abilities[i].abilityName);
                }
            }

            if (abFounds.Count == 0)
            {
                Debug.Log("------------------!!!! No abilities found with ID " + g);
            }
            else if (abFounds.Count > 1)
            {
                string alert = "....................!!! Multiple abilities found with ID " + g + ": ";

                for(int i = 0; i < abFounds.Count; i++)
                {
                    alert += " " + abFounds[i];
                }

                Debug.Log(alert);
            }
            else
            {
                Debug.Log(g + " - " + abFounds[0]);
            }
        }
    }
}
