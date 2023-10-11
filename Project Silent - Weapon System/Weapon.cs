using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : MonoBehaviour, ITabs
{
    [Header("Data for the weapon -- REQUIRED")]
    public WeaponData weaponData;

    [Header("Assign")]
    public GrabType weaponGrab;
    public Transform shellTransform;
    public GameObject shellObject;

    [Header("Weapon VFX")]
    public ParticleSystem weaponVFX;

    [Header("Weapon Sounds")]
    public AudioClip[] attackSounds;
    public AudioClip[] aimSounds;

    [Header("Aiming Options")]
    public Vector3 aimPosition;
    public float fovAddition = 10;
    public float aimSpeed;

    [Header("Other weapon options")]
    public AttackType attackType;
    public float fireRate;
    public float refreshBuffer;
    public float attackBuffer;
    public float attackDamage = 1;
    public Vector3 aimPosOffset;
    public Quaternion aimRotOffset;
    public float weaponAnmSwapTimer = 2;
    public float ejectionForce = 0.5f;
    public float ejectionTorque = 5f; 
    public bool WeaponWheel 
    { 
        get => true; 
    }

    //Private data 
    public enum AttackType{semiAuto, auto, burst};
    public int burstAmount;
    [HideInInspector] public int currentClip
    {
        get {return cClip;}
        set {cClip = value; if (isWeaponActive) {PUiManager.instance.clipText.text = cClip.ToString();}}
    }
    private int cClip;
    [HideInInspector] public int currentBank
    {
        get {return cBank;}
        set {cBank = value; if (isWeaponActive) {PUiManager.instance.bankText.text = cBank.ToString();}}
    }
    private int cBank;
    private WaitForSeconds reloadWait;
    private WaitForSeconds attackWait;
    public WaitForSeconds fireWait;
    [HideInInspector] public bool isAiming;
    public bool isReloading = false;
    public bool canAttack = true;
    private WaitForEndOfFrame waitFrame;
    private Coroutine aimCoro;
    Vector3 startPos;
    private bool canAim = true;
    private bool attackReady;
    private int attackAnm = 1;
    private float wSwapTimer;
    private bool isWeaponActive = false;
    public virtual void Start() 
    {  
        //Sets Coros
        attackWait = new WaitForSeconds(fireRate);

        if (weaponData.refreshAnimation)
        {
            reloadWait = new WaitForSeconds(weaponData.refreshAnimation.length + refreshBuffer);
        }
        else
        {
            reloadWait = new WaitForSeconds(refreshBuffer);
        }

        //Sets the ammo
        currentBank = weaponData.maxBankAmmo;
        currentClip = weaponData.maxClipAmmo;

        //Assigns the unity event so the script activates and deactivates when needed
        weaponGrab.onGrabEvent.AddListener(ActivateWeapon);
        weaponGrab.onUnGrabEvent.AddListener(DeactivateWeapon);

        //Sets startpos
        startPos = PlayerManager.instance.pGrabManager.rHandTarget.localPosition;
        fireWait = new WaitForSeconds(attackBuffer);

        //Sets weapon swap timer
        wSwapTimer = weaponAnmSwapTimer;

        //Spawns shells in pooler 
        if (shellObject)
        {
            ObjectPooler.Instance.SetPool(shellObject.name, shellObject, 10);
        }
    }

    public virtual void Update() 
    {
        if (wSwapTimer > 0 && canAttack)
        {
            wSwapTimer -= Time.deltaTime;
        }
        else if (canAttack)
        {
            attackAnm = 1;
        }
    }

    public void SetMoveAnimations()
    {
        if (PlayerManager.instance.pMovement.moveState == PlayerMovement.MoveState.Walking)
        {
            //Walk animation
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Walk", 0.25f, 0);
            
            if (!canAim) {canAim = true;}
        }
        else if (PlayerManager.instance.pMovement.moveState == PlayerMovement.MoveState.Sprinting)
        {
            //Run animation
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Sprint", 0.25f, 0);

            if (isAiming) {StopAim();}
            canAim = false;
        }
        else if (PlayerManager.instance.pMovement.moveState == PlayerMovement.MoveState.Still)
        {
            //Run animation
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Idle", 0.1f, 0);
            
            if (!canAim) {canAim = true;}
        }
        
    }

    void ActivateWeapon()
    {
        isWeaponActive = true;

        //Sets input
        //Attacking
        PlayerManager.instance.playerInput.Player.LeftClickRightTrigger.performed += StartFireWeapon;

        //Refreshing weapon
        PlayerManager.instance.playerInput.Player.RNorth.performed += StartWeaponRefresh;

        //Aiming 
        if (weaponData.canAim) 
        {
            PlayerManager.instance.playerInput.Player.RightClickLeftTrigger.performed += StartWeaponAim;
            PlayerManager.instance.playerInput.Player.RightClickLeftTrigger.canceled += StopWeaponAim;
            weaponGrab.onGrabEvent.AddListener(StopAim);
        }

        //enables this script
        this.enabled = true;

        //plays animation
        PlayerManager.instance.pAnimator.CrossFade("Weapon_Draw", 0.25f);

        //Assigns unity events
        PlayerManager.instance.pMovement.onMoveState.AddListener(SetMoveAnimations);

        //Sets ammo text 
        if (weaponData.infiniteBank)
        {
            PUiManager.instance.bankText.text = "X";
        }
        else
        {
            PUiManager.instance.bankText.text = currentBank.ToString();
        }

        if (weaponData.infiniteClip)
        {
            PUiManager.instance.clipText.text = "X";
        }
        else
        {
            PUiManager.instance.clipText.text = currentClip.ToString();
        }
    }
    void DeactivateWeapon()
    {
        isWeaponActive = false;

        //Sets input
        //Attacking
        PlayerManager.instance.playerInput.Player.LeftClickRightTrigger.performed -= StartFireWeapon;

        //Refreshing weapon
        PlayerManager.instance.playerInput.Player.RNorth.performed -= StartWeaponRefresh;

        //Aiming 
        PlayerManager.instance.playerInput.Player.RightClickLeftTrigger.performed -= StartWeaponAim;
        PlayerManager.instance.playerInput.Player.RightClickLeftTrigger.canceled -= StopWeaponAim;

        //Reset animation 
        if (isReloading || !canAttack)
        {
            StopAllCoroutines();
            canAttack = true;
            isReloading = false;
        }

        if (isAiming) 
        {
            weaponGrab.onGrabEvent.RemoveListener(StopAim);
            //weaponGrab.OverrideGrabber(false, startPos, startRot);
        }

        //Assigns unity events
        PlayerManager.instance.pMovement.onMoveState.RemoveListener(SetMoveAnimations);

        //Sets ammo text 
        PUiManager.instance.clipText.text = "";
        PUiManager.instance.bankText.text = "";

        //disables this script
        this.enabled = false;
    }
    private void StartWeaponRefresh(InputAction.CallbackContext context)
    {
        if (!isReloading && currentBank > 0 && currentClip != weaponData.maxClipAmmo)
        {
            StartCoroutine(RefreshWeapon());
        }
    }

    private void StartWeaponAim(InputAction.CallbackContext context)
    {
        if (isAiming || isReloading) {return;} 
        AimWeapon();
    }

    private void StopWeaponAim(InputAction.CallbackContext context)
    {
        if (!isAiming) {return;}
        StopAim();
    }
    public void StopAim()
    {
        if (!canAim || !isAiming) {return;}

        //Disables the focus image
        PUiManager.instance.focusImage.enabled = false;
        isAiming = false; 

        //Sets the aim FOV
        PlayerManager.instance.mainCam.fieldOfView += fovAddition;

        //Starts the move
        if (aimCoro != null) {StopCoroutine(aimCoro);}
        aimCoro = StartCoroutine(MoveWeapon(Vector3.zero, aimSpeed));
    }

    public void AimWeapon()
    {
        if (!canAim) {return;}

        //Enables the focus image
        PUiManager.instance.focusImage.enabled = true;
        isAiming = true;

        //Sets the aim FOV
        PlayerManager.instance.mainCam.fieldOfView -= fovAddition;

        //Starts the aim move
        if (aimCoro != null) {StopCoroutine(aimCoro);}
        aimCoro = StartCoroutine(MoveWeapon(aimPosOffset, aimSpeed));
    }

    private IEnumerator MoveWeapon(Vector3 posOffset, float speed)
    {
        Transform pGrabber = PlayerManager.instance.pGrabManager.rHandTarget;
        while (pGrabber.localPosition != startPos + posOffset)
        {
            //Sets position offset
            pGrabber.localPosition = Vector3.Lerp(pGrabber.localPosition, startPos + posOffset, speed * Time.deltaTime);

            yield return waitFrame;
        }
    }

    private IEnumerator RefreshWeapon()
    {
        if (currentBank <= 0) {yield break;}

        print("Reload");
        isReloading = true;

        //Shoot animation
        PlayerManager.instance.pAnimator.CrossFade("Weapon_Refresh", 0.1f, 1);

        //Waits for the reload time
        yield return reloadWait;

        //gets the ammo needed to refill the clip
        int ammoNeeded = weaponData.maxClipAmmo - currentClip;
        if (weaponData.infiniteBank)
        {
            currentClip = weaponData.maxClipAmmo;
        }
        else
        {
            if (ammoNeeded >= currentBank)
            {
                currentClip = currentBank;
                currentBank = 0;
            }
            else
            {
                currentClip = weaponData.maxClipAmmo;
                currentBank -= ammoNeeded;
            }
        }

        isReloading = false;
    }
    public IEnumerator WeaponAttack()
    {
        canAttack = false;

        //Reduces clip size
        if (!weaponData.infiniteClip)
        {
            print("Retract Ammo");
            currentClip--;
        }
        
        //Plays weapon effect -- example: Muzzle Flash

        if (weaponVFX) {weaponVFX.Emit(1); weaponVFX.Play();}
        
        //Shoot animation
        if (attackAnm == 2 && weaponData.attackAnimation2)
        {
            //attackWait = new WaitForSeconds(weaponData.attackAnimation2.length + 0.1f);
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Attack" + "_0" + attackAnm, 0.1f, 1);
        }
        else if (attackAnm == 3 && weaponData.attackAnimation3)
        {
            //attackWait = new WaitForSeconds(weaponData.attackAnimation3.length + 0.1f);
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Attack" + "_0" + attackAnm, 0.1f, 1);
        }
        else
        {
            //attackWait = new WaitForSeconds(weaponData.attackAnimation1.length + 0.1f);
            PlayerManager.instance.pAnimator.CrossFade("Weapon_Attack_01", 0.1f, 1);
        }

        StartCoroutine(FireWeapon());


        //Spawns shells in pooler 
        if (shellObject)
        {
            //Gets the rigidbody
            Rigidbody rb = ObjectPooler.Instance.SpawnFromPool(shellObject.name, shellTransform.position, shellTransform.rotation).GetComponent<Rigidbody>();
            if (rb) 
            {
                //Adds force to sheel
                rb.AddForce(ejectionForce * transform.right, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * ejectionTorque, ForceMode.Impulse);
            }
        }

        yield return attackWait;

        //resets timer
        wSwapTimer = weaponAnmSwapTimer;

        //Sets animation iterator
        if (attackAnm >= 3)
        {
            print("WHY BRUH");
            attackAnm = 1;
        }
        else
        {
            print("WORKING");
            attackAnm++;
        }

        if (currentClip > 0)
        {
            //Sets up the recursive nature of this function based on the shoot type
            if (attackType == AttackType.auto && PlayerManager.instance.playerInput.Player.LeftClickRightTrigger.IsPressed())
            {
                StartCoroutine(WeaponAttack());
            }
            else
            {
                canAttack = true;
            }
        }
        else
        {
            canAttack = true;
        }
    }

    public IEnumerator FireWeapon()
    {
        yield return fireWait;
        if (attackSounds.Length > 0)
        {
            AudioPooler.instance.PlaySound(attackSounds.RandomArrayElement<AudioClip>(), transform.position, 1, 1, 1, 10);
        }
        OnAttack();
    }
    
    //Interface function
    public void StartFireWeapon(InputAction.CallbackContext context)
    {
        //if there is ammo left in the clip and can attack
        if (canAttack && currentClip > 0 && !isReloading)
        {
            print("Shoot!");
            StartCoroutine(WeaponAttack());
        }
        else if (!isReloading && currentClip <= 0 && currentBank > 0)
        {
            StartCoroutine(RefreshWeapon());
        }
    }

    //abstract functions
    public abstract void OnAttack();
}
