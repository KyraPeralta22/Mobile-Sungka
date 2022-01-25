using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SungkaBoard : MonoBehaviour
{
        public Font textHudFont;
        public Color textHudColor;
        public int textHudFontSize;
        public MaterialType materialType;

    void Start()
    {
        Pit[] pits = transform.GetComponentsInChildren<Pit>();
            foreach (Pit pit in pits)
            {
                Text textFollow = pit.hudText;
                if (textFollow != null)
                {
                    textFollow.text = "0";
                }
                else
                {
                    Debug.LogWarning("Add a Text object with to the pit if you want to show the hud");
                }
            }
    }
}