using Il2Cpp;
using Il2CppSteamworks;
using Il2CppSystem;
using MelonLoader;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

[assembly: MelonInfo(typeof(FS_CustomOST.OST_Main), "FS_CustomOST", "0.2.2", "Javialon_qv", null)]
[assembly: MelonGame("Haze Games", "Fractal Space")]
[assembly: MelonColor(1, 27, 2, 242)]
[assembly: MelonAuthorColor(1, 255, 0, 0)]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]

namespace FS_CustomOST
{
    public class OST_Main : MelonMod
    {
        public static OST_Main Instance;
        public string audioClipsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Mods", "FS_CustomOST");
        public string currentSceneName = "";

        public AudioSource audioSource;
        public string[] clips;
        public string currentClipName;
        public int currentClipID;
        public AudioClip ostClip;
        public bool audioClipLoaded;

        public float currentOSTTimeBeforeDead;

        public override void OnInitializeMelon()
        {
            Instance = this;

            // Anti-Piracy system against Baldus, LOL.
            TheUltimateAntiPiracySystem();
        }

        void TheUltimateAntiPiracySystem()
        {
            if (SteamManager.Initialized)
            {
                // Baldus' Steam ID.
                if (SteamUser.GetSteamID().m_SteamID == 76561199467049949)
                {
                    LoggerInstance.Msg("YOU'RE BALDUS? Sorry bro, I'm not letting you steal my mods again...");
                    Application.Quit();
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentSceneName = sceneName;

            if (OST_UIManager.Instance != null) OST_UIManager.Instance.OnSceneLoaded();

            if (sceneName.Contains("Menu"))
            {
                MelonCoroutines.Start(Init(false));
            }
            else if (sceneName.Contains("Level"))
            {
                MelonCoroutines.Start(Init(true));
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                if (audioSource.isPlaying) audioSource.Pause();
                else audioSource.Play();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                PreviousSong(false, true);
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                NextSong(false, true);
            }
        }

        IEnumerator Init(bool playSong)
        {
            // Refresh this.
            audioClipLoaded = false;

            // Get some references.
            audioSource = GetOSTAudioSource();
            clips = FetchAllClips(audioClipsPath);

            // Init the UI (and the settings class).
            InitOST_UI();

            // Just a little threshold ;)
            yield return new WaitForSecondsRealtime(1f);
            
            if (playSong)
            {
                if (string.IsNullOrEmpty(currentClipName))
                {
                    LoggerInstance.Msg("----------");
                    LoggerInstance.Msg("No song selected before loading the chapter, picking the first one...");
                    PlaySongWithID(0);
                }
                else
                {
                    LoggerInstance.Msg("----------");
                    LoggerInstance.Msg($"\"{currentClipName}\" selected before loading the chapter, picking it...");
                    PlaySongWithID(currentClipID);
                }
            }
        }

        AudioSource GetOSTAudioSource()
        {
            GameObject levelNormalSource = GameObject.Find("MusicManager/LevelNormalSource");
            return levelNormalSource.GetComponent<AudioSource>();
        }

        void MuteCurrentOST()
        {
            audioSource.mute = true;
            audioSource.Stop();
            LoggerInstance.Msg("Current OST muted!");
        }

        IEnumerator LoadAudioFile(int clipID)
        {
            // BROOOO, IT TOOK ME HOURS JUST TO BE ABLE TO LOAD THE FREAKING AUDIO FILE, HOLY SHIT.

            string audioClipPath = clips[clipID];

            if (string.IsNullOrEmpty(audioClipPath))
            {
                yield break;
            }

            LoggerInstance.Msg($"Loading \"{Path.GetFileName(audioClipPath)}\" audio file...");
            UnityWebRequest www;
            www = UnityWebRequest.Get(audioClipPath);
            www.SendWebRequest();

            OST_UIManager.Instance.UpdateCurrentTrackText();

            while (!www.isDone) yield return null;
            if (www.isNetworkError || www.isHttpError) yield return null;

            try
            {
                ostClip = WebRequestWWW.InternalCreateAudioClipUsingDH(www.downloadHandler, www.url, false, true, UnityEngine.AudioType.UNKNOWN);
                LoggerInstance.Msg("Audio file loaded!");
                audioClipLoaded = true;
            }
            catch
            {
                LoggerInstance.Error("Error loading audio file! Aborting...");
                // Even if the audio failed to load, I just need this bool to know if the audio loading has finished, i dont care if it was succesfully or not.
                audioClipLoaded = true;
            }
        }

        void PlayOST()
        {
            // Update info.
            OST_UIManager.Instance.UpdateCurrentTrackText();

            // Just don't put Rick roll on the audio source, please jaja.

            audioSource.clip = ostClip;
            audioSource.mute = false;
            audioSource.Play();
            LoggerInstance.Msg("OST played!");
        }

        public override void OnGUI()
        {
            if (OST_Settings.Instance != null)
            {
                if (!OST_Settings.Instance.showTrackInfo) return;
            }

            if (!currentSceneName.Contains("Level") || audioSource == null) return;

            if (clips.Length == 0)
            {
                GUI.Label(new Rect(10, 10, 500, 30), $"No tracks found.");
                return;
            }

            GUI.Label(new Rect(10, 10, 500, 30), $"Current clip name: {currentClipName}");
            GUI.Label(new Rect(10, 30, 500, 30), $"Play time: {OST_Settings.SecondsToMinutes(audioSource.time)}/{OST_Settings.SecondsToMinutes(audioSource.clip.length)}");
            GUI.Label(new Rect(10, 50, 500, 30), $"Current clip ID: {currentClipID + 1}/{clips.Length}");
            GUI.Label(new Rect(10, 70, 500, 30), $"Playing OST: {audioSource.isPlaying}");
            GUI.Label(new Rect(10, 90, 500, 30), $"CONTROLS:");
            GUI.Label(new Rect(10, 110, 500, 30), $"Pause/Play: Right Control");
            GUI.Label(new Rect(10, 130, 500, 30), $"Previous OST: Keypad Minus");
            GUI.Label(new Rect(10, 150, 500, 30), $"Next OST: Keypad Plus");
        }

        int RandomizeClip()
        {
            LoggerInstance.Msg("Randomizing clip...");

            if (clips.Length == 0)
            {
                LoggerInstance.Warning("No audio files found!");

                currentClipName = "";
                currentClipID = 0;

                return 0;
            }

            if (clips.Length == 1)
            {
                LoggerInstance.Msg("Just one file found.");

                currentClipName = Path.GetFileName(clips[0]);
                currentClipID = 0;

                return 0;
            }

            if (clips.Length > 1)
            {
                LoggerInstance.Msg("Multiple files found. Selecting random...");
                int random = UnityEngine.Random.Range(0, clips.Length);
                string fileName = Path.GetFileName(clips[random]);
                LoggerInstance.Msg($"\"{fileName}\" selected!");

                return random;
            }

            return 0;
        }

        string[] FetchAllClips(string clipsDirectory)
        {
            if (Directory.Exists(clipsDirectory))
            {
                return Directory.GetFiles(clipsDirectory);
            }
            else
            {
                LoggerInstance.Warning("Can't find \"FS_CustomOST\" folder.");
                return System.Array.Empty<string>();
            }
        }

        public void SaveOSTCurrentTime()
        {
            currentOSTTimeBeforeDead = audioSource.time;
        }

        public void OnDead()
        {
            MelonCoroutines.Start(Coroutine());
            IEnumerator Coroutine()
            {
                // Cause some stupid reason, I need to wait a sec just because the default track takes one second to reproduce itself, so I need to wait until it starts playing
                // so I can stop it and put the custom one...
                yield return new WaitForSecondsRealtime(1f);
                PlaySongWithID(currentClipID, currentOSTTimeBeforeDead);
            }
        }

        void InitOST_UI()
        {
            // If I find a gameobject with the same name, that means the UI is already initialized, do nothing.
            if (GameObject.Find("OST_Settings_Manager")) return;

            GameObject manager = new GameObject("OST_Settings_Manager");
            manager.AddComponent<OST_UIManager>().Init(audioSource);
            GameObject.DontDestroyOnLoad(manager);
        }

        // Returns true if a new song is played.
        public bool PreviousSong(bool beginFromTheEndIfStartIsReached, bool play = true)
        {
            bool toReturn = false;
            int previousClipID = currentClipID;

            if (beginFromTheEndIfStartIsReached)
            {
                previousClipID--;
                if (previousClipID <= 0) { previousClipID = clips.Length - 1; }
                currentClipName = Path.GetFileName(clips[previousClipID]);
                toReturn = true;
            }
            else
            {
                previousClipID--;
                if (previousClipID <= 0) { previousClipID = 0; } else { toReturn = true; }
                currentClipName = Path.GetFileName(clips[previousClipID]);
            }

            if (play && toReturn)
            {
                LoggerInstance.Msg($"PLAYING PREVIOUS SONG WITH ID {previousClipID}");
                PlaySongWithID(currentClipID);
            }

            return toReturn;
        }

        // Returns true if a new song is played.
        public bool NextSong(bool restartSongQueueIfReachTheEnd, bool play = true)
        {
            bool toReturn = false;
            int nextClipID = currentClipID;

            if (restartSongQueueIfReachTheEnd)
            {
                nextClipID++;
                if (nextClipID >= clips.Length) { nextClipID = 0; }
                currentClipName = Path.GetFileName(clips[nextClipID]);
                toReturn = true;
            }
            else
            {
                nextClipID++;
                if (nextClipID >= clips.Length) { nextClipID = clips.Length - 1; } else { toReturn = true; }
                currentClipName = Path.GetFileName(clips[nextClipID]);
            }

            if (play && toReturn)
            {
                LoggerInstance.Msg($"PLAYING NEXT SONG WITH ID {nextClipID}");
                PlaySongWithID(nextClipID);
            }

            return toReturn;
        }

        public void PlayRandomSong()
        {
            // Well, this is obvious, if there aren't files, do nothing, lol.
            if (clips.Length == 0) return;

            currentClipID = RandomizeClip();
            currentClipName = Path.GetFileName(clips[currentClipID]);

            LoggerInstance.Msg($"Playing random song with ID: {currentClipID}");

            PlaySongWithID(currentClipID);
        }

        public void PlaySongWithID(int clipID, float time = 0)
        {
            // Well, this is obvious, if there aren't files, do nothing, lol.
            if (clips.Length == 0) return;

            bool theSongTryingToPlayIsTheSameThanTheCurrentOne = clipID == currentClipID && audioClipLoaded;

            // If it's in main menu, then select the song, but NOT play it.
            if (currentSceneName.Contains("Menu"))
            {
                // Set as selected, lol.
                currentClipID = clipID;
                currentClipName = Path.GetFileName(clips[currentClipID]);
                // Set this like it was loaded, but really NOTHING was loaded ;)
                audioClipLoaded = true;

                // Update info.
                OST_UIManager.Instance.UpdateCurrentTrackText();

                LoggerInstance.Msg($"\"{currentClipName}\" selected to play when start playing!");

                return;
            }

            MelonCoroutines.Start(Coroutine());

            IEnumerator Coroutine()
            {
                if (!theSongTryingToPlayIsTheSameThanTheCurrentOne) audioClipLoaded = false;
                currentClipID = clipID;
                currentClipName = Path.GetFileName(clips[currentClipID]);

                LoggerInstance.Msg("----------");
                LoggerInstance.Msg($"Playing song with ID: {clipID}");

                MuteCurrentOST();

                // If the song trying to play is the same than the current one, just restart the song.
                if (theSongTryingToPlayIsTheSameThanTheCurrentOne)
                {
                    LoggerInstance.Msg("The song trying to play is the same than the current one! Restarting it...");
                }
                else // Otherwise, then load the clip file.
                {
                    yield return MelonCoroutines.Start(LoadAudioFile(clipID));
                    yield return new WaitForSecondsRealtime(0.5f);
                }

                PlayOST();
                audioSource.time = time;
            }
        }
    }
}