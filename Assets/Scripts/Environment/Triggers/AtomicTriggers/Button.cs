using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    [SerializeField]
    public GameObject targetReationObject;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ButtonTriggered");
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, true);
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("ButtonTriggered");
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, false);
    }
    
}
