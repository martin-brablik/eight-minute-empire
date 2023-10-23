using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
	public string Name { get; set; }
	public Color Color { get; set; }
	public uint Armies { get; set; }
	public uint Cities { get; set; }
	public short Coins { get; set; }
	public Dictionary<Resource, uint> Resources { get; set; }
	public uint Score { get; private set; }

	public enum Stat { NAME, SCORE, ARMIES, CITIES, COINS }

	private GameController _controller;

	public void RecruitArmy(Land land, uint count)
	{
		_controller.UpdatePlayerStat(this, Stat.ARMIES, (p) => p.Armies -= count);
		land.ArmiesPresence[this] += count;
	}
}
