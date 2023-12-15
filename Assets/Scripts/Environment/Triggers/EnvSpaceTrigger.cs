using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvSpaceTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ENT_PLAYER"))
        {
           Debug.Log("Enter space.");
           EventCenterManager.Instance.EventTrigger<bool>(GameEvent.PlayerEnterSpace, true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ENT_PLAYER"))
        {
            Debug.Log("Exit space.");
            EventCenterManager.Instance.EventTrigger<bool>(GameEvent.PlayerEnterSpace, false);
        }
    }
    
    
}
