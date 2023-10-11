using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChaseState : State
{
    [Header("Settings")]
    public float attackDistance = 1;
    public float hitDistance = 1;
    public float attackBuffer = 2;
    public float attackDamage = 1;

    [Header("Circle Settings")]
    public int numberOfPoints = 10;
    public float circleRadius = 5f;
    public float rotationSpeed = 1;
    private List<Vector3> points = new List<Vector3>();
    private int circleInt = 0;

    [Header("Assign")]
    public AnimationClip attackAnimation;
    public Transform attackTransform;
    public EnemyAttack customAttack;

    [Header("Unity Events")]
    public UnityEvent onAttack;

    //Data
    [HideInInspector] public bool canAttack = true;
    private bool isInAttackRange = false;
    private WaitForSeconds attackWait;
    private Coroutine attackCoro;
    [HideInInspector] public Vector3 playerLastPosition;
    private void Awake() 
    {
        attackWait = new WaitForSeconds(attackAnimation.length + attackBuffer);
    }
    private void Start() 
    {
        if (!attackTransform) { attackTransform = transform; }
    }

    public override void EndState()
    {
        StopAllCoroutines();

        if (customAttack)
        {
            customAttack.AttackFinished();
        }

        canAttack = true;


        //Stops the enemy
        enemy.navMeshAgent.isStopped = true;
        enemy.navMeshAgent?.SetDestination(transform.position);
    }

    public override void StartState()
    {

        //Gets player positon 
        playerLastPosition = PlayerManager.instance.transform.position;

        //Sets enemy animation
        enemy.enemyAnimator.CrossFade("Move", 0.25f);

        //Activates the navmeshagent 
        enemy.navMeshAgent.SetDestination(PlayerManager.instance.transform.position);
        enemy.navMeshAgent.isStopped = false;
    }

    public override void UpdateState()
    {
        //State condition 
        if (!stateManager.playerPresent && canAttack) {stateManager.searchState.target = playerLastPosition; stateManager.currentState = stateManager.searchState; return; }

        //Gets player positon 
        playerLastPosition = PlayerManager.instance.transform.position;

        //Checks if the player is in attack distance
        if (Vector3.Distance(attackTransform.position, PlayerManager.instance.transform.position) <= attackDistance)
        {
            if (!isInAttackRange)
            {
                points = GeneratePointsOnCircle(numberOfPoints, circleRadius, PlayerManager.instance.transform.position);
                circleInt = 0;
            }

            isInAttackRange = true;

            if (Vector3.Distance(transform.position, PlayerManager.instance.transform.position) <= 2f)
            {
                circleInt++;
                if (circleInt == numberOfPoints)
                {
                    circleInt = 0;
                }

                if (enemy.navMeshAgent.isActiveAndEnabled)
                {
                    enemy.navMeshAgent?.SetDestination(points[circleInt]);
                }
            }

            // Calculate the rotation to face the target
            Vector3 targetDirection = PlayerManager.instance.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            // Smoothly rotate the agent
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (canAttack)
            {
                print("STARTING ATTACK: " + stateManager.currentState);
                canAttack = false;
                if (attackCoro != null) {StopCoroutine(AttackPlayer()); attackCoro = null;}
                attackCoro = StartCoroutine(AttackPlayer());
            }

        }
        else if (canAttack)
        { 
            if (isInAttackRange) 
            {
                isInAttackRange = false;

                //Sets destination for the enemies
                enemy.navMeshAgent?.SetDestination(PlayerManager.instance.transform.position);
            }
        }
    }

    public void TryHurt()
    {
        if (Vector3.Distance(attackTransform.position, PlayerManager.instance.transform.position) <= hitDistance)
        {
            PlayerManager.instance.Damage(attackDamage);
        }
    }

    public IEnumerator AttackPlayer()
    {
        //Sets bool
        canAttack = false;

        if (customAttack)
        {
            print("DOING ATTACK WHYTY");
            customAttack.OnAttack();
        }
        else
        {
            //Starts animation
            enemy.enemyAnimator.CrossFade("Attack", 0.25f);
        }

        //Calls event
        onAttack.Invoke();

        if (customAttack)
        {
            //Halts for a period
            yield return new WaitForSeconds(attackBuffer);
        }
        else
        {
            //Halts for a period
            yield return attackWait;
        }

        //Sets bool
        canAttack = true;

        if (customAttack)
        {
            customAttack.AttackFinished();
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Vector3 vec = new Vector3(transform.position.x, transform.position.y, transform.position.z + attackDistance);
        Gizmos.DrawLine(transform.position, vec);
    }

    private List<Vector3> GeneratePointsOnCircle(int numPoints, float radius, Vector3 center)
    {
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * (360f / numPoints);
            float x = center.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = center.y;
            float z = center.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            points.Add(new Vector3(x, y, z));
        }

        return points;
    }
}
