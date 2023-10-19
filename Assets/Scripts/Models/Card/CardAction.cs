using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction
{
    public string Name { get; private set; }
    public int Index { get; private set; }
    public Player sourcePlayer { get; set; }
    public Player targetPlayer { get; set; }
    public Stack<Land> sourceLands { get; set; } = new Stack<Land>();
    public Stack<Land> targetLands { get; set; } = new Stack<Land>();
    public short Count { get; set; }

    public CardAction(string name, int index)
    {
        Name = name;
        Index = index;
    }

    public CardAction() { }

    public static readonly CardAction[] ActionsRegistry =
    {
        new CardAction("recruit", 0),
        new CardAction("walk", 1),
        new CardAction("sail", 2),
        new CardAction("build", 3),
        new CardAction("battle", 4)
    };
}