using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePreloader
{
	private static GamePreloader s_instance = null;

	public Map[] GlobalMapsArray;
	public Game Game;

	private GamePreloader()
	{
		GlobalMapsArray = MapLoader.LoadMaps().ToArray();
	}

	public static GamePreloader Instance
	{
		get
		{
			if(s_instance == null)
				s_instance= new GamePreloader();

			return s_instance;
		}
	}
}
