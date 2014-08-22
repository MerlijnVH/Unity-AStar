using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private AStar _aStar;

	private float stopWatchTime;

	public enum Edit_T
	{
		EDIT_NONE,
		EDIT_START,
		EDIT_GOAL
	}

	public Edit_T EditType;

	private void Awake()
	{
		_aStar = gameObject.GetComponent<AStar>();
		_aStar.Initialize();
	}

	private void Update()
	{
		UpdateCamera();
		UpdateInput();
	}

	private void OnGUI()
	{
		if (_aStar == null)
		{
			return;
		}

		float positionX = 32f;
		float positionY = 32f;
		float heightLabel = 24f;
		float heightButton = 32f;

		GUI.Label(new Rect(positionX, positionY, 400, heightLabel), string.Format("Generated in {0} ms.", stopWatchTime));

		positionY += heightLabel;

		GUI.Label(new Rect(positionX, positionY, 400, heightLabel), string.Format("Map With {0} - Map Height {1}", _aStar.MapWidth, _aStar.MapHeight));

		positionY += heightLabel;

		if (GUI.Button(new Rect(positionX, positionY, 128f, heightButton), "Set Start"))
		{
			EditType = Edit_T.EDIT_START;
		}

		positionY += heightButton;

		if (GUI.Button(new Rect(positionX, positionY, 128f, heightButton), "Set Goal"))
		{
			EditType = Edit_T.EDIT_GOAL;
		}
	}

	private void UpdateCamera()
	{
		float xAxis = Input.GetAxis("Horizontal");
		float yAxis = Input.GetAxis("Vertical");
		float cameraSpeed = 16f;

		Camera.main.transform.position += new Vector3(xAxis * cameraSpeed * Time.deltaTime, 0, yAxis * cameraSpeed * Time.deltaTime);
	}

	private void UpdateInput()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			_aStar.ResetMap();

			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

			stopWatch.Start();

			if (_aStar.FindPath(_aStar.NodeStart, _aStar.NodeGoal))
			{
				_aStar.DrawMap(_aStar.Nodes);
			}

			_aStar.DrawMap(_aStar.Nodes);

			stopWatch.Stop();

			stopWatchTime = stopWatch.ElapsedMilliseconds;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			int x = (int)hit.transform.position.x;
			int y = (int)hit.transform.position.z;

			if (Input.GetMouseButtonDown(0))
			{
				if (EditType == Edit_T.EDIT_NONE)
				{
					_aStar.Nodes[x, y].IsWalkable = !_aStar.Nodes[x, y].IsWalkable;

					if (_aStar.Nodes[x, y].IsWalkable)
					{
						hit.transform.gameObject.renderer.material.color = Color.white;
					}
					else
					{
						hit.transform.gameObject.renderer.material.color = Color.black;
					}
				}

				if (EditType == Edit_T.EDIT_START)
				{
					_aStar.NodeStart = _aStar.Nodes[x, y];

					EditType = Edit_T.EDIT_NONE;
				}

				if (EditType == Edit_T.EDIT_GOAL)
				{
					_aStar.NodeGoal = _aStar.Nodes[x, y];

					EditType = Edit_T.EDIT_NONE;
				}
			}

			if (Input.GetMouseButtonDown(1))
			{
				hit.transform.gameObject.renderer.material.color = Color.magenta;

				if (_aStar.Nodes[x, y].Weight <= 0)
				{
					_aStar.Nodes[x, y].Weight = 16f;
				}
				else
				{
					_aStar.Nodes[x, y].Weight = 0f;
				}
			}
		}
	}
}
