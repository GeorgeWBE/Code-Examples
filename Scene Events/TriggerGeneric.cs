using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

public class TriggerGeneric : MonoBehaviour
{
    public enum CollisionOptions {AnyCollider, PlayerOnly, TagOnly, ColliderOnly}; 
    [Header("Trigger options")]
    public CollisionOptions collisionOptions;
    public string triggerTag;
    public Collider2D[] colliderObjs; 
    [Header("Unity Events -- Trigger")]
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerEnterOnce;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;
    [Header("Unity Events -- Collision")]
    public UnityEvent collisionEnter;
    public UnityEvent collisionEnterOnce;
    public UnityEvent collisionStay;
    public UnityEvent collisionExit; 
    private int triggerIterations = 0; 
    private int collisionIterations = 0;

    private void OnTriggerEnter(Collider other) 
    {
        if (CheckCollider(other))
        {
            onTriggerEnter.Invoke();
            if (triggerIterations == 0)
            {
                onTriggerEnterOnce.Invoke();
                triggerIterations++;
            }
        }
    }
    private void OnTriggerStay(Collider other) 
    {
        if (CheckCollider(other))
        {
            onTriggerStay.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (CheckCollider(other))
        {
            onTriggerExit.Invoke(); 
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (CheckCollider(other.collider))
        {
            if (collisionIterations == 0)
            {
                collisionEnterOnce.Invoke();
                collisionIterations++;
            }
            collisionEnter.Invoke();
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if (CheckCollider(other.collider))
        {
            collisionExit.Invoke();
        }
        
    }
    private void OnCollisionStay(Collision other) 
    {
        if (CheckCollider(other.collider))
        {
            collisionStay.Invoke();
        }
    }
    public bool CheckCollider(Collider collider)
    {
        switch (collisionOptions)
        {
            case CollisionOptions.TagOnly:
                if (collider.gameObject.tag == triggerTag) { return true; } else { return false; } 

            case CollisionOptions.PlayerOnly:
                if (collider.gameObject.tag == "Player") { return true; } else { return false; }
            case CollisionOptions.ColliderOnly:
                for (int i = 0; i < colliderObjs.Length; i++)
                {
                    if (collider == colliderObjs[i]) { return true; } 
                }
                return false; 
                
            case CollisionOptions.AnyCollider: 
                if (collider) { return true; } else { return false; }
            default:
                return false;
        }
    }

}
