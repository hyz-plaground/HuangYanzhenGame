using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public abstract class ReactMachine : MonoBehaviour
{
    [SerializeField] 
    public bool triggeredBySpecificValue;
        
    protected void Start()
    {
        if (triggeredBySpecificValue)
        {
            EventCenterManager.Instance.AddEventListener<GameObject, bool>(GameEvent.MachineTriggeredSpecific, React);
        }
        else
        {
            EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.MachineTriggered, React);
        }
       
    }

    protected abstract void React(GameObject triggerTarget);

    protected abstract void React(GameObject triggerTarget, bool isLetMachineEnable);
    
}