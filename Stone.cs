using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /*
    The Stone class represents a stone GameObject
    */
    public class Stone : MonoBehaviour
    {
        private AudioSource audioSource;
        public GameObject hightlight;
        public MaterialType materialType;

        void Awake()
        {
            audioSource = FindObjectOfType<AudioSource>();
        }

        // Plays a sound according to the material type of both colliders
        void OnCollisionEnter(Collision col)
        {
            // Ignore small collisions
            if (col.relativeVelocity.magnitude < 1)
            {
                return;
            }

            var materialType = MaterialType.WOOD;

            if (col.gameObject.tag == "Stone")
            {
                Stone stone = col.gameObject.GetComponent<Stone>();
                materialType = stone.materialType;
            }

            if (col.gameObject.tag == "SungkaBoard")
            {
                SungkaBoard board = col.gameObject.GetComponentInParent<SungkaBoard>();
                materialType = board.materialType;
            }

            switch (materialType)
            {
                case MaterialType.WOOD:
                    switch (materialType)
                    {
                       
                        case MaterialType.STONE:
                            audioSource.clip = SoundResources.instance.woodStone;
                            break;
                    }
                    break;
                case MaterialType.STONE:
                    switch (materialType)
                    {
                        case MaterialType.WOOD:
                            audioSource.clip = SoundResources.instance.woodStone;
                            break;
                    }
                    break;
            }
            audioSource.Play();
        }

        // Highlights the Stone to show selection
        public void Highlight(bool show, bool canSelect)
        {
            if (canSelect)
            {
                // Green
                hightlight.GetComponent<Renderer>().material.SetColor("_OutlineColor", new Color(0, 255, 0));
            }
            else
            {
                // Red
                hightlight.GetComponent<Renderer>().material.SetColor("_OutlineColor", new Color(255, 0, 0));
            }
            hightlight.SetActive(show);
        }
    }