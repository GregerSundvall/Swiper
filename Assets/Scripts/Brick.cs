using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
	private GameController gameController;
	public Color color;

	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
	}


	public void RegisterPlayerMovementInput(Vector3 movement)
	{
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, movement, out hit, transform.localScale.x / 2))
		{
			transform.position += movement;
		}
	}
	
    
}
