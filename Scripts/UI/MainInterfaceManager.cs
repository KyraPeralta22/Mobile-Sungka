using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    /**
    * The Main Interface manager handles the interface actions and transitions of the Main Menu
    **/
    public class MainInterfaceManager : InterfaceManager
    {
        [Header("Selection")]
        public GameObject panelSelection;
        public Button buttonPlayAI;
        public Button buttonPlaySolo;

        public static MainInterfaceManager instance;

        private void Awake()
        {
            instance = this;
        }

        // Loads player 1 data and updates the initial screen
        void Start()
        {
            playerData = PlayerDataManager.Load();

            buttonPlayAI.onClick.AddListener(() =>
            {
                GameConfiguration gameConfiguration = ConfigurationManager.instance.aiConfiguration;
                playerData.gameMode = gameConfiguration.gameMode;
                Preloader.instance.PreloadScene(gameConfiguration.sceneName);
            });

            buttonPlaySolo.onClick.AddListener(() =>
            {
                GameConfiguration gameConfiguration = ConfigurationManager.instance.offlineConfiguration;
                playerData.gameMode = gameConfiguration.gameMode;
                Preloader.instance.PreloadScene(gameConfiguration.sceneName);
            });
        }

    }
