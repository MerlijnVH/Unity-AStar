using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldNode : MonoBehaviour
{
	public Node Node;

	public TextMesh TextF;
	public TextMesh TextG;

	private void Awake()
	{
		TextF = gameObject.transform.FindChild("F").GetComponent<TextMesh>();
		TextG = gameObject.transform.FindChild("G").GetComponent<TextMesh>();
	}
}

public class AStar : MonoBehaviour
{
	public int MapWidth = 16;
	public int MapHeight = 16;

	public float CostStraight = 1f;
	public float CostDiagonal = 1.414f;

	public bool IsAllowDiagonal;
	public bool DontCrossCorners;

	public GameObject PrefabNode;

	public Node[,] Nodes;

	public Node NodeStart;
	public Node NodeGoal;

	public void Initialize()
	{
		Nodes = new Node[MapWidth, MapHeight];

		ResetMap();

		NodeStart = Nodes[0, 0];
		NodeGoal = Nodes[MapWidth - 1, MapHeight - 1];
	}

	public bool FindPath(Node startNode, Node endNode)
	{
		// Keep a list of nodes that are marked as dirty.

		List<Node> dirtyNodes = new List<Node>();

		// Keep a list of open nodes to process.

		PriorityQueue<float, Node> openNodes = new PriorityQueue<float, Node>();

		// Add the start node to the open set.

		openNodes.Enqueue(startNode.F, startNode);

		// Set the parameters of the start node.

		startNode.G = 0;
		startNode.F = startNode.G + Heuristic(startNode, endNode);
		startNode.IsOpened = true;

		while (!openNodes.IsEmpty)
		{
			// Select the node with the lowest F score from the open set.

			Node current = openNodes.Dequeue();
			current.IsClosed = true;

			// Mark the current node as dirty.

			dirtyNodes.Add(current);

			// If the current node equals the end node, we've found our path.

			if (IsNodeMatch(current, endNode))
			{
				while (current != null)
				{
					// Mark the node as part of the optimal path.

					current.IsOptimal = true;
					current = current.Parent;
				}

				return true;
			}

			// Fetch and iterate over neighbors.

			List<Node> neighbors = FindNeighborNodes(current, IsAllowDiagonal, DontCrossCorners);

			for (int i = 0; i < neighbors.Count; i++)
			{
				Node neighbor = neighbors[i];

				if (neighbor.IsClosed)
				{
					continue;
				}

				float gCost = 0;

				if (neighbor.X == current.X || neighbor.Y == current.Y)
				{
					gCost = current.G + 1;
				}
				else
				{
					gCost = current.G + Mathf.Sqrt(2);
				}

				if (!neighbor.IsOpened || gCost < neighbor.G)
				{
					neighbor.Parent = current;

					neighbor.G = gCost;

					float oldF = neighbor.F;
					neighbor.F = neighbor.Weight + gCost + Heuristic(neighbor, endNode);

					if (!neighbor.IsOpened)
					{
						openNodes.Enqueue(neighbor.F, neighbor);

						neighbor.IsOpened = true;
					}
					else
					{
						openNodes.Replace(oldF, neighbor.F, neighbor);
					}
				}

				dirtyNodes.Add(neighbor);
			}
		}

		return false;
	}

	private void Clean(List<Node> dirtyNodes)
	{
		for (int i = 0; i < dirtyNodes.Count; i++)
		{
			Node node = dirtyNodes[i];

			node.F = 0;
			node.G = 0;

			node.IsOpened = false;
			node.IsClosed = false;
			node.IsOptimal = false;

			node.Parent = null;

			Nodes[node.X, node.Y] = node;
		}
	}

	public void ResetMap()
	{
		for (int x = 0; x < MapWidth; x++)
		{
			for (int y = 0; y < MapHeight; y++)
			{
				Node node = new Node(x, y);

				if (Nodes[x, y] != null)
				{
					node.IsWalkable = Nodes[x, y].IsWalkable;
					node.Weight = Nodes[x, y].Weight;
					node.Graphics = Nodes[x, y].Graphics;
				}
				else
				{
					node.IsWalkable = true;
					node.Weight = 0;

					GameObject graphics = (GameObject)GameObject.Instantiate(PrefabNode);
					graphics.transform.position = new Vector3(x, 0, y);
					node.Graphics = graphics;
				}

				if (node.IsWalkable)
				{
					node.Graphics.renderer.material.color = Color.white;
				}

				Nodes[x, y] = node;
			}
		}

		if (NodeStart != null && NodeGoal != null)
		{
			NodeStart = Nodes[NodeStart.X, NodeStart.Y];
			NodeGoal = Nodes[NodeGoal.X, NodeGoal.Y];
		}
	}

	#region Drawing

