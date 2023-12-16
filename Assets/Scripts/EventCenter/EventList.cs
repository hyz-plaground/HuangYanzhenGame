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
    CollectableEnterSpace,
    
    // Double param.
    MachineTriggeredByButton,
}