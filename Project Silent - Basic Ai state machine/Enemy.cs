using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamagable, IHear
{
    [Header("Assign Components")]
    public StateManager stateManager;
    public NavMeshAgent navMeshAgent;
    public Rigidbody ragdollRb;
    public Animator enemyAnimator;

    [Header("Enemy Settings")]
    public float enemyHealth = 100;

    //Data
    private float currentHealth;
    [HideInInspector] public bool isDead = false;

    private void Awake()
    {
        currentHealth = enemyHealth;
    }

    public void Damage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0) 
        {
            EnemyDeath();
        }

        if (!stateManager.searchState) {return;}
        stateManager.searchState.target = PlayerManager.instance.transform.position;
        stateManager.currentState = stateManager.searchState;
    }

    public void OnHear(Vector3 pos, float range)
    {
        if (!stateManager.searchState ||  stateManager.chaseState && stateManager.currentState == stateManager.chaseState) 
        {
            if (stateManager.chaseState && stateManager.currentState == stateManager.chaseState && stateManager.chaseState.canAttack)
            {
                print("COG SJAOFDJOSJFOJF");
                Vector3 targetDirection = transform.forward;

                float angle = Vector3.SignedAngle(targetDirection, pos, Vector3.up);

                if (angle > 0)
                {
                    Debug.Log("Vector is to the right.");
                    enemyAnimator.Play("Dodge_Right");
                }
                else if (angle < 0)
                {
                    Debug.Log("Vector is to the left.");
                    enemyAnimator.Play("Dodge_Left");
                }
            }

            return;     
        }

        stateManager.searchState.target = pos;
        stateManager.currentState = stateManager.searchState;
    }

    public void EnemyDeath()
    {
        //Disables nav agent
        navMeshAgent.enabled = false;
        enemyAnimator.Play("Idle");
        isDead = true;

        //Enables the rb
        ragdollRb.isKinematic = false;

        //Disables enemy scripts
        stateManager.enabled = false;
        this.enabled = false;
    }
}
