using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    /**
    * Simple preloader activated upon scene transaction
    **/
    public class Preloader : UnitySingleton<Preloader>
    {
        public CanvasGroup panelPreloader;

        public void PreloadScene(string sceneName)
        {
            panelPreloader.alpha = 0;
            panelPreloader.gameObject.SetActive(true);
            StartCoroutine(LoadSceneCR(sceneName));
        }

        private IEnumerator LoadSceneCR(string sceneName)
        {
            var ao = SceneManager.LoadSceneAsync(sceneName);
            ao.allowSceneActivation = false;

            var t = 0f;
            // Fades In the Preloader
            while (t < 1f)
            {
                panelPreloader.alpha = Mathf.Lerp(0, 1, t);
                t += Time.deltaTime;
                yield return null;
            }

            panelPreloader.alpha = 1f;

            while (ao.progress < 0.9f)
            {
                yield return null;
            }

            ao.allowSceneActivation = true;
            //yield return new WaitForSeconds(1f);

            t = 0f;
            // Fades Out the Preloader
            while (t < 1f)
            {
                panelPreloader.alpha = Mathf.Lerp(1, 0, t);
                t += Time.deltaTime;
                yield return null;
            }

            panelPreloader.gameObject.SetActive(false);
        }
    }
