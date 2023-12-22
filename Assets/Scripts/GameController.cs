using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;

	private Brick brickBeingHeld;
	private Vector3 previousHitPosition;

	private Vector3 mostRecentClickPoint;
	private Vector3 clickPointToPositiondelta;
	

	private void Awake()
	{
		
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				if (brick != null)
				{
					Debug.Log("Caught a brick");
					brickBeingHeld = brick;
					clickPointToPositiondelta = hit.point - brick.transform.position;
				}
			}
		}

		if (Input.GetMouseButton(0) && brickBeingHeld != null)
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				if (brick == brickBeingHeld)
				{
					Debug.Log("Holding a brick");
					var brickTF = brick.transform;
					var newPosition = hit.point + clickPointToPositiondelta;
					newPosition.y = brickTF.position.y;
					brickTF.position = newPosition;
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			Debug.Log("Brick released");
			brickBeingHeld = null;
		}
	}
}
