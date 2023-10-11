using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine;

public class SpawnerManager : MonoBehaviour, Operatable
{
    [Header("Variables to set")]
    public Transform[] spawnTransforms;
    public float spawnInterval;
    public bool fadeObjects = false;
    public float fadeTime; 
    //Enums
    public enum SpawnType {SpawnRandom, SpawnInSequence}
    public SpawnType spawnType;
    //priv vars
    private WaitForSeconds spawnTime;
    private int[] objectClasses;
    private int classesFinished = 0;
    [System.Serializable]  
    public class SpawnObject
    {
        public Transform overrideSpawnTrans;
        public bool fadeInObject;
        public float fadeInTime;
        public GameObject spawnObject;
        public int spawnCount;
        [HideInInspector] public GameObject[] spawnedObjects; 
    }
    public List<SpawnObject> SpawnObjects;
    private void Awake()
    {
        if (spawnTransforms == null)
        {
            spawnTransforms = new Transform[1];
            spawnTransforms[0] = transform; 
        }
        for (int i = 0; i < SpawnObjects.Count; i++)
        {
            SpawnObjects[i].spawnedObjects = new GameObject[SpawnObjects[i].spawnCount];
            for (int x = 0; x < SpawnObjects[i].spawnCount; x++)
            {
                GameObject objSpawned = Instantiate(SpawnObjects[i].spawnObject);
                SpawnObjects[i].spawnedObjects[x] = objSpawned;
                objSpawned.SetActive(false);
            }
        }
        objectClasses = new int[SpawnObjects.Count];
        for (int i = 0; i < objectClasses.Length; i++)
        {
            objectClasses[i] = 9999;
        }
        spawnTime = new WaitForSeconds(spawnInterval);
    }
    public void TurnOn()
    {
        StopAllCoroutines();
        StartCoroutine(StartSpawn());
    }
    public void TurnOff()
    {
        StopAllCoroutines(); 
    }
   
    public IEnumerator StartSpawn()
    {
        if (spawnType == SpawnType.SpawnInSequence)
        {
            for (int i = 0; i < SpawnObjects.Count; i++)
            {
                for (int x = 0; x < SpawnObjects[i].spawnCount; x++)
                {
                    yield return spawnTime; 
                    //setting object transform
                    if (SpawnObjects[i].overrideSpawnTrans)
                    {
                        SpawnObjects[i].spawnedObjects[x].transform.position = SpawnObjects[i].overrideSpawnTrans.position;
                        SpawnObjects[i].spawnedObjects[x].transform.rotation = SpawnObjects[i].overrideSpawnTrans.rotation;
                    }
                    else
                    {
                        SpawnObjects[i].spawnedObjects[x].transform.position = spawnTransforms[UnityEngine.Random.Range(0, spawnTransforms.Length)].position;
                        SpawnObjects[i].spawnedObjects[x].transform.rotation = spawnTransforms[UnityEngine.Random.Range(0, spawnTransforms.Length)].rotation;
                    }
                    //sets the object active
                    SpawnTheObject(SpawnObjects[i].spawnedObjects[x], SpawnObjects[i]); 
                }
            }
        }
        else if (spawnType == SpawnType.SpawnRandom)
        {
            classesFinished = 0;
            StartCoroutine(SpawnRandom());
        }
    }
    IEnumerator SpawnRandom()
    {
        int classNum = UnityEngine.Random.Range(0, SpawnObjects.Count);
        for (int i = 0; i < objectClasses.Length; i++)
        {
            if (classNum + 1 == objectClasses[i])
            {
                //StartCoroutine(SpawnRandom()); 
                break;
            }
        }
        if (SpawnObjects[classNum].spawnCount > 0)
        {
            yield return spawnTime;
            //setting object transform
            if (SpawnObjects[classNum].overrideSpawnTrans)
            {
                SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1].transform.position = SpawnObjects[classNum].overrideSpawnTrans.position;
                SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1].transform.rotation = SpawnObjects[classNum].overrideSpawnTrans.rotation;
            }
            else
            {
                SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1].transform.position = spawnTransforms[UnityEngine.Random.Range(0, spawnTransforms.Length)].position;
                SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1].transform.rotation = spawnTransforms[UnityEngine.Random.Range(0, spawnTransforms.Length)].rotation;
            }
            //sets the object active
            //SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1].SetActive(true);
            SpawnTheObject(SpawnObjects[classNum].spawnedObjects[SpawnObjects[classNum].spawnCount - 1], SpawnObjects[classNum]);
            SpawnObjects[classNum].spawnCount--; 
        }
        else
        {
            classesFinished++;
            objectClasses[classesFinished - 1] = classNum;
        }
        if (classesFinished < SpawnObjects.Count)
        {
            StartCoroutine(SpawnRandom());
        }
    }
    public void SpawnTheObject(GameObject objToSpawn, SpawnObject spawnObject)
    {
        if (fadeObjects || spawnObject.fadeInObject)
        {
            ObjectProperties objectProperties = objToSpawn.GetComponent<ObjectProperties>();    
            if (objectProperties && objectProperties.objectSpriteRenderers != null)
            {
                for (int i = 0; i < objectProperties.objectSpriteRenderers.Length; i++)
                {
                    SpriteRenderer objSpriteRend = objectProperties.objectSpriteRenderers[i];
                    Color color = objSpriteRend.color;
                    color.a = 0;
                    objSpriteRend.color = color;
                    objToSpawn.SetActive(true);
                    StartCoroutine(FadeIn(objSpriteRend, objectProperties));
                    return;
                }
                if (objectProperties.objectColliders != null)
                {
                    for (int i = 0; i < objectProperties.objectColliders.Length; i++)
                    {
                        objectProperties.objectColliders[i].enabled = false; 
                    }
                }
                
            }
        }
        objToSpawn.SetActive(true);
    }
    public IEnumerator FadeIn(SpriteRenderer spriteRend, ObjectProperties objectProperties)
    {
        float elapsedTime = 0;
        while (spriteRend.color.a != 1)
        {
            elapsedTime += Time.deltaTime;
            float percentComplete = elapsedTime / fadeTime;
            Color color = spriteRend.color;
            color.a = Mathf.Lerp(0, 1, percentComplete);
            spriteRend.color = color;
            //transform.position = Vector3.Lerp(startPosition, desiredPos, percentComplete);
            yield return new WaitForEndOfFrame();
        }
        if (objectProperties.objectColliders != null)
        {
            for (int i = 0; i < objectProperties.objectColliders.Length; i++)
            {
                objectProperties.objectColliders[i].enabled = true;
            }
        }
    }
    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
    }

}
