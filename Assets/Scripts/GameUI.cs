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
	Finish
}


public class GameUI : MonoBehaviour
{
	//Main menu
	[SerializeField] private GameObject mainMenuUiGroup;
	[SerializeField] private GameObject playButton;
	[SerializeField] private TMP_Text mainMenuLevelText;
	
	// In-game
	[SerializeField] private GameObject inGameUiGroup;
	[SerializeField] private GameObject menuButton;
	[SerializeField] private TMP_Text timeLeftText;
	[SerializeField] private TMP_Text movesLeftText;
	
	// Pause menu
	[SerializeField] private GameObject pauseMenuUiGroup;
	[SerializeField] private GameObject newGameButton;
	
	// Finish screen
	[SerializeField] private GameObject finishMenuUiGroup;
	[SerializeField] private TMP_Text finishTimeText;
	[SerializeField] private GameObject newRecord;
	[SerializeField] private GameObject playNextLevel;
	
	// Init stuff
	[SerializeField] private GameObject targetPatternPiecePrefab;
	[SerializeField] private GameObject targetPatternRowPrefab;
	[SerializeField] private GameObject targetPatternParent;
	
	
	private UiState uiState;
	private GameController gameController;

	private float timeLeftUpdateTimer = 0;
	private float timeLeftUpdateDelay = 0.2f;
	

	private void Awake()
	{
		SetState(UiState.Main);
		
		menuButton.GetComponent<Button>().onClick.AddListener(OnMenuButtonPressed);
		playButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		newGameButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		playNextLevel.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
	}

	private void Start()
	{
		gameController = FindObjectOfType<GameController>();
		mainMenuLevelText.text = "Level " + gameController.GetPlayerLevel();
	}

	private void Update()
	{
		if (timeLeftUpdateTimer > timeLeftUpdateDelay)
		{	
			UpdateTimeLeftText();
			timeLeftUpdateTimer = 0;
		}
		else
		{
			timeLeftUpdateTimer += Time.deltaTime;
		}
	}

	private void UpdateTimeLeftText()
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
		else
		{
			timeLeftText.text = "Time's up!";
		}
	}

	public void UpdateMovesLeftText(int moves)
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
		else
		{
			movesLeftText.text = "No more moves!";
		}
	}

	private void SetState(UiState state)
	{
		mainMenuUiGroup.SetActive(false);
		inGameUiGroup.SetActive(false);
		pauseMenuUiGroup.SetActive(false);
		finishMenuUiGroup.SetActive(false);
		menuButton.SetActive(false);
		
		switch (state)
		{
			case UiState.Main:
			{
				mainMenuUiGroup.SetActive(true);
				break;
			}
			case UiState.Playing:
			{
				inGameUiGroup.SetActive(true);
				menuButton.SetActive(true);
				break;
			}
			case UiState.Pause:
			{
				pauseMenuUiGroup.SetActive(true);
				menuButton.SetActive(true);
				break;
			}
			case UiState.Finish:
			{
				finishMenuUiGroup.SetActive(true);
				finishTimeText.text = TimeFloatToString(gameController.GetGameTime(), true);
				newRecord.SetActive(gameController.GetDidSetNewRecord());
				menuButton.SetActive(true);
				break;
			}
		}
		uiState = state;
	}
	
	public void OnPuzzleSolved() => SetState(UiState.Finish);

	private void OnMenuButtonPressed()
	{
		switch (uiState)
		{
			case UiState.Playing: SetState(UiState.Pause);
				break;
			case UiState.Pause: SetState(UiState.Playing);
				break;
			case UiState.Finish: SetState(UiState.Main);
				break;
		}
	}

	private void OnPlayButtonPressed()
	{
		gameController.StartNewGame();
		
		var colors = gameController.GetTargetPattern();
		var parent = targetPatternParent;
		var piecePrefab = targetPatternPiecePrefab;
		var rowPrefab = targetPatternRowPrefab;

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
