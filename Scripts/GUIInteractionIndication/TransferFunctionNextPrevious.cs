using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using ThreeDeeHeartPlugins;


public class TransferFunctionNextPrevious : MonoBehaviour
{
	private int _iTransferFunction = 0;
	private int _nTransferFunctions = 1;

	public TransferFunctionCounterText CounterText;

	private void Start()
	{
		Debug.Log("TransferFunctionNextPrevious::Start");
		VtkToUnityPlugin.SetTransferFunctionIndex(_iTransferFunction);
		_nTransferFunctions = VtkToUnityPlugin.GetNTransferFunctions();

		if (!CounterText)
		{
			return;
		}

		CounterText.SetCount(_iTransferFunction, _nTransferFunctions);
	}

	public void OnNextTransferFunction()
	{
		if (_nTransferFunctions <= ++_iTransferFunction)
		{
			_iTransferFunction = 0;
		}

		VtkToUnityPlugin.SetTransferFunctionIndex(_iTransferFunction);

		if (!CounterText)
		{
			return;
		}

		CounterText.SetCount(_iTransferFunction, _nTransferFunctions);
	}

	public void OnPreviousTransferFunction()
	{
		if (0 > --_iTransferFunction)
		{
			_iTransferFunction = _nTransferFunctions - 1;
		}

		VtkToUnityPlugin.SetTransferFunctionIndex(_iTransferFunction);

		if (!CounterText)
		{
			return;
		}

		CounterText.SetCount(_iTransferFunction, _nTransferFunctions);
	}
}
