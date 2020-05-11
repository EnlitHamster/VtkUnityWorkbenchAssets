using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ViveControllerToolManager : MonoBehaviour
{
	private ViveControllerToolBase[] _tools; // = new ViveControllerToolBase[];
	//private List<ViveControllerToolBase> _tools; // = new List<ViveControllerToolBase>();

	//private enum Mode
	//{
	//	Initial = -1,
	//	GuiPresser,
	//	Move,
	//	NModes
	//}

	//private Mode _mode = Mode.Initial;
	//private Mode _requestedMode = Mode.Initial;

	private string _mode = "XX";
	private string _requestedMode = "XX";

	public string DefaultMode = "MO";
	//private List<string> _zoneModes = new List<string>();

	void Awake()
	{
		//_tools = new ViveControllerToolBase[(int)Mode.NModes];
		//_tools[(int)Mode.GuiPresser] = GetComponent<ViveControllerToolGuiPresser>();
		//_tools[(int)Mode.Move] = GetComponent<ViveControllerToolMove>();
		_tools = GetComponents<ViveControllerToolBase>();

		//var tool = Array.Find(_tools, x => x.Id == "xy");

		// get a list of the zoned tools for easier searching
		// (this may turn out to be completely unnecessary)
		//foreach (var tool in _tools)
		//{

		//}

		// This is hard coded on the assumption we always have a move tool 
		// finding a more generic way of naming it would be good too
		UpdateMode(DefaultMode);
	}

	public void OnTriggerEnter(Collider other)
	{
		//if (other.gameObject.layer == LayerMask.NameToLayer("SwitchToUI") && 
		//	!_tools[(int)_mode].Busy() &&
		//          _mode != Mode.GuiPresser)
		//{
		//	_requestedMode = _mode;
		//	UpdateMode(Mode.GuiPresser);
		//}

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
  //      //if (_moveTool != null && _guiPresserTool != null)
  //      {
		//	if (other.gameObject.layer == LayerMask.NameToLayer("SwitchToUI"))
		//	{
		//		UpdateMode(_requestedMode);
		//	}
		//}

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

		//if (!_tools[(int)requestedMode])
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
		return (!CurrentTool().Zone.IsNullOrEmpty());
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
