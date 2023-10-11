using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Singleton
    public static ObjectPooler Instance;
    private void Awake() 
    {
        Instance = this; 

        //Creates the dicionary for storing the pools
        poolDicionary = new Dictionary<string, Queue<GameObject>>();

        //Creates and store all the pools
        foreach (ObjectPool pool in objectPools)
        {
            SetPool(pool.tag, pool.pooledObject,  pool.poolSize); 
        }
    }
    #endregion

    [Header("POOLED OBJECTS")]
    public List<ObjectPool> objectPools;
    public Dictionary<string, Queue<GameObject>> poolDicionary;

    [System.Serializable]
    public class ObjectPool
    {
        public GameObject pooledObject;
        public int poolSize;
        public string tag;
    }

    public void SetPool(string tag, GameObject poolObj, int size)
    {
        //Avoids creating exiting pools
        if (poolDicionary.ContainsKey(tag)) {return;}

        //Creates a queue to store the pooled objects
        Queue<GameObject> objectpool = new Queue<GameObject>();

        //Creates all the objects in the pool and adds it to the queue
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(poolObj);
            obj.SetActive(false); 
            obj.transform.parent = transform;
            objectpool.Enqueue(obj); 
        }

        //Adds the queue to the dictionary
        poolDicionary.Add(tag, objectpool); 
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        //Checks if the dictionary contains the pool
        if (!poolDicionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't excist.");
            return null; 
        }

        //Gets the target object
        GameObject objectToSpawn = poolDicionary[tag].Dequeue();

        //Resets the rigidbody if there is one
        Rigidbody rb = objectToSpawn.GetComponent<Rigidbody>();
        if (rb) {rb.velocity = Vector3.zero;}

        //Spawns the object
        objectToSpawn.SetActive(true); 
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation; 
        objectToSpawn.transform.parent = null;

        //Puts the oject back into the quare
        poolDicionary[tag].Enqueue(objectToSpawn); 

        return objectToSpawn;
    }
}
