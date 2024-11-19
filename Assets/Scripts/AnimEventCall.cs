using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimEventCall : MonoBehaviour
{

    public Button.ButtonClickedEvent[] onCall;

    public void EventCall(int index)
    {
        if (index < 0 || index >= onCall.Length)
            return;

        if (onCall != null)
            onCall[index].Invoke();
    }

    public void DestroyCall()
    {
        Destroy(gameObject, 0.2f);
    }
}