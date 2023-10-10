using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayer : Player
{
	public short Bid() => (short)Random.Range(0, 5);
}
