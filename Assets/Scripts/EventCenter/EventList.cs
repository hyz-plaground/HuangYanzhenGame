public enum GameEvent
{
    // No param
    PlayerDie,
    PlayerTryInteract,

    #region Single Param
    // Scene Switch
    SwitchToAnotherScene,
    
    // Player Property adjustments
    PlayerEnterSpace,
    PlayerGetHurt,
    PlayerCollectObject,
    PlayerReleaseObject,
    
    // React Machine Trigger
    MachineTriggered,
    
    // Collectable 
    PlayerApproachThisCollectable,      // A player approaches a specific collectable
    PlayerLeaveThisCollectable,         // A player leaves a specific collectable
    CollectableEnterSpace,
    
    // Interactable
    WithinRangeOfInteractable,
    WithinRangeOfLock,
    OutOfRangeOfLock,
    KeyInsertedInLock,
    
    #endregion

    #region Double Param
    // React Machine Trigger
    MachineTriggeredSpecific,
    #endregion
}