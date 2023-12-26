using System.Text.RegularExpressions;
using UnityEngine;

public class Lock : AtomicTrigger
{
    [SerializeField] 
    public MatchCode keyMatchCode = MatchCode.DEF;

    private void InitParams()
    {
        usePlayerInteractRange = false;
    }
    
    private void InitDelegates()
    {
        EventCenterManager.Instance.AddEventListener<MatchCode>(GameEvent.KeyInsertedInLock, Verify);
    }
    
    private void Start()
    {
        InitParams();
        InitDelegates();
    }
    
    protected override void DoPlayerEnterAction()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.WithinRangeOfLock, gameObject, keyMatchCode);
    }
    
    protected override void DoPlayerExitAction()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.OutOfRangeOfLock, gameObject);
    }

    private void Verify(MatchCode insertedKeyMatchCode)
    {
        if (insertedKeyMatchCode != keyMatchCode)
            return;
        EnableReactMachine();
    }
    
}