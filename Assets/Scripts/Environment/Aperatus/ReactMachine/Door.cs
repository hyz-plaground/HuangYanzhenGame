using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Door : ReactMachine
{
    public float moveSpeed = MachineProperties.DOOR_MOVE_SPEED;
    public float doorMoveOffset = MachineProperties.DOOR_MOVE_OFFSET;
    public bool isSetDefaultOpen = false;       // Unchecked most of the time.
    
    // Static property
    private Vector3 _closedPosition;
    private Vector3 _openedPosition;
    
    // Dynamic property
    private bool _isOpen;

    /* Initialize Parameters */
    private void InitParams()
    {
        // Initialize positions
        _closedPosition = transform.position;
        _openedPosition = transform.position + new Vector3(0,transform.localScale.y/doorMoveOffset,0);
        
        // Initialize status
        _isOpen = isSetDefaultOpen;
        StopAllCoroutines();
        StartCoroutine(DoorMove(isSetDefaultOpen));
    }

    private new void Start()
    {
        // Start method in the parent class.
        base.Start();   // This have to be invoked otherwise no event can registered!
        InitParams();
    }

    /* React to triggers. */
    protected override void React(GameObject triggerTarget)
    {
        _isOpen = !_isOpen;
        StopAllCoroutines();
        StartCoroutine(DoorMove(_isOpen));
    }

    /* React to buttons.*/
    protected override void React(GameObject triggerTarget, bool isLetMachineEnable)
    {
        // Guardian: If the reference is not self, don't act!
        if (!ReferenceEquals(triggerTarget, gameObject))
            return;
            
        // Syncronize states
        _isOpen = isLetMachineEnable;
        
        // Stop current coroutines. This prevents door lagging.
        StopAllCoroutines();
        
        // Start a new coroutine.
        StartCoroutine(DoorMove(isLetMachineEnable));

    }
    
    /* Door move to target position. */
    private IEnumerator DoorMove(bool isLetMachineEnable)
    {
        Vector3 targetPosition = isLetMachineEnable ? _openedPosition : _closedPosition;
        
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