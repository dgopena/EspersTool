using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpManager : MonoBehaviour
{
    public static HelpManager _instance;

    private int helpModeActive = -1; //-1 - inactive, 0 - TerrainTool, 1 - UnitMaker, 2 - ObjectsTool, 3 - Coloring, 4 - GameMode, 5 - Options

    [SerializeField] private Transform[] helpPanelSets;
    [SerializeField] private Transform helpPanelParent;

    public bool IsHelpPanelActive { get { return helpModeActive >= 0; } }

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;
    }

    public void SetHelpPanelActive()
    {
        if(helpModeActive >= 0)
        {
            helpPanelParent.gameObject.SetActive(false);
            MapManager._instance.EnableControls(true);
            helpModeActive = -1;
            return;
        }

        helpPanelParent.gameObject.SetActive(true);
        MapManager._instance.EnableControls(false);

        if (MapManager._instance.optionsManager.optionsScreen.activeSelf)
            helpModeActive = 5;
        else
        {
            helpModeActive = (int)MapManager._instance.toolMode;
        }

        for(int i = 0; i < helpPanelSets.Length; i++)
        {
            helpPanelSets[i].gameObject.SetActive(i == helpModeActive);
        }
    }


}
