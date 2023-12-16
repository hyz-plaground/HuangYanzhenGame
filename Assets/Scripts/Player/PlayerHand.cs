using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private bool _isHandOccupied = false;
    private float _lastReleaseTime;

    private GameObject _objectInRange;
    private Rigidbody2D _objectRigid;

    private void Start()
    {
        _lastReleaseTime = 0;
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, CollectObject);
    }

    private void CollectObject()
    {
        if (!_objectInRange)
            return;
        if (!_isHandOccupied)
        {
            // Collect this object, remove event listener.
            _isHandOccupied = true;
            _lastReleaseTime = 0;
            _objectInRange.transform.position = transform.position;
            _objectRigid.simulated = false;
            _objectInRange.transform.SetParent(gameObject.transform);
            EventCenterManager.Instance.RemoveEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);
        }
        else
        {
            _isHandOccupied = false;
            _objectRigid.simulated = true;
            _objectInRange.transform.SetParent(null);
            _lastReleaseTime = Time.time;
            _objectInRange = null;
            _objectRigid = null;
            EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);
        }
        
    }
    
    private void ObserveObject(GameObject thisObject)
    {
        if (Time.time - _lastReleaseTime < 1f)
            return;
        _objectInRange = thisObject;
        _objectRigid = thisObject.GetComponent<Rigidbody2D>();
        Debug.Log("Observed " + thisObject.name);
    }
}