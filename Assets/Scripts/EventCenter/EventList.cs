public enum GameEvent
{
    // No param
    PlayerDie,
    PlayerTryInteract,
    
    // Single param.
    PlayerEnterSpace,
    PlayerGetHurt,
    MachineTriggeredBySwitch,
    ExistCollectable,
    NonExistCollectable,
    WithinRangeOfInteractable,
    CollectableEnterSpace,
    
    // Double param.
    MachineTriggeredByButton,
}