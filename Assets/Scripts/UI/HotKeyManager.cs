using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HotKeyManager : MonoBehaviour
{
    [System.Serializable]
    public struct HotKeyBind
    {
        public KeyCode[] keys;
        public UnityEvent action;
    }

    public List<HotKeyBind> bindings;

    private void FixedUpdate()
    {
        if (UnitManager._instance.IsUnitEditing())
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
