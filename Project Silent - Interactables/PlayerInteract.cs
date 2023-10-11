using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Assign")]
    public LayerMask interactableMask; 
    public int interactableLayer;
    public PlayerManager pManager;
    public InteractIndicator indicator;
    public GameObject interactInfo;

    [Header("Variables to set")]
    public float interactionLength; 
    public float maxInteractDistance;
    
    [Header("Interactable State")]
    //Data
    public RaycastHit interactHit;
    public IInteractable currentInteractable;
    public IInteractable foundInteractable;
    bool wasFound = false;
    GameObject currentObject;
    private InteractInfo infoScript;
    void Start() 
    {
        //Sets interact input
        PlayerManager.instance.playerInput.Player.Interact.performed += StartInteract;
        PlayerManager.instance.playerInput.Player.Interact.canceled += StopInteract;

        //Text
        GameObject obj = Instantiate(interactInfo);
        infoScript = obj.GetComponent<InteractInfo>();
        obj.transform.parent = transform.parent;
        obj.SetActive(true);
    }
    void Update() 
    {
        //Shoots a ray looking for objects to interact with
        Physics.Raycast(PlayerManager.instance.mainCam.transform.position, PlayerManager.instance.mainCam.transform.forward, out interactHit, interactionLength, interactableMask); 

        //Gets the hit
        if (interactHit.collider)
        {
            //Assigns
            IInteractable interactFound = interactHit.collider.gameObject.GetComponent<IInteractable>();

            //Checks if the found interactable is a new one
            if (foundInteractable == null || foundInteractable != null && foundInteractable != interactFound && !PlayerManager.instance.pGrabManager.currentGrabbedObject)
            {
                foundInteractable = interactFound;
                wasFound = false;
            }
        }
        else
        {
            foundInteractable = null;
        }

        if (foundInteractable != null && currentInteractable == null || foundInteractable != null && currentInteractable != foundInteractable)
        {
            if (!wasFound)
            {
                wasFound = true;

                InteractionSettings interactionSettings = interactHit.collider.GetComponent<InteractionSettings>();

                //Sets crosshair
                if (interactionSettings && interactionSettings.interactIcon)
                {
                    if (interactionSettings.interactIcon)
                    {
                        //Sets icon
                        PUiManager.instance.interactCrosshair.sprite = interactionSettings.interactIcon;
                        PUiManager.instance.interactCrosshair.enabled = true;

                    }

                    if (interactionSettings.interactName.Length > 0)
                    {
                        indicator.interactNameText.text = interactionSettings.interactName;
                    }
                    else
                    {
                        indicator.interactNameText.text = "Interact";
                    }
                }




                //Sets child
                indicator.transform.parent = interactHit.collider.transform;

                //Plays the animation
                indicator.ActivateIndicator();
            }

            //Sets Indicator
            Vector3 position = interactHit.collider.bounds.center + new Vector3(0f, interactHit.collider.bounds.extents.y + 0.1f, 0f);
            indicator.transform.position = position;
            indicator.lineRenderer.SetPosition(0, indicator.transform.position);
            indicator.lineRenderer.SetPosition(1, interactHit.collider.bounds.center);

        }
        else
        {
            if (wasFound)
            {
                //Resets crosshair 
                PUiManager.instance.interactCrosshair.enabled = false;

                //Resets text
                //PUiManager.instance.interactText.text = "";
                
                //Stops animation
                indicator.DisableIndicator();

                wasFound = false;
            }
        }

        //Removes current interact if far enough
        if (currentObject != null)
        {
            if (Vector3.Distance(PlayerManager.instance.transform.position, currentObject.transform.position) > maxInteractDistance)
            {
                StopInteract(new InputAction.CallbackContext());
            } 
        }
    }
    void StartInteract(InputAction.CallbackContext context)
    {
        //Checks if there is an interactable object found
        if (interactHit.collider != null)
        {
            //finds the interactable script
            currentInteractable = interactHit.collider.GetComponent<IInteractable>(); 
            currentObject = interactHit.collider.gameObject;

            //if there is an interactable script, fire off the interact method attached to the object
            if (currentInteractable != null)
            {
                currentInteractable.Interact(); 
            }
            else
            {
                currentInteractable = interactHit.collider.GetComponentInParent<IInteractable>(); 
                if (currentInteractable != null)
                {
                    currentInteractable.Interact(); 
                }
            }
        } 
    }   
    void StopInteract(InputAction.CallbackContext context)
    {
        //Checks if there is an interactable object found
        if (currentInteractable != null)
        {
            //Stops the interaction
            currentInteractable.StopInteract(); 
            currentInteractable = null;
            currentObject = null;    
        }
    }

    public void SetInteractInfo(string infoText)
    {
        Vector3 position = interactHit.collider.bounds.center + new Vector3(0f, interactHit.collider.bounds.extents.y + 0.15f, 0f);
        infoScript.transform.position = position;
        infoScript.textMeshPro.text = infoText;
        infoScript.easyTween.OpenCloseObjectAnimation();
    }
    public void SetInteractInfo()
    {
        if (!infoScript.easyTween.IsObjectOpened()) {return;}
        infoScript.easyTween.OpenCloseObjectAnimation();
    }

    private void OnDrawGizmos() 
    {
        //Interact Gizmo
        Gizmos.color = Color.yellow;
        Vector3 endLinePos = new Vector3(pManager.mainCam.transform.position.x, pManager.mainCam.transform.position.y, pManager.mainCam.transform.position.z + interactionLength);
        Gizmos.DrawLine(pManager.mainCam.transform.position,endLinePos);
    }
}
