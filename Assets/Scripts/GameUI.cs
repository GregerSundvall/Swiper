using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
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
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private GameObject inGameUi;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject finishMenu;
	
	[SerializeField] private GameObject menuButton;
	[SerializeField] private GameObject playButton;
	[SerializeField] private GameObject newGameButton;
	[SerializeField] private GameObject newRecord;

	[SerializeField] private TMP_Text mainMenuLevelText;
	[SerializeField] private TMP_Text inGameLevelText;
	[SerializeField] private TMP_Text finishTimeText;
	[SerializeField] private TMP_Text timeLeftText;
	[SerializeField] private TMP_Text movesLeftText;

	[SerializeField] private GameObject targetPatternParent;
	[SerializeField] private List<Image> targetPatternImages;

	[SerializeField] private GameObject targetPatternPiecePrefab;
	[SerializeField] private GameObject targetPatternRowPrefab;
	
	private UiState uiState;
	private GameController gameController;

	private float timeLeftTimer = 0;
	private float timeLeftUpdateDelay = 0.2f;
	

	private void Awake()
	{
		SetState(UiState.Main);
		
		menuButton.GetComponent<Button>().onClick.AddListener(OnMenuButtonPressed);
		playButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
		newGameButton.GetComponent<Button>().onClick.AddListener(OnPlayButtonPressed);
	}

	private void Start()
	{
		gameController = FindObjectOfType<GameController>();
		inGameLevelText.text = "Level " + gameController.GetPlayerLevel();
		mainMenuLevelText.text = "You are at level " + gameController.GetPlayerLevel();
	}

	private void Update()
	{
		if (timeLeftTimer > timeLeftUpdateDelay)
		{	
			UpdateTimeLeftText();
			timeLeftTimer = 0;
		}
		else
		{
			timeLeftTimer += Time.deltaTime;
		}
	}

	private void UpdateTimeLeftText() => timeLeftText.text = TimeFloatToString(gameController.GetTimeLeft());

	public void UpdateMovesLeftText(int moves) => movesLeftText.text = "Moves left: " + moves;

	private void SetState(UiState state)
	{
		mainMenu.SetActive(false);
		inGameUi.SetActive(false);
		pauseMenu.SetActive(false);
		finishMenu.SetActive(false);
		menuButton.SetActive(false);
		
		switch (state)
		{
			case UiState.Main:
			{
				mainMenu.SetActive(true);
				break;
			}
			case UiState.Playing:
			{
				inGameUi.SetActive(true);
				break;
			}
			case UiState.Pause:
			{
				pauseMenu.SetActive(true);
				break;
			}
			case UiState.Finish:
			{
				finishMenu.SetActive(true);
				finishTimeText.text = TimeFloatToString(gameController.GetGameTime(), true);
				newRecord.SetActive(gameController.GetDidSetNewRecord());
				break;
			}
		}
	}
	
	public void OnPuzzleSolved() => SetState(UiState.Finish);

	private void OnMenuButtonPressed()
	{
		SetState(uiState == UiState.Playing ? UiState.Pause : UiState.Playing);
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
		
		// for (int i = 0; i < colors.Count; i++)
		// {
		// 	var color = colors[i];
		// 	Debug.Log(color);
		// 	color.a = 1;
		// 	targetPatternImages[i].color = color;
		// }
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
