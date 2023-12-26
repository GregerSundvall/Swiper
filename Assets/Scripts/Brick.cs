using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
	public bool isBeingMovedByPlayer;
	public Vector3 movement;
	
    public void Push()
    {
	    
    }
	
	private void OnCollisionEnter(Collision other)
	{
		GameObject otherGO = other.gameObject;
		Brick otherBrick = otherGO.GetComponent<Brick>();
		if (otherBrick != null)
		{
			otherBrick.Push();
		}
		
	}
}
