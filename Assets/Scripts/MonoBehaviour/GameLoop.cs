using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameLoop : MonoBehaviour
{
	private GameController _controller;

	private short _activePlayerIndex { get; set; }

	public Player ActivePlayer { get => _controller.Players[_activePlayerIndex]; }
	public short SelectingStage { get; set; } = 0;
    public Move CurrentPlayersMove { get; set; }
    public Dictionary<Player, short> Bids { get; private set; } = new Dictionary<Player, short>();
    public Card[] Deck { get; private set; }

    private void Awake()
    {
		_controller = GetComponent<GameController>();
		_activePlayerIndex = 0;
		CurrentPlayersMove = new Move();
    }

	private void Start()
	{
		PreparePlayers();
		Deck = _controller.DealCards();
		_controller.ToggleControls(false);
		DivideLands();
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
			if (SelectingStage == 1)
			{
				var selectedLand = GetClickedLand();

				_controller.OnSourceLandSelected(selectedLand);
				print("stage1");
			}
			else if (SelectingStage == 2)
			{
				var selectedLand = GetClickedLand();

				_controller.OnTargetLandSelected(selectedLand);
				print("stage2");
			}
		}
	}

    public void DivideLands()
    {
        for (var i = 0; i < _controller.Players.Length; i++)
            _controller.Lands[i].ArmiesPresence.Add(_controller.Players[i], 0);
    }

    private void InitiateBidding()
	{
		var random = new System.Random();

		foreach (var player in _controller.Players.Where(p => p is ComputerPlayer))
		{
			short bid = (short)random.Next(0, 5);

            Bids.Add(player, bid);
			print($"{player.Name} {bid}");
		}

		_controller.PlaceBid(ActivePlayer);
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

	private void PlaceFirstArmies()
	{
		foreach(var player in _controller.Players)
			player.RecruitArmy(_controller.la)
	}

	public void PlayTurn(Card card)
	{
		//ostatn� v�ci
		var cardGenerator = new System.Random();
		var originalPrice = card.Price;
		var cardIndex = card.Index;

		//card = _controller.RegenerateCard(cardGenerator, originalPrice, cardIndex);
	}
}
