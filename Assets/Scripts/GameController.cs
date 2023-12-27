using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;
	[SerializeField] private int solutionWidth = 3;
	[SerializeField] private int solutionHeight = 3;
	[SerializeField] private bool bufferEdges = true;
	[SerializeField] private List<Color> colors = new();
    
	[SerializeField] private Brick brickPrefab;
	[SerializeField] public float brickSpacing = 1.0f;

	[SerializeField] private GameObject barrierLeft;
	[SerializeField] private GameObject barrierRight;
	[SerializeField] private GameObject barrierForward;
	[SerializeField] private GameObject barrierBack;
    
	
    
	private List<List<Vector3>> positions = new();
	private List<List<Color>> solution = new();
	private List<Brick> bricks = new();

	private Vector3 freePosition;
	private Brick currentlyHeldBrick;
	private Brick previouslyHeldBrick;
	
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPointToBrickPositionDelta;

	
	
	private void Awake()
	{
		SetupGame();
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

	private void SetupGame()
	{
		var brickColors = new List<Color>();
		
		// Populate solution
		for (int i = 0; i < solutionHeight; i++)
		{
			var row = new List<Color>();
			for (int j = 0; j < solutionWidth; j++)
			{
				var color = colors[Random.Range(0, colors.Count)];
				row.Add(color);
				brickColors.Add(color);
			}
			solution.Add(row);
		}
		
		// Set solution UI colors
		FindObjectOfType<GameUI>().ShowSolution(brickColors);
        
		// Nice to have variables
		var boardHeight = solutionHeight + (bufferEdges ? 2 : 0);
		var boardWidth = solutionWidth + (bufferEdges ? 2 : 0);
		var halfHeight = boardHeight * 0.5f;
		var halfWidth = boardWidth * 0.5f;
		var halfBrick = brickSpacing * 0.5f;
		
		// Top up brickcolors list with additional random color instances.
		while (brickColors.Count < boardHeight * boardWidth -1) // -1 because of the free slot
		{
			brickColors.Add(colors[Random.Range(0, colors.Count)]);
		}

		// Spawn bricks. Color is randomized from brickColors list.
		for (float i = -halfHeight; i < halfHeight; i++)
		{
			var line = new List<Vector3>();
			for (float j = -halfWidth; j < halfWidth; j++)
			{
				Vector3 position = new Vector3(i + halfBrick, 0, j + halfBrick);
				line.Add(position);

				bool isLastPosition = (i + brickSpacing >= halfHeight) && (j + brickSpacing >= halfWidth);
				if (!isLastPosition)
				{
					var brick = Instantiate(brickPrefab, position, Quaternion.identity);
					var colorIndex = Random.Range(0, brickColors.Count);
					var color = brickColors[colorIndex];
					brick.GetComponentInChildren<MeshRenderer>().material.color = color;
					brickColors.RemoveAt(colorIndex);
					bricks.Add(brick);
				}
				else
				{
					freePosition = position;
				}
			}

			positions.Add(line);
		}
		
		// Place barriers
		barrierLeft.transform.position = new Vector3(-halfWidth - halfBrick, 0, 0);
		barrierRight.transform.position = new Vector3(halfWidth + halfBrick, 0, 0);
		barrierBack.transform.position = new Vector3(0, 0, -halfHeight - halfBrick);
		barrierForward.transform.position = new Vector3(0, 0, halfHeight + halfBrick);
	}
}
