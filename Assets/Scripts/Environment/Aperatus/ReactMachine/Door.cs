using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : ReactMachine
{
    public float moveSpeed = MachineProperties.DOOR_MOVE_SPEED;
    public float doorMoveOffset = MachineProperties.DOOR_MOVE_OFFSET;
    private Vector3 _closedPosition;
    private Vector3 _openedPosition;

    private void Start()
    {
        // Start method in the parent class.
        base.Start();   // This have to be invoked otherwise no event can registered!
        _closedPosition = transform.position;
        _openedPosition = new Vector3(transform.position.x,transform.position.y + transform.localScale.y/doorMoveOffset, transform.position.z);
    }

    protected override void React(GameObject triggerTarget)
    {
    }

    protected override void React(GameObject triggerTarget, bool isEnable)
    {
        // Guardian: If the reference is not self, don't act!
        if (!ReferenceEquals(triggerTarget, gameObject))
            return;
            
        // Stop current coroutines. This prevents door lagging.
        StopAllCoroutines();
        
        // Start a new coroutine.
        StartCoroutine(DoorMove(isEnable));

    }
    
    // Door move to target position.
    private IEnumerator DoorMove(bool isEnable)
    {
        Debug.Log("Coroutine Started.");

        Vector3 targetPosition = isEnable ? _openedPosition : _closedPosition;
        
        // Distance between current & target position.
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Move
        while (distance > 0.01f)
        {
            // Distance to move each frame.
            float step = moveSpeed * Time.deltaTime;

            // Position in the next frame.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // Update distance.
            distance = Vector3.Distance(transform.position, targetPosition);

            // Wait for one frame.
            yield return null;
        }
    }
}