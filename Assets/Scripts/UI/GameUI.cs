using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor.ShaderGraph;
using Unity.VisualScripting;

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
	[SerializeField] private RectTransform _playerHolder;

	[SerializeField] private GameObject _cover;
	[SerializeField] private GameObject _selectAction;

    [SerializeField] private Transform _board;

    private GameController _controller;

	private Color _colorSourceLandSelected = new Color(1, 0.96470588235294117647058823529412f, 0.56078431372549019607843137254902f);
    private Color _colorTargetLandSelected = new Color(0.62745098039215686274509803921569f, 1, 0.56078431372549019607843137254902f);

    private delegate void LoadData<T>(T data, RectTransform obj);

	public Transform Lands;

	private void Awake()
	{
		_controller = GetComponent<GameController>();

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

	private void DrawArray<T>(T[] objects, float startingHeight, float heightDifference, RectTransform prefab, RectTransform panel, LoadData<T> loadData)
	{
		for(var i = 0; i < objects.Length; i++)
		{
			var obj = instantiateObject(i);
			loadData(objects[i], obj);
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

	private void LoadCardGraphics(Card card, RectTransform cardObj)
	{
		var price = cardObj.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
		var resourceIcon = cardObj.GetChild(1).GetChild(0).GetComponent<Image>();
		Image[] cardActions = new Image[card.CardActions.Length];

		price.text = card.Price.ToString();
		resourceIcon.sprite = _spriteResource[(int)card.Resource];

        cardActions[0] = cardObj.GetChild(1).GetChild(1).GetComponent<Image>();

        if (card.CardActions.Length > 1)
		{
            cardActions[0] = cardObj.GetChild(1).GetChild(2).GetChild(0).GetComponent<Image>();
            cardActions[1] = cardObj.GetChild(1).GetChild(2).GetChild(1).GetComponent<Image>();

			cardObj.GetChild(1).GetChild(1).gameObject.SetActive(false);
			cardObj.GetChild(1).GetChild(2).gameObject.SetActive(true);
		}

		for(var i = 0; i < cardActions.Length; i++)
			cardActions[i].sprite = _spriteAction[card.CardActions[i].Index];
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
                _controller.CurrentPlayersMove.action = action1;
                _controller.InitiateSelectingStage(1);
				_selectAction.SetActive(false);
            });
            actionObj2.GetComponent<Image>().sprite = _spriteAction[action2.Index];
            actionObj2.GetComponent<Button>().onClick.AddListener(() =>
			{
				Cover(false);
				_controller.ToggleControls(true);
				_controller.CurrentPlayersMove.action = action2;
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
		DrawArray<Player>(players, 0f, 92f, _playerHolder, _playerPanel, LoadPlayerStats);
	}


	public void DrawCards(Card[] cards)
	{
		DrawArray<Card>(cards, 400.42f, 160.17f, _cardHolder, _cardPanel, LoadCardGraphics);
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

			_controller.Bids.Add(_controller.ActivePlayer, bid);

			var nextPlayer = _controller.NextPlayer();
			print($"now active is player: {nextPlayer.Name}");

			if (_controller.Bids.ContainsKey(nextPlayer))
			{
				_controller.FinishBidding();
			}
				
			else
				_controller.StartBidding(nextPlayer);
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