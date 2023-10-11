using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("Script refs")]
    public PlayerManager playerManager;

    [Header("Assign")]
    public float groundCheckRadius = 0.2f;
    public float maxGroundAngle = 45f;

    [Header("Settings")]
    public float groundCheckOffset = 0.15f;

    [Header("Player state")]
    private bool grounded;
    public bool isGrounded
    {
        get {return grounded;}
        set 
        {
            grounded = value;
            if (grounded == true)
            {
                onGroundedEvent.Invoke();
            }
            else
            {
                print("Ungrounded event");
                onUnGroundedEvent.Invoke();
            }
        }
    }

    [Header("Unity Events")]
    public UnityEvent onGroundedEvent;
    public UnityEvent onUnGroundedEvent;
    //player data
    public RaycastHit hit; 
    private bool wasGrounded;
    [HideInInspector] public ObjectProperties currentGroundProperties;
    public Collider[] groundColliders;
    Vector3 groundCheckPosition => new Vector3(playerManager.playerCol.bounds.center.x, playerManager.playerCol.bounds.center.y - playerManager.playerCol.bounds.size.y / 2 - groundCheckOffset, playerManager.playerCol.bounds.center.z);

    // Update is called once per frame
    void Update()
    {
        // Check if the groundCheck position intersects with any colliders
        groundColliders = Physics.OverlapSphere(groundCheckPosition, groundCheckRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        if (groundColliders.Length > 1)
        { 
            // Player is grounded if the surface angle is within the acceptable range
            isGrounded = true;

            //Gets the current ground material
            if (groundColliders[0].TryGetComponent(out ObjectProperties op))
            {
                print("SET GROUND PROPS");
                currentGroundProperties = op;
            }
            else
            {
                currentGroundProperties = null;
            }

            return;
        }

        // Player is not grounded if no valid surface angles are found
        isGrounded = false;
    }
     // Visualize the gizmo in the Unity editor scene view
    private void OnDrawGizmosSelected()
    {
        // Draw a gizmo sphere at the groundCheck position with the specified radius
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
    }
}
