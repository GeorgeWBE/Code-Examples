using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    [Header("Assign - STATE")]
    public StateManager stateManager;
    public Enemy enemy;

    [Header("Generic State Settings")]
    public bool overrideSpeed;
    public float speed = 2.5f;

    public void FirstState()
    {
        //Overrides speed
        if (overrideSpeed) {enemy.navMeshAgent.speed = speed;}

        StartState();
    }

    public void RecursiveState()
    {
        UpdateState();
    }

    public void LastState()
    {
        EndState();
    }

    //ABSTRACT METHODS
    public abstract void StartState();
    public abstract void UpdateState();
    public abstract void EndState();
}
