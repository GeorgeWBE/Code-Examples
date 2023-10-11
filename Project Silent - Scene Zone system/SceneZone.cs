using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
[RequireComponent(typeof(Collider))]
public class SceneZone : MonoBehaviour
{
    public List<ObjectSpawner> zoneObjects;
    public List<GameObject> activeObjects;

    [Header("Scene Zone Events")]
    public UnityEvent onEnter;
    public UnityEvent onStay;
    public UnityEvent onExit;

    //Data
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private void Start() 
    {
        //Adds all the zone objects 
        for (int i = 0; i < zoneObjects.Count; i++)
        {
            spawnedObjects.Add(zoneObjects[i].spawnedObject);
            zoneObjects[i].spawnedObject.transform.parent = transform;
        }

        //Adds all active objects 
        for (int i = 0; i < activeObjects.Count; i++)
        {
            spawnedObjects.Add(activeObjects[i]);
            activeObjects[i].transform.parent = transform;
        }
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        onEnter.Invoke();

        if (other.tag == "Player")
        {
            //Despawns the objects of the previous zone
            if (SceneManagement.instance.currentZone && SceneManagement.instance.currentZone != this) {SceneManagement.instance.currentZone.SpawnObjects(false);}
            SceneManagement.instance.currentZone = this;

            //Spawns zone objects
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (!spawnedObjects[i].activeSelf && !PUiManager.instance.inventoryManager.InventoryContains(spawnedObjects[i])) 
                {
                    spawnedObjects[i].SetActive(true);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        onStay.Invoke();
    }

    private void OnTriggerExit(Collider other) 
    {
        onExit.Invoke();
        if (other.attachedRigidbody && other.tag != "Player")
        {
            //Checks if the object is already in the array
            if (spawnedObjects.Contains(other.attachedRigidbody.gameObject))
            {
                print("Trigger 3");
                //Removes it from the list
                spawnedObjects.Remove(other.attachedRigidbody.gameObject);
            }    
        }
    }

    void SpawnObjects(bool set)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (!PUiManager.instance.inventoryManager.InventoryContains(spawnedObjects[i])) 
            {
                spawnedObjects[i].SetActive(set);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneZone))]
public class SceneZoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector after button...
		base.OnInspectorGUI();

        serializedObject.Update();

        SceneZone collector = (SceneZone)target;

        if (GUILayout.Button("Collect Zone Objects "))
        {
            //Resets the array
            collector.zoneObjects.Clear();

            //Looks for child object spawners
            ObjectSpawner[] childTransforms = collector.GetComponentsInChildren<ObjectSpawner>();

            //Adds it to the list 
            for (int i = 0; i < childTransforms.Length; i++)
            {
                collector.zoneObjects.Add(childTransforms[i]);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif


