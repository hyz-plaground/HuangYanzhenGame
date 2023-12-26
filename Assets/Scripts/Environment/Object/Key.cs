using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public class Key : Collectable
{
    [SerializeField] 
    public MatchCode keyMatchCode = MatchCode.DEF;

    private GameObject NearestLock { get; set; } // This should be unique.
    private void InitDelegates()
    {
        EventCenterManager.Instance.AddEventListener<GameObject, MatchCode>(GameEvent.WithinRangeOfLock,ObserveLock);
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.OutOfRangeOfLock,IgnoreLock);
        EventCenterManager.Instance.AddEventListener<GameObject>(GameEvent.PlayerReleaseObject,OnPlayerRelease);
    }
    
    private new void Start()
    {
        base.Start();
        InitDelegates();
    }

    #region Observe and Ignore Lock
    private void ObserveLock(GameObject targetLock, MatchCode targetLockMatchCode)
    {
        // Code doesn't match, then simply don't "observe" this lock!
        if (keyMatchCode != targetLockMatchCode)
            return;
        NearestLock = targetLock;
    }

    private void IgnoreLock(GameObject targetLock)
    {
        // Key never observed this lock, or key observed a different lock.
        if (!NearestLock || NearestLock != targetLock)
            return;
        NearestLock = null;
    }
    
    #endregion

    public void ChangeMatchCode(MatchCode targetMatchCode)
    {
        keyMatchCode = targetMatchCode;
    }

    private void OnPlayerRelease(GameObject targetReleasedObject)
    {
        if (!targetReleasedObject || targetReleasedObject != gameObject)
            return;
        
        Vector3 nearestLockPosition = NearestLock.transform.position;
        
        PermanentlyBanCollect();    // To make object no longer be collectable to player.

        //StopAllCoroutines();
        StartCoroutine(KeyMoveToLockCoroutine(nearestLockPosition));
    }
    
    private IEnumerator KeyMoveToLockCoroutine(Vector3 nearestLockPosition)
    {
        
        // Distance between current & target position.
        var distance = Vector3.Distance(transform.position, nearestLockPosition);
        
        // Move
        while (distance > 0.01f)
        {
            // Distance to move each frame.
            var step = 10f * Time.deltaTime;    // TODO: Move speed change.

            // Position in the next frame. 
            transform.position = Vector3.MoveTowards(transform.position, nearestLockPosition, step);

            // Update distance.
            distance = Vector3.Distance(transform.position, nearestLockPosition);
            
            // Wait for one frame.
            yield return null;
        }
        
        DestroyImmediate(gameObject);
    }

    private void PermanentlyBanCollect()
    {
        Rigid.simulated = false;
        transform.SetParent(NearestLock.transform);
        IsAllowCollect = false;
    }

    private void OnDestroy()
    {
        EventCenterManager.Instance.EventTrigger(GameEvent.KeyInsertedInLock,keyMatchCode);
    }
}