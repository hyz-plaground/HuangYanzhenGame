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
    MachineTriggeredSpecific,
    #endregion
}