using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.IO;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private RectTransform[] _menuScreens; 

	[SerializeField] private Button _btnPlaySolo;
	[SerializeField] private Button _btnPlayNetwork;

	[SerializeField] private Image _imageSelectMap;
	[SerializeField] private TMP_Dropdown _dropdownSelectMap;
	[SerializeField] private Button _btnSelectMap;

	[SerializeField] private Image[] _imgPlayersImages;
	[SerializeField] private TMP_InputField[] _inPlayersNames;
	[SerializeField] private TMP_Dropdown[] _dropdownPlayersTypes;
	[SerializeField] private Button _btnPlayersBegin;

	[SerializeField] private Button _btnQuit;

	private const short c_maxPlayers = 5;

	private const short c_playerTypeNone = 0;
	private const short c_playerTypeLocal = 1;
	private const short c_playerTypeComputer = 2;
	private const short c_playerTypeRemote = 3;

	private NewGame _newGame;

	private void Awake()
	{
		InstallDefaultMap(Path.Combine(Application.dataPath, "Europe"), Path.Combine(Application.persistentDataPath, "Maps", "Europe"));

		_btnPlaySolo.onClick.AddListener(delegate { CallbackPlaySolo(); });
		_btnSelectMap.onClick.AddListener(delegate { CallbackSelectMap(); });
		_btnPlayersBegin.onClick.AddListener(delegate { CallbackPlayersBegin(); });

		_dropdownSelectMap.AddOptions(GamePreloader.Instance.GlobalMapsArray.Select(m => m.Name).ToList());
		_dropdownSelectMap.onValueChanged.AddListener(o => _imageSelectMap.sprite = GamePreloader.Instance.GlobalMapsArray[o].Preview);
		_imageSelectMap.sprite = GamePreloader.Instance.GlobalMapsArray[_dropdownSelectMap.value].Preview;

		_inPlayersNames[0].onValueChanged.AddListener(delegate { CallbackPlayersNames(0); });
		_inPlayersNames[1].onValueChanged.AddListener(delegate { CallbackPlayersNames(1); });
		_inPlayersNames[2].onValueChanged.AddListener(delegate { CallbackPlayersNames(2); });
		_inPlayersNames[3].onValueChanged.AddListener(delegate { CallbackPlayersNames(3); });
		_inPlayersNames[4].onValueChanged.AddListener(delegate { CallbackPlayersNames(4); });

		_btnQuit.onClick.AddListener(() => Application.Quit());
	}

	private void InstallDefaultMap(string sourceDir, string destinationDir)
	{
		var dir = new DirectoryInfo(sourceDir);

		if (!Directory.Exists(sourceDir) || Directory.Exists(destinationDir))
			return;

		DirectoryInfo[] dirs = dir.GetDirectories();
		Directory.CreateDirectory(Path.Combine(destinationDir));

		foreach(FileInfo file in dir.GetFiles())
		{
			var targetFilePath = Path.Combine(destinationDir, file.Name);
			file.CopyTo(targetFilePath);
		}

		foreach(DirectoryInfo subDir in dirs)
		{
			var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
			InstallDefaultMap(subDir.FullName, newDestinationDir);
		}
	}

	private void HideAll()
	{
		foreach(var screen in _menuScreens)
			screen.gameObject.SetActive(false);
	}

	private void CallbackPlaySolo()
	{
		HideAll();
		_menuScreens[1].gameObject.SetActive(true);
	}

	private void CallbackPlayNetwork()
	{

	}

	private void CallbackSelectMap()
	{
		HideAll();
		_menuScreens[2].gameObject.SetActive(true);
		_newGame.GameMap = GamePreloader.Instance.GlobalMapsArray[_dropdownSelectMap.value];
	}

	private bool IsValidPlayer(int i) => !_inPlayersNames[i].text.Equals("") && _dropdownPlayersTypes[i].value != c_playerTypeNone;

	private bool AllPlayersReady(out int playerCount)
	{
		playerCount = 0;

		for (var i = 0; i < c_maxPlayers; i++)
		{
			if (IsValidPlayer(i))
				playerCount++;
		}

		// TODO Verify that at least one player is local human
		return playerCount > 1;
	}

	private void CallbackPlayersBegin()
	{
		int playerCount;

		if (!AllPlayersReady(out playerCount))
			return;

		if (playerCount < 2)
			return;

		_newGame.Players = new Player[playerCount];

		var invlaidPlayerCount = 0;

		for(var i = 0; i < c_maxPlayers; i++)
		{
			if (IsValidPlayer(i))
			{
				Player player;

				var playerColor = i switch
				{
					0 => new Color(0.83f, 0.1f, 0.21f),
					1 => new Color(0.1f, 0.34f, 0.83f),
					2 => new Color(0.33f, 0.83f, 0.1f),
					3 => new Color(0.83f, 0.75f, 0.1f),
					4 => new Color(0.83f, 0.1f, 0.71f),
					_ => new Color(1f, 0f, 1f)
				};

				player = _dropdownPlayersTypes[i].value switch
				{
					c_playerTypeLocal => new LocalPlayer() { Name = _inPlayersNames[i].text, Color = playerColor },
					c_playerTypeComputer => new ComputerPlayer() { Name = _inPlayersNames[i].text, Color = playerColor },
					//c_playerTypeRemote => new RemotePlayer()
				};
				_newGame.Players[i - invlaidPlayerCount] = player;
			}
			else
				invlaidPlayerCount++;
		}

		GamePreloader.Instance.Game = new Game(_newGame.Players, _newGame.GameMap);
		SceneManager.LoadScene(1);
	}

	private void CallbackPlayersNames(int playerIndex)
	{
		if (_dropdownPlayersTypes[playerIndex].value == 0)
			_dropdownPlayersTypes[playerIndex].value = 1;

		if (_inPlayersNames[playerIndex].text.Equals(""))
			_dropdownPlayersTypes[playerIndex].value = 0;

		int playerCount;

		_btnPlayersBegin.interactable = AllPlayersReady(out playerCount);

	}

	private struct NewGame
	{
		public Player[] Players;
		public Map GameMap;
	}
}
