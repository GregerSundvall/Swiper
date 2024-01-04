using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct LevelSettings
{
	public readonly int patternWidth;
	public readonly int patternHeight;
	public readonly bool useBufferEdges;
	public readonly float timeLimit;
	public int maxMoves;

	public LevelSettings(int patternWidth, int patternHeight, bool useBufferEdges, float timeLimit, int maxMoves) : this()
	{
		this.patternWidth = patternWidth;
		this.patternHeight = patternHeight;
		this.useBufferEdges = useBufferEdges;
		this.timeLimit = timeLimit;
		this.maxMoves = maxMoves;
	}
}


public class GameController : MonoBehaviour
{
	[SerializeField] private Camera gameCamera;
	[SerializeField] private GameUI gameUI;
	[SerializeField] private GameObject boardGO;
	[SerializeField] private GameObject barrierLeft;
	[SerializeField] private GameObject barrierRight;
	[SerializeField] private GameObject barrierForward;
	[SerializeField] private GameObject barrierBack;
	[SerializeField] private Brick brickPrefab;
	[SerializeField] private List<Color> colorPalette = new();

	
	private List<LevelSettings> levelSettings = new();
	
	private List<List<Vector3>> possiblePositions = new();
	private List<List<Color>> targetPattern = new();
	private List<Brick> bricks = new();

	private Brick currentlyHeldBrick;
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPositionToBrickPositionDelta;

	private float brickSpacing = 1.0f;
	private float brickColliderExtentY;
	private float movementPerFrameLimit;

	private int playerLevel = 1;
	private LevelSettings currentLevelSettings;
	private bool didSetNewRecord;
	private bool puzzleSolved = true;
	private float gameTime;
	private bool timeIsUp;
	private int movesMade;

	private void Awake()
	{
		gameUI = FindObjectOfType<GameUI>();
		movementPerFrameLimit = brickSpacing * 0.1f;
		boardGO.SetActive(false);
	}

	private void Start()
	{
		InitLevelSettings();
		playerLevel = PlayerPrefs.GetInt("level", 1);
		currentLevelSettings = levelSettings[Mathf.Min(playerLevel, levelSettings.Count - 1)-1];
	}