	public void DrawMap(Node[,] nodes)
	{
		for (int x = 0; x < MapWidth; x++)
		{
			for (int y = 0; y < MapHeight; y++)
			{
				Node node = Nodes[x, y];

				if (node.IsOpened)
				{
					DrawNode(x, y, new Color(60f / 255f, 60f / 255f, 60f / 255f));
				}

				if (node.IsClosed)
				{
					DrawNode(x, y, new Color(40f / 255f, 40f / 255f, 40f / 255f));
				}

				if (node.Weight > 0)
				{
					DrawNode(x, y, new Color(172f / 255f, 96f / 255f, 62f / 255f));
				}

				if (node.IsOptimal)
				{
					DrawNode(x, y, new Color(255f / 255f, 160f / 255f, 40f / 255f));
				}

				if (!node.Graphics.GetComponent<WorldNode>())
				{
					node.Graphics.AddComponent<WorldNode>();
				}

				WorldNode worldNode = node.Graphics.GetComponent<WorldNode>();
				worldNode.Node = node;

				string numberG = string.Format("{0}", System.Math.Round(node.G, 2).ToString());
				worldNode.TextG.text = "G: " + numberG;

				string numberF = string.Format("{0}", System.Math.Round(node.F, 2).ToString());
				worldNode.TextF.text = "F: " + numberF;
			}
		}

		DrawNode(NodeStart.X, NodeStart.Y, new Color(200f / 255f, 60f / 255f, 40f / 255f));
		DrawNode(NodeGoal.X, NodeGoal.Y, new Color(160f / 255f, 200f / 255f, 80f / 255f));
	}

	private void DrawNode(int x, int y, Color color)
	{
		if (Nodes[x, y].Graphics != null)
		{
			Nodes[x, y].Graphics.renderer.material.color = color;
		}
	}

	#endregion

	#region Heuristics

	private float Heuristic(Node node, Node otherNode)
	{
		return ManhattanDistance(node, otherNode);
	}

	private float ManhattanDistance(Node node, Node goal)
	{
		int dx = Mathf.Abs(node.X - goal.X);
		int dy = Mathf.Abs(node.Y - goal.Y);

		float result = dx + dy;

		return result;
	}

	private float DiagonalDistance(Node node, Node goal)
	{
		int dx = Mathf.Abs(node.X - goal.X);
		int dy = Mathf.Abs(node.Y - goal.Y);

		float result = Mathf.Max(dx, dy);

		return result;
	}

	private float EuclideanDistance(Node node, Node goal)
	{
		int dx = Mathf.Abs(node.X - goal.X);
		int dy = Mathf.Abs(node.Y - goal.Y);

		// Use square root to avoid scale problem.

		float result = Mathf.Sqrt(dx * dx + dy * dy);

		return result;
	}

	#endregion

	#region Helpers

	private List<Node> FindNeighborNodes(Node node, bool allowDiagonal, bool dontCrossCorners)
	{
		List<Node> neighbors = new List<Node>();

		bool s0 = false;
		bool s1 = false;
		bool s2 = false;
		bool s3 = false;

		bool d0 = false;
		bool d1 = false;
		bool d2 = false;
		bool d3 = false;

		// ↑

		if (IsWalkable(node.X, node.Y - 1))
		{
			neighbors.Add(Nodes[node.X, node.Y - 1]);
			s0 = true;
		}

		// →

		if (IsWalkable(node.X + 1, node.Y))
		{
			neighbors.Add(Nodes[node.X + 1, node.Y]);
			s1 = true;
		}

		// ↓

		if (IsWalkable(node.X, node.Y + 1))
		{
			neighbors.Add(Nodes[node.X, node.Y + 1]);
			s2 = true;
		}

		// ←

		if (IsWalkable(node.X - 1, node.Y))
		{
			neighbors.Add(Nodes[node.X - 1, node.Y]);
			s3 = true;
		}

		if (!allowDiagonal)
		{
			return neighbors;
		}

		if (dontCrossCorners)
		{
			d0 = s3 && s0;
			d1 = s0 && s1;
			d2 = s1 && s2;
			d3 = s2 && s3;
		}
		else
		{
			d0 = s3 || s0;
			d1 = s0 || s1;
			d2 = s1 || s2;
			d3 = s2 || s3;
		}

		// ↖

		if (d0 && IsWalkable(node.X - 1, node.Y - 1))
		{
			neighbors.Add(Nodes[node.X - 1, node.Y - 1]);
		}

		// ↗

		if (d1 && IsWalkable(node.X + 1, node.Y - 1))
		{
			neighbors.Add(Nodes[node.X + 1, node.Y - 1]);
		}

		// ↘

		if (d2 && IsWalkable(node.X + 1, node.Y + 1))
		{
			neighbors.Add(Nodes[node.X + 1, node.Y + 1]);
		}

		// ↙

		if (d3 && IsWalkable(node.X - 1, node.Y + 1))
		{
			neighbors.Add(Nodes[node.X - 1, node.Y + 1]);
		}

		return neighbors;
	}

	private bool IsWalkable(int x, int y)
	{
		if (IsWithinMap(x, y))
		{
			if (Nodes[x, y].IsWalkable)
			{
				return true;
			}
		}

		return false;
	}

	private bool IsWithinMap(int x, int y)
	{
		return x >= 0 && x < MapWidth && y >= 0 && y < MapHeight;
	}

	private bool IsNodeMatch(Node a, Node b)
	{
		return a.X == b.X && a.Y == b.Y;
	}

	#endregion
}
