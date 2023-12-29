using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	[SerializeField] private GameController gameController;
	
	[SerializeField] private List<Image> solutionImages;
	
	[SerializeField] private GameObject messageGO;
	[SerializeField] private GameObject menuButtonGO;
	[SerializeField] private GameObject menuGO;
	[SerializeField] private GameObject newGameGO;
	[SerializeField] private GameObject solutionGO;
	[SerializeField] private GameObject recordTimeGO;
	[SerializeField] private GameObject timerGO;
	
	[SerializeField] private TMP_Text winTimeText;
	[SerializeField] private TMP_Text timerText;
    
	


	private void Awake()
	{
		messageGO.SetActive(false);
		menuButtonGO.SetActive(false);
		menuGO.SetActive(true);
		solutionGO.SetActive(false);
		timerGO.SetActive(false);

		menuButtonGO.GetComponent<Button>().onClick.AddListener(OnMenuButtonPressed);
		newGameGO.GetComponent<Button>().onClick.AddListener(OnNewGamePressed);
	}

	private void Update()
	{
		// timerText.text = GetTimeString();
	}

	private string GetTimeString()
	{
		var time = gameController.GetGameTime();
		
		var hours = "";
		var minutes = "";
		var seconds = "";
		var hundredths = "";
		
		if (time >= 3600)
		{
			var hoursInt = (int)(time / 3600);
			time -= hoursInt * 3600;
			hours = hoursInt + ":";
		}

		if (time >= 60)
		{
			var minutesInt = (int) (time / 60);
			time -= minutesInt * 60;
			minutes = minutesInt + ":";
		}

		var secondsInt = (int) time;
		time -= secondsInt;
		seconds = secondsInt + ":";
        
		var hundredthsInt = (int) (time * 100);
		hundredths = hundredthsInt.ToString();

		var timeString = hours + minutes + seconds + hundredths;
		return timeString;
	}
	
	

	private void OnMenuButtonPressed()
	{
		messageGO.SetActive(false);
		menuButtonGO.SetActive(false);
		menuGO.SetActive(true);
		solutionGO.SetActive(false);
	}
	
	public void ShowSolution(List<Color> colors)
	{
		for (int i = 0; i < colors.Count; i++)
		{
			var color = colors[i];
			color.a = 1;
			solutionImages[i].color = color;
		}
	}
	
	private void OnNewGamePressed()
	{
		messageGO.SetActive(false);
		menuButtonGO.SetActive(true);
		menuGO.SetActive(false);
		solutionGO.SetActive(true);
		
		gameController.OnNewGamePressed();
	}
	
	public void OnPuzzleSolved(bool setNewRecord)
	{
		messageGO.SetActive(true);
		timerGO.SetActive(false);
		winTimeText.text = GetTimeString();
		recordTimeGO.SetActive(setNewRecord);
	}
}
