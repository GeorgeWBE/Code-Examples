using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("State manager options")]
    public StartState enemyState;
    public enum StartState {idle, wonder};
    public float searchRadius;
    [Range(0,360)]
    public float searchAngle;
    public float searchTime = 0.2f;

    [Header("Assign States")]
    public IdleState idleState;
    public ChaseState chaseState;
    public WonderState wonderState;
    public SearchState searchState;

    [Header("Assign Misc")]
    public Enemy enemy;
    public LayerMask playerMask;

    [Header("Info")]
    public bool playerPresent;

    private RaycastHit hit;
    private WaitForSeconds searchWait;
    public State currentState
    {
        get {return cState;}
        set 
        {
            if (enemy.isDead) {return;}

            //Removes the current state
            if (cState) {cState.LastState(); cState = null;}
            
            cState = value;

            //Activates new state
            cState.FirstState();
        }
    }
    private State cState;

    private void Awake() 
    {
        //sets start state 
        if (enemyState == StartState.idle)
        {
            currentState = idleState;
        }
        else if (enemyState == StartState.wonder)
        {
            currentState = wonderState;
        }
    }

    private void Start()
    {
        //Sets Coro 
        searchWait = new WaitForSeconds(searchTime);
        StartCoroutine(FindPlayerAlgorithm());
    }

    private void Update() 
    {
        if (currentState)
        {
            currentState.RecursiveState();
        }
    }

    public IEnumerator FindPlayerAlgorithm()
    {
        while (true)
        {
            print("Search for player");
            yield return searchWait;
            FindPlayer();
        }
    }
    public void FindPlayer()
    {

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, searchRadius, playerMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < searchAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget);
                if (hit.collider && hit.collider.tag == "Player")
                {
                    playerPresent = true;
                }
                else
                {
                    playerPresent = false;
                }
            }
            else
            {
                playerPresent = false;
            }
        }
        else if (playerPresent)
        {
            playerPresent = false;
        }
    }
}
