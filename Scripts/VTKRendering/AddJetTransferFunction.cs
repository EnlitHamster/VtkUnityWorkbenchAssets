using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeDeeHeartPlugins;

public class AddJetTransferFunction : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        var transferFunctionId = VtkToUnityPlugin.AddTransferFunction();

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            -0.5, 0.0, 0.0, 0.56, 0.0);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            -0.39, 0.0, 0.0, 1.0, 0.11);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            -0.14, 0.0, 1.0, 1.0, 0.36);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            0.0, 0.5, 1.0, 0.5, 0.5);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            0.12, 1.0, 1.0, 0.0, 0.62);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            0.37, 1.0, 0.0, 0.0, 0.87);

        VtkToUnityPlugin.SetTransferFunctionPoint(
            transferFunctionId,
            0.5, 0.5, 0.0, 0.0, 1.0);

        DontDestroyOnLoad(this.gameObject);
    }

    void OnApplicationQuit()
    {
        VtkToUnityPlugin.ResetTransferFunctions();
    }
}
