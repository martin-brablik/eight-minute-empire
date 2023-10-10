using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

public class GameLoop : MonoBehaviour
{
	private GameController _controller;

	private short _activePlayerIndex { get; set; }

	public Player ActivePlayer { get => _controller.Players[_activePlayerIndex]; }
	public short SelectingStage = 0;

    private void Awake()
    {
		_controller = GetComponent<GameController>();
		_activePlayerIndex = 0;
    }

	private void Start()
	{
		PreparePlayers();
		_controller.DealCards();
		_controller.ToggleControls(false);
		InitiateBidding();
	}

	public void EndRound()
	{
		_activePlayerIndex = 0;
	}

	private Land GetClickedLand()
	{
        var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(worldPoint, Vector3.forward);
        var clickedNode = _controller.GameMap.Graph.Nodes.FirstOrDefault(n => n.Value.Name.Equals(hit.collider.name));

		print(clickedNode.Value.Name);
		return clickedNode.Value;
    }

	private void Update()
	{
        if (Input.GetMouseButtonDown(0) && _controller.IsSelectingAllowed && ActivePlayer is LocalPlayer)
		{
			print(SelectingStage);
			if(SelectingStage == 1)
			{
				try
				{
					var selectedLand = GetClickedLand();

					_controller.OnSourceLandSelected(selectedLand);
					print("stage1");
				}
				catch (Exception ex)
				{
					print(ex.StackTrace);
				}
			}
			else if(SelectingStage == 2)
			{
                try
                {
                    var selectedLand = GetClickedLand();

                    _controller.OnTargetLandSelected(selectedLand);
					print("stage2");
                }
                catch (Exception ex)
				{
					print(ex.StackTrace);
				}
            }
		}

	}

	private void InitiateBidding()
	{
		var random = new System.Random();

		foreach (var player in _controller.Players.Where(p => p is ComputerPlayer))
		{
			short bid = (short)random.Next(0, 5);

            _controller.Bids.Add(player, bid);
			print($"{player.Name} {bid}");
		}

		_controller.StartBidding(ActivePlayer);
	}

	public int NextPlayer() => _activePlayerIndex == _controller.Players.Length - 1 ? 0 : ++_activePlayerIndex;

	private void PreparePlayers()
	{
		foreach(var player in _controller.Players)
		{
			player.Armies = 14;
			player.Cities = 3;
			player.Coins = _controller.Players.Length switch
			{
				5 => 8,
				4 => 9,
				3 => 11,
				2 => 14
			};
		}
	}
}
