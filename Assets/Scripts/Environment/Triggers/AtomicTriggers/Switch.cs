using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Switch : AtomicTrigger
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, TriggerReactMachineIgnoreState);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EventCenterManager.Instance.RemoveEventListener(GameEvent.PlayerTryInteract, TriggerReactMachineIgnoreState);
    }
    
}