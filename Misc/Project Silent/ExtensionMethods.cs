using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ExtensionMethods 
{
    //Float extensions
    public static float Map (this float value, float from1, float to1, float from2, float to2) 
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float ClampAngle (this float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360+from);
        return Mathf.Min(angle, to);
    }

    //Collider2D extensions
    public static void IgnoreTargetCollision (this Collider2D col, Collider2D[] colliderArray)
    {
        if (colliderArray != null)
        {
            for (int i = 0; i < colliderArray.Length; i++)
            {
                Physics2D.IgnoreCollision(col, colliderArray[i]);
            }
        }
    }

    public static GameObject IsTouchingObj(this Collider2D col, GameObject targetObj)
    {
        //Gets contacts from collider
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        col.GetContacts(contacts);

        //Loops through all colliders to find it's object
        for (int i = 0; i < contacts.Length; i++)
        {
            if (contacts[i].collider.gameObject == targetObj)
            {
                return contacts[i].collider.gameObject;
            }
        }

        return null;
    }

    public static List<T> IsTouchingComponent<T>(this Collider2D col) where T : class
    {
        //Gets contacts from collider
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        col.GetContacts(contacts);

        List<T> foundComps = new List<T>();
        
        //Loops through all colliders to find it's object
        for (int i = 0; i < contacts.Length; i++)
        {
            T foundComp = contacts[i].collider?.gameObject.GetComponent<T>();
            if (foundComp != null) {foundComps.Add(foundComp);}
        }

        if (foundComps.Count > 0)
        {
            return foundComps;
        }
        else
        {
            return null;
        }
    }

    //Transform extensions
    public static void FaceTransform (this Transform start, Vector3 targetTransform, float moveSpeed = 1, float negClamp = -90, float posClam = 90)
    {
        //Caclulates the rotation offset
        //Vector3 localTrans = start.worldToLocalMatrix.MultiplyVector(start.position);
        Vector3 offset = targetTransform - start.localPosition;

        //Converts global forward to local
        //Vector3 localForward = start.worldToLocalMatrix.MultiplyVector(start.forward);

        Quaternion target = Quaternion.LookRotation(start.forward, offset);
        Quaternion rotToTarg = start.localRotation;
        rotToTarg = Quaternion.Slerp(start.rotation, target, Time.deltaTime * moveSpeed);

        //set global rotation
        start.rotation = rotToTarg;

        //clamp local rotation 
        Quaternion localRot = start.localRotation;
        localRot.eulerAngles = new Vector3(0, 0, localRot.eulerAngles.z.ClampAngle(negClamp, posClam));
        start.localRotation = localRot;
    }

    //Array extensions
    public static void ArrayObjectSet (this GameObject[] arr, bool set)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i].SetActive(set);
        }
    }

    public static T CheckStringArray<T> (this T[] arr, string compString) where T : class
    {
        // Check each class in the array for a string variable 
        for (int i = 0; i < arr.Length; i++)
        {
            var fields = arr[i].GetType().GetFields();

            for (int x = 0; x < fields.Length; x++)
            {
                if (fields[x].FieldType == typeof(string))
                {
                    if ((string)fields[x].GetValue(arr[i]) == compString) {return arr[i];}
                }
            }    
        }

        return null;
    }

    public static T RandomArrayElement<T> (this T[] arr, int startIndex = 0) where T : class
    {
        // Ensures the array has elements & the start value is in the array bounds
        if (arr.Length <= 0 || startIndex > arr.Length - 1) {return null;}

        //Gets a random number relative to the size of the array
        int rNum = Random.Range(startIndex, arr.Length);

        //Returns a element using a random indexs
        return arr[rNum];
    }

    //Vector extenstions
    public static Vector3 AbsVector(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

}
