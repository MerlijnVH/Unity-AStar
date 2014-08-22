using UnityEngine;
using System.Collections;

public class Node
{
	public int X;
	public int Y;

	public float F;
	public float G;

	public float Weight;

	public Node Parent;

	public bool IsWalkable;

	public bool IsOpened;
	public bool IsClosed;

	public bool IsOptimal;

	public GameObject Graphics;

	public Node(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}
}
