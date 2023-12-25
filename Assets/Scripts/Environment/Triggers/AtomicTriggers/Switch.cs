using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Switch : AtomicTrigger
{
    
    protected override void PlayerEnterAction()
    {
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, TriggerReactMachineIgnoreState);
    }

    protected override void PlayerExitAction()
    {
        EventCenterManager.Instance.RemoveEventListener(GameEvent.PlayerTryInteract, TriggerReactMachineIgnoreState);
    }
}