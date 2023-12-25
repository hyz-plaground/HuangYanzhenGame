using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureButton : AtomicTrigger
{
    private Transform _button;
    private Vector3 _releasePosition;
    private Vector3 _pressedPosition;

    private void InitButtonPosition()
    {
        _button = transform.GetChild(0);
        try
        {
            _releasePosition = _button.position;
            _pressedPosition = _button.position - new Vector3(0, 0.1f, 0);
        }
        catch
        {
            throw new Exception("Button body not found!");
        }
    }

    private void Start()
    {
        InitButtonPosition();
    }

    /* Player stand on button, triggers react machine. */
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Play Animation
        StopAllCoroutines();
        StartCoroutine(DoButtonPressOrReleaseAnimation(true));
        // Disable the react machine.
        EnableReactMachine();   // Enable the react machine.
        //EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, true);
    }

    /* Player leaves a button, de-triggers react machine. */
    private void OnTriggerExit2D(Collider2D other)
    {
        // Play animation.
        StopAllCoroutines();
        StartCoroutine(DoButtonPressOrReleaseAnimation(false));
        // Disable the react machine.
        DisableReactMachine(); 
        //EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, false);
    }

    /* Play trigger & de-trigger animation. */
    private IEnumerator DoButtonPressOrReleaseAnimation(bool isButtonPressed)
    {
        Vector3 targetPosition = isButtonPressed ? _pressedPosition : _releasePosition;
        
        // Distance between current & target position.
        float distance = Vector3.Distance(_button.position, targetPosition);
        
        // Move
        while (distance > 0.01f)
        {
            // Distance to move each frame.
            float step = 0.1f * Time.deltaTime;

            // Position in the next frame.
            _button.position = Vector3.MoveTowards(_button.position, targetPosition, step);

            // Update distance.
            distance = Vector3.Distance(_button.position, targetPosition);

            // Wait for one frame.
            yield return null;
        }
    }

}
