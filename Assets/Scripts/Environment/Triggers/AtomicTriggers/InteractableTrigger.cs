using UnityEngine;

public abstract class InteractableTrigger : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            return;
        EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, true);
        PlayerEnterAction();

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            return;
        EventCenterManager.Instance.EventTrigger<bool>(GameEvent.WithinRangeOfInteractable, false);
       PlayerExitAction();
    
    }

    protected abstract void PlayerEnterAction();
    protected abstract void PlayerExitAction();

}