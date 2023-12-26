using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// AtomicTrigger is an abstract class defined for the concept "Button" and Switch. Button can be interpreted in
/// many ways: A pressure button, or a player interactable button using F key. This abstract class only
/// defines  ways of Enabling, Disabling &amp; Triggering the target react machine. The detailed condition of when or how to
/// do this is described in its inheritors.
/// 
/// <param name="usePlayerInteractRange">
/// If set to true, this apparatus will trigger WithinRangeOfInteractable event.
/// Such that player can not drop items during its staying in this range.
/// </param>
/// </summary>
public abstract class AtomicTrigger : MonoBehaviour
{
    /* This is the target react machine. Done in unity editor or assigned by others.*/
    [SerializeField]
    public GameObject targetReactionObject;
    
    public bool usePlayerInteractRange = true;
    
    private bool isAcceptAnyEntryType = false;

    #region React Machine State
    
    /// <summary>
    /// No matter which state the react machine is in, intentionally enable the react machine.
    /// </summary>
    protected void EnableReactMachine()
    {
        if (!targetReactionObject)
            return;
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReactionObject, true);
    }

    /// <summary>
    /// No matter which state the react machine is in, intentionally disable the react machine.
    /// </summary>
    protected void DisableReactMachine()
    {
        if (!targetReactionObject)
            return;
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReactionObject, false);
    }

    /// <summary>
    /// Disregard the target state. Let react machine change into a state it is currently not in.
    /// </summary>
    protected void TriggerReactMachineIgnoreState()
    {
        if (!targetReactionObject)
            return;
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredBySwitch,targetReactionObject);
    }
    
    #endregion


    #region Detect Player in Range
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG) && !isAcceptAnyEntryType)
            return;
        
        if (CheckUsePlayerInteractRange())
            EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, true);
        
        DoPlayerEnterAction();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG) && !isAcceptAnyEntryType)
            return;
        
        if (CheckUsePlayerInteractRange())
            EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, false);
        
        DoPlayerExitAction();
    }

    #region Set and Check 
    private bool CheckUsePlayerInteractRange()
    {
        return usePlayerInteractRange;
    }


    protected void SetUsePlayerInteractRange(bool targetBoolValue)
    {
        usePlayerInteractRange = targetBoolValue;
    }

    /// <summary>
    /// Some collider triggers only accept player, while others can accept anything.
    /// For example, a pressure button accepts anything. However, a trigger only accepts player.
    /// Whether an object (collider) is a player is defined by tag.
    /// <param name="targetBoolValue"> Target bool value. Default false. Can manually set to true
    /// if required. </param>
    /// </summary>
    protected void SetIsAcceptAnyEntryType(bool targetBoolValue)
    {
        isAcceptAnyEntryType = targetBoolValue;
    }
    #endregion
    
    #endregion
    
    /// <summary>
    /// What to do when player enters (stays in) the collision trigger of the apparatus.
    /// </summary>
    protected abstract void DoPlayerEnterAction();
    
    /// <summary>
    /// What to do when player exits the collision trigger of the apparatus.
    /// </summary>
    protected abstract void DoPlayerExitAction();

}