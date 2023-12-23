using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;

	private bool isHoldingBrick = false;
	private Brick brickBeingHeld;
	private Vector3 previousHitPoint = Vector3.zero;

	private Vector3 previousFingerPosition;
	private Vector3 clickPointToPositionDelta;
	

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				var hitIsOnTopSide = hit.normal == Vector3.up;

				if (brick != null && hitIsOnTopSide)
				{
					var isFirstFrameOfSwipe = previousHitPoint == Vector3.zero;
					if (!isFirstFrameOfSwipe)
					{
						brick.transform.position += GetMovement(hit.point);
					}

					brickBeingHeld = brick;
					previousHitPoint = hit.point;
				}
				else if (previousHitPoint != Vector3.zero)
				{
					// Player "lost grip" of the brick. We'll give it a nudge, so it doesn't stop totally.
					var movement = GetMovement(hit.point);
					var brickRB = brickBeingHeld.GetComponent<Rigidbody>();
					brickRB.AddForce(movement * (600 * Time.deltaTime), ForceMode.Impulse);
				
					previousHitPoint = Vector3.zero;
					brickBeingHeld = null;
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			previousHitPoint = Vector3.zero;
			brickBeingHeld = null;
		}

		Vector3 GetMovement(Vector3 hitPoint)
		{
			var movement = Vector3.zero;
			var sceneProjectedSwipe = hitPoint - previousHitPoint;
            
			if (Mathf.Abs(sceneProjectedSwipe.x) > Mathf.Abs(sceneProjectedSwipe.z))
			{
				movement.x = sceneProjectedSwipe.x;
			}
			else
			{
				movement.z = sceneProjectedSwipe.z;
			}

			return movement;
		}
	}
}
