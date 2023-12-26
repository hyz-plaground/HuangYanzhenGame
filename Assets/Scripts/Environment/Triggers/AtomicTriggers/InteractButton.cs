using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Interact Button, i.e. interactable button, is a timer-based button which player can interact with.
/// When player press the interact key, enables the machine no matter the machine's state.
/// If not interrupted, after count down, the button automatically disables the machine.
/// If interrupted, the button re-sets the timer and re-enables the machine.
/// </summary>
public class InteractButton : AtomicTrigger
{ 
    [SerializeField] 
    public int countdownSeconds = 5;

    protected override void DoPlayerEnterAction()
    {
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, PerformEnableReactMachineCycle);
    }
    
    protected override void DoPlayerExitAction()
    {
        EventCenterManager.Instance.RemoveEventListener(GameEvent.PlayerTryInteract, PerformEnableReactMachineCycle);
    }
    

    /* Enable-countdown-Disable method. Uses coroutine.*/
    private void PerformEnableReactMachineCycle()
    {
        StopAllCoroutines();
        StartCoroutine(PerformEnableReactMachineCycleCoroutine());
    }

    private IEnumerator PerformEnableReactMachineCycleCoroutine()
    {
        EnableReactMachine();
        yield return new WaitForSeconds(countdownSeconds);
        DisableReactMachine();
    }
}