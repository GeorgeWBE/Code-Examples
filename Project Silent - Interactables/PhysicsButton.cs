using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsButton : MonoBehaviour, IInteractable
{
    [Header("Assign")]
    public GameObject buttonPlunger; 
    public ConfigurableJoint buttonJoint;

    [Header("Variables")]
    public float downThreshold; 
    public float upThreshold; 

    [Header("Unity Events")]
    public UnityEvent onButtonPressed; 
    public UnityEvent onButtonPressedOnce; 
    public UnityEvent onButtonUp; 
    public UnityEvent onButtonHold; 

    [Header("Info")]
    private bool isButtonDown = false;   
    private PlayerManager playerManagerScript;
    private bool isPushingButton; 
    private RaycastHit hit; 
    private int timesCalled = 0;
    void Start()
    {
        playerManagerScript = PlayerManager.instance;
    }
    void Update()
    {
        HandleButtonState(); 
    }
    public void Interact()
    {
        ButtonPress(); 
    }
    public void StopInteract()
    {
        ButtonDepress(); 
    }
    public void ButtonPress()
    { 
        buttonJoint.targetPosition = new Vector3(0, -downThreshold + 0.01f, 0);
        isPushingButton = true;   
    }
    public void ButtonDepress()
    { 
        if (isPushingButton)
        {
            buttonJoint.targetPosition = new Vector3(0, 0 ,0); 
            isPushingButton = false; 
        }
    }
    public void HandleButtonState()
    { 
        if (buttonPlunger.transform.localPosition.y <= downThreshold && !isButtonDown)
        {
            //Invokes the event
            onButtonPressed.Invoke();  

            if (timesCalled < 1)
            {
                onButtonPressedOnce.Invoke();  
            }

            timesCalled++; 
            isButtonDown = true; 
        }
        else if (buttonPlunger.transform.localPosition.y <= downThreshold && isButtonDown)
        {
            onButtonHold.Invoke(); 
        }   
        else if (buttonPlunger.transform.localPosition.y >= upThreshold && isButtonDown)
        {
            isButtonDown = false; 
            onButtonUp.Invoke();
        }
    }
}
