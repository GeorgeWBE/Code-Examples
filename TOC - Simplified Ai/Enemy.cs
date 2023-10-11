using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations; 
public abstract class Enemy : MonoBehaviour, Damageable
{
    [Header("Assign - Animation")]
    public AnimationClip idleAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip detectAnimation; 
    public Animator enemyAnimator;

    [Header("Assign Components")]
    public SpriteRenderer enemyRenderer;
    public VisableEvents visableEventsScript;

    [Header("Set Variables")]
    public bool attacksWithCollision; 
    public Collider2D[] attackColliders; 
    public int damageDealt; 
    public int enemyHealth; 
    public float detectRange;
    public float attackRange; 
    public float firstAttackTime = 5; 
    public float fadeToIdleTime = 2;
    public float fadeToAttackTime = 1;

    [Header("Options")]
    public bool applyDamageWithCollison = true; 
    public Transform overrideDamagePoint;
    public Transform overrideDetectPoint;
    public abstract void Attack();
    public abstract void OnPlayerDetectionRepeat(); 
    public abstract void OnPlayerDetectionSingle();
    public abstract void OnPlayerUnDetect(); 
    public abstract void OnPlayerOutOfAttackRange(); 
    public abstract void OnEnemyDeath(); 

    [Header("Assign - Audio")]
    public AudioClip[] attackSound; 
    public AudioClip[] hitSound; 
    public AudioClip[] deathSound; 

    [Header("Unity Events")]
    public UnityEvent onAttack; 
    public UnityEvent onDeath; 

    [Header("Debug")]
    private bool canAttack = true; 
    private WaitForSeconds timeBetweenAttacks;
    private WaitForSeconds timeBeforeFirstAttack;
    public bool playerInAttackRange;
    public bool playerInDetectRange; 

    [SerializeField] private bool firstAttack = true; 
    private WaitForSeconds distanceCheckTime;
    private Coroutine distanceCheckCoro;

    public virtual void Awake() 
    {
        if(attackAnimation)
        {
            timeBetweenAttacks = new WaitForSeconds(attackAnimation.length); 
        }
        else
        {
            //timeBetweenAttacks = new WaitForSeconds(attackIntervalTime); 
        }
        timeBeforeFirstAttack = new WaitForSeconds(firstAttackTime); 
        if (!overrideDamagePoint)
        {
            overrideDamagePoint = this.transform;
        }

        if (!overrideDetectPoint)
        {
            overrideDetectPoint = this.transform;
        }
        distanceCheckTime = new WaitForSeconds(.25f);
    }

    public virtual void Start()
    {
        if (visableEventsScript)
        {
            //checks the distanc
            visableEventsScript.onVisable.AddListener(CheckDistance);

            //stops checking the distance
            visableEventsScript.onInvisable.AddListener(StopChecking);
        }
        else
        {
            StartCoroutine(DistanceCheck());
        }
    }
    void CheckDistance()
    {
        if (distanceCheckCoro == null)
        {
            distanceCheckCoro = StartCoroutine(DistanceCheck());
        }
    }
    void StopChecking()
    {
        if (distanceCheckCoro != null)
        {
            StopCoroutine(distanceCheckCoro);
            distanceCheckCoro = null;
            playerInAttackRange = false;
            playerInDetectRange = false;
        }
    }
    IEnumerator DistanceCheck()
    {
        for(;;)
        {
            //Distance check between player and the enemy, if its smaller or equal to the enemy detection range it will call the attack function
            if (Vector3.Distance(overrideDetectPoint.position, PlayerManager.instance.player.transform.position) <= detectRange)
            {
                if (!playerInDetectRange)
                {
                    playerInDetectRange = true; 
                    OnPlayerDetectionSingle(); 
                    if (enemyAnimator && detectAnimation)
                    {
                        enemyAnimator.CrossFade(detectAnimation.name, 1); 
                    }
                }
                OnPlayerDetectionRepeat();
                
                //checks if the player is in the attack range
                if (Vector3.Distance(overrideDamagePoint.position, PlayerManager.instance.player.gameObject.transform.position) <= attackRange)
                {
                    if (enemyAnimator && attackAnimation && !playerInAttackRange)
                    {
                        enemyAnimator.CrossFade(attackAnimation.name, 1);
                        //enemyAnimator.Play(attackAnimation.name); 
                    }
                    playerInAttackRange = true; 
                }
                else if (playerInAttackRange)
                {
                    playerInAttackRange = false;
                    OnPlayerOutOfAttackRange(); 
                    if (enemyAnimator)
                    {
                        if (detectAnimation)
                        {
                            enemyAnimator.CrossFade(detectAnimation.name, 1);
                        }
                        else if (idleAnimation)
                        {
                            enemyAnimator.CrossFade(idleAnimation.name, 1);
                        } 
                    }
                }
            }
            else if (playerInDetectRange)
            {
                playerInDetectRange = false;
                if (enemyAnimator && idleAnimation)
                {
                    enemyAnimator.CrossFade(idleAnimation.name, fadeToIdleTime);
                }
                //StopAllCoroutines(); 
                canAttack = true;
                OnPlayerUnDetect(); 
            }
            yield return distanceCheckTime;
        }
    }

    public void Damage(int damageDealt)
    {
        enemyHealth -= damageDealt;
        onAttack.Invoke();
        if (hitSound.Length > 0)
        {
            PoolManager.instance.PlaySound(hitSound[Random.Range(0, hitSound.Length)], transform.position, 1, 1);
        }
        if (enemyHealth <= 0)
        {
            OnEnemyDeath(); 
        }
    }
    public void AttackCheck()
    {
        if (attacksWithCollision)
        {
            for (int i = 0; i < attackColliders.Length; i++)
            {
                if (attackColliders[i].IsTouching(PlayerManager.instance.playerCollider))
                {
                    Attack();
                    onAttack.Invoke();
                    if (attackSound.Length > 0)
                    {
                        PoolManager.instance.PlaySound(attackSound[Random.Range(0, attackSound.Length)], transform.position, 1, 1);
                    }
                    return;
                }
            }
        }
        else if (playerInAttackRange)
        {
            Attack(); 
            onAttack.Invoke();
            if (attackSound.Length > 0)
            {
                PoolManager.instance.PlaySound(attackSound[Random.Range(0, attackSound.Length)], transform.position, 1, 1);
            }
        }
    }
    //public IEnumerator AttackPlayer()
    //{
    //    print("Player Attack"); 
    //    canAttack = false; 
    //    if (firstAttack)
    //    {
    //        firstAttack = false;
    //        yield return timeBeforeFirstAttack; 
    //    }
    //    else
    //    {
    //        yield return timeBetweenAttacks;
    //    }
    //    if (attackSound.Length > 0)
    //    {
    //        audioSource.PlayOneShot(attackSound[Random.Range(0, attackSound.Length)]); 
    //    }
    //    Attack(); 
    //    canAttack = true; 
    //}
    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.cyan;
        if (!overrideDetectPoint)
        {
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }
        else
        {
            Gizmos.DrawWireSphere(overrideDetectPoint.position, detectRange);
        }
        Gizmos.color = Color.red;
        if (!overrideDamagePoint)
        {
            Gizmos.DrawWireSphere(transform.position, attackRange); 
        }
        else
        {
            Gizmos.DrawWireSphere(overrideDamagePoint.position, attackRange);
        }
    }
}
