public enum GameEvent
{
    // No param
    PlayerDie,
    PlayerTryInteract,

    #region Single Param
    // Player Property adjustments
    PlayerEnterSpace,
    PlayerGetHurt,
    PlayerReleaseObject,
    
    // React Machine Trigger
    MachineTriggeredBySwitch,
    
    // Collectable 
    ExistCollectable,
    NonExistCollectable,
    CollectableEnterSpace,
    
    // Interactable
    WithinRangeOfInteractable,
    WithinRangeOfLock,
    OutOfRangeOfLock,
    KeyInsertedInLock,
    
    #endregion

    #region Double Param
    // React Machine Trigger
    MachineTriggeredByButton,
    #endregion
}