using System;
using UnityEngine;

public class KeyHolder : MonoBehaviour
{
    public Rigidbody2D parentRigid;
    public float forceMagnitude = 9.8f;

    private void Start()
    {
        var parentTransform = transform.parent;
        parentRigid = parentTransform.gameObject.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector3 upwardForce = forceMagnitude * Vector3.up; // 在竖直方向上施加力的向量，力的大小取决于forceMagnitude变量

        parentRigid.AddForce(upwardForce); 
    }
    
}