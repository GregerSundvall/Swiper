using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
	public Color color;
	
	private GameController gameController;
	private float raycastDistance;
	private bool wasMoved;
	
	
	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
		raycastDistance = GetComponent<Collider>().bounds.extents.x * 1.1f;
	}

	public bool WasMoved() => wasMoved;

	public bool TryToMove(Vector3 movement)
	{
		wasMoved = true;
		
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
		wasMoved = false;
		transform.position = gameController.GetNearestPosition(transform.position);
	}
}
