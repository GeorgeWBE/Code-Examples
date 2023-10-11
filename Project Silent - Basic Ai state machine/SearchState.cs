using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : State
{
    [HideInInspector] public Vector3 target;
    public override void EndState()
    {

    }

    public override void StartState()
    {
        //Sets enemy animation
        enemy.enemyAnimator.CrossFade("Move", 0.25f);

        //Activates the navmeshagent 
        enemy.navMeshAgent.isStopped = false;
        enemy.navMeshAgent.SetDestination(target);
    }

    public override void UpdateState()
    {
        //State condition 
        if (stateManager.playerPresent && stateManager.chaseState) {stateManager.currentState = stateManager.chaseState; return; }
        if (!enemy.navMeshAgent.isActiveAndEnabled) { print("NAVMESH AGENT DISABLED"); }
        if (enemy.navMeshAgent.remainingDistance <= 0.1f)
        {
            if (stateManager.wonderState)
            {
                stateManager.currentState = stateManager.wonderState;
            }
            else if (stateManager.idleState)
            {
                stateManager.currentState = stateManager.idleState;
            }
        }

    }
}
