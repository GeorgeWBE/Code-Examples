using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Walker : Enemy
{
    [Header("Assign - Walker")]
    [SerializeField] private float moveForce; 
    [SerializeField] private Rigidbody2D enemyRigidbody;
    [SerializeField] private bool canSwitchDir = true;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool switchDirBuffered = false;
    [SerializeField] private float timeToAllowSwitch = 2;
    [SerializeField] private float timeToSwitch = 5; 
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float groundDeceleration;
    private WaitForSeconds canSwitchTimer; 
    private WaitForSeconds startSwitchTimer;
    public Vector2 moveDirection;
    private ContactPoint2D[] contacts;
    private float timer;
    public override void Awake() 
    {
        base.Awake();

        //moveDirection = new Vector2(-transform.forward.magnitude, 0);
        startSwitchTimer = new WaitForSeconds(timeToSwitch);
        canSwitchTimer = new WaitForSeconds(timeToAllowSwitch);
        contacts = new ContactPoint2D[20];
    }
    public override void Attack()
    {
        PlayerManager.instance.Damage(damageDealt);  
    }
    public override void OnEnemyDeath()
    {

    }
    public override void OnPlayerDetectionRepeat()
    {

    }
    public override void OnPlayerDetectionSingle()
    {
        
    }
    public override void OnPlayerUnDetect()
    {
        
    }
    public override void OnPlayerOutOfAttackRange()
    {

    }
    private void FixedUpdate() 
    {
        //enemyRigidbody.AddForce(moveDirection * moveForce); 
        if (canMove && enemyRigidbody && enemyRigidbody.velocity.y < 1)
        {
            Move(groundAcceleration, groundDeceleration);
        }
    }
    public IEnumerator BetweenSwitch()
    {
        canSwitchDir = false;
        //canMove = false;
        yield return timeToSwitch;
        //Switches direction
        transform.forward = -transform.forward; 
        canMove = true;
        moveDirection = new Vector2(-moveDirection.x, 0);
        StartCoroutine(BeforeCanSwitch()); 
    }
    public IEnumerator BeforeCanSwitch()
    {
   
        yield return canSwitchTimer;
        canSwitchDir = true;

        if (switchDirBuffered)
        {
            StartCoroutine(BetweenSwitch());
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    { 
        if (canSwitchDir && other.gameObject.tag != PlayerManager.instance.playerTag)
        {
            StartCoroutine(BetweenSwitch());  
        }
        else if (other.gameObject.tag != PlayerManager.instance.playerTag)
        {
            switchDirBuffered = true;
        }
    }
    void Move(float accel, float decel)
    {
        Vector2 difference = new Vector2(0f, 0f);
        difference.x = moveDirection.x * moveForce - enemyRigidbody.velocity.x;
        enemyRigidbody.velocity += (difference * accel);   
        // enemyRigidbody.velocity = new Vector2(moveDirection.x * moveForce, 0);
    }
}
