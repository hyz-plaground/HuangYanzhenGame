using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// AtomicTrigger is just an abstract class defined for the concept "Button" and Switch. Button can be interpreted in
/// many ways: A pressure button, or a player interactable button using F key. This abstract class only
/// defines two ways of Enabling, Disabling &amp; Triggering the target react machine. The detailed condition of when or how to
/// do this is described in its inheritors.
/// </summary>
public abstract class AtomicTrigger : MonoBehaviour
{
    /* This is the target react machine. Done in unity editor or assigned by others.*/
    [SerializeField]
    public GameObject targetReactionObject;

     public bool usePlayerInteractRange = true;

    #region React Machine State
    
    /// <summary>
    /// No matter which state the react machine is in, intentionally enable the react machine.
    /// </summary>
    protected void EnableReactMachine()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReactionObject, true);
    }

    /// <summary>
    /// No matter which state the react machine is in, intentionally disable the react machine.
    /// </summary>
    protected void DisableReactMachine()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReactionObject, false);
    }

    /// <summary>
    /// Disregard the target state. Let react machine change into a state it is currently not in.
    /// </summary>
    protected void TriggerReactMachineIgnoreState()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredBySwitch,targetReactionObject);
    }
    
    #endregion


    #region Detect Player in Range
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            return;
        if (CheckUsePlayerInteractRange())
        {
            EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, true);
        }
        PlayerEnterAction();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG) && CheckUsePlayerInteractRange())
            return;
        if (CheckUsePlayerInteractRange())
        {
            EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, false);
        }
        PlayerExitAction();
    }

    private bool CheckUsePlayerInteractRange()
    {
        return usePlayerInteractRange;
    }

    protected void SetUsePlayerInteractRange(bool targetBoolValue)
    {
        usePlayerInteractRange = targetBoolValue;
    }

    #endregion
    
    protected abstract void PlayerEnterAction();
    protected abstract void PlayerExitAction();

}