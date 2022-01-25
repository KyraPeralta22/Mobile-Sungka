using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    /**
    * Dynamic Room Button UI
    **/
    public class RoomButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text textName;

        public GameConfiguration gameConfiguration;

        public void Initialize()
        {
            textName.text = gameConfiguration.name;
        }
    }