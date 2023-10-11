using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System; 

public abstract class Pickup : MonoBehaviour
{
    public UnityEvent onPickupEvent; 
    [Header("Pickup Behaviour")]
    public float effectTime; 
    public bool anmOnPickup = true; 
    public bool destoryAfterAnm = true;
    public bool repeatPickup = false; 
    [Header("How many times can this pickup's effect stack? Set to 1 if you want it unstackable")]
    public int stackableAmount = 1; 
    [Header("Assign - Animation")]
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private AnimationClip pickupAnimation;
    [SerializeField] private Animator animator; 
    [Header("Assign - Audio")]
    [SerializeField] private AudioClip[] pickUpSound; 
    
    [Header("Assign -- Graphics")]
    [SerializeField] private SpriteRenderer pickupSprite;
    [SerializeField] private Sprite pickupIcon;
    [Header("Pickup Infomation -- IMPORTANT")]
    public string pickupTag; 
    [Header("Debug")]
    [SerializeField] private bool canPickup = true;
    [SerializeField] private PowerupUi powerupUi;
    [SerializeField] private int firstNullSlot;
    [SerializeField] private int firstNullPickup;
    public int pickupIndex; 
    private WaitForSeconds effectTimeVar;
    void Awake() 
    {
        effectTimeVar = new WaitForSeconds(effectTime); 
        if (pickupTag.Length < 1)
        {
            pickupTag = this.gameObject.name;
            Debug.LogError("No tag given to powerup"); 
        }
    }
    public abstract void OnPickup(); 
    public abstract void RemoveEffect(); 
    //called when the player collides with the pickup
    private void OnTriggerEnter2D(Collider2D other) 
    {
        //Checks if the colliding object is the player
        if (other.tag == "Player")
        {
            StartCoroutine(StartEffect()); 
            if (effectTime > 0)
            {
                //Checks if a pickup of the same type(tag) exists, if it does then it is deactivated and replaced 
                //with this one, otherwise this pickup is added to the array 
                int arrayIndex = CheckEffect();
                if (arrayIndex != -1)
                {
                    PowerupManager.instance.pickTypes[arrayIndex].RemoveEffect(); 
                    PowerupManager.instance.pickTypes[arrayIndex].StopAllCoroutines();
                    PowerupManager.instance.pickTypes[arrayIndex].SetTimerUi(false);
                    PowerupManager.instance.pickTypes[arrayIndex].SetPickup();
                    PowerupManager.instance.pickTypes[arrayIndex] = null;
                    PowerupManager.instance.pickTypes[arrayIndex] = this;
                }
                else
                {
                    firstNullPickup = Array.FindIndex(PowerupManager.instance.pickTypes, I => I == null);
                    if (firstNullPickup != -1)
                    {
                        PowerupManager.instance.pickTypes[firstNullPickup] = this;
                        pickupIndex = firstNullPickup; 
                    }
                }
                SetTimerUi(true); 
            }
            OnPickup();
            onPickupEvent.Invoke(); 
            StartCoroutine(EffectTimer()); 
        }
    }
    //Waits for the time of the effect then calls the function which removes the effect 
    public IEnumerator EffectTimer()
    {
        yield return effectTimeVar;
        //reactivates the pickup if its repeatable or destroys it otherwise
        SetPickup(); 
        //removes the buffs added by the pickup 
        RemoveEffect(); 
        //deactivates the timer ui used to repesent this powerup
        if (effectTime > 0 && powerupUi)
        {
            SetTimerUi(false);
            PowerupManager.instance.pickTypes[pickupIndex] = null;
        }
    }
    //if the pickup is repeatable, it is reactivated, else it's destroyed
    public void SetPickup()
    {
        if (!repeatPickup)
        {
            Destroy(this.gameObject); 
        }
        else
        {
            if (idleAnimation)
            {
                animator.Play(idleAnimation.name); 
            }
            pickupSprite.enabled = true;
            this.gameObject.GetComponent<Collider2D>().enabled = true; 
        }
    }
    //deactivates the sprite for the pickup once the animation has finished if there is one
    public IEnumerator StartEffect()
    {
        //Plays an animation if the bool is true
        this.gameObject.GetComponent<Collider2D>().enabled = false;
        //audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 

        //Plays a random sound effect from the array
        PoolManager.instance.PlaySound(pickUpSound[UnityEngine.Random.Range(0, pickUpSound.Length)], transform.position, 0.5f, 1);
        if (anmOnPickup)
        {
            animator.Play(pickupAnimation.name);
            yield return new WaitForSeconds(pickupAnimation.length); 
        }
        else
        {
            yield return new WaitForSeconds(0); 
        }
        pickupSprite.enabled = false; 
    }
    //if true: finds a slot in the pickupSlots array and adds the current powerup and activates ui which..
    //shows the time left on the pickup
    //if false: it sets the pickupslot used for this pickup to null and disables the ui for it
    public void SetTimerUi(bool setActive)
    {
        if (setActive)
        {
            UiManager uiManager =  SceneHandler.instance.uiManager;
            firstNullSlot = Array.FindIndex(uiManager.pickupSlots, I => I == null);
            if (firstNullSlot != -1)
            {
                uiManager.pickupSlots[firstNullSlot] = this;
                powerupUi = uiManager.powerupUiTiles[firstNullSlot]; 
                if (pickupIcon)
                {
                    powerupUi.iconSlot.sprite = pickupIcon;
                }
                powerupUi.EnableElements(true, effectTime); 
            }
        }
        else
        {
            powerupUi.EnableElements(false, 0);
            powerupUi = null;   
            SceneHandler.instance.uiManager.pickupSlots[firstNullSlot] = null;
        }

    }
    //checks through the picktype array to check if this pickup matches any currently active ones
    //if it does then the array index of it is returned, otherwise -1 is returned to signify no match
    public int CheckEffect()
    {
        for (int i = 0; i < PowerupManager.instance.pickTypes.Length; i++)
        {
            if (PowerupManager.instance.pickTypes[i] && PowerupManager.instance.pickTypes[i].pickupTag == this.pickupTag)
            {
                return i;
            }
        }
        return -1; 
    }
}
