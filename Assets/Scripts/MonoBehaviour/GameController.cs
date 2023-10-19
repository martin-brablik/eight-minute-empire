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


    public delegate object StatSetter(Player player);

    private GameController()
    {
        Game = GamePreloader.Instance.Game;
    }

    private void Awake()
    {
        _view = GetComponent<GameUI>();
        _gameLoop = GetComponent<GameLoop>();
    }

    public Move GetCurrentPlayersMove() => _gameLoop.CurrentPlayersMove;

    public void PlaceBid(Player player)
    {
        if (player is ComputerPlayer)
            PlaceBid(NextPlayer());

        else if (!_gameLoop.Bids.ContainsKey(player))
            _view.ShowBiddingScreen(player);

        else
            FinishBidding();
    }

    public void WriteBid(Player player, short amount)
    {
        _gameLoop.Bids.Add(player, amount); 
    }

    public Boolean IsBiddingFinished() => _gameLoop.Bids.ContainsKey(ActivePlayer);

    public void FinishBidding()
    {
        var winner = _gameLoop.Bids.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        _view.HideBiddingScreen();
        OrderPlayers();
        UpdatePlayerStat<short>(winner, Player.Stat.COINS, p => p.Coins -= _gameLoop.Bids[p]);
        _gameLoop.EndRound();
        _view.Cover(false);
    }

    public void UpdatePlayerStat<T>(Player player, Player.Stat stat, StatSetter setter)
    {
        var value = setter(player);
        _view.UpdatePlayerStat(player, stat, value.ToString());
    }

    public Player NextPlayer()
    {
        var nextPlayer = Game.Players[_gameLoop.NextPlayer()];

        if(GetCurrentPlayersMove() is not null && GetCurrentPlayersMove().Action is not null)
            GetCurrentPlayersMove().Action.sourcePlayer = nextPlayer;

        return nextPlayer;
    }

    public void RegenerateCard(System.Random cardGenerator, short cardPrice, int cardIndex)
    {
        
    }

    public Card GenerateCard(System.Random cardGenerator, short cardPrice)
    {
        var cardResource = (Resource)cardGenerator.Next(0, 5);
        var actionCount = cardGenerator.Next(1, 3);
        var cardActions = new CardAction[actionCount];

        for (var i = 0; i < actionCount; i++)
        {
            var actionIndex = cardGenerator.Next(CardAction.ActionsRegistry.Length);
            var count =  (short)(actionIndex == 3 ? 1 : cardGenerator.Next(1, 6));
            var newCardAction = new CardAction(CardAction.ActionsRegistry[actionIndex].Name, actionIndex);

            newCardAction.Count = count;

            cardActions[i] = newCardAction;
        }

        var card = new Card(cardResource, cardActions, cardPrice);
        return card;
    }

    private Card[] GenerateCards()
    {
        var cardGenerator = new System.Random();
        var deck = new Card[c_cardMaxAmount];

        for (var i = 0; i < c_cardMaxAmount; i++)
        {
            var card = GenerateCard(cardGenerator, i switch
            {
                0 => 0,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                5 => 3
            });

            card.Index = i;
            deck[i] = card;
        }

        return deck;
    }

    public Card[] DealCards()
    {
        var cards = GenerateCards();

        _view.DrawCards(cards);
        return cards;
    }

    public void ToggleControls(bool state)
    {
        IsSelectingAllowed = state;
        _view.Cover(!state);
    }

    public void OrderPlayers()
    {
        Game.OrderPlayers(_gameLoop.Bids);
        _view.DrawPlayers(Game.Players);
    }

    public void PickCard(Card card)
    {
        if (ActivePlayer.Coins < card.Price)
            return;

        UpdatePlayerStat<short>(ActivePlayer, Player.Stat.COINS, (p) => p.Coins -= card.Price);

        if (card.CardActions.Length > 1)
        {
            _view.Cover(true);
            print("select action");
            _view.ToggleSelectAction(card.CardActions[0], card.CardActions[1]);
        }
        else
        {
            ToggleControls(true);
            GetCurrentPlayersMove().Action = card.CardActions[0];
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
        var move = GetCurrentPlayersMove();
        var action = move.Action;
        var sourceLands = action.sourceLands;
        sourceLands.Push(selectedLand);
        GetCurrentPlayersMove().Action.sourceLands.Push(selectedLand);

        print("count: " + GetCurrentPlayersMove().Action.Count);
        if (GetCurrentPlayersMove().Action.Index == 1 || GetCurrentPlayersMove().Action.Index == 2) // pøesunout armády - vybrat cíl
            InitiateSelectingStage(2);
        else if(GetCurrentPlayersMove().Action.Count <= 0) // ostatní akce - vybrat území kterých se akce dotkne
        {
            //ToggleControls(false);
        }
        else // postavit mìsto
        {
            //ToggleControls(false);
        }

        GetCurrentPlayersMove().Action.Count--;
    }

    public void OnTargetLandSelected(Land selectedLand)
    {
        ToggleControls(false);
        _view.MarkLandSelected(selectedLand, true);
        print("selectedTargetLand: " + selectedLand.Name);
        GetCurrentPlayersMove().Action.targetLands.Push(selectedLand);
        _gameLoop.SelectingStage++;
    }
}
