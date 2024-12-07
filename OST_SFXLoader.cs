using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS_CustomOST
{
    public class OST_SFXLoader
    {
        Il2CppAssetBundle assetBundle;
        GameObject okSound;
        GameObject exitSound;

        public AudioClip okSoundClip
        {
            get
            {
                if (okSound == null) return null;

                return okSound.GetComponent<AudioSource>().clip;
            }
        }

        public AudioClip exitSoundClip
        {
            get
            {
                if (exitSound == null) return null;

                return exitSound.GetComponent<AudioSource>().clip;
            }
        }

        public OST_SFXLoader()
        {
            LoadAssetBundle();
            LoadOkSound();
            LoadExitSound();
        }

        public void LoadAssetBundle()
        {
            Stream assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FS_CustomOST.Properties.sfx");
            byte[] assetBytes = new byte[assetStream.Length];
            assetStream.Read(assetBytes);

            assetBundle = Il2CppAssetBundleManager.LoadFromMemory(assetBytes);
            assetStream.Close();
        }

        public void LoadOkSound()
        {
            okSound = new GameObject("OkSound_Save");
            GameObject.DontDestroyOnLoad(okSound);
            okSound.AddComponent<AudioSource>().clip = assetBundle.Load<AudioClip>("Ok");
        }

        public void LoadExitSound()
        {
            exitSound = new GameObject("ExitSound_Save");
            GameObject.DontDestroyOnLoad(exitSound);
            exitSound.AddComponent<AudioSource>().clip = assetBundle.Load<AudioClip>("Exit");
        }

        public AudioClip LoadOriginalChapterOST(int chapterNumber)
        {
            return assetBundle.Load<AudioClip>($"CH {chapterNumber}");
        }
    }
}
