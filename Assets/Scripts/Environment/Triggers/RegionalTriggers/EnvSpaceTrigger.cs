using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvSpaceTrigger : RegionalTrigger
{
    #region On Trigger Enter
    protected override void DoPlayerEnterAction()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.PlayerEnterSpace, true);
    }
    
    protected override void DoObjectEnterAction(Collider2D other)
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.CollectableEnterSpace, other,true);
    }
    
    #endregion

    #region On Trigger Exit
    protected override void DoPlayerExitAction()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.PlayerEnterSpace, false);
    }

    
    protected override void DoObjectExitAction(Collider2D other)
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.CollectableEnterSpace, other,false);
    }
    #endregion
}
