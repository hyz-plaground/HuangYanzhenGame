using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Player.IO
{
    public enum Intention
    {
        X,
        Jump,
        Fly,
        Rush,
        Interact
    }

    public class InputConfig
    {
        private readonly Dictionary<Intention, Func<object>> _inputConfig = new()
        {
            { Intention.X, () => Input.GetAxis("Horizontal") },
            { Intention.Jump, () => Input.GetKey(KeyCode.Space) },
            { Intention.Fly, () => Input.GetKey(KeyCode.Space) },
            { Intention.Rush, () => Input.GetKey(KeyCode.R) },
            { Intention.Interact, () => Input.GetKey(KeyCode.F) },
        };
        
        /// <summary>
        /// Get the corresponding input values for a desired player movement.
        /// </summary>
        /// <param name="item">Name of the movement.</param>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <returns>The input value of the desired player movement.</returns>
        public T GetInput<T>(Intention item)
        {
            return (T)_inputConfig[item]();
        }
    }
}