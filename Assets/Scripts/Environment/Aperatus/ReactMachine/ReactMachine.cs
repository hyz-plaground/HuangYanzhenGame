using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public abstract class ReactMachine : MonoBehaviour
{
    [SerializeField] 
    public bool useButtonAsTrigger;
        
    protected void Start()
    {
        if (useButtonAsTrigger)
        {
            EventCenterManager.Instance.AddEventListener<GameObject, bool>(GameEvent.MachineTriggeredByButton, React);
        }
        else
        {
            EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.MachineTriggeredBySwitch, React);
        }
       
    }

    protected abstract void React(GameObject triggerTarget);

    protected abstract void React(GameObject triggerTarget, bool isLetMachineEnable);
    
}