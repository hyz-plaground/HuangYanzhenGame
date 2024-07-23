using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private bool _isHandOccupied = false;
    private float _lastCollectTime;
    private GameObject _objectInRange;
    private Rigidbody2D _objectRigid;
    private bool _isCollecting = false;
    private bool _isPlayerWithinRangeOfInteractable = false;

    private void Start()
    {
        _lastCollectTime = 0;
        // Collectable object tells player that there is an object to be collected.
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.NonExistCollectable, IgnoreObject);
        
        // Player intend to collect object.
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerTryInteract, InteractWithThisObject);
        
        // Player is in range of an interactable object.
        EventCenterManager.Instance.AddEventListener<bool>(GameEvent.WithinRangeOfInteractable, SetIsPlayerWithinInteractable);
    }
    
    /* Player Observes this object, but doesn't collect it.*/
    private void ObserveObject(GameObject thisObject)
    {
        _objectInRange = thisObject;
        _objectRigid = thisObject.GetComponent<Rigidbody2D>();
        Debug.Log("Observed " + thisObject.name);
    }

    /* Player is out of the observe range of this object.*/
    private void IgnoreObject(GameObject thisObject)
    {
        if(thisObject == _objectInRange && !_isHandOccupied)
            _objectInRange = null;
        Debug.Log("Ignored " + thisObject.name);
    }

    /* Designed in a coroutine way so that it is not bothered by frequent happen of events.*/
    // This part comes from chat gpt. I have absolutely no idea how it works.
    // (In fact, I'm just being lazy to interpret this. As long as it works it's fine.)
    private void InteractWithThisObject()
    {
        if (!_objectInRange || _isCollecting)
            return;

        if (!_isHandOccupied)
        {
            // Collect object.
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerCollectObject, _objectInRange);
            StopAllCoroutines();
            StartCoroutine(CollectCoroutine());
        }
        else if(CheckIsAllowRelease())
        {
            // Release object.
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerReleaseObject,_objectInRange);
            StopAllCoroutines();
            StartCoroutine(ReleaseCoroutine());
        }
    }
    
    private IEnumerator CollectCoroutine()
    {
        _isCollecting = true;
        // Set crucial variables.
        _lastCollectTime = Time.time;
        _isHandOccupied = true;

        // Set position & belongings of the target object.
        _objectInRange.transform.position = transform.position;
        _objectRigid.simulated = false;
        _objectInRange.transform.SetParent(gameObject.transform);

        // Remove ExistCollectable event listener. Do not listen to outer object anymore.
        EventCenterManager.Instance.RemoveEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);

        yield return new WaitForSeconds(0.5f); // Wait for 0.5 sec
        
        _isCollecting = false;
    }

    private IEnumerator ReleaseCoroutine()
    {
        _isCollecting = true;
        // Set crucial variables.
        _lastCollectTime = 0;
        _isHandOccupied = false;

        // Re-set belongings of the target object.
        _objectRigid.simulated = true;
        _objectInRange.transform.SetParent(null);
        

        // Clear variable.
        _objectInRange = null;
        _objectRigid = null;

        yield return new WaitForSeconds(0.5f); // Wait for 0.5 sec

        // Re-enable the event listener. 
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.ExistCollectable, ObserveObject);
        _isCollecting = false;
        
        
    }

    private void SetIsPlayerWithinInteractable(bool targetBoolValue)
    {
        _isPlayerWithinRangeOfInteractable = targetBoolValue;
    }

    private bool CheckIsAllowRelease()
    {
        return !_isPlayerWithinRangeOfInteractable;
    }
    
}