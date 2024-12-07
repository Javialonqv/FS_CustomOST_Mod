using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using MelonLoader;
using System.Collections;
using Il2Cpp;

namespace FS_CustomOST
{
#pragma warning disable IDE0051
    [RegisterTypeInIl2Cpp]
    public class OST_Settings : MonoBehaviour
    {
        public static OST_Settings Instance;

        AudioClip originalOST;

        public AudioSource audioSource;
        public bool inOstMenu;
        public AudioSource uiSoundSource;

        public enum LoopMode { Queue, Random, Song, None }
        public LoopMode loopMode = LoopMode.Queue;
        public int currentSongsPage = 0;
        public int totalSongPages = 1;

        public bool enableCustomOST;
        public bool showTrackInfo;
        public bool randomizeTrackAtStart;
        public float pitchValue;
        public float menuTransparency;

        public UIToggle enableOSTToggle;
        public UIPopupList loopModeDropdown;
        public UIToggle showTrackInfoToggle;
        public UIToggle randomizeTrackAtStartToggle;
        public UISlider pitchSlider;
        public UISlider menuTransparencySlider;

        public EventDelegate uiResumeButtonOriginalEvent;

        void Awake()
        {
            Instance = this;
            totalSongPages = GetTotalSongPages();
        }

        void Start()
        {
            uiSoundSource = GameObject.Find("MainMenu/UISound").GetComponent<AudioSource>();
        }

        void Update()
        {
            // If you're in the ost settings menu and press the escape key, exit the settings and enable the MAIN menu.
            if (inOstMenu && Input.GetKeyDown(KeyCode.Escape))
            {
                SwitchBetweenSettingsAndMenu();
            }

            // If the loop mode is in Queue.
            if ((loopMode == LoopMode.Random || loopMode == LoopMode.Queue) && audioSource.clip != null)
            {
                // And the current song ended.
                if (audioSource.time >= audioSource.clip.length - 0.1f)
                {
                    if (loopMode == LoopMode.Queue)
                    {
                        Melon<OST_Main>.Logger.Msg("----------");
                        Melon<OST_Main>.Logger.Msg("PLAYING NEXT SONG!");
                        Melon<OST_Main>.Instance.NextSong(true);
                    }
                    else if (loopMode == LoopMode.Random)
                    {
                        Melon<OST_Main>.Logger.Msg("----------");
                        Melon<OST_Main>.Logger.Msg("PLAYING RANDOM SONG!");
                        Melon<OST_Main>.Instance.PlayRandomSong();
                    }
                }
            }
        }

