using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    [SerializeField]
    public GameObject targetReationObject;

    public Transform button;
    private Vector3 _releasePosition;
    private Vector3 _pressedPosition;

    private void Start()
    {
        button = transform.GetChild(0);
        try
        {
            _releasePosition = button.position;
            _pressedPosition = button.position - new Vector3(0, 0.1f, 0);
        }
        catch
        {
            throw new Exception("Button body not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        StopAllCoroutines();
        StartCoroutine(ButtonPressOrRelease(true));
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, true);
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StopAllCoroutines();
        StartCoroutine(ButtonPressOrRelease(false));
        EventCenterManager.Instance.EventTrigger(GameEvent.MachineTriggeredByButton, targetReationObject, false);
    }

    private IEnumerator ButtonPressOrRelease(bool isButtonPressed)
    {
        Vector3 targetPosition = isButtonPressed ? _pressedPosition : _releasePosition;
        
        // Distance between current & target position.
        float distance = Vector3.Distance(button.position, targetPosition);
        
        // Move
        while (distance > 0.01f)
        {
            // Distance to move each frame.
            float step = 0.1f * Time.deltaTime;

            // Position in the next frame.
            button.position = Vector3.MoveTowards(button.position, targetPosition, step);

            // Update distance.
            distance = Vector3.Distance(button.position, targetPosition);

            // Wait for one frame.
            yield return null;
        }
    }

}
