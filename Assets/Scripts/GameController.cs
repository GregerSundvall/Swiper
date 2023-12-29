using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;
	[SerializeField] private GameUI gameUI;
	[SerializeField] private GameObject boardGO;
    
	[SerializeField] private int solutionWidth = 3;
	[SerializeField] private int solutionHeight = 3;
	[SerializeField] public float brickSpacing = 1.0f;
	[SerializeField] private bool bufferEdges = true;
    
	[SerializeField] private GameObject barrierLeft;
	[SerializeField] private GameObject barrierRight;
	[SerializeField] private GameObject barrierForward;
	[SerializeField] private GameObject barrierBack;
    
	[SerializeField] private List<Color> colors = new();
	[SerializeField] private Brick brickPrefab;


	private List<List<Vector3>> positions = new();
	private List<List<Color>> solution = new();
	private List<Brick> bricks = new();

	private Brick currentlyHeldBrick;
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPositionToBrickPositionDelta;

	private float brickColliderExtentY;
	private float movementPerFrameLimit;
    
	private bool puzzleSolved = true;
	private float gameTime;
	
	
	
	private void Awake()
	{
		gameUI = FindObjectOfType<GameUI>();
		movementPerFrameLimit = brickSpacing * 0.1f;
		boardGO.SetActive(false);
	}

	private void Update()
	{
		if (puzzleSolved)
		{
			return;
		}

		gameTime += Time.deltaTime;
		
		if (Input.GetMouseButtonDown(0))
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				var isValidHit = hit.normal == Vector3.up && 
				                 brick != null;
				if (isValidHit)
				{
					currentlyHeldBrick = brick;
					previousHitPoint = hit.point;
				}
			}
		}
		
		if (Input.GetMouseButton(0) && currentlyHeldBrick != null)
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				var isValidHit = hit.normal == Vector3.up && brick == currentlyHeldBrick;
				
				if (isValidHit)
				{
					var sceneProjectedSwipe = hit.point - previousHitPoint;
					var movement = Vector3.zero;
                        
					if (Mathf.Abs(sceneProjectedSwipe.x) > Mathf.Abs(sceneProjectedSwipe.z))
					{
						movement.x = Mathf.Min(sceneProjectedSwipe.x, movementPerFrameLimit);
					}
					else
					{
						movement.z = Mathf.Min(sceneProjectedSwipe.z, movementPerFrameLimit);
					}
						
					currentlyHeldBrick.TryToMove(movement);
					
                    previousHitPoint = hit.point;
				}
            }
		}
	
		if (Input.GetMouseButtonUp(0))
		{
			foreach (var b in bricks)
			{
				b.SnapToNearestPosition();
			}

			currentlyHeldBrick = null;
			previousHitPoint = Vector3.zero;

			CheckWinCondition();
		}
	}
	
	private void CheckWinCondition()
	{
		var win = true;
		int bufferEdge = bufferEdges ? 1 : 0;
		float maxRayDistance = brickColliderExtentY;
		var rayOriginOffset = new Vector3(0, maxRayDistance * 1.1f, 0);
		var rayDirection = Vector3.down;
				
		for (int i = 0; i < solution.Count; i++)
		{
			for (int j = 0; j < solution[i].Count; j++)
			{
				RaycastHit hit;
				var brickPosition = positions[i + bufferEdge][j + bufferEdge];
				var rayOrigin = brickPosition + rayOriginOffset;
						
				if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
				{
					Brick brick = hit.collider.GetComponent<Brick>();
					var isValidHit = brick != null;
					if (isValidHit)
					{
						var brickColor = brick.color;
						var solutionColor = solution[i][j];
						if (brickColor != solutionColor)
						{
							win = false;
							break;
						}
					}
					else
					{
						win = false;
						break;
					}
				}
				else
				{
					win = false;
					break;
				}
			}
					
			if (!win)
			{
				break;
			}
		}

		if (win)
		{
			puzzleSolved = true;
			var newRecord = SetPlayerPrefs();
			gameUI.OnPuzzleSolved(newRecord);
		}
	}
	
	public float GetGameTime()
	{
		return gameTime;
	}
	
	private bool SetPlayerPrefs()
	{
		bool newRecord = false;
		
		if (!PlayerPrefs.HasKey("bestTime"))
		{
			PlayerPrefs.SetFloat("bestTime", gameTime);
		}
			
		if (gameTime < PlayerPrefs.GetFloat("bestTime"))
		{
			PlayerPrefs.SetFloat("bestTime", gameTime);
			newRecord = true;
		}
		
		PlayerPrefs.Save();

		return newRecord;
	}

	public Vector3 GetNearestPosition(Vector3 currentPosition)
	{
		Vector3 closestPosition = new();
		float smallestDelta = Single.PositiveInfinity;
		foreach (var row in positions)
		{
			foreach (var position in row)
			{
				var delta = Mathf.Abs((currentPosition - position).magnitude);
				if (delta < smallestDelta)
				{
					smallestDelta = delta;
					closestPosition = position;
				}
			}
		}

		return closestPosition;
	}

	private void SetupGame()
	{
		if (bricks.Count > 0)
		{
			foreach (var brick in bricks)
			{
				Destroy(brick.gameObject);
			}
		}
		
		bricks.Clear();
		positions.Clear();
		solution.Clear();
		var brickColors = new List<Color>();
		
		boardGO.SetActive(true);
		
		
		// Create a random solution. 
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
		
		// Fill up brickColors list with additional random "color instances".
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
					brick.color = color;
					brick.GetComponentInChildren<MeshRenderer>().material.color = color;
					brickColors.RemoveAt(colorIndex);
					bricks.Add(brick);
				}
			}

			positions.Add(line);
		}
		
		// Place barriers according to board size.
		barrierLeft.transform.position = new Vector3(-halfWidth - halfBrick, 0, 0);
		barrierRight.transform.position = new Vector3(halfWidth + halfBrick, 0, 0);
		barrierBack.transform.position = new Vector3(0, 0, -halfHeight - halfBrick);
		barrierForward.transform.position = new Vector3(0, 0, halfHeight + halfBrick);
        
		brickColliderExtentY = bricks[0].GetComponent<Collider>().bounds.extents.y;

		puzzleSolved = false;
		gameTime = 0;
	}

	public void OnNewGamePressed()
	{
		SetupGame();
	}
}