        public void SaveOriginalOST()
        {
            originalOST = GameObject.Find("MusicManager/LevelNormalSource").GetComponent<AudioSource>().clip;
            Melon<OST_Main>.Logger.Msg($"Null? {originalOST == null} and the source? {GameObject.Find("MusicManager/LevelNormalSource").GetComponent<AudioSource>() == null}");
            originalOST.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        public int GetTotalSongPages()
        {
            int temp = OST_Main.Instance.clips.Length / 5;
            if ((float)OST_Main.Instance.clips.Length / 5 > temp) temp++;
            OST_Main.Instance.LoggerInstance.Msg($"Total clips pages: {temp}");
            return temp;
        }

        // Switch between the main menu and the ost settings menu.
        public void SwitchBetweenSettingsAndMenu()
        {
            // Find the needed UI objects.
            GameObject mainMenu = GameObject.Find("MainMenu/Camera/Holder/Main");
            GameObject settings = GameObject.Find("MainMenu/Camera/Holder/OST_Settings");

            // Switch!
            inOstMenu = !inOstMenu;

            MelonCoroutines.Start(Animation());

            IEnumerator Animation()
            {
                if (inOstMenu)
                {
                    uiSoundSource.clip = OST_UIManager.Instance.sfxLoader.okSoundClip;
                    uiSoundSource.Play();

                    mainMenu.GetComponent<TweenAlpha>().PlayIgnoringTimeScale(true);
                    settings.SetActive(true);
                    settings.GetComponent<TweenAlpha>().PlayIgnoringTimeScale(false);
                    settings.GetComponent<TweenScale>().PlayIgnoringTimeScale(false);
                    yield return new WaitForSecondsRealtime(0.2f);
                    mainMenu.SetActive(false);
                }
                else
                {
                    uiSoundSource.clip = OST_UIManager.Instance.sfxLoader.exitSoundClip;
                    uiSoundSource.Play();

                    mainMenu.SetActive(true);
                    mainMenu.GetComponent<TweenAlpha>().PlayIgnoringTimeScale(false);
                    settings.GetComponent<TweenAlpha>().PlayIgnoringTimeScale(true);
                    settings.GetComponent<TweenScale>().PlayIgnoringTimeScale(true);
                    yield return new WaitForSecondsRealtime(0.2f);
                    settings.SetActive(false);
                }
            }

            if (inOstMenu)
            {
                ChangeUIResumeButtonAction(); // Patch the UI resume button so when you press it the OST menu is disabled correctly.
                EditNavigationButtonsWhenEnteringOSTMenu(); // Also, edit the navigation bar buttons when entering the OST menu.
            }
            else
            {
                ResetUIResumeButtonAction(); // If you're exiting, then reset the UI resume button to its original state.
                EditNavigationButtonsWhenExitingOSTMenu(); // Also, edit the navigation bar buttons to its original state when exiting the OST menu.
            }

            // To fix the transparent menu bug, force the alpha to be 1 (opaque) when the ost settings is open.
            if (settings.activeSelf)
            {
                Color settingsColor = settings.GetChildWithName("Window").GetComponent<UISprite>().color;
                settingsColor.a = 1f;
                settings.GetChildWithName("Window").GetComponent<UISprite>().color = settingsColor;
            }
        }

        public void OnEnableOSTToggle()
        {
            enableCustomOST = enableOSTToggle.value;

            OST_UIManager.Instance.EnableOrDisableUIButtons(enableCustomOST, randomizeTrackAtStart);

            if (!enableCustomOST)
            {
                OST_Main.Instance.currentClipName = "";
                OST_Main.Instance.audioClipLoaded = false;
                OST_UIManager.Instance.UpdateCurrentTrackText();

                audioSource.clip = originalOST;
                audioSource.Play();
            }
        }

        public void OnLoopModeChange()
        {
            // Change the label of the current mode in the dropdown.
            loopModeDropdown.gameObject.GetChildWithName("CurrentLanguageBG").GetChildWithName("CurrentLanguageLabel").GetComponent<UILabel>().text = loopModeDropdown.value;
            // Also change the variable.
            loopMode = (LoopMode)Enum.Parse(typeof(LoopMode), loopModeDropdown.value);

            // Only when is in Song the loop is true, because when it's Queue, the looop is managed by this class manually.
            switch (loopMode)
            {
                case LoopMode.Queue:
                    audioSource.loop = false;
                    break;

                case LoopMode.Random:
                    audioSource.loop = false;
                    break;

                case LoopMode.Song:
                    audioSource.loop = true;
                    break;

                case LoopMode.None:
                    audioSource.loop = false;
                    break;
            }
        }

        public void OnPrevSongsPage()
        {
            currentSongsPage--;
            if (currentSongsPage < 0)
            {
                currentSongsPage = 0;
                OST_UIManager.Instance.UpdateCurrentPageText();
                return;
            }

            GameObject songSelection = GameObject.Find("MainMenu/Camera/Holder/OST_Settings/Game_Options/Buttons/SongSelection");

            foreach (GameObject obj in songSelection.GetChilds())
            {
                obj.SetActive(false);
            }

            songSelection.transform.GetChild(currentSongsPage).gameObject.SetActive(true);

            OST_UIManager.Instance.UpdateCurrentPageText();
        }

        public void OnNextSongsPage()
        {
            currentSongsPage++;
            if (currentSongsPage >= totalSongPages)
            {
                currentSongsPage = totalSongPages - 1;
                OST_UIManager.Instance.UpdateCurrentPageText();
                return;
            }

            GameObject songSelection = GameObject.Find("MainMenu/Camera/Holder/OST_Settings/Game_Options/Buttons/SongSelection");

            foreach (GameObject obj in songSelection.GetChilds())
            {
                obj.SetActive(false);
            }

            songSelection.transform.GetChild(currentSongsPage).gameObject.SetActive(true);

            OST_UIManager.Instance.UpdateCurrentPageText();
        }

        public void OnShowTrackInfoToggle()
        {
            showTrackInfo = showTrackInfoToggle.value;
        }

        public void OnPitchSliderValueChanged()
        {
            // Multiply by 2 to simulate the slider goes from 0 to 2.
            pitchValue = pitchSlider.value * 2f;

            // Change the pitch of the song.
            audioSource.pitch = pitchValue;

            // Change the label showing the current pitch level.
            pitchSlider.gameObject.GetChildWithName("Thumb").GetChildWithName("ValueLabel").GetComponent<UILabel>().text = pitchValue.ToString("F2");
        }

        public void OnMenuTransparencySliderValueChanged()
        {
            menuTransparency = menuTransparencySlider.value;

            UISprite menuWindowSprite = OST_UIManager.Instance.ostSettingsPanel.GetChildWithName("Window").GetComponent<UISprite>();
            menuWindowSprite.color = new Color(menuWindowSprite.color.r, menuWindowSprite.color.g, menuWindowSprite.color.b, menuTransparency);
        }


        public void OnRandomizeTrackAtStart()
        {
            if (OST_Main.Instance.currentSceneName.Contains("Level")) return;

            // Set the value.
            randomizeTrackAtStart = randomizeTrackAtStartToggle.value;

            // Disable or enable every track button.
            OST_UIManager.Instance.EnableOrDisableUIButtons(enableOSTToggle.value, randomizeTrackAtStartToggle.value);

            // Clear selected song if randomize is enabled.
            if (randomizeTrackAtStart)
            {
                OST_Main.Instance.currentClipName = "";
                OST_Main.Instance.audioClipLoaded = false;
                OST_UIManager.Instance.UpdateCurrentTrackText();
            }
        }

        public void ChangeSongToOneWithName(string songNameWithExtension)
        {
            int clipID = Array.FindIndex(OST_Main.Instance.clips, clip => Path.GetFileName(clip) == songNameWithExtension);

            OST_Main.Instance.PlaySongWithID(clipID);
        }

        public void ChangeUIResumeButtonAction()
        {
            GameObject uiResumeButton = GameObject.Find("MainMenu/Camera/Holder/Navigation/Holder/Bar/ActionsHolder/NavigationAction(Clone)/Background");

            uiResumeButtonOriginalEvent = uiResumeButton.GetComponent<UIButton>().onClick[0];
            uiResumeButton.GetComponent<UIButton>().onClick.Clear();
            uiResumeButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "SwitchBetweenSettingsAndMenu"));
        }

