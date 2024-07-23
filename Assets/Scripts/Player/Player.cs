using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace Player
{

    public class Player : MonoBehaviour
    {
        [SerializeField]

        // Basic Player Property
        public Collider2D playerCollider;       // Player Collider
        private Rigidbody2D _rigid;             // Player's main rigid body

        // Environment Awareness
        private readonly EnvAware _envAware = new EnvAware();
        
        // Player Movement
        private Movements _movements;

        // Initialize parameters.
        private void InitParams()
        {
            _rigid = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            _movements = new Movements(_envAware, playerCollider, transform, _rigid);
        }

        // Initialize event listeners.
        private void InitDelegates()
        {
            EventCenterManager.Instance.AddEventListener<bool>(GameEvent.PlayerEnterSpace, _movements.OnPlayerEnterSpace);
        }

        private void Start()
        {
            InitParams(); // Initialize player properties.
            InitDelegates(); // Initialize event listeners.
        }

        private void Update()
        {
            _movements.Move();
        }
        
        public void OnDrawGizmos()
        {
            _envAware.groundCheck.OnDrawGizmos(playerCollider, transform);
        }
    }
}