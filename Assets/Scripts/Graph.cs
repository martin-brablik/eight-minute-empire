using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using UnityEngine;

public class Graph<T>
{
	public List<GraphNode<T>> Nodes { get; set; }
	public List<GraphLink<T>> Links { get; set; }

	public Graph(List<GraphNode<T>> nodes, List<GraphLink<T>> links)
	{
		Nodes = nodes;
		Links = links;
	}

	public List<GraphNode<T>> GetNeighboringNodes(GraphNode<T> node)
	{
		var validLinks = Links.Where(p => p.IsLinked(node)).ToList();
		var nodes = validLinks.Select(l => l.RetreiveNode(node)).ToList();
		return nodes;
	}
}

public class GraphNode<T>
{
	public static GraphNode<T> FindByName(List<GraphNode<T>> nodeCollection, string name) => nodeCollection.FirstOrDefault(n => n.Value.ToString().Equals(name));

	public T Value { get; set; }

	public GraphNode(T data)
	{
		Value = data;
	}
}

public class GraphLink<T>
{
	public static GraphLink<T> BuildLink(GraphLinkFoundation foundation, List<GraphNode<T>> nodeCollection, bool isOversea) => new GraphLink<T>(GraphNode<T>.FindByName(nodeCollection, foundation.Lands[0]), 
		GraphNode<T>.FindByName(nodeCollection, foundation.Lands[1]),
		isOversea
		);

	private string[] _nodeNames;
	private GraphNode<T>[] _nodes;

	public bool IsOversea { get; private set; } = false;

	public GraphNode<T> First
	{
		get => _nodes[0];
		set => _nodes[0] = value;
	}
	public GraphNode<T> Second
	{
		get => _nodes[1];
		set => _nodes[1] = value;
	}

	public GraphLink(GraphNode<T> first, GraphNode<T> second, bool isOversea)
	{
		_nodes = new GraphNode<T>[2];
		_nodes[0] = first;
		_nodes[1] = second;
		IsOversea= isOversea;
	}

	public GraphNode<T> this[int index]
	{
		get => index switch
		{
			0 => First,
			1 => Second,
			_ => throw new IndexOutOfRangeException()
		};
	}

	public bool IsLinked(GraphNode<T> node)
	{
		return _nodes.Contains(node);
	}

	public GraphNode<T> RetreiveNode(GraphNode<T> node) => _nodes.FirstOrDefault(n => n != node);
}

[JsonObject(MemberSerialization.Fields)]
public struct GraphLinkFoundation
{
	public string[] Lands;
	public bool IsOversea;
}