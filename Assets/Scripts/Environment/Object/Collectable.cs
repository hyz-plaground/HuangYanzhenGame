using UnityEngine;

public class Collectable : MonoBehaviour
{
    // Basic Properties
    protected Rigidbody2D Rigid;
    protected bool IsAllowCollect = true;

    // Event center manager
    private EventCenterManager ecm = EventCenterManager.Instance;

    private void InitParams()
    {
        IsAllowCollect = true;
        Rigid = GetComponent<Rigidbody2D>();
    }

    private void InitDelegates()
    {
        ecm.AddEventListener<Collider2D, bool>(GameEvent.CollectableEnterSpace, AdjustGravityScale);
    }
    
    protected void Start()
    {
        InitParams();
        InitDelegates();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsAllowCollect && other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            ecm.EventTrigger(GameEvent.ExistCollectable, gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsAllowCollect && other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            ecm.EventTrigger(GameEvent.NonExistCollectable, gameObject);
    }

    private void AdjustGravityScale(Collider2D notifiedCollider, bool isCollectableEnterSpace)
    {
        if (!ReferenceEquals(notifiedCollider.gameObject, gameObject))
            return;
        Rigid.gravityScale = isCollectableEnterSpace ? 0 : 1;
    }
}