using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public string Name { get; private set; }
    public Land[] Lands { get; private set; }
    public Graph<Land> Graph { get; private set; }
    public Sprite Preview { get; private set; }

    public Map(string name, Land[] lands, Graph<Land> graph, Sprite preview)
    {
        Name = name;
        Lands = lands;
        Graph = graph;
        Preview = preview;
    }
}
