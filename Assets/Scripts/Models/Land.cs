using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

[JsonObject(MemberSerialization.OptOut)]
public class Land
{
    #region Internal
	public string Name { get; private set; }
    public float[] Position { get; private set; }
    public short Scale { get; private set; }
    [JsonProperty("Boundaires")]
    public Boundaries Boundaries { get; private set; } = new Boundaries();
    public string TexturePath { get; set; }
	#endregion

    public Dictionary<Player, uint> ArmiesPresence { get; private set; }
    public Player HasCity { get; set; }
    public Image LandObj { get; set; }
    public Player Owner
    {
        get => ArmiesPresence.Aggregate((a, b) => a.Value > b.Value ? a : b).Key;
    }

	public Land(string name, float[] position, short scale, Boundaries boundaries)
    {
        Name = name;
        Position = position;
        Scale = scale;
        Boundaries = boundaries;
    }

    public override string ToString() => Name;
}

[JsonObject(MemberSerialization.OptOut)]
public class Boundaries
{
    [JsonProperty("Areas")]
    public List<Area> Areas { get; private set; } = new List<Area>();
}

[JsonObject(MemberSerialization.Fields)]
public class Area
{
	[JsonProperty("Points")]
	private List<Point> _points = new List<Point>();

    [JsonIgnore]
    public Vector2[] Points
    {
        get => _points.Select<Point, Vector2>(p => p.Vector).ToArray();
    }
}

[JsonObject(MemberSerialization.OptOut)]
public class Point
{
    public float X { get;  set; }
    public float Y { get; set; }

    [JsonIgnore]
    public Vector2 Vector
    {
        get => new Vector2(X, Y);
    }
}
