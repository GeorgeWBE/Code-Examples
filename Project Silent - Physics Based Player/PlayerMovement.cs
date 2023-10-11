using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody playerRb;
    public PlayerManager playerManager;

    [Header("Animation Components")]
    public AnimationClip moveAnimation;
    public AnimationClip idleAnimation;

    [Header("Crouch Settings")]
    public float crouchHeight;
    private float startHeight;

    [Header("Movement Audio")]
    public AudioSource playerStepSource;
    public AudioClip[] defaultSteps;

    [Header("Movement settings")]
    public float playerGroundSpeed = 5;
    public float playerAirSpeed = 5;
    public float playerCrouchDuration = 3.5f;
    public float groundAcceleration = 0.3f;
    public float groundDeceleration = 0.91f;
    public float airAcceleration = 0.1f;
    public float airDeceleration = 0.98f;
    public float jumpForce;

    [Header("The amount which will be added to the move speed")]
    public float sprintAddition = 8f;

    [Header("Cam movement")]
    public float rotDelay = 8;
    public float pRotSpeed = 0.5f;
    [Range(0, 100)]
    public float sensX;
    [Range(0, 100)]
    public float sensY;
    float mouseX;
    float mouseY;
    float multiplier = 0.01f;
    float xRotation;
    float yRotation;

    [Header("Player head movement settings")]
    public Animator headAnimator;
    public AnimationClip headIdleAnm;
    public AnimationClip headMoveAnm;
    public AnimationClip defaultCamAnm;
    public float timeDelay = 2;

    [Header("Arm Sway Settings")]
    public float swayAmount;
    public float maxSwayAmount;
    public float swaySmoothness;

    [Header("Bump settings")]
    public float bumpForce = 5f; 
    public float bumpDistance = 10f;
    public Vector3 bumpOffset;

    [Header("Crouch check Settings")]
    public float checkDistance = 0.1f;
    public float checkOffset = 0.1f;
    public float checkRadius;

    private Rigidbody rb;

    [Header("Events")]
    public UnityEvent onMoveState;

    //Data
    private bool isMoving;
    private Vector3 moveForce;
    private bool isRotating;
    private float startX = 0;
    private Vector3 currentVelocity;
    Vector3 moveDirection;
    bool canJump = true;
    WaitForEndOfFrame waitForEndOfFrame;
    Coroutine lerpColCoro;
    public enum MoveState{Walking, Sprinting, Still};
    private Quaternion initialRotation;
    private RaycastHit bumpHit;
    private float stepTime = 1f;
    private float currentStepTime = 0;
    [HideInInspector] public MoveState moveState
    {
        get { return mState; }
        set { mState = value; onMoveState.Invoke();}
    }
    private MoveState mState;
    private float timer;
    private bool movingHead;
    private float currentRotationAngle = 0.0f;
    private Vector2 targetSway;
    private float anchorStartPos;

    void Awake() 
    {
        //Caches waitforframe coro
        waitForEndOfFrame = new WaitForEndOfFrame();
    }
    void Start()
    {
        //Sets jump input 
        PlayerInputActions playerInput = PlayerManager.instance.playerInput;
        playerInput.Player.JumpInput.performed += StartJump;

        //Sets crouch input
        playerInput.Player.Crouch.performed += StartCrouch;
        playerInput.Player.Crouch.canceled += StartStopCrouch;

        //Sets crouch height
        startHeight = PlayerManager.instance.playerCol.height;

        initialRotation = PlayerManager.instance.mainCam.transform.rotation;

        //Sets camera height
        anchorStartPos = PlayerManager.instance.rotateAnchor.transform.localPosition.y;

        //Sets default player values
        SetMoveSettings();
    }

    void FixedUpdate()
    {
        PlayerMove();
        CheckBump();
    }

    void LateUpdate() 
    {
        PlayerMoveCamera();
    }

    void Update() 
    {
        //Lerps animation speed
        if (moveState == MoveState.Sprinting && headAnimator.speed != 2)
        {
            headAnimator.speed = Mathf.Lerp(headAnimator.speed, 2, 0.5f * Time.deltaTime);
        }
        else if (headAnimator.speed != 1)
        {
            headAnimator.speed = Mathf.Lerp(headAnimator.speed, 1, 0.5f * Time.deltaTime);
        }
    }

    public void SetMoveSettings()
    {
        //Max Speed
        playerGroundSpeed = playerManager.pData.defaultGroundSpeed;
        playerAirSpeed = playerManager.pData.defaultAirSpeed;
        playerCrouchDuration = playerManager.pData.defaultCrouchDuration;
    }

    void PlayerMove()
    {
        //gets the input for the movement in the form of a vector 
        Vector2 moveInput = PlayerManager.instance.playerInput.Player.MoveInput.ReadValue<Vector2>();
        
        // Ground movement
        if (PlayerManager.instance.pGroundCheck.isGrounded)
        {
            Move(moveInput, groundAcceleration, groundDeceleration, playerGroundSpeed);
        }
        // Air movement
        else
        {
            Move(moveInput, airAcceleration, airDeceleration, playerAirSpeed);
        }
        
    }
    void Move(Vector2 input, float accel, float decel, float maxSpeed)
    { 
        // Calculate the movement direction based on camera's forward and right vectors
        Vector3 cameraForward = PlayerManager.instance.transform.forward;
        Vector3 cameraRight = PlayerManager.instance.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        moveDirection = cameraForward * input.y + cameraRight * input.x;

        if (moveDirection.magnitude > 0f)
        {
            // //Checks if the player is sprinting
            int isSprinting = System.Convert.ToInt16(PlayerManager.instance.playerInput.Player.SprintInput.IsPressed());

            //Plays animation
            if (moveState == MoveState.Still && headMoveAnm)
            {
                //Plays idle animation
                headAnimator.CrossFade(headMoveAnm.name, 0.25f);
            }

            // //Sets Move state
            if (isSprinting == 1 && moveState != MoveState.Sprinting) {moveState = MoveState.Sprinting; stepTime = 0.5f;}
            else if (isSprinting == 0 && moveState != MoveState.Walking) {moveState = MoveState.Walking; stepTime = 1; }
       
            Vector3 desiredVelocity = moveDirection.normalized * (maxSpeed + (isSprinting * sprintAddition));

            // Calculate the change in velocity required
            Vector3 velocityChange = desiredVelocity - playerRb.velocity;

            // Calculate the required force
            Vector3 accelerationForce = velocityChange / Time.fixedDeltaTime;

            // Clamp the acceleration force to avoid exceeding the maximum speed
            accelerationForce = Vector3.ClampMagnitude(accelerationForce, accel);
            accelerationForce.y = 0;

            // Apply the acceleration force to the Rigidbody
            playerRb.AddForce(accelerationForce, ForceMode.Acceleration);

            if (playerManager.pGroundCheck.isGrounded)
            {
                //Step sound
                currentStepTime += Time.deltaTime;
                if (currentStepTime >= stepTime)
                {
                    if (playerManager.pGroundCheck.currentGroundProperties)
                    {
                        playerStepSource.clip = playerManager.pGroundCheck.currentGroundProperties.objectMaterial.footstepSounds.RandomArrayElement<AudioClip>();
                    }
                    else
                    {
                        playerStepSource.clip = defaultSteps.RandomArrayElement<AudioClip>();
                    }

                    playerStepSource.Play();

                    currentStepTime = 0;
                }
            }

        }
        else
        {
            if (moveState != MoveState.Still) 
            {
                moveState = MoveState.Still;
                headAnimator.CrossFade(defaultCamAnm.name, 0.5f);
            }
            moveForce = Vector3.zero;

            if (playerRb.velocity.magnitude > 0)
            {
                // Apply deceleration force to gradually stop the player
                Vector3 decelerationForce = -playerRb.velocity.normalized * decel;
                playerRb.AddForce(decelerationForce, ForceMode.Acceleration);
            }

            currentStepTime = 0;
        }

    }
    void StartJump(InputAction.CallbackContext context)
    {
        print("Start Jump");
        //checks if the player is grounded if canjump, the player is grounded, and that the collider that the player is jumping from is colliding with the player
        if (canJump && PlayerManager.instance.pGroundCheck.isGrounded)
        {
            //Checks if its the current grabbed object 
            for (int i = 0; i < PlayerManager.instance.pGroundCheck.groundColliders.Length; i++)
            {
                if (Physics.GetIgnoreCollision(PlayerManager.instance.playerCol, PlayerManager.instance.pGroundCheck.groundColliders[i])) {return;}
            }

            StartCoroutine(Jump());
        }
    }
    IEnumerator Jump()
    {
        //Adds vertical force to the player
        playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        //Waits for the next frame before allowing another jump
        canJump = false;
        yield return waitForEndOfFrame;
        canJump = true;
    }
    void StartCrouch(InputAction.CallbackContext context)
    {
        if (PlayerManager.instance.pGroundCheck.isGrounded)
        {

            if (lerpColCoro != null)
            {
                StopCoroutine(lerpColCoro);
            }

            lerpColCoro = StartCoroutine(CrouchCollider(crouchHeight));

            //if the player is going at a speed that meet the requirment
            playerGroundSpeed = playerGroundSpeed / 2;

            //disables sprint input
            PlayerManager.instance.playerInput.Player.SprintInput.Disable();
        }
    }
    void StartStopCrouch(InputAction.CallbackContext context)
    {
        StopCrouch();
    }
    IEnumerator CrouchCollider(float targetHeight)
    {
        //Lerps crouch collider
        float timeElapsed = 0;
        float startingHeight = PlayerManager.instance.playerCol.height;
        float startingAnchor = PlayerManager.instance.rotateAnchor.localPosition.y;
        while (timeElapsed < playerCrouchDuration)
        {
            float y;

            //Lerps the collider
            PlayerManager.instance.playerCol.height = Mathf.Lerp(startingHeight, targetHeight, timeElapsed / playerCrouchDuration);

            //Lerps player anchor
            if (targetHeight == startHeight)
            {
                y = Mathf.Lerp(startingAnchor, anchorStartPos, timeElapsed / playerCrouchDuration);
            }
            else
            {
                y = Mathf.Lerp(startingAnchor, anchorStartPos - ((startHeight - crouchHeight) / 3), timeElapsed / playerCrouchDuration);
            }
            PlayerManager.instance.rotateAnchor.transform.localPosition = new Vector3(PlayerManager.instance.rotateAnchor.transform.localPosition.x, y, PlayerManager.instance.rotateAnchor.transform.localPosition.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
    void StopCrouch()
    {
        if (!CheckAbove()) {StartCoroutine(CrouchCheck()); return;}

        if (lerpColCoro != null) {StopCoroutine(lerpColCoro);}

        //Starts the lerp
        lerpColCoro = StartCoroutine(CrouchCollider(startHeight));

        //Resets player speed
        playerGroundSpeed = PlayerManager.instance.pData.defaultGroundSpeed;

        //enables sprint input
        PlayerManager.instance.playerInput.Player.SprintInput.Enable();
    }

    bool CheckAbove()
    {
        //Shoots a raycast to see if there is a collider above
        RaycastHit hit;

        // Calculate the top position of the player collider with the offset
        Vector3 topPosition = playerManager.playerCol.transform.position + Vector3.up * (playerManager.playerCol.height * 0.25f + checkOffset);

        // Perform the sphere cast from the top position
        float sphereRadius = playerManager.playerCol.radius + checkRadius;

        Physics.SphereCast(topPosition, sphereRadius, Vector3.up, out hit, checkDistance);

        if (hit.collider)
        {
            return false;
        }

        return true;
    }

    IEnumerator CrouchCheck()
    {
        bool canUnCrouch;

        do
        {
            canUnCrouch = CheckAbove();
            yield return null;
        } while(!canUnCrouch);

        if (!playerManager.playerInput.Player.Crouch.IsPressed())
        {
            StopCrouch();
        }
    }

    private void PlayerMoveCamera()
    {
        mouseX = PlayerManager.instance.playerInput.Player.CameraMovement.ReadValue<Vector2>().x; 
        mouseY = PlayerManager.instance.playerInput.Player.CameraMovement.ReadValue<Vector2>().y;

        //Only move the camera if there is input
        if (mouseX != 0 || mouseY != 0)
        {
            //Resets animation
            if (moveState == MoveState.Still)
            {
                headAnimator.CrossFade(defaultCamAnm.name, 0.25f);
            }

            //Sets up timer delay 
            timer = timeDelay;

            yRotation += mouseX * PlayerManager.instance.playerSave.sensitivity * multiplier;
            xRotation -= mouseY * PlayerManager.instance.playerSave.sensitivity * multiplier;

            //Clamps the rotation
            xRotation = Mathf.Clamp(xRotation, -90f, 60f);

            //Rotates player camera
            PlayerManager.instance.rotateAnchor.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            
            //Rotates the player
            playerRb.MoveRotation(Quaternion.Euler(0, yRotation, 0));

            //Rotates the arms
            // Calculate the target sway based on player input
            float targetSwayX = -mouseX * swayAmount;
            float targetSwayY = -mouseY * swayAmount;
            targetSwayX = Mathf.Clamp(targetSwayX, -maxSwayAmount, maxSwayAmount);
            targetSwayY = Mathf.Clamp(targetSwayY, -maxSwayAmount, maxSwayAmount);
            targetSway = new Vector2(targetSwayX, targetSwayY);
            
            // Smoothly interpolate the current rotation with the target sway
            Quaternion targetRotation = initialRotation * Quaternion.Euler(targetSway.y, targetSway.x, 0f);
            PlayerManager.instance.armRig.transform.localRotation = Quaternion.Lerp(PlayerManager.instance.armRig.transform.localRotation, targetRotation, Time.deltaTime * swaySmoothness);
        }
        else if (moveState == MoveState.Still)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (moveState == MoveState.Still && headIdleAnm)
            {
                //Plays idle animation
                headAnimator.CrossFade(headIdleAnm.name, 0.25f);
            }
        }
    }

    public void CheckBump()
    {
        // Perform the sphere cast
        Physics.Raycast(transform.position + bumpOffset, transform.forward, out bumpHit, bumpDistance);

        //Checks if hit and its not the player
        if (bumpHit.collider && bumpHit.collider != playerManager.playerCol && moveDirection.magnitude > 0 && bumpHit.collider.bounds.extents.y < 0.5f && !bumpHit.collider.attachedRigidbody)
        {
            print("Bump Pushin");
            playerRb.AddForce(transform.up * bumpForce);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a wire sphere to visualize the overlap area in the scene view
        Gizmos.color = Color.blue;

        //Bump Sphere 
        Gizmos.DrawLine(transform.position + bumpOffset, transform.position + bumpOffset + transform.forward * bumpDistance);

        //Head check
        // Calculate the top position of the player collider with the offset
        Vector3 topPosition = playerManager.playerCol.transform.position + Vector3.up * (playerManager.playerCol.height * 0.25f + checkOffset);

        // Perform the sphere cast from the top position
        float sphereRadius = playerManager.playerCol.radius + checkRadius;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(topPosition, sphereRadius);
    }
    
}
