using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum UiState
{
	Main,
	Playing,
	Pause,
	Solved,
	OutOfTime,
	OutOfMoves
}


public class GameUI : MonoBehaviour
{
	//Main menu
	[SerializeField] private GameObject mainMenuUiGroup;
	[SerializeField] private GameObject playButton;
	[SerializeField] private TMP_Text mainMenuLevelText;
	[SerializeField] private GameObject resetButton;
	
	// In-game
	[SerializeField] private GameObject inGameUiGroup;
	[SerializeField] private GameObject menuButton;
	[SerializeField] private TMP_Text timeLeftText;
	[SerializeField] private TMP_Text movesLeftText;
	// Pause menu
	[SerializeField] private GameObject pauseMenuUiGroup;
	[SerializeField] private GameObject backToGameButton;
	[SerializeField] private GameObject newGameButton;
	[SerializeField] private GameObject goToMainMenuButton;
	
	// Finish screen
	[SerializeField] private GameObject finishMenuUiGroup;
	[SerializeField] private TMP_Text finishUpperText;
	[SerializeField] private TMP_Text finishMiddleText;
	[SerializeField] private GameObject newBestTime;
	[SerializeField] private TMP_Text finishLowerText;
	
	// Init stuff
	[SerializeField] private GameObject targetPatternPiecePrefab;
	[SerializeField] private GameObject targetPatternRowPrefab;
	[SerializeField] private GameObject targetPatternParent;
	
	
	private UiState uiState;
	private GameController gameController;

	private float timeLeftUpdateTimer = 0;
	private float timeLeftUpdateDelay = 0.2f;

	private Color textColor;
	

	private void Awake()
	{
		menuButton.GetComponent<Button>().onClick.AddListener(OnMenuButtonPressed);
		playButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		newGameButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		finishLowerText.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		backToGameButton.GetComponent<Button>().onClick.AddListener(OnBackToGameButtonPressed);
		goToMainMenuButton.GetComponent<Button>().onClick.AddListener(OnGoToMainMenuButtonPressed);
		resetButton.GetComponent<Button>().onClick.AddListener(OnResetProgressButtonPressed);
	}

	private void Start()
	{
		gameController = FindObjectOfType<GameController>();
		mainMenuLevelText.text = "Level " + gameController.GetPlayerLevel();
		textColor = timeLeftText.color;
		SetState(UiState.Main);
	}

	private void Update()
	{
		if (uiState == UiState.Playing || uiState == UiState.Pause)
		{
			bool timeToUpdate = timeLeftUpdateTimer > timeLeftUpdateDelay;
			Action uiTimerUpdate = timeToUpdate ? UpdateUiTimer : () => { timeLeftUpdateTimer += Time.deltaTime; };
			uiTimerUpdate.Invoke();
		}
	}

	public void OnPuzzleSolved() => SetState(UiState.Solved);

	public void OnOutOfTime() => SetState(UiState.OutOfTime);
	
	public void OnOutOfMoves() => SetState(UiState.OutOfMoves);
	
	private void UpdateUiTimer()
	{
		float timeLeft = gameController.GetTimeLeft();
		if (timeLeft > 30)
		{
			timeLeftText.text = TimeFloatToString(timeLeft);
		}
		else if (timeLeft > 0)
		{
			timeLeftText.text = TimeFloatToString(timeLeft);
			timeLeftText.color = Color.red;
		}
	}

	public void SetMovesLeftText(int moves)
	{
		if (moves > 20)
		{
			movesLeftText.text = "Moves left: " + moves;
		}
		else if (moves > 0)
		{
			movesLeftText.text = "Moves left: " + moves;
			movesLeftText.color = Color.red;
		}
	}

