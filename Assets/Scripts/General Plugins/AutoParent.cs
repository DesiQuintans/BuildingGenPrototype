using UnityEngine;
using System.Collections;

public class AutoParent : MonoBehaviour
{
	// Inspector-accessible variables.
	public string uniqueParentName;

	void Awake ()
	{
		transform.parent = GameObject.Find (uniqueParentName).transform;
	}
}
