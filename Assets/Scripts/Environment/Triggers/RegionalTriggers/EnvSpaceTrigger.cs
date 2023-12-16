using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvSpaceTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
        {
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerEnterSpace, true);
        }
        else
        {
            EventCenterManager.Instance.EventTrigger(GameEvent.CollectableEnterSpace, true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
        {
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerEnterSpace, false);
        }
        else
        {
            EventCenterManager.Instance.EventTrigger(GameEvent.CollectableEnterSpace, false);
        }
    }
}
