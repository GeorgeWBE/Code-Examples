using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject spawnObject;
    private ObjectData objectData;
    [HideInInspector] public GameObject spawnedObject;
    private void Awake() 
    {
        //Creates and stores the object
        spawnedObject = Instantiate(spawnObject, transform.position, transform.rotation * spawnObject.transform.rotation);
        spawnedObject.name = spawnObject.name;
    }

    private void OnDrawGizmos() 
    {
        if (spawnObject && spawnObject.GetComponent<ObjectData>())
        {
            objectData = spawnObject.GetComponent<ObjectData>();
            for (int i = 0; i < objectData.objectMeshes.Length; i++)
            {
                Gizmos.DrawMesh(objectData.objectMeshes[i].sharedMesh, -1, transform.position + objectData.objectMeshes[i].transform.localPosition, transform.rotation * objectData.objectMeshes[i].transform.rotation, objectData.objectMeshes[i].transform.lossyScale);
            }

            for (int i = 0; i < objectData.skinnedMeshRenderer.Length; i++)
            {
                Gizmos.DrawMesh(objectData.skinnedMeshRenderer[i].sharedMesh, -1, transform.position + objectData.skinnedMeshRenderer[i].transform.localPosition, transform.rotation * objectData.skinnedMeshRenderer[i].transform.rotation, objectData.skinnedMeshRenderer[i].transform.lossyScale);
            }
        }
    }
}
