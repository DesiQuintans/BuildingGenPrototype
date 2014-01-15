using UnityEngine;
using System.Collections;

public class LockCursor : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKey)
		{
			if (Input.GetKeyDown (KeyCode.End))
			{
				Screen.lockCursor = !Screen.lockCursor;
			}
		}
	}
	
}