        public void ResetUIResumeButtonAction()
        {
            MelonCoroutines.Start(Coroutine());

            IEnumerator Coroutine()
            {
                GameObject uiResumeButton = GameObject.Find("MainMenu/Camera/Holder/Navigation/Holder/Bar/ActionsHolder/NavigationAction(Clone)/Background");

                uiResumeButton.GetComponent<UIButton>().onClick.Clear();
                yield return new WaitForSecondsRealtime(0.1f);
                uiResumeButton.GetComponent<UIButton>().onClick.Add(uiResumeButtonOriginalEvent);
            }
        }

        public void EditNavigationButtonsWhenEnteringOSTMenu()
        {
            // Reference.
            GameObject navigationBarButtonsParent = GameObject.Find("MainMenu/Camera/Holder/Navigation/Holder/Bar/ActionsHolder");

            // Change the label text of the button containing the "ESC" image.
            navigationBarButtonsParent.transform.GetChild(0).GetChildWithName("Label").GetComponent<UILabel>().text = "BACK";
            // Disable the "CONFIRM" button.
            navigationBarButtonsParent.transform.GetChild(1).gameObject.SetActive(false);
        }

        public void EditNavigationButtonsWhenExitingOSTMenu()
        {
            // Reference.
            GameObject navigationBarButtonsParent = GameObject.Find("MainMenu/Camera/Holder/Navigation/Holder/Bar/ActionsHolder");

            // Change the label text of the button containing the "ESC" image.
            navigationBarButtonsParent.transform.GetChild(0).GetChildWithName("Label").GetComponent<UILabel>().text = "EXIT";
            // Enable the "CONFIRM" button again.
            navigationBarButtonsParent.transform.GetChild(1).gameObject.SetActive(true);
        }

        /// <summary>
        /// Converts Seconds to minutes folowwing the format "MM:SS";
        /// </summary>
        /// <param name="time">The time in seconds.</param>
        /// <returns>A string, lol.</returns>
        public static string SecondsToMinutes(float time)
        {
            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = Mathf.FloorToInt(time % 60);

            return $"{minutes}:{seconds}";
        }

        public static string GetTrackButtonTooltip(string trackFilePath)
        {
            // If it's an original chapter's track, there's a CUSTOM tooltip for them.
            if (OST_Main.originalChapterTracks.Contains(trackFilePath))
            {
                string chapterNumber = trackFilePath.Split(' ')[1];
                return $"The original OST of the [b][c][00ffff]CHAPTER {chapterNumber}.[-][/c][/b]";
            }
            else // Otherwise, use TagLib# as always.
            {
                // Load metadata.
                var file = TagLib.File.Create(trackFilePath);

                // Misc things.
                TimeSpan durationSpan = file.Properties.Duration;

                // Fetch track metadata.
                string title = file.Tag.Title;
                string album = file.Tag.Album;
                string artist = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "";
                string duration = durationSpan.Hours > 0 ? $"{durationSpan.Hours}h {durationSpan.Minutes}m {durationSpan.Seconds}" :
                    (durationSpan.Minutes > 0 ? $"{durationSpan.Minutes}m {durationSpan.Seconds}s" : $"{durationSpan.Seconds}s"); // Ik this code it bullshit, but I'm lazy, ok?

                // Extra stuff.
                string artistTooltipName = artist.Contains(";") ? "ARTISTS:" : "ARTIST:";

                // Manage when they are null.
                if (string.IsNullOrEmpty(title)) title = "[b][c][fe0000]UNKNOWN[-][/c][/b]";
                if (string.IsNullOrEmpty(album)) album = "[b][c][fe0000]UNKNOWN[-][/c][/b]";
                if (string.IsNullOrEmpty(artist)) artist = "[b][c][fe0000]UNKNOWN[-][/c][/b]";

                // Make the final tooltip.
                StringBuilder tooltip = new StringBuilder();
                tooltip.AppendLine($"[b][c][00ffff]TITLE:[-][/c][/b] {title}");
                tooltip.AppendLine($"[b][c][00ffff]ALBUM:[-][/c][/b] {album}");
                tooltip.AppendLine($"[b][c][00ffff]{artistTooltipName}:[-][/c][/b] {artist}");
                tooltip.Append($"[b][c][00ffff]DURATION:[-][/c][/b] {duration}");

                // Return the tooltip.
                return tooltip.ToString();
            }
        }
    }
}
