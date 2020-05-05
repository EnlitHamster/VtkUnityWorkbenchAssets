using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerToolManager : MonoBehaviour {

	private ViveControllerToolBase[] _tools;

	private enum Mode
	{
		Initial = -1,
		GuiPresser,
		Move,
		NModes
	}

	private Mode _mode = Mode.Initial;
	private Mode _requestedMode = Mode.Initial;

	void Awake()
	{
		_tools = new ViveControllerToolBase[(int)Mode.NModes];
		_tools[(int)Mode.GuiPresser] = GetComponent<ViveControllerToolGuiPresser>();
		_tools[(int)Mode.Move] = GetComponent<ViveControllerToolMove>();

		UpdateMode(Mode.Move);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("SwitchToUI") && 
			!_tools[(int)_mode].Busy() &&
            _mode != Mode.GuiPresser)
		{
			_requestedMode = _mode;
			UpdateMode(Mode.GuiPresser);
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
        //if (_moveTool != null && _guiPresserTool != null)
        {
			if (other.gameObject.layer == LayerMask.NameToLayer("SwitchToUI"))
			{
				UpdateMode(_requestedMode);
			}
		}
	}

	// Use this for initialization
	//void Start()
	//{

	//}

	//// Update is called once per frame
	//void Update () {

	//}

	public void SelectMoveMode()
	{
		if (_mode != Mode.GuiPresser)
		{
			UpdateMode(Mode.Move);
			return;
		}

		_requestedMode = Mode.Move;
	}

	private void UpdateMode(Mode requestedMode)
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

		if (!_tools[(int)requestedMode])
		{
			// fall back to move if line measurement is not available and asked for
			requestedMode = Mode.Move;
		}

		_tools[(int)requestedMode].Activate();

		_mode = requestedMode;
	}
}
