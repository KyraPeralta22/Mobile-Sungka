using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PitSelect : MonoBehaviour
    {

        private Pit pit;
        private bool isOver = false;                // Check if its over the object
        private GameplayManager gameplayManager;

        void Awake()
        {
            print("Awake");
            pit = transform.GetComponentInParent<Pit>();
            gameplayManager = GameplayManager.instance;
        }

        public void OnMouseUp()
        {
            print("OnMouseUp");
            if (isOver)
            {
                GameplayManager.instance.OnPitClick(pit.number);
            }
        }

        // highlights on mouse over
        public void OnMouseEnter()
        {
            isOver = true;
            print("OnMouseEnter");
 
            if (gameplayManager.state == GameplayManager.GameState.TURN_START)
            {
                // Enables highlight
                Stone[] stones = transform.parent.GetComponentsInChildren<Stone>();
                bool canSelect = gameplayManager.CanSelectPit(pit.number, false);
                foreach (Stone stone in stones)
                {
                    stone.Highlight(true, canSelect);
                }
            }
        }

        public void OnMouseExit()
        {
            isOver = false;
            print("OnMouseExit");

            if (gameplayManager.state == GameplayManager.GameState.TURN_START)
            {
                // Disables highlight
                Stone[] stones = transform.parent.GetComponentsInChildren<Stone>();
                foreach (Stone stone in stones)
                {
                    stone.Highlight(false, true);
                }
            }
        }
    }
