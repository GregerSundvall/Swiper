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
	
	[SerializeField] private TMP_Text levelText;
	[FormerlySerializedAs("winTimeText")]
	[SerializeField] private TMP_Text finishTimeText;
	[FormerlySerializedAs("timerText")]
	[SerializeField] private TMP_Text timeLeftText;
	[SerializeField] private TMP_Text movesLeftText;
	
	
	[SerializeField] private List<Image> solutionImages;

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
		levelText.text = "Level " + gameController.GetCurrentLevel(); 
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
		
		var colors = gameController.GetColorPattern();
		for (int i = 0; i < colors.Count; i++)
		{
			var color = colors[i];
			color.a = 1;
			solutionImages[i].color = color;
		}
		SetState(UiState.Playing);	
	}

	private string TimeFloatToString(float time, bool includeHundredths = false)
	{
		var hours = 0;
		var minutes = 0;
		var seconds = 0;
		var hundredths = 0;
		
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

		string timeString = (hours > 0 ? hours.ToString() : "") +
		                    (minutes > 0 ? minutes.ToString() : "") + 
		                    seconds +
		                    (includeHundredths ? hundredths.ToString() : "");

		return timeString;
	}
}
