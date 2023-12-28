using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
	private GameController gameController;
	public Color color;
	float raycastDistance;
	
	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
		//raycastDistance = transform.localScale.x / 2;
		raycastDistance = GetComponent<Collider>().bounds.extents.x * 1.1f;
	}


	public bool TryToMove(Vector3 movement)
	{
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, movement, out hit, raycastDistance))
		{
			transform.position += movement;
			return true;
		}
		
		Brick otherBrick = hit.collider.GetComponent<Brick>();
		if (otherBrick != null)
		{
			var couldMove = otherBrick.TryToMove(movement);
			if (couldMove)
			{
				transform.position += movement;
				return true;
			}
		}

		return false;
	}
	
	public void SnapToNearestPosition()
	{
		transform.position = gameController.GetNearestPosition(transform.position);
	}
}
