using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade_Jump : Upgrade
{
    public override void SetValues()
    {
        if (upgradeLevel > 0)
        {
            playerData.setPlayerJumpForce = (playerData.defaultPlayerJumpForce + upgradeValues[upgradeLevel - 1]); 
        }
        else 
        {
            playerData.setPlayerJumpForce = playerData.defaultPlayerJumpForce; 
        }
    }
}
