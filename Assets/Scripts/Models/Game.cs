using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Game
{
	public Map GameMap { get; private set; }
	public Player[] Players { get; private set; }
	public ushort Rounds { get; private set; }
	public ushort CurrentRound { get; private set; }
	public ushort PlayerCount
	{
		get => (ushort)Players.Length;
	}

	public Game(Player[] players, Map map)
	{
		Players = players;
		GameMap = map;
		Rounds = players.Length switch
		{
			3 => 10,
			4 => 8,
			5 => 7,
			_ => 13,
		};
		CurrentRound = 0;
	}

	public void NextRond()
	{
		if(CurrentRound < Rounds)
		{
			CurrentRound++;
		}
	}

	public void OrderPlayers(Dictionary<Player, short> sample)
	{
		var orderedPairs = sample.OrderByDescending(kvp => kvp.Value).ToArray();
		Players = orderedPairs.Select(p => p.Key).ToArray();
	}
}
