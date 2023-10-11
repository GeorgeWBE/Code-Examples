using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioPooler : MonoBehaviour
{
    #region Singleton
    public static AudioPooler instance;
    //Audio Pooler Vars
    public class AudioPoolObject
    {
        public int audioId;
        public float volume;
        public GameObject audioObject;
        public AudioSource audioSource;
        public Transform targetTransform;
        public bool is3d;
        public bool isPlaying;
        public IEnumerator coroutine;
    }
    public int audioPoolSize = 10;
    private int audioId = 0;
    public List<AudioPoolObject> audioPools = new List<AudioPoolObject>();
    Dictionary<int, AudioPoolObject> activePool = new Dictionary<int, AudioPoolObject>();

    void Awake() 
    {
        instance = this; 
        //DontDestroyOnLoad(gameObject);
    }
    #endregion
    private void Start() 
    {
        for (int i = 0; i < audioPoolSize; i++)
        {
            CreateAudioPoolObject();
        }
    }
    void CreateAudioPoolObject()
    {
        //creates object
        GameObject go = new GameObject("Audio_Pool_Item");
        //adds audio source
        AudioSource audioSource = go.AddComponent<AudioSource>();
        //makes the object the parent of the pool obj            
        go.transform.parent = transform;

        //creates a audiopoolobject to add to the list
        AudioPoolObject audioPoolObject = new AudioPoolObject();
        audioPoolObject.audioObject = go;
        audioPoolObject.isPlaying = false;
        audioPoolObject.audioSource = audioSource;
        audioPoolObject.audioSource.outputAudioMixerGroup = SceneManagement.instance.sfxGroup;
        go.SetActive(false);
        audioPools.Add(audioPoolObject);
    }
    public void ConfigureAudioObject(int poolIndex, AudioClip clip, Vector3 position, float volume, float spatialBlend, float minDistance = 1, float maxDistance = 10)
    {
        //checks if the index given is in range
        if (poolIndex < 0 || poolIndex >= audioPools.Count)
        {
            return;
        }
        
        //increments the id
        audioId++;

        //fetching the pool item
        AudioPoolObject audioPoolObject = audioPools[poolIndex];

        //Configure the audio source
        audioPoolObject.audioSource.clip = clip;
        audioPoolObject.audioSource.volume = volume;
        audioPoolObject.audioSource.spatialBlend = spatialBlend;
        audioPoolObject.audioSource.minDistance = minDistance;
        audioPoolObject.audioSource.maxDistance = maxDistance;

        //Sets the position of the audio source
        audioPoolObject.audioSource.transform.position = position;

        //Activates the audio, sets the id and plays the audio
        audioPoolObject.isPlaying = true;
        audioPoolObject.audioObject.SetActive(true);
        audioPoolObject.audioSource.Play();
        audioPoolObject.audioId = audioId;
        audioPoolObject.coroutine = StopSoundDelayed(audioId, audioPoolObject.audioSource.clip.length);
        StartCoroutine( audioPoolObject.coroutine);

        //adds the sound to the active pool 
        activePool[audioId] = audioPoolObject;
    }
    public IEnumerator StopSoundDelayed(int id, float duration)
    {
        yield return new WaitForSeconds(duration);
        AudioPoolObject activeSoundOnj;
        //checks if it exists in the pool
        if (activePool.TryGetValue(id, out activeSoundOnj))
        {
            //Deactivates the sound and removes it from the active pool
            activeSoundOnj.audioSource.Stop();
            activeSoundOnj.audioSource.clip = null;
            activeSoundOnj.audioObject.SetActive(false);
            activePool.Remove(id);
            activeSoundOnj.isPlaying = false;
        }
    }
    public void StopOneShotSound(int id)
    {
        AudioPoolObject activeSoundOnj;

        //checks if it exists in the pool
        if (activePool.TryGetValue(id, out activeSoundOnj))
        {
            //Deactivates the sound and removes it from the active pool
            StopCoroutine(activeSoundOnj.coroutine);
            activeSoundOnj.audioSource.Stop();
            activeSoundOnj.audioSource.clip = null;
            activeSoundOnj.audioObject.SetActive(false);
            activePool.Remove(id);
            activeSoundOnj.isPlaying = false;
        }
    }
    public void PlaySound(AudioClip clip, Vector3 position, float volume, float spartialBlend, float minDistance = 1, float maxDistance = 10)
    {
        //do nothing if left null
        if (!clip || volume.Equals(0.0f))
        {
            return;
        }

        //Find audiosource to use
        for (int i = 0; i < audioPools.Count; i++)
        {
            AudioPoolObject audioObj = audioPools[i];

            //checks if it's avaliable
            if (!audioObj.isPlaying)
            {
                ConfigureAudioObject(i, clip, position, volume, spartialBlend, minDistance, maxDistance);

                //Plays sound wave
                MakeSound(position, maxDistance);

                return;
            }
            else
            {
                //if all audio sources are being used, add a new one and use that
                if (i >= audioPools.Count)
                {
                    CreateAudioPoolObject();
                    ConfigureAudioObject(i + 1, clip, position, volume, spartialBlend, minDistance, maxDistance);

                    //Plays sound wave
                    MakeSound(position, maxDistance);
                    
                    return;
                }
            }

        }
    }

    private void MakeSound(Vector3 pos, float range)
    {
        Collider[] col = Physics.OverlapSphere(pos, range);
        for (int i = 0; i < col.Length; i++)
        {
            if (col[i].TryGetComponent(out IHear hearer))
            {
                hearer.OnHear(pos, range);
            }
        }
    }
}
