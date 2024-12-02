using System.IO;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    _instance = obj.AddComponent<AudioManager>();
                    _instance.audioSource = obj.AddComponent<AudioSource>();
                    Logger.Log("AudioManager instantiated dynamically.");
                }
            }
            return _instance;
        }
    }

    public void PlayAudio(string folderPath, string audioName)
    {
        StartCoroutine(PlayAudioCoroutine(folderPath, audioName));
    }

    private IEnumerator PlayAudioCoroutine(string folderPath, string audioName)
    {
        string[] audioExtensions = new string[] { ".mp3", ".wav" };
        string audioPath = null;

        foreach (var ext in audioExtensions)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, folderPath, audioName + ext);
            if (File.Exists(filePath))
            {
                audioPath = filePath;
                break;
            }
        }

        if (audioPath == null)
        {
            Debug.LogError("Audio file not found: " + audioName);
            yield break;
        }

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.UNKNOWN);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading audio: " + www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
