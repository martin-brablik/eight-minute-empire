using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction
{
    public string Name { get; set; }
    public int Index { get; set; }
    public Player sourcePlayer { get; set; }
    public Player targetPlayer { get; set; }
    public Land sourceLand { get; set; }
    public Land targetLand { get; set; }

    public static CardAction[] ActionsRegistry =
    {
        new CardAction()
        {
            Name = "recruit",
            Index = 0,
        },
        new CardAction()
        {
            Name = "walk",
            Index = 1,
        },
        new CardAction()
        {
            Name = "sail",
            Index = 2,
        },
        new CardAction()
        {
            Name = "build",
            Index = 3,
        },
        new CardAction()
        {
            Name = "battle",
            Index = 4,
        },
    };
}