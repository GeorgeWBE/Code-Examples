using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup_Speed : Pickup
{
    [Header("Pickup Speed Vars")]
    public float playerSpeedAddition = 2; 
    public override void OnPickup()
    {
        PlayerManager.instance.playerMovement.playerGroundSpeed += playerSpeedAddition;
    }
    public override void RemoveEffect()
    {
        PlayerManager.instance.playerMovement.playerGroundSpeed -= playerSpeedAddition;
    }
}
