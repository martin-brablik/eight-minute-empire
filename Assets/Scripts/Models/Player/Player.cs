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
}
