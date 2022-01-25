using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pit : MonoBehaviour {

	[Tooltip("The Pit Number")]
	public int number;
	[Tooltip("Is it a house object?")]
	public bool house;
	[Tooltip("Is Player1 the owner of this pit?")]
	public bool p1Owner;
	[Tooltip("The position of the text that shows the seed count")]
	public Text hudText;
	public GameObject pitSelect;
	
	private int mStones;
	public int stones{
		get{return mStones;}
		set{
			mStones = value;
			if(hudText != null) {
				hudText.text = mStones.ToString();
			}
		}
		
	}			 
}