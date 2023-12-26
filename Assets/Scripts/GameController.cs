using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;
	[SerializeField] private int boardWidth = 3;
	[SerializeField] private int boardHeight = 3;
	[SerializeField] private bool bufferEdges = true;
	[SerializeField] private Brick brickPrefab;
	[SerializeField] private float brickSpacing = 1.0f;
	
    
	private List<List<Vector3>> positions = new();
	private List<Brick> bricks = new();

	private Tuple<int, int> freePosition = new Tuple<int, int>(0, 0);
	private Brick brickBeingHeld;
	
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPointToBrickPositionDelta;

	
	
	private void Awake()
	{
		var height = boardHeight + (bufferEdges ? 2 : 0);
		var width = boardWidth + (bufferEdges ? 2 : 0);

		var halfRow = height * 0.5f;
		var halfColumn = width * 0.5f;
		var halfBrick = brickSpacing * 0.5f;
		
		for (float i = -halfRow; i < halfRow; i++)
		{
			var line = new List<Vector3>();
			for (float j = -halfColumn; j < halfColumn; j++)
			{
				bool isLastPosition = i + brickSpacing >= halfRow && j + brickSpacing >= halfColumn ;
				if (isLastPosition)
				{
					continue;
				}
				
				Vector3 position = new Vector3(i + halfBrick, 0, j + halfBrick);
				line.Add(position);
				bricks.Add(Instantiate(brickPrefab, position, Quaternion.identity));
			}
			positions.Add(line);
		}
	}

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
						var movement = Vector3.zero;
						var sceneProjectedSwipe = hit.point - previousHitPoint;
                        
						if (Mathf.Abs(sceneProjectedSwipe.x) > Mathf.Abs(sceneProjectedSwipe.z))
						{
							movement.x = sceneProjectedSwipe.x;
						}
						else
						{
							movement.z = sceneProjectedSwipe.z;
						}
						
						brick.transform.position += movement;
					}

					brickBeingHeld = brick;
					previousHitPoint = hit.point;
				}
				else
				{
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
	}

	
}
