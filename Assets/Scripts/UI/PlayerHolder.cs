using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHolder
{
    public Player Player { get; private set; }
    public TMP_Text Name { get; set; }
    public TMP_Text Score { get; set; }
    public TMP_Text Cities { get; set; }
    public TMP_Text Armies { get; set; }
    public TMP_Text Coins { get; set; }

    public PlayerHolder(Player player)
    {
        this.Player = player;
    }
}
