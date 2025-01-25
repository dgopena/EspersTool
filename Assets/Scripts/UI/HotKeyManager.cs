using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HotKeyManager : MonoBehaviour
{
    public static HotKeyManager _instance;
    
    [System.Serializable]
    public struct HotKeyBind
    {
        public KeyCode[] keys;
        public UnityEvent action;
    }

    public List<HotKeyBind> bindings;

    private bool hotKeysEnabled = true;
    
    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;
    }

    public void EnableHotKeys(bool active)
    {
        hotKeysEnabled = active;
    }
    
    private void FixedUpdate()
    {
        if (UnitManager._instance.IsUnitEditing())
            return;

        if (!hotKeysEnabled)
            return;
        
        for(int i = 0; i < bindings.Count; i++)
        {
            for (int k = 0; k < bindings[i].keys.Length; k++)
            {
                if (Input.GetKeyDown(bindings[i].keys[k]))
                {
                    if (bindings[i].action != null)
                        bindings[i].action.Invoke();
                }
            }
        }
    }
}
