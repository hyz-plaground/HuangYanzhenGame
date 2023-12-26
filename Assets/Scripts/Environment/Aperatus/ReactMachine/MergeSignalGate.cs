using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum GateType
{
    And,
    Or,
    Xor,
}

/// <summary>
/// MergeSignalGate (MSG) is a gate that process multiple sources of "enable" signals.
/// MSG takes multiple inputs of triggers as a react machine, and gives an output as an atomic trigger.
/// </summary>
public class MergeSignalGate : ReactMachine
{
    public GateType gateType = GateType.And;
    public GameObject targetReactionObject;

    /* Static properties.*/
    [SerializeField] 
    public int acceptPositiveSignalNum = 2;

    /* Dynamic properties.*/
    private int _currentPositiveSignalNum = 0;

    private void InitParams()
    {
        triggeredBySpecificValue = true;
        if (targetReactionObject)
            transform.position = targetReactionObject.transform.position;
    }

    protected new void Start()
    {
        InitParams();
        base.Start();
    }

    protected override void React(GameObject triggerTarget)
    {

    }

    protected override void React(GameObject triggerTarget, bool isLetMachineEnable)
    {
        /*
         * Develop log: December 26, 2023. Always add a guardian condition
         * to prevent Stack Overflow.
         */
        if (triggerTarget != gameObject)
            return;
        
        switch (gateType)
        {
            case GateType.And:
                DetectANDGateOutput(isLetMachineEnable);
                return;
            case GateType.Or:
                DetectORGateOutput(isLetMachineEnable);
                return;
            case GateType.Xor:
                DetectXORGateOutput(isLetMachineEnable);
                return;
            default:
                return;
        }
    }

    private void CountCurrentPositiveSignals(bool isLetMachineEnable)
    {
        // Increase or decrease current positive signal number.
        _currentPositiveSignalNum += isLetMachineEnable ? 1 : -1;
        if (_currentPositiveSignalNum >= acceptPositiveSignalNum)
        {
            _currentPositiveSignalNum = acceptPositiveSignalNum;
        }
        else if (_currentPositiveSignalNum < 0)
        {
            // This shouldn't occur in normal circumstances.
            _currentPositiveSignalNum = 0;
        }
    }

    private void DetectANDGateOutput(bool isLetMachineEnable)
    {
        CountCurrentPositiveSignals(isLetMachineEnable);
        EnableReactMachineBySignalGate(_currentPositiveSignalNum >= acceptPositiveSignalNum);

    }

    private void DetectORGateOutput(bool isLetMachineEnable)
    {
        CountCurrentPositiveSignals(isLetMachineEnable);
        EnableReactMachineBySignalGate(_currentPositiveSignalNum > 0);
    }

    private void DetectXORGateOutput(bool isLetMachineEnable)
    {
        CountCurrentPositiveSignals(isLetMachineEnable);
        EnableReactMachineBySignalGate(_currentPositiveSignalNum % 2 != 0);
    }

    private void EnableReactMachineBySignalGate(bool targetBoolValue)
    {
        if (!targetReactionObject)
            return;
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredSpecific, targetReactionObject, targetBoolValue);
    }
}