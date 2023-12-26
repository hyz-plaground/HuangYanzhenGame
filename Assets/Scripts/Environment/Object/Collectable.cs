using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    protected Rigidbody2D Rigid;
    protected bool IsAllowCollect = true;

    private void InitParams()
    {
        IsAllowCollect = true;
        Rigid = GetComponent<Rigidbody2D>();
    }

    private void InitDelegates()
    {
        EventCenterManager.Instance.AddEventListener<Collider2D,bool>(GameEvent.CollectableEnterSpace,AdjustGravityScale);
    }
    
    /// <summary>
    /// Initialize rigid body, add an event listener to control gravity scale.
    /// </summary>
    protected void Start()
    {
        InitParams();
        InitDelegates();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if ( IsAllowCollect && other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            EventCenterManager.Instance.EventTrigger(GameEvent.ExistCollectable,gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if( IsAllowCollect && other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
            EventCenterManager.Instance.EventTrigger(GameEvent.NonExistCollectable,gameObject);
    }

    private void AdjustGravityScale(Collider2D notifiedCollider, bool isCollectableEnterSpace)
    {
        if (!ReferenceEquals(notifiedCollider.gameObject, gameObject))
            return;
        Rigid.gravityScale = isCollectableEnterSpace ? 0 : 1;
    }

}