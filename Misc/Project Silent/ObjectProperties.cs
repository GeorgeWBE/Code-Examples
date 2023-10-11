using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    [Header("Assign")]
    public ObjectMaterial objectMaterial;

    private void Start() 
    {
        SetPools();
    }

    void SetPools()
    {
        if (!objectMaterial) {return;}

        //creates pool for the decals
        for (int i = 0; i < objectMaterial.decals.Length; i++)
        {
            ObjectPooler.Instance.SetPool(objectMaterial.decals[i].name, objectMaterial.decals[i], 5);
        }

        //Creates a pool for the effect
        if (objectMaterial.hitEffect)
        {
            ObjectPooler.Instance.SetPool(objectMaterial.hitEffect.name, objectMaterial.hitEffect, 2);
        }
    }
}
