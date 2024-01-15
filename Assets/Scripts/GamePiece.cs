using System;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
	public Color color;
	
	private GameController gameController;
	private float raycastDistance;
	private bool wasMoved;
	private bool hasNewPosition;
	private Tuple<int, int> previousSlot;
	
	
	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
		raycastDistance = GetComponent<Collider>().bounds.extents.x * 1.1f;
	}

	public bool WasMoved() => wasMoved;
	public bool HasNewPosition() => hasNewPosition;

	public bool TryToMove(Vector3 movement)
	{
		if (wasMoved == false)
		{
			wasMoved = true;
			hasNewPosition = false;
			previousSlot = gameController.GetBoardSlot(transform.position);
		}
		
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, movement, out hit, raycastDistance))
		{
			transform.position += movement;
			return true;
		}

		GamePiece otherPiece = hit.collider.GetComponent<GamePiece>();
		if (otherPiece != null)
		{
			var couldMove = otherPiece.TryToMove(movement);
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
		var nearestPosition = gameController.GetNearestPosition(transform.position);
		transform.position = nearestPosition;
		var currentSlot = gameController.GetBoardSlot(nearestPosition);
		hasNewPosition = !previousSlot.Equals(currentSlot);
		previousSlot = null;
		wasMoved = false;
	}
}
