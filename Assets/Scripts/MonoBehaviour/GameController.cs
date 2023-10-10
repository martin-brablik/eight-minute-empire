using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ShaderGraph;
using System.CodeDom;

public class GameController : MonoBehaviour
{
    private const short c_cardMaxAmount = 6;

    public bool IsSelectingAllowed { get; private set; }


    [SerializeField] private GameUI _view;
    [SerializeField] private GameLoop _gameLoop;
    private Game Game { get; set; }
    public Map GameMap { get => Game.GameMap; }
    public Player ActivePlayer { get => _gameLoop.ActivePlayer; }
    public Player[] Players { get => Game.Players; }
    public Dictionary<Player, short> Bids { get; private set; } = new Dictionary<Player, short>();
    public Move CurrentPlayersMove { get; private set; }

    private GameController()
    {
        Game = GamePreloader.Instance.Game;
    }

    private void Awake()
    {
        _view = GetComponent<GameUI>();
        _gameLoop = GetComponent<GameLoop>();
    }

    public void StartBidding(Player player)
    {
        if (player is ComputerPlayer)
            StartBidding(NextPlayer());

        else if (!Bids.ContainsKey(player))
            _view.ShowBiddingScreen(player);

        else
            FinishBidding();
    }

    public void FinishBidding()
    {
        var winner = Bids.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        _view.HideBiddingScreen();
        OrderPlayers();
        UpdatePlayerStat(winner, p => p.Coins -= Bids[p]);
        CurrentPlayersMove = new Move();
        _gameLoop.EndRound();
        _view.Cover(false);
    }

    public void UpdatePlayerStat(Player player, Action<Player> setter)
    {
        setter(player);
    }

    public Player NextPlayer()
    {
        var nextPlayer = Game.Players[_gameLoop.NextPlayer()];

        if(CurrentPlayersMove is not null)
            CurrentPlayersMove.action.sourcePlayer = nextPlayer;

        return nextPlayer;
    }

    private Card GenerateCard(System.Random cardGenerator, short cardPrice)
    {
        var cardResource = (Resource)cardGenerator.Next(0, 5);
        var actionCount = cardGenerator.Next(1, 3);
        var cardActions = new CardAction[actionCount];

        for (var i = 0; i < actionCount; i++)
        {
            var newCardAction = new CardAction();
            var actionIndex = cardGenerator.Next(CardAction.ActionsRegistry.Length);


            newCardAction.Name = CardAction.ActionsRegistry[actionIndex].Name;
            newCardAction.Index = CardAction.ActionsRegistry[actionIndex].Index;

            cardActions[i] = CardAction.ActionsRegistry[cardGenerator.Next(CardAction.ActionsRegistry.Length)];
        }

        var card = new Card(cardResource, cardActions);

        card.Price = cardPrice;
        return card;
    }

    private Card[] GenerateCards()
    {
        var cardGenerator = new System.Random();
        var cardSet = new Card[c_cardMaxAmount];

        for (var i = 0; i < c_cardMaxAmount; i++)
        {
            cardSet[i] = GenerateCard(cardGenerator, i switch
            {
                0 => 0,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                5 => 3
            });
        }

        return cardSet;
    }

    public void DealCards()
    {
        _view.DrawCards(GenerateCards());
    }

    public void ToggleControls(bool state)
    {
        IsSelectingAllowed = state;
        _view.Cover(!state);
    }

    public void OrderPlayers()
    {
        Game.OrderPlayers(this.Bids);
        _view.DrawPlayers(Game.Players);
    }

    public void PickCard(Card card)
    {
        if(card.CardActions.Length > 1)
        {
            _view.Cover(true);
            print("select action");
            _view.ToggleSelectAction(card.CardActions[0], card.CardActions[1]);
        }
        else
        {
            ToggleControls(true);
            print("select land");
            InitiateSelectingStage(1);
        }
    }

    public void InitiateSelectingStage(short stage=1)
    {
        print("satge: " + stage);
        _gameLoop.SelectingStage = stage;
    }

    public void OnSourceLandSelected(Land selectedLand)
    {
        _view.MarkLandSelected(selectedLand, false);
        CurrentPlayersMove.action.sourceLand = selectedLand;
        print("selectedSourceLand: " + selectedLand.Name);
        print("hello?");
        print(CurrentPlayersMove.action.Name + " " + CurrentPlayersMove.action.Index);
        if (CurrentPlayersMove.action.Index == 1 || CurrentPlayersMove.action.Index == 2)
            InitiateSelectingStage(2);
        else
        {
            ToggleControls(false);
        }
    }

    public void OnTargetLandSelected(Land selectedLand)
    {
        ToggleControls(false);
        _view.MarkLandSelected(selectedLand, true);
        print("selectedTargetLand: " + selectedLand.Name);
        CurrentPlayersMove.action.targetLand = selectedLand;
        _gameLoop.SelectingStage++;
    }
}
