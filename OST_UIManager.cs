using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using Il2CppInterop.Runtime;

namespace FS_CustomOST
{
    [RegisterTypeInIl2Cpp]
    public class OST_UIManager : MonoBehaviour
    {
        public static OST_UIManager Instance;

        public OST_Settings ostSettings;
        public GameObject ostSettingsButton;
        public GameObject ostSettingsPanel;
        public GameObject ostSettingsOptionsParent;

        public List<GameObject> songSelectionsGrids = new List<GameObject>();

        public GameObject currentTrackText;
        public GameObject currentTrackNameText;
        public GameObject currentTrackExtensionText;
        public GameObject noClipsFoundText;
        public GameObject songsPagesText;

        public GameObject showTrackInfoToggle;
        public GameObject randomizeFirstTrackToggle;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {

        }

        public void OnSceneLoaded()
        {
            if (OST_Main.Instance.currentSceneName.Contains("Level"))
            {
                // Just call that function when a scene is loaded to "refresh" the track buttons, since they are only disabled if the randomize toggle is on AND the scene is the Menu.
                EnableAllTrackButtons();
            }
            if (OST_Main.Instance.currentSceneName.Contains("Menu") && OST_Settings.Instance != null)
            {
                // And if in Menu and the toggle is enable, disable all the track buttons.
                if (OST_Settings.Instance.randomizeTrackAtStartToggle) DisableAllTrackButtons();
            }
        }

        public void Init(AudioSource ostAudioSource)
        {
            CreateOSTSettingsManager(ostAudioSource);
            CreateOSTSettingsButton();
            CreateOSTSettingsPanel();
            SetupOSTSettingsButtonsAndOptions();
            CreateAllOfTheOptions();
        }

        public void CreateOSTSettingsManager(AudioSource ostAudioSource)
        {
            // Add also the OST_Setings manager.
            ostSettings = gameObject.AddComponent<OST_Settings>();
            ostSettings.audioSource = ostAudioSource;
        }

        public void CreateOSTSettingsButton()
        {
            // Get the Debug Mode button and create a copy.
            GameObject debugModeBtn = GameObject.Find("MainMenu/Camera/Holder/Main/LatestNews/DebugMode");
            ostSettingsButton = GameObject.Instantiate(debugModeBtn, debugModeBtn.transform.parent);

            // Change the name of the copy.
            ostSettingsButton.name = "CustomOST";

            // Remove the anchors so we can move the object.
            ostSettingsButton.GetComponent<UISprite>().SetAnchor((Transform)null);
            ostSettingsButton.transform.localPosition = new Vector3(800, 130, 0);

            // Change the label of the button.
            ostSettingsButton.transform.GetChild(0).GetComponent<UILabel>().text = "OST Settings";

            // Remove any older OnClick events and add a new one to the OST_Settings class.
            ostSettingsButton.GetComponent<UIButton>().onClick.Clear();
            ostSettingsButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(ostSettings, "SwitchBetweenSettingsAndMenu"));
        }

        public void CreateOSTSettingsPanel()
        {
            // Get the Options menu and create a copy.
            GameObject originalOptionsMenu = GameObject.Find("MainMenu/Camera/Holder/Options");
            ostSettingsPanel = GameObject.Instantiate(originalOptionsMenu, originalOptionsMenu.transform.parent);

            // Change the name of the copy.
            ostSettingsPanel.name = "OST_Settings";

            // Remove the OptionsController and UILocalize components so I can change the title of the panel.
            GameObject.Destroy(ostSettingsPanel.GetComponent<OptionsController>());
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(2).GetComponent<UILocalize>());

            // Change the title of the panel.
            ostSettingsPanel.transform.GetChild(2).GetComponent<UILabel>().text = "Custom OST Settings";

