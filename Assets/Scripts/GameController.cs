using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public struct LevelSettings
{
	public readonly int patternWidth;
	public readonly int patternHeight;
	public readonly bool useBufferEdges;
	public readonly float timeLimit;
	public readonly int maxMoves;

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
	
	[SerializeField] private GameObject gamePiecePrefab;
	[SerializeField] private GameObject boardBarrierPrefab;
	[SerializeField] private GameObject boardBottomPrefab;
	
	[SerializeField] private List<Color> colorPalette = new();

	
	private List<LevelSettings> levelSettings = new();
	
	private List<List<Vector3>> possiblePositions = new();
	private List<List<Color>> targetPattern = new();
	private List<GamePiece> gamePieces = new();

	private GamePiece currentlyHeldGamePiece;
	private Vector3 previousHitPoint = Vector3.zero;
	private Vector3 previousFingerPosition;
	private Vector3 clickPositionToBrickPositionDelta;

	private float pieceSpacing = 1.0f;
	private float pieceColliderExtentY;
	private float movementPerFrameLimit;
	private bool distributeExtraPieceColorsEvenly = true;
	
	private int playerLevel = 1;
	private LevelSettings currentLevelSettings;
	private bool didSetNewRecord;
	private bool puzzleSolved = true;
	private float gameTime;
	private bool timeIsUp;
	private bool noMovesLeft;
	private int movesMade;

	private void Awake()
	{
		gameUI = FindObjectOfType<GameUI>();
		movementPerFrameLimit = pieceSpacing * 0.1f;
	}

	private void Start()
	{
		InitLevelSettings();
		playerLevel = PlayerPrefs.GetInt("level", 1);
		pieceColliderExtentY = gamePiecePrefab.gameObject.GetComponent<Collider>().bounds.extents.y;
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
				GamePiece gamePiece = hit.collider.GetComponent<GamePiece>();
				var isValidHit = hit.normal == Vector3.up && gamePiece != null;
				if (isValidHit)
				{
					currentlyHeldGamePiece = gamePiece;
					previousHitPoint = hit.point;
				}
			}
		}
		
		if (Input.GetMouseButton(0) && currentlyHeldGamePiece != null)
		{
			var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit))
			{
				GamePiece gamePiece = hit.collider.GetComponent<GamePiece>();
				var isValidHit = hit.normal == Vector3.up && gamePiece == currentlyHeldGamePiece;
				
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
						
					currentlyHeldGamePiece.TryToMove(movement);
					
					previousHitPoint = hit.point;
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			var movedPiecesThisSwipe = 0;
			
			foreach (var b in gamePieces)
			{
				if (b.WasMoved())
				{
					movedPiecesThisSwipe++;
					b.SnapToNearestPosition();
				}
			}

			movesMade += movedPiecesThisSwipe;
			noMovesLeft = currentLevelSettings.maxMoves - movesMade < 0;
			gameUI.UpdateMovesLeftText(Mathf.Max(0, currentLevelSettings.maxMoves - movesMade));
			
			currentlyHeldGamePiece = null;
			previousHitPoint = Vector3.zero;

			CheckWinCondition();
		}
	}

	public int GetPlayerLevel() => playerLevel;

	public float GetTimeLeft() => Mathf.Max(0, currentLevelSettings.timeLimit - gameTime);
	
	private void InitLevelSettings()
	{
		
		levelSettings.Add(new LevelSettings(2, 2, true, 120, 50));
		levelSettings.Add(new LevelSettings(3, 3, true, 180, 150));
		levelSettings.Add(new LevelSettings(4, 4, true, 300, 400));
		levelSettings.Add(new LevelSettings(3, 3, false, 300, 200));
		levelSettings.Add(new LevelSettings(4, 4, false, 300, 200));
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
		float maxRayDistance = 0.1f;
		var rayOriginOffset = new Vector3(0, maxRayDistance, 0);
		var rayDirection = Vector3.down;
				
		for (int i = 0; i < targetPattern.Count; i++)
		{
			for (int j = 0; j < targetPattern[i].Count; j++)
			{
				RaycastHit hit;
				var piecePosition = possiblePositions[i + bufferEdge][j + bufferEdge];
				var rayOrigin = piecePosition + rayOriginOffset;
				if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
				{
					GamePiece gamePiece = hit.collider.GetComponent<GamePiece>();
					var isValidHit = gamePiece != null;
					if (isValidHit)
					{
						var brickColor = gamePiece.color;
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

			var isOnHighestUnlockedLevel = playerLevel == PlayerPrefs.GetInt("level", 1);
			var shouldLevelUp = !timeIsUp && !noMovesLeft && isOnHighestUnlockedLevel;
			if (shouldLevelUp)
			{
				PlayerPrefs.SetInt("level", playerLevel + 1);
			}
			
			gameUI.OnPuzzleSolved();
		}
	}
	
	public float GetGameTime() => gameTime;
	
	public List<List<Color>> GetTargetPattern() => targetPattern;

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
		if (gamePieces.Count > 0)
		{
			foreach (var piece in gamePieces)
			{
				Destroy(piece.gameObject);
			}
			gamePieces.Clear();
		}
		puzzleSolved = false;
		gameTime = 0;
		didSetNewRecord = false;
		possiblePositions.Clear();
		targetPattern.Clear();
		
		
		// Set game level. The second -1 below is because starting level is 1.
		currentLevelSettings = levelSettings[Mathf.Min(playerLevel, levelSettings.Count - 1) - 1]; 

		// Create board
		var barrierWidth = 0.3f;
		var barrierThickness = 0.1f;
		var playAreaWidth = (currentLevelSettings.patternWidth + (currentLevelSettings.useBufferEdges ? 2 : 0)) * pieceSpacing;
		var playAreaHeight = (currentLevelSettings.patternHeight + (currentLevelSettings.useBufferEdges ? 2 : 0)) * pieceSpacing;
		var boardWidth = playAreaWidth + barrierWidth * 2;
		var boardHeight = playAreaHeight + barrierWidth * 2;
		
		
		var horizontalBarrierSize = new Vector3(boardWidth, barrierThickness, barrierWidth);
		var verticalBarrierSize = new Vector3(barrierWidth, barrierThickness, boardHeight);
		var topBarrierPosition = new Vector3(0, 0, (boardHeight - barrierWidth) * 0.5f);
		var rightBarrierPosition = new Vector3((boardWidth - barrierWidth) * 0.5f, 0, 0);

		var bottom = Instantiate(boardBottomPrefab);
		bottom.transform.localScale = new Vector3(boardWidth, 0.1f, boardHeight);
		
		var barrierLeft = Instantiate(boardBarrierPrefab, -rightBarrierPosition, Quaternion.identity);
		var barrierRight = Instantiate(boardBarrierPrefab, rightBarrierPosition, Quaternion.identity);
		var barrierFar = Instantiate(boardBarrierPrefab, topBarrierPosition, Quaternion.identity);
		var barrierNear = Instantiate(boardBarrierPrefab, -topBarrierPosition, Quaternion.identity);

		barrierLeft.transform.localScale = verticalBarrierSize;
		barrierRight.transform.localScale = verticalBarrierSize;
		barrierFar.transform.localScale = horizontalBarrierSize;
		barrierNear.transform.localScale = horizontalBarrierSize;
		
		
		// Create a target pattern. 
		var pieceColors = new List<Color>();
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
				pieceColors.Add(color);
			}
			targetPattern.Add(row);
		}
		
		// Depending on level settings, fill up pieceColors list with additional "color instances".
		if (currentLevelSettings.useBufferEdges)
		{
			var additionalBrickCount = boardHeight * boardWidth - pieceColors.Count - 1; // -1 because of the free slot needed to move anything
			if (distributeExtraPieceColorsEvenly)
			{
				for (int i = 0; i < additionalBrickCount; i++)
				{
					pieceColors.Add(colorPalette[i % (colorPalette.Count - 1)]);
				}
			}
			else
			{
				for (int i = 0; i < additionalBrickCount; i++)
				{
					pieceColors.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
				}
			}
		}
		

		// Spawn game pieces and also populate list of possible positions. Color is randomized from brickColors list.
		var halfPlayAreaWidth = playAreaWidth * 0.5f;
		var halfPlayAreaHeight = playAreaHeight * 0.5f;
		var halfPiece = pieceSpacing * 0.5f;
		var bufferEdges = currentLevelSettings.useBufferEdges ? 2 : 0;

		for (int z = 0; z < currentLevelSettings.patternHeight + bufferEdges; z++)
		{
			var row = new List<Vector3>();
			
			for (int x = 0; x < currentLevelSettings.patternWidth + bufferEdges; x++)
			{
				var xx = x * pieceSpacing - halfPlayAreaWidth + halfPiece;
				var zz = -z * pieceSpacing + halfPlayAreaHeight - halfPiece;
				var position = new Vector3(xx, 0, zz);
				row.Add(position);
				
				bool notLastPosition = (z < currentLevelSettings.patternHeight + bufferEdges -1 || x < currentLevelSettings.patternWidth + bufferEdges -1);
				if (notLastPosition)
				{
					var piece = Instantiate(gamePiecePrefab, position, Quaternion.identity).GetComponent<GamePiece>();
					var colorIndex = Random.Range(0, pieceColors.Count);
					var color = pieceColors[colorIndex];
					piece.color = color;
					piece.GetComponentInChildren<MeshRenderer>().material.color = color;
					pieceColors.RemoveAt(colorIndex);
					gamePieces.Add(piece);
				}
			}
			
			possiblePositions.Add(row);
		}
		
		
		// for (float i = halfPlayAreaHeight; i > -halfPlayAreaHeight; i--)
		// {
		// 	var row = new List<Vector3>();
		// 	for (float j = -halfPlayAreaWidth; j < halfPlayAreaWidth; j++)
		// 	{
		// 		Vector3 position = new Vector3(j + halfPiece, 0, i - halfPiece);
		// 		Debug.Log(position.x + " " + position.z);
		// 		row.Add(position);
		// 		bool isLastPosition = (i + pieceSpacing >= halfPlayAreaHeight) && (j + pieceSpacing >= halfPlayAreaWidth);
		// 		if (!isLastPosition)
		// 		{
		// 			var piece = Instantiate(gamePiecePrefab, position, Quaternion.identity).GetComponent<GamePiece>();
		// 			var colorIndex = Random.Range(0, pieceColors.Count);
		// 			var color = pieceColors[colorIndex];
		// 			piece.color = color;
		// 			piece.GetComponentInChildren<MeshRenderer>().material.color = color;
		// 			pieceColors.RemoveAt(colorIndex);
		// 			gamePieces.Add(piece);
		// 		}
		// 	}
		//
		// 	possiblePositions.Add(row);
		// }
	}

	public void StartNewGame()
	{
		InitGame();
	}
}
