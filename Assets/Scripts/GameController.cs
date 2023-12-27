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
	[SerializeField] public float brickSpacing = 1.0f;

	[SerializeField] private GameObject barrierLeft;
	[SerializeField] private GameObject barrierRight;
	[SerializeField] private GameObject barrierForward;
	[SerializeField] private GameObject barrierBack;
	
	
    
	private List<List<Vector3>> positions = new();
	private List<Brick> bricks = new();

	private Vector3 freePosition;
	private Brick currentlyHeldBrick;
	private Brick previouslyHeldBrick;
	
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPointToBrickPositionDelta;

	
	
	private void Awake()
	{
		SetupBoardAndBricks();
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
						
						brick.RegisterPlayerMovementInput(movement);
					}

					if (currentlyHeldBrick != brick)
					{
						previouslyHeldBrick = currentlyHeldBrick;
					}
					currentlyHeldBrick = brick;
					previousHitPoint = hit.point;
				}
				else
				{
					previousHitPoint = Vector3.zero;
					previouslyHeldBrick = currentlyHeldBrick;
					currentlyHeldBrick = null;
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			previousHitPoint = Vector3.zero;
			previouslyHeldBrick = currentlyHeldBrick;
			currentlyHeldBrick = null;
		}

		if (previouslyHeldBrick != null)
		{
			Vector3 closestPosition = new();
			float smallestDelta = Single.PositiveInfinity;
			foreach (var row in positions)
			{
				foreach (var position in row)
				{
					var delta = Mathf.Abs((previouslyHeldBrick.transform.position - position).magnitude);
					if (delta < smallestDelta)
					{
						smallestDelta = delta;
						closestPosition = position;
					}
				}
			}

			previouslyHeldBrick.transform.position = closestPosition;
			previouslyHeldBrick = null;
		}
	}

	private void SetupBoardAndBricks()
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
				Vector3 position = new Vector3(i + halfBrick, 0, j + halfBrick);
				line.Add(position);

				bool isLastPosition = (i + brickSpacing >= halfRow) && (j + brickSpacing >= halfColumn);
				if (!isLastPosition)
				{
					var brick = Instantiate(brickPrefab, position, Quaternion.identity);
					bricks.Add(brick);
				}
				else
				{
					freePosition = position;
				}
			}

			positions.Add(line);
		}

		barrierLeft.transform.position = new Vector3(-halfColumn - halfBrick, 0, 0);
		barrierRight.transform.position = new Vector3(halfColumn + halfBrick, 0, 0);
		barrierBack.transform.position = new Vector3(0, 0, -halfRow - halfBrick);
		barrierForward.transform.position = new Vector3(0, 0, halfRow + halfBrick);
		
	}
}
