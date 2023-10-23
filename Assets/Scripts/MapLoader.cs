using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Drawing.Printing;

public class MapLoader
{
    public static string MapsLocation = MapsLocation = Path.Combine(Application.persistentDataPath, "Maps").Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

	private delegate GameObject DrawMethod(Image image, string texturePath, Transform parent, Land land = null);

	private const float c_homeIconSize = 16;

	private static Texture2D ReadTextureData(string file)
	{
		Texture2D texture = null;
		byte[] fileData;

		if (File.Exists(file))
		{
			try
			{
				fileData = File.ReadAllBytes(file);
				texture = new Texture2D(2, 2);

				texture.LoadImage(fileData);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
		return texture;
	}

	private static Sprite LoadSpriteFromFile(string file, Func<string, Texture2D> spriteLoader)
	{
		var pixelsPerUnit = 100.0f;
		var texture = spriteLoader(file);
		var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, pixelsPerUnit);
		return sprite;
	}

	public static IEnumerable<Map> LoadMaps()
    {
        foreach(string dirname in Directory.GetDirectories(MapsLocation))
        {
            var mapFile = Path.Combine(dirname, $"{Path.GetFileName(dirname)}.json");
			var linksFile = Path.Combine(dirname, "links.json");
			var mapJson = File.ReadAllText(mapFile);
			var linksJson = File.ReadAllText(linksFile);
            var lands = JsonConvert.DeserializeObject<Land[]>(mapJson);
			var links = JsonConvert.DeserializeObject<List<GraphLinkFoundation>>(linksJson);
			var nodes = lands.Select(l => new GraphNode<Land>(l)).ToList();
			var graph = new Graph<Land>(nodes, links.Select(l => GraphLink<Land>.BuildLink(l, nodes, l.IsOversea)).ToList());
			var preview = LoadSpriteFromFile(Path.Combine(dirname, "preview.png"), ReadTextureData);
			var map = new Map(Path.GetFileName(dirname), lands, graph, preview);

			foreach (var land in map.Lands)
				land.TexturePath = Path.Combine(MapsLocation, map.Name, "lands", $"{land.Name}.png").Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

			yield return map;
        }
    }

    public static Transform DrawMap(Map map, Transform board)
    {
		var defaultSize = 386.49f;
		var backgroundObj = Draw("Background", Path.Combine(MapLoader.MapsLocation, map.Name, "background.png"), DrawCover, board, null, typeof(CanvasRenderer), typeof(Image));
		var landsRoot = Draw("Lands", null, null, board, null, typeof(RectTransform));
		var bordersObj = Draw("Borders", Path.Combine(MapLoader.MapsLocation, map.Name, "borders.png"), DrawCover, board, null, typeof(CanvasRenderer), typeof(Image));

		landsRoot.GetComponent<RectTransform>().localPosition = Vector3.zero;
		landsRoot.GetComponent<RectTransform>().localScale = Vector3.one;

		foreach (var land in map.Lands)
		{
			var landObj = Draw(land.Name, land.TexturePath, DrawLand, landsRoot.transform, land, typeof(CanvasRenderer), typeof(Image));
			if(land.IsHome)
			{
				var homeIconSprite = LoadSpriteFromFile("home", Resources.Load<Texture2D>);

				if(homeIconSprite != null)
				{
                    var homeIcon = new GameObject().AddComponent<Image>();
                    homeIcon.transform.SetParent(landObj.transform);
                    homeIcon.color = new Color(0, 0, 0);
                    homeIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(land.HomeIconOffset[0], land.HomeIconOffset[1]);
                    homeIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(c_homeIconSize * land.HomeIconScale, c_homeIconSize * land.HomeIconScale);
                    homeIcon.sprite = homeIconSprite;
                }
            }

			foreach (var area in land.Boundaries.Areas)
			{
				landObj.AddComponent<PolygonCollider2D>().points = area.Points;
			}
		}

		return landsRoot.transform;

		GameObject DrawLand(Image image, string texturePath, Transform parent, Land land)
		{
			image.sprite = LoadSpriteFromFile(texturePath, ReadTextureData);
			image.rectTransform.anchorMin = Vector2.up;
			image.rectTransform.anchorMax = Vector2.up;
			image.rectTransform.pivot = Vector2.one / 2;
			image.rectTransform.anchoredPosition = new Vector2(land.Position[0], land.Position[1]);
			image.rectTransform.sizeDelta = new Vector2(defaultSize * land.Scale, defaultSize * land.Scale);
			image.rectTransform.localScale = Vector3.one;

			return image.gameObject;
		}

		GameObject DrawCover(Image image, string texturePath, Transform parent, Land land = null)
		{
			image.sprite = LoadSpriteFromFile(texturePath, ReadTextureData);
			image.rectTransform.anchorMin = Vector2.zero;
			image.rectTransform.anchorMax = Vector2.one;
			image.rectTransform.pivot = Vector2.one / 2;
			image.rectTransform.offsetMin = Vector2.zero;
			image.rectTransform.offsetMax = Vector2.zero;
			image.rectTransform.localScale = Vector3.one;

			return image.gameObject;
		}

		GameObject Draw(string objectName, string texturePath, DrawMethod drawMethod, Transform parent = null, Land land = null, params Type[] components)
		{
			var obj = new GameObject(objectName, components);
			Image img;

			obj.transform.SetParent(parent);
			return obj.TryGetComponent(out img) ? drawMethod(img, texturePath, parent, land) : obj;
		}
	}
}
