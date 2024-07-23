using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private bool _isHandOccupied = false;
    private float _lastCollectTime;
    private bool _isCollecting = false;
    private bool _isPlayerWithinRangeOfInteractable = false;

    // Event center manager instance
    private EventCenterManager ecm = EventCenterManager.Instance;

    // Observer
    private readonly Observer _observer = new EnvAware().Observer;

    private void InitDelegates()
    {
        // Collectable object tells player that there is an object to be collected.
        ecm.AddEventListener<GameObject>(GameEvent.ExistCollectable, _observer.ObserveObject);
        ecm.AddEventListener<GameObject>(GameEvent.NonExistCollectable, _observer.IgnoreObject);

        // Player intend to collect object.
        ecm.AddEventListener(GameEvent.PlayerTryInteract, InteractWithThisObject);

        // Player is in range of an interactable object.
        ecm.AddEventListener<bool>(GameEvent.WithinRangeOfInteractable,
            SetIsPlayerWithinInteractable);
    }

    private void Start()
    {
        _lastCollectTime = 0;
        InitDelegates();
    }

    /* Designed in a coroutine way so that it is not bothered by frequent happen of events.*/
    // This part comes from chat gpt. I have absolutely no idea how it works.
    // (In fact, I'm just being lazy to interpret this. As long as it works it's fine.)
    private void Do(GameEvent e, GameObject observedObj, Func<IEnumerator> action)
    {
        ecm.EventTrigger(e, observedObj);
        StopAllCoroutines();
        StartCoroutine(action());
    }

    private void InteractWithThisObject()
    {
        if (!_observer.ObservedObj || _isCollecting)
            return;

        if (!_isHandOccupied) // Collect object.
        {
            Do(GameEvent.PlayerCollectObject, _observer.ObservedObj, CollectCoroutine);
        }
        else if (CheckIsAllowRelease()) // Release object.
        {
            Do(GameEvent.PlayerReleaseObject, _observer.ObservedObj, ReleaseCoroutine);
        }
    }

    private IEnumerator CollectCoroutine()
    {
        _isCollecting = true;
        // Set crucial variables.
        _lastCollectTime = Time.time;
        _isHandOccupied = true;

        // Set position & belongings of the target object.
        _observer.ObservedObj.transform.position = transform.position;
        _observer.ObservedObjRigid.simulated = false;
        _observer.ObservedObj.transform.SetParent(gameObject.transform);

        // Remove ExistCollectable event listener. Do not listen to outer object anymore.
        ecm.RemoveEventListener<GameObject>(GameEvent.ExistCollectable,
            _observer.ObserveObject);

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
        _observer.ObservedObjRigid.simulated = true;
        _observer.ObservedObj.transform.SetParent(null);
        
        // Clear variable.
        _observer.ObservedObj = null;
        _observer.ObservedObjRigid = null;

        yield return new WaitForSeconds(0.5f); // Wait for 0.5 sec

        // Re-enable the event listener. 
        ecm.AddEventListener<GameObject>(GameEvent.ExistCollectable, _observer.ObserveObject);
        _isCollecting = false;
    }

    private void SetIsPlayerWithinInteractable(bool targetBoolValue)
    {
        _isPlayerWithinRangeOfInteractable = targetBoolValue;
    }

    /// <summary>
    /// If player is in the range of an interactable (e.g., A button),
    /// it can't release the collectable in its hand.
    /// </summary>
    /// <returns></returns>
    private bool CheckIsAllowRelease()
    {
        return !_isPlayerWithinRangeOfInteractable;
    }
}