            // Destroy a bunch of useless GameObjects (Look dude, I'm adding these comments right now, and I'm too lazy to see which objects are these, ok?).
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(5).gameObject);
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(7).gameObject);
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(8).gameObject);
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(9).gameObject);
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(10).gameObject);
            GameObject.Destroy(ostSettingsPanel.transform.GetChild(11).gameObject);

            // Reset the scale of the new custom menu to one.
            ostSettingsPanel.transform.localScale = Vector3.one;

            // Add a UIPanel so the TweenScale can work, also change the depth of the panel itself.
            UIPanel panel = ostSettingsPanel.AddComponent<UIPanel>();
            panel.depth = 3;
            ostSettingsPanel.GetComponent<TweenAlpha>().mRect = panel;
        }

        public void SetupOSTSettingsButtonsAndOptions()
        {
            // Get and enable the Game_Options object, since it's disabled by default.
            ostSettingsOptionsParent = ostSettingsPanel.GetChildWithName("Game_Options").GetChildWithName("Buttons");
            ostSettingsOptionsParent.transform.parent.gameObject.SetActive(true);

            // Destroy the UIPanel of "Game_Options" so the TweenScale component can work.
            Destroy(ostSettingsOptionsParent.transform.parent.GetComponent<UIPanel>());

            // Go foreach every option in this menu and disable it, so I can enable them by one by and setup them.
            foreach (GameObject child in ostSettingsOptionsParent.GetChilds())
            {
                child.SetActive(false);
            }
        }

        public void CreateAllOfTheOptions()
        {
            CreateSongSelectionList();
            CreateLoopModeDropdown();
            CreateShowCurrentTrackInfoToggle();
            CreateRandomizeTrackAtStartToggle();
            CreateCloseButton();
            CreateCreditsText();
        }


        void CreateSongSelectionList()
        {
            #region Create the BG
            // Get some references.
            GameObject subtitlesButton = ostSettingsOptionsParent.GetChildWithName("Subtitles");

            // Scale and adjust the selection list background.
            GameObject selectionBg = GameObject.Instantiate(subtitlesButton.GetChildWithName("Background"));
            selectionBg.name = "SongSelection";
            selectionBg.transform.parent = ostSettingsOptionsParent.transform;
            selectionBg.transform.localScale = new Vector3(10f, 9f, 1f);
            selectionBg.transform.localPosition = new Vector3(-500f, 0, 0);
            selectionBg.GetComponent<UISprite>().color = Color.black;
            selectionBg.GetComponent<UISprite>().atlas = ostSettingsPanel.transform.parent.GetChildWithName("Main").GetChildWithName("LargeButtons").GetChildWithName("1_Resume").GetComponent<UISprite>().atlas;
            selectionBg.GetComponent<UISprite>().spriteName = "PixelHard";
            selectionBg.GetComponent<UISprite>().depth = 3;

            // And destroy the checkmark.
            Destroy(selectionBg.GetChildWithName("Checkmark"));
            #endregion

            #region Create Grid and Buttons
            // Get another reference.
            GameObject templateButton = ostSettingsButton;

            GameObject songButtonsParent = null; // The current grid.
            for (int i = 0; i < OST_Main.Instance.clips.Length; i++)
            {
                if (i % 5 == 0 || i == 0) // Idk how the fuck explain this.
                {
                    // Create a new grid.
                    songButtonsParent = new GameObject($"Grid {i}");
                    songButtonsParent.transform.parent = selectionBg.transform;
                    songButtonsParent.transform.localPosition = Vector3.up * 14f;
                    songButtonsParent.transform.localScale = Vector3.one;
                    songButtonsParent.AddComponent<UIGrid>().maxPerLine = 1;
                    songButtonsParent.GetComponent<UIGrid>().cellWidth = 50f;
                    songButtonsParent.GetComponent<UIGrid>().cellHeight = 8f;

                    // If this isn't the first page, disable it by default, only enable the first page :D
                    if (i != 0) songButtonsParent.gameObject.SetActive(false);

                    // Add it to the grids list.
                    songSelectionsGrids.Add(songButtonsParent);
                }

                // Create a new button.
                GameObject templateInstance = Instantiate(templateButton);
                templateInstance.name = i + "";
                templateInstance.transform.parent = songButtonsParent.transform;
                templateInstance.transform.localPosition = Vector3.zero;
                templateInstance.transform.localScale = Vector3.one * 0.1f;
                templateInstance.GetComponent<UIButtonScale>().mScale = Vector3.one * 0.1f;
                templateInstance.GetComponent<UISprite>().width = 405;
                templateInstance.GetComponent<BoxCollider>().size = new Vector3(405f, templateInstance.GetComponent<BoxCollider>().size.y, 0f);

                // Change the label text.
                Destroy(templateInstance.GetChildWithName("Label").GetComponent<UILocalize>());
                templateInstance.GetChildWithName("Label").GetComponent<UILabel>().text = Path.GetFileName(OST_Main.Instance.clips[i]);

                // Set a new action (play the specified song).
                templateInstance.GetComponent<UIButton>().onClick.Clear();
                EventDelegate onClickEvent = new EventDelegate(OST_Settings.Instance, "ChangeSongToOneWithName");
                onClickEvent.mParameters = new EventDelegate.Parameter[]
                {
                    new EventDelegate.Parameter
                    {
                        field = "songNameWithExtension",
                        value = Path.GetFileName(OST_Main.Instance.clips[i]),
                        obj = OST_Settings.Instance,
                    }
                };
                templateInstance.GetComponent<UIButton>().onClick.Add(onClickEvent);
            }
            #endregion

            if (OST_Settings.Instance.totalSongPages > 1)
            {
                #region Create Pages Buttons
                GameObject previousPageButton = Instantiate(ostSettingsButton);
                previousPageButton.name = "PrevPage";
                previousPageButton.transform.parent = ostSettingsOptionsParent.transform;
                previousPageButton.transform.localScale = Vector3.one;
                previousPageButton.transform.localPosition = new Vector3(-630f, 180f, 0f);

                previousPageButton.transform.GetComponent<UISprite>().width = 45;
                previousPageButton.transform.GetComponent<UISprite>().height = 45;
                previousPageButton.transform.GetComponent<BoxCollider>().size = new Vector3(45f, 45f, 0f);
                previousPageButton.transform.GetComponent<UIButtonScale>().mScale = Vector3.one;
                previousPageButton.transform.GetChild(0).GetComponent<UILabel>().text = "<";
                previousPageButton.transform.GetChild(0).GetComponent<UILabel>().fontSize = 50;

                previousPageButton.transform.GetComponent<UIButton>().onClick.Clear();
                previousPageButton.transform.GetComponent<UIButton>().onClick.Add(new EventDelegate(OST_Settings.Instance, "OnPrevSongsPage"));

                GameObject nextPageButton = Instantiate(ostSettingsButton);
                nextPageButton.name = "PrevPage";
                nextPageButton.transform.parent = ostSettingsOptionsParent.transform;
                nextPageButton.transform.localScale = Vector3.one;
                nextPageButton.transform.localPosition = new Vector3(-370f, 180f, 0f);

                nextPageButton.transform.GetComponent<UISprite>().width = 45;
                nextPageButton.transform.GetComponent<UISprite>().height = 45;
                nextPageButton.transform.GetComponent<BoxCollider>().size = new Vector3(45f, 45f, 0f);
                nextPageButton.transform.GetComponent<UIButtonScale>().mScale = Vector3.one;
                nextPageButton.transform.GetChild(0).GetComponent<UILabel>().text = ">";
                nextPageButton.transform.GetChild(0).GetComponent<UILabel>().fontSize = 50;

                nextPageButton.transform.GetComponent<UIButton>().onClick.Clear();
                nextPageButton.transform.GetComponent<UIButton>().onClick.Add(new EventDelegate(OST_Settings.Instance, "OnNextSongsPage"));
                #endregion

                #region Create Pages Text
                // Create the text object.
                songsPagesText = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
                songsPagesText.name = "PagesText";
                songsPagesText.transform.parent = ostSettingsOptionsParent.transform;

                // Change its label text and font size too.
                songsPagesText.GetComponent<UILabel>().text = $"{OST_Settings.Instance.currentSongsPage + 1}/{OST_Settings.Instance.totalSongPages}";
                songsPagesText.GetComponent<UILabel>().fontSize = 30;
                songsPagesText.transform.localPosition = new Vector3(-500f, 180f, 0f);
                songsPagesText.transform.localScale = Vector3.one;
                #endregion
            }

            #region Create Current Track Text
            // Create the text object.
            currentTrackText = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
            currentTrackText.name = "CurrentTrack";
            currentTrackText.transform.parent = ostSettingsOptionsParent.transform;

            // Change its label text and font size too.
            currentTrackText.GetComponent<UILabel>().text = $"Current track: ?/{OST_Main.Instance.clips.Length}";
            currentTrackText.GetComponent<UILabel>().fontSize = 30;
            currentTrackText.transform.localPosition = new Vector3(-500f, 350f, 0f);
            currentTrackText.transform.localScale = Vector3.one;

            // Create track NAME object.
            currentTrackNameText = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
            currentTrackNameText.name = "CurrentTrackName";
            currentTrackNameText.transform.parent = ostSettingsOptionsParent.transform;

            // Change its label text and font size TOO.
            currentTrackNameText.GetComponent<UILabel>().text = "None";
            currentTrackNameText.GetComponent<UILabel>().fontSize = 50;
            currentTrackNameText.GetComponent<UILabel>().color = Color.yellow;
            currentTrackNameText.transform.localPosition = new Vector3(-500f, 300f, 0f);
            currentTrackNameText.transform.localScale = Vector3.one;

            // Create track EXTENSION object.
            currentTrackExtensionText = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
            currentTrackExtensionText.name = "CurrentTrackExtension";
            currentTrackExtensionText.transform.parent = ostSettingsOptionsParent.transform;

            // Change its label text and font size TOO.
            //if (!string.IsNullOrEmpty(OST_Main.Instance.currentClipName)) // Since I use the ToUpper method, I need to check this, SHIT.
            //{
            //    currentTrackExtensionText.GetComponent<UILabel>().text = Path.GetExtension(OST_Main.Instance.currentClipName).Substring(1).ToUpper();
            //}
            currentTrackExtensionText.GetComponent<UILabel>().text = "???";
            currentTrackExtensionText.GetComponent<UILabel>().fontSize = 30;
            currentTrackExtensionText.GetComponent<UILabel>().color = Color.red;
            currentTrackExtensionText.transform.localPosition = new Vector3(-500f, 250f, 0f);
            currentTrackExtensionText.transform.localScale = Vector3.one;
            #endregion

            #region Create No Tracks Found Text
            if (OST_Main.Instance.clips.Length == 0)
            {
                // Create text object.
                noClipsFoundText = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
                noClipsFoundText.name = "NoTracksFound";
                noClipsFoundText.transform.parent = ostSettingsOptionsParent.transform;

                // Change its label text and font size TOO.
                noClipsFoundText.GetComponent<UILabel>().text = "No tracks found.";
                noClipsFoundText.GetComponent<UILabel>().fontSize = 30;
                noClipsFoundText.GetComponent<UILabel>().color = Color.yellow;
                noClipsFoundText.transform.localPosition = new Vector3(-500f, 170f, 0f);
                noClipsFoundText.transform.localScale = Vector3.one;
            }
            #endregion
        }

        void CreateLoopModeDropdown()
        {
            // Get the old language dropdown popup (since it's the only dropdown in this menu) and enable it.
            GameObject loopModeDropdown = ostSettingsOptionsParent.GetChildWithName("LanguagePanel").GetChildWithName("LanguagePopup");
            loopModeDropdown.transform.parent.gameObject.SetActive(true);
            loopModeDropdown.transform.parent.name = "LoopModePanel";
            loopModeDropdown.name = "LoopModePopup";

            // Change the dropdown position.
            loopModeDropdown.transform.parent.localPosition = new Vector3(100f, loopModeDropdown.transform.parent.localPosition.y, loopModeDropdown.transform.parent.localPosition.z);

            // Like always, remove the UI_Localize component and change the dropdown title.
            GameObject.Destroy(loopModeDropdown.GetChildWithName("LanguageTite").GetComponent<UILocalize>()); // No, it's not me, the name is bad written, lol
            loopModeDropdown.GetChildWithName("LanguageTite").GetComponent<UILabel>().text = "Loop Mode";

            // Set the default option and also change the text in the selected language label.
            loopModeDropdown.GetComponent<UIPopupList>().selection = "Queue";
            loopModeDropdown.gameObject.GetChildWithName("CurrentLanguageBG").GetChildWithName("CurrentLanguageLabel").GetComponent<UILabel>().text = loopModeDropdown.GetComponent<UIPopupList>().value;

            // Same, remove all the old options and add the new ones.
            loopModeDropdown.GetComponent<UIPopupList>().items.Clear();
            loopModeDropdown.GetComponent<UIPopupList>().items.Add("Queue");
            loopModeDropdown.GetComponent<UIPopupList>().items.Add("Random");
            loopModeDropdown.GetComponent<UIPopupList>().items.Add("Song");
            loopModeDropdown.GetComponent<UIPopupList>().items.Add("None");

            // SAME, remove all the old events and add a new one.
            loopModeDropdown.GetComponent<UIPopupList>().onChange.Clear();
            loopModeDropdown.GetComponent<UIPopupList>().onChange.Add(new EventDelegate(ostSettings, "OnLoopModeChange"));

            // Add Tooltip.
            FractalTooltip tooltip = loopModeDropdown.AddComponent<FractalTooltip>();
            tooltip.staticTooltipOffset = new Vector2(0f, 0.2f);
            tooltip.toolTipLocKey = "Changes the loop mode between [b][c][00ffff]tracks.[-][/b]\n" +
                                    "\n" +
                                    "- [b][c][00ffff]Queue:[-][/b] Plays all the tracks of the list in order.\n" +
                                    "- [b][c][00ffff]Random:[-][/b] Plays tracks of the list randomly.\n" +
                                    "- [b][c][00ffff]Song:[-][/b] Plays the same selected track indefinitely.\n" +
                                    "- [b][c][00ffff]None:[-][/b] When the track ends, the OST stops.";

            // Set the dropdown in the OST_Settings class.
            ostSettings.loopModeDropdown = loopModeDropdown.GetComponent<UIPopupList>();
        }

        void CreateShowCurrentTrackInfoToggle()
        {
            // Get the subtitles toggle and change its name.
            showTrackInfoToggle = ostSettingsOptionsParent.GetChildWithName("Subtitles");
            showTrackInfoToggle.name = "ShowTrackInfo";
            showTrackInfoToggle.SetActive(true);

            // DESTROY THE FUCKING LOCALIZATION COMPONENT and change the text :).
            Destroy(showTrackInfoToggle.GetChildWithName("Label").GetComponent<UILocalize>());
            showTrackInfoToggle.GetChildWithName("Label").GetComponent<UILabel>().text = "Show Current Track Info";

            // Set the new action of the toggle.
            showTrackInfoToggle.GetComponent<UIButton>().onClick.Clear();
            showTrackInfoToggle.GetComponent<UIButton>().onClick.Add(new EventDelegate(ostSettings, "OnShowTrackInfoToggle"));
            showTrackInfoToggle.GetComponent<UIToggle>().Set(true);
            showTrackInfoToggle.transform.localPosition = new Vector3(400f, 170f, 0f);

            // Set the text of the tooltip.
            FractalTooltip tooltip = showTrackInfoToggle.AddComponent<FractalTooltip>();
            tooltip.staticTooltipOffset = new Vector2(0.4f, 0.1f);
            tooltip.toolTipLocKey = "Displays info about the [b][c][00FFFF]tracks[-][/c][/b] in the top-left of the screen, such as:\n\n- Current clip name.\n- Play time.\n- Current clip ID.\n- Is the OST playing?\n- [b][c][ffde21]CONTROLS[-][/c][/b]";

            // Set the toggle in the OST_Settings class.
            ostSettings.showTrackInfoToggle = showTrackInfoToggle.GetComponent<UIToggle>();

            // Execute this one since the default value of the toggle is true.
            ostSettings.OnShowTrackInfoToggle();
        }

        void CreateRandomizeTrackAtStartToggle()
        {
            // Get the speedrun toggle and change its name.
            randomizeFirstTrackToggle = ostSettingsOptionsParent.GetChildWithName("SpeedRunMode");
            randomizeFirstTrackToggle.name = "RandomizeTrackAtStart";
            randomizeFirstTrackToggle.SetActive(true);

            // DESTROY THE FUCKING LOCALIZATION COMPONENT and change the text :).
            Destroy(randomizeFirstTrackToggle.GetChildWithName("Label").GetComponent<UILocalize>());
            randomizeFirstTrackToggle.GetChildWithName("Label").GetComponent<UILabel>().text = "Randomize Track At Start";

            // Set the new action of the toggle.
            randomizeFirstTrackToggle.GetComponent<UIButton>().onClick.Clear();
            randomizeFirstTrackToggle.GetComponent<UIButton>().onClick.Add(new EventDelegate(ostSettings, "OnRandomizeTrackAtStart"));
            randomizeFirstTrackToggle.transform.localPosition = new Vector3(-700f, -270f, 0f);

            // Set the text of the tooltip, since when you put a localization key that doesn't exists here, it puts the text as is, so, no problem :)
            randomizeFirstTrackToggle.GetComponent<FractalTooltip>().toolTipLocKey = "Randomizes the [c][00FFFF]first track[-][/c] when you start or enter in a chapter.";

            // Set the toggle in the OST_Settings class.
            ostSettings.randomizeTrackAtStartToggle = randomizeFirstTrackToggle.GetComponent<UIToggle>();
        }

        void CreateCloseButton()
        {
            // Create a copy of the OST Settings button and change its parent to the options' parent.
            GameObject closeBtn = GameObject.Instantiate(ostSettingsButton);
            closeBtn.transform.parent = ostSettingsOptionsParent.transform;
            closeBtn.name = "CloseBtn";

            // Change its label to "Close", and I don't need to remove the damn UI_Localize component anymore since the copy don't has it :)
            closeBtn.transform.GetChild(0).GetComponent<UILabel>().text = "Close";

            // Change the button scale, and for some reason I also needed to modify the scale in the UIButtonScale component.
            closeBtn.GetComponent<UIButtonScale>().mScale = Vector3.one;
            closeBtn.transform.localScale = Vector3.one;

            // Change the button's position.
            closeBtn.transform.localPosition = new Vector3(700f, -330f, 0f);

            // SAME CRAP, remove the previous event and add a new one.
            closeBtn.GetComponent<UIButton>().onClick.Clear();
            closeBtn.GetComponent<UIButton>().onClick.Add(new EventDelegate(ostSettings, "SwitchBetweenSettingsAndMenu"));
        }

        void CreateCreditsText()
        {
            // Create a copy of the menu title and change its partent to the options' parent.
            GameObject credits = GameObject.Instantiate(ostSettingsPanel.GetChildWithName("Title"));
            credits.transform.parent = ostSettingsOptionsParent.transform;
            credits.name = "Credits";

            // Change its label text and font size too.
            credits.GetComponent<UILabel>().text = "Created by Javialon_qv & Testing by Fefeh";
            credits.GetComponent<UILabel>().fontSize = 30;
            credits.GetComponent<UILabel>().alignment = NGUIText.Alignment.Left;
            credits.GetComponent<UILabel>().width = 800;

            // Reset scale to one.
            credits.transform.localScale = Vector3.one;

            // Change its position to the botton-left.
            credits.transform.localPosition = new Vector3(-430f, -350f, 0f);
        }


        public void UpdateCurrentTrackText()
        {
            currentTrackText.GetComponent<UILabel>().text = $"Current track: {OST_Main.Instance.currentClipID + 1}/{OST_Main.Instance.clips.Length}";

            if (OST_Main.Instance.audioClipLoaded)
            {
                currentTrackNameText.GetComponent<UILabel>().text = Path.GetFileNameWithoutExtension(OST_Main.Instance.currentClipName);
                currentTrackExtensionText.GetComponent<UILabel>().text = Path.GetExtension(OST_Main.Instance.currentClipName).Substring(1).ToUpper();
            }
            else
            {
                if (!string.IsNullOrEmpty(OST_Main.Instance.currentClipName)) currentTrackNameText.GetComponent<UILabel>().text = "Loading...";
                else currentTrackNameText.GetComponent<UILabel>().text = "None";
                currentTrackExtensionText.GetComponent<UILabel>().text = "???";
            }
        }

        public void UpdateCurrentPageText()
        {
            songsPagesText.GetComponent<UILabel>().text = $"{OST_Settings.Instance.currentSongsPage + 1}/{OST_Settings.Instance.totalSongPages}";
        }

        public void EnableAllTrackButtons()
        {
            foreach (GameObject grid in songSelectionsGrids)
            {
                foreach (GameObject obj in grid.GetChilds())
                {
                    obj.GetComponent<UIButton>().isEnabled = true;
                }
            }
        }
        public void DisableAllTrackButtons()
        {
            foreach (GameObject grid in songSelectionsGrids)
            {
                foreach (GameObject obj in grid.GetChilds())
                {
                    obj.GetComponent<UIButton>().isEnabled = false;
                }
            }
        }
    }
}
