using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

    /*
    The Abstract Interface manager handles the interface actions and transitions for all interfaces
    */
    public abstract class InterfaceManager : MonoBehaviour
    {

        protected static int openState = Animator.StringToHash("Base Layer.Open");
        protected static int closeState = Animator.StringToHash("Base Layer.Close");

        // Dynamic player 1 Data
        protected PlayerData playerData
        {
            get { return PlayerDataManager.instance.playerData; }
            set { PlayerDataManager.instance.playerData = value; }
        }

        // Closes the menu
        protected void CloseMenu(GameObject menu)
        {
            StartCoroutine(CloseMenuCR(menu));
        }

        // Closes then opens a menu after is closed
        protected IEnumerator CloseMenuCR(GameObject menuClose, GameObject menuOpen)
        {
            if (menuClose != null)
            {
                yield return StartCoroutine(CloseMenuCR(menuClose));
            }

            OpenMenu(menuOpen);
        }

        protected IEnumerator CloseMenuCR(GameObject menu)
        {
            if (menu != null && menu.activeInHierarchy)
            {
                Animator animator = menu.GetComponent<Animator>();
                if (animator != null)
                {
                    int nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

                    if (nameHash != closeState)
                    {
                        animator.SetTrigger("close");
                    }

                    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                    menu.SetActive(false);
                }
                else
                {
                    menu.SetActive(false);
                }
            }
        }

        // Opens a menu then after a few seconds, closes it
        protected void OpenMenu(GameObject menu, float delay)
        {
            StartCoroutine(OpenMenuCR(menu, delay));
        }

        private IEnumerator OpenMenuCR(GameObject menu, float delay)
        {
            if (menu != null)
            {
                menu.SetActive(true);
                Animator animator = menu.GetComponent<Animator>();
                if (animator != null)
                {
                    int nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                    if (nameHash != openState)
                    {
                        animator.SetTrigger("open");
                    }
                }
                yield return new WaitForSeconds(delay);

                yield return StartCoroutine(CloseMenuCR(menu));
            }
        }

        protected void OpenMenu(GameObject menu)
        {
            if (menu != null)
            {
                menu.SetActive(true);
                Animator animator = menu.GetComponent<Animator>();
                if (animator != null)
                {
                    int nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                    if (nameHash != openState)
                    {
                        animator.SetTrigger("open");
                    }
                }
            }
        }

    }
