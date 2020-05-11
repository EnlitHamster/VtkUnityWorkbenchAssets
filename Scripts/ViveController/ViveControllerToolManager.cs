using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerToolManager : MonoBehaviour
{
	private ViveControllerToolBase[] _tools;
	
	private string _mode = "XX";
	private string _requestedMode = "XX";

	public string DefaultMode = "MO";

	void Awake()
	{
		_tools = GetComponents<ViveControllerToolBase>();

		// This is hard coded on the assumption we always have a move tool 
		// finding a more generic way of naming it would be good
		UpdateMode(DefaultMode);
	}

	public void OnTriggerEnter(Collider other)
	{
		var zoneTool = GetToolForZone(other.gameObject.layer);
		if (zoneTool &&
			!CurrentTool().Busy() &&
			!CurrentToolIsZoneTool())
		{
			_requestedMode = _mode;
			UpdateMode(zoneTool.Id);
		}
	}

	//public void OnTriggerStay(Collider other)
	//{
	//	if (_moveTool != null && _guiPresserTool != null)
	//	{

	//	}
	//}

	public void OnTriggerExit(Collider other)
	{
		if (CurrentToolIsZoneTool() &&
			LayerMask.LayerToName(other.gameObject.layer) == CurrentTool().Zone)
		{
			UpdateMode(_requestedMode);
		}
	}

	// Use this for initialization
	//void Start()
	//{

	//}

	//// Update is called once per frame
	//void Update () {

	//}

	//public void SelectMoveMode()
	//{
	//	if (!CurrentToolIsZoneTool())
	//	{
	//		UpdateMode("MO");
	//		return;
	//	}

	//	_requestedMode = "MO";
	//}

	public void SelectMode(string mode)
	{
		if (!CurrentToolIsZoneTool())
		{
			UpdateMode(mode);
			return;
		}

		_requestedMode = mode;
	}

	private void UpdateMode(string requestedMode)
	{
		if (_mode == requestedMode)
		{
			return;
		}

		foreach (var tool in _tools)
		{
			if (tool)
			{
				tool.DeActivate();
			}
		}

		{
			var tool = GetTool(requestedMode);
			if (!tool)
			{
				// fall back to move if line measurement is not available and asked for
				requestedMode = DefaultMode;
			}

			tool.Activate();

			_mode = requestedMode;
		}
	}

	private ViveControllerToolBase CurrentTool()
	{
		return GetTool(_mode);
	}

	private bool CurrentToolIsZoneTool()
	{
		return (!string.IsNullOrEmpty(CurrentTool().Zone));
	}

	private ViveControllerToolBase GetTool(string toolId)
	{
		return Array.Find(_tools, x => x.Id == toolId);
	}

	private ViveControllerToolBase GetToolForZone(int layer)
	{
		return Array.Find(_tools, x => x.Zone == LayerMask.LayerToName(layer));
	}
}