	private void Update()
	{
		if (puzzleSolved)
		{
			return;
		}

		gameTime += Time.deltaTime;
		if (gameTime > currentLevelSettings.timeLimit)
		{
			timeIsUp = true;
		}
		
		if (Input.GetMouseButtonDown(0))
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				Brick brick = hit.collider.GetComponent<Brick>();
				var isValidHit = hit.normal == Vector3.up && brick != null;
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
			var movedPiecesCount = 0;
			
			foreach (var b in bricks)
			{
				if (b.WasMoved())
				{
					movedPiecesCount++;
					b.SnapToNearestPosition();
				}
			}

			movesMade += movedPiecesCount;
			gameUI.UpdateMovesLeftText(Mathf.Max(0, currentLevelSettings.maxMoves - movedPiecesCount));
			
			currentlyHeldBrick = null;
			previousHitPoint = Vector3.zero;

			CheckWinCondition();
		}
	}

	public int GetPlayerLevel() => playerLevel;

	public float GetTimeLeft() => Mathf.Max(0, currentLevelSettings.timeLimit - gameTime);
	
	private void InitLevelSettings()
	{
		
		levelSettings.Add(new LevelSettings(3, 3, true, 300, 300));
		levelSettings.Add(new LevelSettings(4, 4, true, 300, 300));
		levelSettings.Add(new LevelSettings(5, 5, true, 300, 300));
		levelSettings.Add(new LevelSettings(3, 3, false, 300, 300));
		levelSettings.Add(new LevelSettings(4, 4, false, 300, 300));
	}

	private void SetPlayerPrefsBestTime()
	{
		string key = "bestTime" + playerLevel;
		float storedValue = PlayerPrefs.GetFloat(key, Single.MaxValue);
			
		if (gameTime < storedValue)
		{
			PlayerPrefs.SetFloat(key, gameTime);
			didSetNewRecord = true;
		}

		PlayerPrefs.Save();
	}

	private void CheckWinCondition()
	{
		var solved = true;
		int bufferEdge = currentLevelSettings.useBufferEdges ? 1 : 0;
		float maxRayDistance = brickColliderExtentY;
		var rayOriginOffset = new Vector3(0, maxRayDistance * 1.1f, 0);
		var rayDirection = Vector3.down;
				
		for (int i = 0; i < targetPattern.Count; i++)
		{
			for (int j = 0; j < targetPattern[i].Count; j++)
			{
				RaycastHit hit;
				var brickPosition = possiblePositions[i + bufferEdge][j + bufferEdge];
				var rayOrigin = brickPosition + rayOriginOffset;
						
				if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
				{
					Brick brick = hit.collider.GetComponent<Brick>();
					var isValidHit = brick != null;
					if (isValidHit)
					{
						var brickColor = brick.color;
						var solutionColor = targetPattern[i][j];
						if (brickColor != solutionColor)
						{
							solved = false;
							break;
						}
					}
					else
					{
						solved = false;
						break;
					}
				}
				else
				{
					solved = false;
					break;
				}
			}
					
			if (!solved)
			{
				break;
			}
		}

		if (solved)
		{
			puzzleSolved = true;
			SetPlayerPrefsBestTime();

			var shouldLevelUp = !timeIsUp && movesMade <= currentLevelSettings.maxMoves && 
			                    playerLevel == PlayerPrefs.GetInt("level", 1);
			if (shouldLevelUp)
			{
				PlayerPrefs.SetInt("level", playerLevel + 1);
			}
			
			gameUI.OnPuzzleSolved();
		}
	}
	
	public float GetGameTime() => gameTime;
	
	public List<List<Color>> GetTargetPattern()
	{
		return targetPattern;
		// var pattern = new List<Color>();
		//
		// for (int i = 0; i < targetPattern.Count; i++)
		// {
		// 	for (int j = 0; j < targetPattern[0].Count; j++)
		// 	{
		// 		pattern.Add(targetPattern[i][j]);
		// 	}
		// }
		//
		// return pattern;
	}

	public bool GetDidSetNewRecord() => didSetNewRecord;

	public Vector3 GetNearestPosition(Vector3 currentPosition)
	{
		Vector3 closestPosition = new();
		float smallestDelta = Single.PositiveInfinity;
		foreach (var row in possiblePositions)
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

	private void InitGame()
	{
		// Destroy and reset stuff from previous game.
		if (bricks.Count > 0)
		{
			foreach (var brick in bricks)
			{
				Destroy(brick.gameObject);
			}
			bricks.Clear();
		}

		didSetNewRecord = false;
		possiblePositions.Clear();
		targetPattern.Clear();
		boardGO.SetActive(true);

		// Create a random solution. 
		var brickColors = new List<Color>();
		for (int i = 0; i < currentLevelSettings.patternHeight; i++)
		{
			var row = new List<Color>();
			for (int j = 0; j < currentLevelSettings.patternWidth; j++)
			{
				if (!currentLevelSettings.useBufferEdges)
				{
					var isLastPosition = (i == currentLevelSettings.patternHeight - 1) && (j == currentLevelSettings.patternWidth - 1);
					if (isLastPosition)
					{
						continue;
					}
				}
				var color = colorPalette[Random.Range(0, colorPalette.Count)];
				row.Add(color);
				brickColors.Add(color);
			}
			targetPattern.Add(row);
		}
		
		
		// Nice to have variables
		var boardHeight = currentLevelSettings.patternHeight + (currentLevelSettings.useBufferEdges ? 2 : 0);
		var boardWidth = currentLevelSettings.patternWidth + (currentLevelSettings.useBufferEdges ? 2 : 0);
		var halfHeight = boardHeight * 0.5f;
		var halfWidth = boardWidth * 0.5f;
		var halfBrick = brickSpacing * 0.5f;
		
		// Fill up brickColors list with additional random "color instances".
		// Evenly distributed colors
		var additionalBrickCount = boardHeight * boardWidth - brickColors.Count - 1; // -1 because of the free slot needed to move anything
		for (int i = 0; i < additionalBrickCount; i++)
		{
			brickColors.Add(colorPalette[i % (colorPalette.Count - 1)]);
		}
		// // Random colors
		// while (brickColors.Count < boardHeight * boardWidth -1) // -1 because of the free slot
		// {
		// 	brickColors.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
		// }

		// Spawn bricks. Color is randomized from brickColors list.
		for (float i = -halfHeight; i < halfHeight; i++)
		{
			var row = new List<Vector3>();
			for (float j = -halfWidth; j < halfWidth; j++)
			{
				Vector3 position = new Vector3(i + halfBrick, 0, j + halfBrick);
				row.Add(position);

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

			possiblePositions.Add(row);
		}
		
		// Set barrier size and positions.
		barrierLeft.transform.position = new Vector3(-halfWidth - halfBrick, 0, 0);
		barrierRight.transform.position = new Vector3(halfWidth + halfBrick, 0, 0);
		barrierBack.transform.position = new Vector3(0, 0, -halfHeight - halfBrick);
		barrierForward.transform.position = new Vector3(0, 0, halfHeight + halfBrick);
        
		brickColliderExtentY = bricks[0].GetComponent<Collider>().bounds.extents.y;

		puzzleSolved = false;
		gameTime = 0;
	}

	public void StartNewGame()
	{
		InitGame();
	}
}
