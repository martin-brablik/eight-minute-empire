using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
	[SerializeField] private RectTransform _biddingScreen;
	[SerializeField] private Image _imgBiddingColor;
	[SerializeField] private TMP_Text _txtBiddingPlayer;
	[SerializeField] private TMP_InputField _inBiddingAmount;
	[SerializeField] private Button _btnBiddingConfirm;

	[SerializeField] private Sprite[] _spriteAction;
	[SerializeField] private Sprite[] _spriteResource;

	[SerializeField] private RectTransform _cardPanel;
	[SerializeField] private RectTransform _cardHolder;

	[SerializeField] private RectTransform _playerPanel;
	[SerializeField] private RectTransform _prefabPlayerHolder;

	[SerializeField] private GameObject _cover;
	[SerializeField] private GameObject _selectAction;

    [SerializeField] private Transform _board;

    private GameController _controller;
	private PlayerHolder[] _playerHolders;

	private Color _colorSourceLandSelected = new Color(1, 0.96470588235294117647058823529412f, 0.56078431372549019607843137254902f);
    private Color _colorTargetLandSelected = new Color(0.62745098039215686274509803921569f, 1, 0.56078431372549019607843137254902f);

    private delegate void DataLoader<T>(T data, RectTransform obj);

	public Transform Lands;

	private void Awake()
	{
		_controller = GetComponent<GameController>();
		_playerHolders = new PlayerHolder[_controller.Players.Length];

        Lands = MapLoader.DrawMap(_controller.GameMap, _board);
        var landObjs = Lands.Cast<Transform>().ToArray();
        var lands = _controller.GameMap.Lands;

        if (landObjs.Length == lands.Length)
        {
            for (var i = 0; i < landObjs.Length; i++)
                lands[i].LandObj = landObjs[i].GetComponent<Image>();
        }

        _btnBiddingConfirm.onClick.AddListener(CallbackBiddingConfirm);
	}

	public void ShowBiddingScreen(Player player)
	{
        _biddingScreen.gameObject.SetActive(true);
		_txtBiddingPlayer.text = player.Name;
		_imgBiddingColor.color = player.Color;
		_inBiddingAmount.text = "";
	}

	private void DrawArray<T>(T[] objects, float startingHeight, float heightDifference, RectTransform prefab, RectTransform panel, DataLoader<T> loadData)
	{
		for(var i = 0; i < objects.Length; i++)
		{
			var obj = instantiateObject(i);
			loadData(objects[i], obj);

			if(typeof(T) == typeof(Player))
			{
				_playerHolders[i] = new PlayerHolder(objects[i] as Player);
				_playerHolders[i].Score = obj.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
				_playerHolders[i].Name = obj.GetChild(1).GetComponent<TMP_Text>();
                _playerHolders[i].Cities = obj.GetChild(2).GetChild(1).GetComponent<TMP_Text>();
				_playerHolders[i].Armies = obj.GetChild(2).GetChild(3).GetComponent<TMP_Text>();
                _playerHolders[i].Coins = obj.GetChild(2).GetChild(5).GetComponent<TMP_Text>();
            }
		}

		RectTransform instantiateObject(int index)
		{
			var obj = Instantiate(prefab, panel);

			if(typeof(T) == typeof(Card))
			{
				obj.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { CallbackCardClick(objects[index] as Card); });
			}

			obj.anchoredPosition = new Vector2(0, startingHeight - index * heightDifference);
			return obj;
		}
	}

	private KeyValuePair<T, RectTransform> DrawObject<T>(T instance, float height, RectTransform prefab, RectTransform panel, DataLoader<T> loadData)
	{
        var instanceObj = Instantiate(prefab, panel);

        if (typeof(T) == typeof(Card))
            instanceObj.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { CallbackCardClick(instance as Card); });

        instanceObj.anchoredPosition = new Vector2(0, height);
		loadData(instance, instanceObj);
		return new KeyValuePair<T, RectTransform>(instance, instanceObj);
    }

	private Dictionary<T, RectTransform> DrawObjects<T>(T[] objects, float startingHeight, float heightDifference, RectTransform prefab, RectTransform panel, DataLoader<T> loadData)
	{
		var result = new Dictionary<T, RectTransform>();

		for(var i = 0; i < objects.Length; i++)
		{
			var objPair = DrawObject<T>(objects[i], startingHeight - i * heightDifference, prefab, panel, loadData);
			var key = objPair.Key;
			var value = objPair.Value;

			result.Add(key, value);
        }

		return result;
	}

	private void LoadCardGraphics(Card card, RectTransform cardObj)
	{
		var price = cardObj.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
		var resourceIcon = cardObj.GetChild(1).GetChild(0).GetComponent<Image>();
		Transform[] cardActions = new Transform[card.CardActions.Length];

		price.text = card.Price.ToString();
		resourceIcon.sprite = _spriteResource[(int)card.Resource];

        cardActions[0] = cardObj.GetChild(1).GetChild(1);

        if (card.CardActions.Length > 1)
		{
            cardActions[0] = cardObj.GetChild(1).GetChild(2).GetChild(0);
            cardActions[1] = cardObj.GetChild(1).GetChild(2).GetChild(1);

			cardObj.GetChild(1).GetChild(1).gameObject.SetActive(false);
			cardObj.GetChild(1).GetChild(2).gameObject.SetActive(true);
		}

		for(var i = 0; i < cardActions.Length; i++)
		{
            cardActions[i].GetComponent<Image>().sprite = _spriteAction[card.CardActions[i].Index];

			if (card.CardActions[i].Count > 1)
				cardActions[i].GetChild(0).GetComponent<TMP_Text>().text = card.CardActions[i].Count.ToString();
        }		
	}

	private void LoadPlayerStats(Player player, RectTransform playerObj)
	{
		var scoreBg = playerObj.GetChild(0).GetComponent<Image>();
		var score = playerObj.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
		var name = playerObj.GetChild(1).GetComponent<TMP_Text>();
		var cities = playerObj.GetChild(2).GetChild(1).GetComponent<TMP_Text>();
		var armies = playerObj.GetChild(2).GetChild(3).GetComponent<TMP_Text>();
		var coins = playerObj.GetChild(2).GetChild(5).GetComponent<TMP_Text>();

		scoreBg.color = player.Color;
		score.text = player.Score.ToString();
		name.text = player.Name;
		cities.text = player.Cities.ToString();
		armies.text = player.Armies.ToString();
		coins.text = player.Coins.ToString();
	}

	public void HideBiddingScreen()
	{
		_biddingScreen.gameObject.SetActive(false);
	}

	public void Cover(bool state)
	{
		_cover.SetActive(state);
	}

	public void UpdatePlayerStat(Player player, Player.Stat stat, string value)
	{
		var playerHolder = _playerHolders.FirstOrDefault((ph) => ph.Player == player);

		switch(stat)
		{
			case Player.Stat.NAME:
				playerHolder.Name.text = value;
				break;
            case Player.Stat.SCORE:
                playerHolder.Score.text = value;
                break;
            case Player.Stat.CITIES:
                playerHolder.Cities.text = value;
                break;
            case Player.Stat.ARMIES:
                playerHolder.Armies.text = value;
                break;
            case Player.Stat.COINS:
                playerHolder.Coins.text = value;
                break;
        }
	}

	public void ToggleSelectAction(CardAction action1, CardAction action2)
	{
        if (action1 is not null && action2 is not null)
		{
            var actionObj1 = _selectAction.transform.GetChild(1);
            var actionObj2 = _selectAction.transform.GetChild(2);

            _selectAction.SetActive(true);
            actionObj1.GetComponent<Image>().sprite = _spriteAction[action1.Index];
            actionObj1.GetComponent<Button>().onClick.AddListener(() =>
            {
				Cover(false);
				_controller.ToggleControls(true);
                _controller.GetCurrentPlayersMove().Action = action1;
                _controller.InitiateSelectingStage(1);
				_selectAction.SetActive(false);
            });
            actionObj2.GetComponent<Image>().sprite = _spriteAction[action2.Index];
            actionObj2.GetComponent<Button>().onClick.AddListener(() =>
			{
				Cover(false);
				_controller.ToggleControls(true);
				_controller.GetCurrentPlayersMove().Action = action2;
				_controller.InitiateSelectingStage(1);
				_selectAction.SetActive(false);
			});
        }
		else
		{
			_selectAction.SetActive(false);
		}
	}

	public void MarkLandSelected(Land selectedLand, bool isTarget)
	{
		selectedLand.LandObj.color = isTarget ? _colorTargetLandSelected : _colorSourceLandSelected;
	}

    public void DrawPlayers(Player[] players)
    {
		var playerPairs = DrawObjects<Player>(players, 0f, 92f, _prefabPlayerHolder, _playerPanel, LoadPlayerStats);

		for(var i = 0; i < players.Length; i++)
		{
			_playerHolders[i] = new PlayerHolder(players[i]);
            _playerHolders[i].Score = playerPairs[players[i]].GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            _playerHolders[i].Name = playerPairs[players[i]].GetChild(1).GetComponent<TMP_Text>();
            _playerHolders[i].Cities = playerPairs[players[i]].GetChild(2).GetChild(1).GetComponent<TMP_Text>();
            _playerHolders[i].Armies = playerPairs[players[i]].GetChild(2).GetChild(3).GetComponent<TMP_Text>();
            _playerHolders[i].Coins = playerPairs[players[i]].GetChild(2).GetChild(5).GetComponent<TMP_Text>();
        }
    }

	public void DrawCards(Card[] cards)
	{
		DrawObjects<Card>(cards, 400.42f, 160.17f, _cardHolder, _cardPanel, LoadCardGraphics);
	}

	public void CallbackBiddingConfirm()
	{
		try
		{
			var bid = short.Parse(_inBiddingAmount.text);

			if(bid > _controller.ActivePlayer.Coins)
			{
				_inBiddingAmount.text = "";
				CallbackBiddingConfirm();
				return;
			}

			_controller.WriteBid(_controller.ActivePlayer, bid);

			var nextPlayer = _controller.NextPlayer();
			print($"now active is player: {nextPlayer.Name}");

			if (_controller.IsBiddingFinished())
			{
				_controller.FinishBidding();
			}
				
			else
				_controller.PlaceBid(nextPlayer);
		}
		catch(FormatException ex)
		{
			// TODO Show error screen
			print(ex.Message);
		}
	}

	public void CallbackCardClick(Card card)
	{
		_controller.PickCard(card);
	}
}