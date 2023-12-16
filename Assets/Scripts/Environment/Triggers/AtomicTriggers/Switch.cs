using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField]
    public GameObject targetReationObject;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, AlterTargetMachineStatus);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EventCenterManager.Instance.RemoveEventListener(GameEvent.PlayerTryInteract, AlterTargetMachineStatus);
    }


    private void AlterTargetMachineStatus()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredBySwitch,targetReationObject);
    }
}