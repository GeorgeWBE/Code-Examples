using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabManager : MonoBehaviour
{
    [Header("Assign")]
    public Transform lHandSocket;
    public Transform rHandSocket;
    public Transform rHandTarget;
    [Header("Information")]
    public GrabType currentGrabbedObject;

    //Data
    [System.NonSerialized] public Transform pGrabber;
    private Vector3 grabberStartPos;
    private Quaternion grabberStartRot;
    public bool canGrab = false;

    private void Start() 
    {
        //Assigns the player grabber
        pGrabber = PlayerManager.instance.grabber.transform;
        grabberStartPos = pGrabber.localPosition;
        grabberStartRot = pGrabber.localRotation;

        //Assigns event 
        PUiManager.instance.onWheelChange.AddListener(SetGrabState);
    }

    public void SetGrabState()
    {
        if (PUiManager.instance.currentSlot == PUiManager.instance.grabSlot)
        {
            canGrab = true;
        }
        else
        {
            //Was true
            if (canGrab == true && currentGrabbedObject && !currentGrabbedObject.slottable)
            {
                print("STOP GRAB: " + currentGrabbedObject.name);
                StopGrab();
            }

            canGrab = false;
        }
    }

    public void StartGrab(GrabType grabType)
    {
        if (!currentGrabbedObject)
        {
            //Sets the rotation of the object to face the camera
            //grabType.transform.rotation = grabType.rotationOffset;

            //Sets collision of object 
            SetCollision(false, grabType);

            //Calls event 
            grabType.onGrabEvent.Invoke();

            //Assigns grabbed object
            currentGrabbedObject = grabType;

            //Sets position of grabber
            OverrideGrabber(true, currentGrabbedObject.positonOffset, currentGrabbedObject.rotationOffset);

            //Grabs the object
            grabType.GrabObject(grabType.gameObject);

            //Removes interactable mask 
            if (currentGrabbedObject.gameObject.layer == PlayerManager.instance.pInteract.interactableLayer)
            {
                print("DONE GRAB LAYER THING");
                currentGrabbedObject.gameObject.layer = 0;
            }
        }
    }

    public void StopGrab()
    {
        //Avoids ungrabbing if there isn't a currently grabbed object or the object is slotted
        if (!currentGrabbedObject) {return;}

        //Calls event 
        currentGrabbedObject.onUnGrabEvent.Invoke();

        //Sets collision of object 
        SetCollision(true, currentGrabbedObject);

        //Ungrabs object
        currentGrabbedObject.UnGrabObject();

        //Sets position of grabber
        OverrideGrabber(false, currentGrabbedObject.positonOffset, currentGrabbedObject.rotationOffset);

        //Adds interactable mask 
        currentGrabbedObject.gameObject.layer = PlayerManager.instance.pInteract.interactableLayer;

        //Removes the current grabbed object
        currentGrabbedObject = null;
    }

    //GENERAL USE FUNCTIONS 
    public void OverrideGrabber(bool overrideGrabber, Vector3 posOffset, Quaternion rotOffset)
    {
        if (overrideGrabber)
        {
            //Sets position offset
            pGrabber.localPosition = pGrabber.localPosition + posOffset;

            //Sets rotation offset
            Quaternion targRot = pGrabber.localRotation;
            targRot.eulerAngles = pGrabber.localRotation.eulerAngles + rotOffset.eulerAngles;
            pGrabber.localRotation = targRot;
        }
        else
        {
            //Sets position offset
            pGrabber.localPosition = grabberStartPos;

            //Sets rotation offset
            Quaternion targRot = pGrabber.localRotation;
            targRot.eulerAngles = grabberStartRot.eulerAngles;
            pGrabber.localRotation = targRot;
        }
    }

    void SetCollision(bool activate, GrabType grabType)
    {
        //Disable collision option
        if (grabType.collideOptions == GrabType.CollideOptions.disableAllCollison)
        {
            if (grabType.itemColliders.Length > 0)
            {
                for (int i = 0; i < grabType.itemColliders.Length; i++)
                {
                    grabType.itemColliders[i].isTrigger = !activate;
                }
            }
        }
        else if (grabType.collideOptions == GrabType.CollideOptions.disablePlayerCollision)
        {
            if (grabType.itemColliders.Length > 0)
            {
                for (int i = 0; i < grabType.itemColliders.Length; i++)
                {
                    Physics.IgnoreCollision(grabType.itemColliders[i], PlayerManager.instance.playerCol, !activate);
                }
            }
        }
    }
}
