using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    #region Singleton
    public static MusicManager instance;
    void Awake() 
    {
        if (MusicManager.instance) {Destroy(gameObject); return;}
        
        instance = this; 

        if (transform.parent) {transform.parent = null;}

        DontDestroyOnLoad(this.gameObject);

        musicState = new MusicState();
    }
    #endregion

    [Header("Assing")]
    public AudioSource mSource;

    [System.Serializable]
    public class Playlist
    {
        public string playlistTag;
        public List<AudioClip>tracks;
        public float fadeTime;
    }
    public List<Playlist>Playlists;

    public class MusicState
    {
        public Playlist currentPlaylist;
        private Coroutine playlistCoro;
        public Coroutine currentPlaylistCoro
        {
            get
            {
                return playlistCoro;
            }
            set
            {
                if (value == null)
                {
                    currentPlaylist = null;
                }

                if (playlistCoro != null)
                {
                    MusicManager.instance.StopCoroutine(playlistCoro);
                }
                playlistCoro = value;
            }
        }
        public AudioClip currentTrack;
        private Coroutine trackCoro;
        public Coroutine currentTrackCoro
        {
            get
            {
                return trackCoro;
            }
            set
            {
                if (value == null)
                {
                    MusicManager.instance.StopCoroutine(trackCoro);
                    currentTrack = null;
                }
                if (trackCoro != null)
                {
                    MusicManager.instance.StopCoroutine(trackCoro);
                }
                trackCoro = value;
            }
        }

        private Coroutine fadeCoro;

        public Coroutine currentFadeCoro
        {
            get
            {
                return fadeCoro;
            }
            set
            {
                // if (value == null)
                // {
                //     MusicManager.instance.StopCoroutine(fadeCoro);
                // }
                if (fadeCoro != null)
                {
                    MusicManager.instance.StopCoroutine(fadeCoro);

                }
                
                fadeCoro = value;
            }
        }
    }
    public MusicState musicState;
    public void PlayPlaylist(string targetPlaylist, int startingTrack = 0, bool repeat = true)
    {
        //Loops through the playlists to find the right one
        for (int i = 0; i < Playlists.Count; i++)
        {
            if (Playlists[i].playlistTag.Equals(targetPlaylist))
            {
                //If the playlist is already playing
                if (musicState.currentPlaylist != null && musicState.currentPlaylist.Equals(Playlists[i])) 
                {return;}

                musicState.currentPlaylistCoro = StartCoroutine(IteratePlaylist(Playlists[i], startingTrack, repeat));
                musicState.currentPlaylist = Playlists[i];
                return;
            }
        }

    }
    public void OverrideTrack(AudioClip track, float fadeTime, bool resumePlaylist = true)
    {
        // if (musicState.currentPlaylistCoro != null)
        // {
        //     musicState.currentPlaylistCoro
        // }
        // //Starts the track
        // musicState.currentTrack = track;
        // musicState.currentTrackCoro = StartCoroutine(PlayTrack(track, fadeTime));
    }

    public void StopPlaylist(float fadeTime)
    {

    }

    //Internal methods
    private IEnumerator IteratePlaylist(Playlist playlist, int startTrack = 0, bool repeat = true)
    {
        //Iterates through each track of the playlist
        for (int i = startTrack; i < playlist.tracks.Count; i++)
        {
            //Starts the track
            musicState.currentTrack = playlist.tracks[i];
            musicState.currentTrackCoro = StartCoroutine(PlayTrack(playlist.tracks[i], playlist.fadeTime));

            //Halts while the track is playing
            while (musicState.currentTrackCoro != null)
            {
                yield return null;
            }
        }

        //Repeats the playlist
        if (repeat) {musicState.currentPlaylistCoro = StartCoroutine(IteratePlaylist(playlist, startTrack, repeat));}
    }
    private IEnumerator PlayTrack(AudioClip track, float fadeTime)
    {
        //checks if there is a track already playing
        if (mSource.clip)
        {
            //Fades out the current clip
            musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 0, true));

            //Haults until it's faded out
            while(mSource.volume != 0)
            {
                yield return null;
            }

            mSource.clip = null;
        }

        //Sets audio source
        mSource.clip = track;
        mSource.volume = 0;
        mSource.Play();

        //Fades in the audio source
        musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 1, true));

        //Haults for the length of the track 
        yield return new WaitForSecondsRealtime(track.length - fadeTime);

        //Fades out the track 
        musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 0, true));

        //Haults until it's faded out
        while(mSource.volume != 0)
        {
            yield return null;
        }

        //Sets audio source
        mSource.clip = null;
        musicState.currentTrackCoro = null;
    }

    //Public Methods
    // public void PlayPlaylist(string targetPlaylist, int startingTrack = 0, bool repeat = true)
    // {
    //     //Loops through the playlists to find the right one
    //     for (int i = 0; i < Playlists.Count; i++)
    //     {
    //         if (Playlists[i].playlistTag.Equals(targetPlaylist))
    //         {
    //             //Checks if there is a current playlist playing
    //             if (musicState.currentPlaylist != null)
    //             {
    //                 //If the current playlist is the one playing
    //                 if (musicState.currentPlaylist.Equals(Playlists[i]))
    //                 {
    //                     return;
    //                 }
    //             }

    //             //Checks if the start track is valid in the playlist, in which case it resets the startrack to 0
    //             if (!Playlists[i].tracks[startingTrack]) 
    //             {
    //                 //Resets the starting track to 0
    //                 Debug.Log("Invalid start track -- resetting start track");
    //                 startingTrack = 0;

    //                 //Checks to make sure that 0 is a valid track
    //                 if (!Playlists[i].tracks[startingTrack]) {Debug.Log("No Valid track in the playlist"); return;}
    //             }
                
    //             //Store stores the palylist
    //             musicState.currentPlaylist = Playlists[i];;
    //             musicState.currentPlaylistCoro = StartCoroutine(IteratePlaylist(Playlists[i]));

    //             return;
    //         }
    //     }

    //     Debug.Log("No Playlist Found");
    // }
    // public void OverrideTrack(AudioClip track, bool resumePlaylist = true)
    // {

    // }

    // public void StopPlaylist(float fadeTime)
    // {
    //     //Checks if there is a playlist playing
    //     if (musicState.currentPlaylist.Equals(null)) {Debug.Log("There is no current playlist"); return;}

    //     //Stops the playlist coro
    //     StopCoroutine(musicState.currentPlaylistCoro);

    //     //Fades the audio out
    //     if (fadeTime.Equals(0)) {return;}
    //     musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 0, true));

    // }

    // public void StopTrack()
    // {

    // }

    // //Internal methods
    // private IEnumerator IteratePlaylist(Playlist playlist, int startTrack = 0, bool repeat = true)
    // {
    //     print("Iterate Playlist");

    //     int playListIndex = startTrack;

    //     for (int i = startTrack; i < playlist.tracks.Count; i++)
    //     {
    //         //Plays the track
    //         musicState.currentTrackCoro = StartCoroutine(PlayTrack(playlist.tracks[i], playlist.fadeTime));

    //         while (musicState.currentTrack)
    //         {
    //             print("While: " + playlist.playlistTag);
    //             yield return null;
    //         }

    //     }

    //     if (repeat)
    //     {
    //         musicState.currentPlaylistCoro = StartCoroutine(IteratePlaylist(playlist));
    //     }

    // }
    // private IEnumerator PlayTrack(AudioClip track, float fadeTime)
    // {
    //     //Checks if there is a currently playing track
    //     if (musicState.currentTrack)
    //     {
    //         print("Currently plahing track");
    //         //If the track is the same as the one playing
    //         if (musicState.currentTrack.Equals(track)) {Debug.Log("Track already playing"); yield break;}

    //         //fades out the current track
    //         musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 0, true));

    //         //Waits for the fade time
    //         while (musicState.currentFadeCoro != null)
    //         {print("Waiting fade: " + musicState.currentFadeCoro); yield return null;}

    //     }

    //     //Stops the audio source
    //     mSource.Stop();
    //     mSource.clip = null;

    //     //Sets the audio source
    //     mSource.clip = track;
    //     mSource.volume = 0;
    //     mSource.Play();

    //     print("Setting current Track");
    //     musicState.currentTrack = track;

    //     //Fades in the track
    //     musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 1, true));

    //     //Waits for the length of the track - the time it takes to fade
    //     yield return new WaitForSecondsRealtime(track.length - fadeTime);

    //     //Fades out the track
    //     StartCoroutine(EndTrack(fadeTime));
    // }

    // private IEnumerator EndTrack(float fadeTime)
    // {
    //     //Fades out the track
    //     musicState.currentFadeCoro = StartCoroutine(FadeFunctions.FadeAudio(mSource, fadeTime, 0, true));

    //     yield return new WaitForSecondsRealtime(fadeTime);

    //     print("Set Current track null");
    //     musicState.currentTrackCoro = null;
    // }

}