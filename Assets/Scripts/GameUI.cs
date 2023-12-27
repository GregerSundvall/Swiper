using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	[SerializeField] private List<Image> images;
	
	public void ShowSolution(List<Color> colors)
	{
		for (int i = 0; i < colors.Count; i++)
		{
			var color = colors[i];
			color.a = 1;
			images[i].color = color;
		}
	}
}
