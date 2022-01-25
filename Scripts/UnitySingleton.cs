using System.Collections;
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T : Component {
	
	private static T _instance;
	public static T instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<T>();
				if (_instance == null) {
                    //GameObject obj = new GameObject();
                    //obj.hideFlags = HideFlags.HideAndDontSave;
                    //_instance = obj.AddComponent<T>();
                    return null;
				}
			}
			
			return _instance;
		}
	}
	
	protected virtual void Awake() {
		DontDestroyOnLoad (this.gameObject);
		
		if (_instance == null) {
			_instance = this as T;
		} else {
			Destroy (gameObject);
		}
	}
}
