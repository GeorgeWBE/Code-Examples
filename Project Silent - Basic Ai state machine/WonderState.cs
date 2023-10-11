using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class WonderState : State
{
    [Header("Wander Path Options")]
    public bool wanderPath;
    public Transform[] movePoints;

    [Header("Random Wander Options")]
    public float wanderRadius = 5;
    public Transform setWanderPosition;

    //Info
    private Vector3 startTrans;
    private int currentPoint = 0;
    private List<Vector3> pos = new List<Vector3>();

    private void Awake() 
    {
        for (int i = 0; i < movePoints.Length; i++)
        {
            pos.Add(movePoints[i].position);
        }
    }

    public override void EndState()
    {
        enemy.navMeshAgent.ResetPath();
        currentPoint = 0;
    }

    public override void StartState()
    {
        if (!wanderPath)
        {
            //Sets wander position
            if (setWanderPosition)
            {
                enemy.navMeshAgent.SetDestination(GetRandomPointWithinRadius(setWanderPosition.position, wanderRadius));
            }
            else
            {
                startTrans = transform.position;
                enemy.navMeshAgent.SetDestination(GetRandomPointWithinRadius(startTrans, wanderRadius));
            }
        }
        else if (pos.Count > 0) 
        {
            enemy.navMeshAgent.SetDestination(pos[currentPoint]);
        }

        //Sets move animation
        enemy.enemyAnimator.CrossFade("Move", 0.25f);

        enemy.navMeshAgent.isStopped = false;
    }

    public override void UpdateState()
    {
        if (enemy.navMeshAgent.remainingDistance <= 0.1f)
        {
            if (!wanderPath)
            {
                //Sets wander position
                if (setWanderPosition)
                {
                    enemy.navMeshAgent.SetDestination(GetRandomPointWithinRadius(setWanderPosition.position, wanderRadius));
                }
                else
                {
                    enemy.navMeshAgent.SetDestination(GetRandomPointWithinRadius(startTrans, wanderRadius));
                }
            }
            else if (pos.Count > 0)
            {
                currentPoint++;
                if (currentPoint >= pos.Count) {currentPoint = 0;}
                enemy.navMeshAgent.SetDestination(pos[currentPoint]);
            }
        }

        //State condition 
        if (stateManager.playerPresent && stateManager.chaseState) {stateManager.currentState = stateManager.chaseState;}
    }

    private Vector3 GetRandomPointWithinRadius(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomDirection, out navMeshHit, radius, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        }

        return center;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
