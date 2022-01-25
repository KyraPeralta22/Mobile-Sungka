using System;
using System.Collections.Generic;
using UnityEngine;

    /**
    * Player Data manager handles the player data as save, load and create
    **/
    public class PlayerDataManager : UnitySingleton<PlayerDataManager>
    {
        private const string PLAYER_DATA_KEY = "Player Data Key";

        public PlayerData playerData;

        // Saves player data
        public static void Save(PlayerData playerData)
        {
            string json = JsonUtility.ToJson(playerData);
            Debug.Log("Saving Player: " + json);
            PlayerPrefs.SetString(PLAYER_DATA_KEY, json);
            PlayerPrefs.Save();
        }

        // Loads player data
        public static PlayerData Load()
        {
            string json = PlayerPrefs.GetString(PLAYER_DATA_KEY);

            if (String.IsNullOrEmpty(json))
            {
                return Create();
            }
            else
            {
                return JsonUtility.FromJson<PlayerData>(json);
            }
        }

        // Checks if the player is new
        public static bool isNew()
        {
            return String.IsNullOrEmpty(PlayerPrefs.GetString(PLAYER_DATA_KEY));
        }

        // Creates the player data with the default configuration
        private static PlayerData Create()
        {
            Debug.Log("Creating new Player");

            PlayerData playerData = new PlayerData();

            Save(playerData);

            return playerData;
        }

    }