	private void SetState(UiState state)
	{
		switch (state)
		{
			case UiState.Main:
			{
				Debug.Log("Set ui state main");
				mainMenuUiGroup.SetActive(true);
				inGameUiGroup.SetActive(false);
				pauseMenuUiGroup.SetActive(false);
				finishMenuUiGroup.SetActive(false);
				menuButton.SetActive(false);
				
				mainMenuLevelText.text = "Level " + gameController.GetPlayerLevel();
				break;
			}
			case UiState.Playing:
			{
				mainMenuUiGroup.SetActive(false);
				inGameUiGroup.SetActive(true);
				pauseMenuUiGroup.SetActive(false);
				finishMenuUiGroup.SetActive(false);
				menuButton.SetActive(true);
				break;
			}
			case UiState.Pause:
			{
				mainMenuUiGroup.SetActive(false);
				inGameUiGroup.SetActive(true);
				pauseMenuUiGroup.SetActive(true);
				finishMenuUiGroup.SetActive(false);
				menuButton.SetActive(true);
				break;
			}
			case UiState.Solved:
			{
				mainMenuUiGroup.SetActive(false);
				inGameUiGroup.SetActive(false);
				pauseMenuUiGroup.SetActive(false);
				finishMenuUiGroup.SetActive(true);
				menuButton.SetActive(true);

				finishUpperText.text = "Well done!";
				finishMiddleText.text = TimeFloatToString(gameController.GetGameTime(), false);
				newBestTime.SetActive(gameController.GetDidSetNewRecord());
				finishLowerText.text = "Play next level!";
				break;
			}
			case UiState.OutOfTime:
			{
				mainMenuUiGroup.SetActive(false);
				inGameUiGroup.SetActive(true);
				pauseMenuUiGroup.SetActive(false);
				finishMenuUiGroup.SetActive(true);
				menuButton.SetActive(true);

				finishUpperText.text = "Great work!";
				finishMiddleText.text = "But you ran \nout of time...";
				finishLowerText.text = "Try again!";
				newBestTime.SetActive(false);
				break;
			}
			case UiState.OutOfMoves:
			{
				mainMenuUiGroup.SetActive(false);
				inGameUiGroup.SetActive(true);
				pauseMenuUiGroup.SetActive(false);
				finishMenuUiGroup.SetActive(true);
				menuButton.SetActive(true);
				
				finishUpperText.text = "Great work!";
				finishMiddleText.text = "But you have \nno more moves...";
				newBestTime.SetActive(false);
				finishLowerText.text = "Try again!";
				break;
			}
		}
		uiState = state;
	}

	private void OnMenuButtonPressed()
	{
		switch (uiState)
		{
			case UiState.Playing: SetState(UiState.Pause);
				break;
			case UiState.Pause: SetState(UiState.Playing);
				break;
			case UiState.Solved: SetState(UiState.Main);
				break;
			case UiState.OutOfTime: SetState(UiState.Main);
				break;
			case UiState.OutOfMoves: SetState(UiState.Main);
				break;
		}
	}

	private void OnPlayButtonPressed()
	{
		gameController.StartNewGame();
		ClearTargetPattern();
		PopulateTargetPattern();
		timeLeftText.text = "";
		timeLeftText.color = textColor;
		movesLeftText.text = "";
		movesLeftText.color = textColor;
	}

	private void OnResetProgressButtonPressed()
	{
		gameController.ResetProgress();
		mainMenuLevelText.text = "Level " + gameController.GetPlayerLevel();
	}

	private void OnGoToMainMenuButtonPressed() => SetState(UiState.Main);
	
	private void OnBackToGameButtonPressed() => SetState(UiState.Playing);

	private void ClearTargetPattern()
	{
		var oldPieces = targetPatternParent.GetComponentsInChildren<Image>();
		if (oldPieces.Length > 0)
		{
			for (int i = oldPieces.Length - 1; i >= 0; i--)
			{
				Destroy(oldPieces[i].gameObject);
			}
		}
		
		var oldRows = targetPatternParent.GetComponentsInChildren<HorizontalLayoutGroup>();
		if (oldRows.Length > 0)
		{
			for (int i = oldRows.Length - 1; i >= 0; i--)
			{
				Destroy(oldRows[i].gameObject);
			}
		}
	}

	private void PopulateTargetPattern()
	{
		var colors = gameController.GetTargetPattern();
		var parent = targetPatternParent;
		var piecePrefab = targetPatternPiecePrefab;
		var rowPrefab = targetPatternRowPrefab;
		bool fillEmptySlot = !gameController.GetUseBufferEdges();

		for (int i = 0; i < colors.Count; i++)
		{
			var row = Instantiate(rowPrefab, parent.transform);
			for (int j = 0; j < colors[i].Count; j++)
			{
				var piece = Instantiate(piecePrefab, row.transform);
				var color = colors[i][j];
				color.a = 1;
				piece.GetComponent<Image>().color = color;
			}
			if (fillEmptySlot && i == colors.Count -1)
			{
				var piece = Instantiate(piecePrefab, row.transform);
				var color = new Color(0, 0, 0, 0);
				piece.GetComponent<Image>().color = color;
			}
		}
		
		SetState(UiState.Playing);
	}

	private string TimeFloatToString(float time, bool includeHundredths = false)
	{
		int hours = 0;
		int minutes = 0;
		int seconds;
		int hundredths;
		string timeString;
		
		if (time >= 3600)
		{
			hours = (int)(time / 3600);
			time -= hours * 3600;
		}

		if (time >= 60)
		{
			minutes = (int) (time / 60);
			time -= minutes * 60;
		}

		seconds = (int) time;
		time -= seconds;
        
		hundredths = (int) (time * 100);


		if (hours > 0)
		{
			timeString = hours + ":" + (minutes < 10 ? "0" + minutes : $"{minutes}");
		}
		else
		{
			timeString = minutes.ToString();
		}

		timeString += ":" + (seconds < 10 ? "0" + seconds : $"{seconds}");
		
		if (includeHundredths)
		{
			timeString += ":" + hundredths;
		}

		return timeString;
	}
}
