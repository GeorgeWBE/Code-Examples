using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public override void EndState()
    {
        
    }

    public override void StartState()
    {
        //Stops the enemy
        enemy.navMeshAgent.isStopped = true;
    }

    public override void UpdateState()
    {
        //State condition 
        if (stateManager.playerPresent && stateManager.chaseState) {stateManager.currentState = stateManager.chaseState;}
    }

}
