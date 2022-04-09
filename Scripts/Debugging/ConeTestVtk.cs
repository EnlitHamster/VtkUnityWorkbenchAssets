﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

using ThreeDeeHeartPlugins;
using VtkUnityWorkbench;

class VtkConeSourceUIFactory : IComponentFactory
{
    public void Destroy()
    {
        // TODO: implement UI hiding routine



        throw new NotImplementedException();
    }

    public void Show()
    {
        // TODO: implement UI showing routine



        throw new NotImplementedException();
    }
}

public class ConeTestVtk : MonoBehaviour
{
    [Range(0.001f, 1.0f)]
    public float ConeScale = 0.2f;

    struct IdPosition
    {
        public int Id;
        public VtkToUnityPlugin.Float4 PositionScale;
    }

    private List<IdPosition> _shapeIdPositions = new List<IdPosition>();

    // Start is called before the first frame update
    void Start()
    {
		// Setting correct localization
		System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
		customCulture.NumberFormat.NumberDecimalSeparator = ".";
		System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
		
        Vector3 scale = new Vector3(ConeScale, ConeScale, ConeScale);
        Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Matrix4x4 transform = Matrix4x4.identity;

        VtkToUnityPlugin.Float4 colour = new VtkToUnityPlugin.Float4();
        colour.SetXYZW(0.0f, 0.0f, 1.0f, 1.0f);

        Profiler.logFile = "coneTest";
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;

        Profiler.BeginSample("Cone Source Creation");

        int id = VtkToUnityPlugin.VtkResource_CallObject("vtkConeSource");

        IdPosition idPosition = new IdPosition();
        idPosition.Id = id;
        idPosition.PositionScale.x = 0.0f;
        idPosition.PositionScale.y = 0.0f;
        idPosition.PositionScale.z = 0.0f;
        idPosition.PositionScale.w = ConeScale;

        Vector3 translation = new Vector3(0.0f, 0.0f, 0.0f);
        transform.SetTRS(translation, rotation, scale);

        VtkToUnityPlugin.Float16 transformArray = VtkToUnityPlugin.UnityMatrix4x4ToFloat16(transform);
        VtkToUnityPlugin.SetProp3DTransform(id, transformArray);

        _shapeIdPositions.Add(idPosition);

        VtkUnityWorkbenchPlugin.SetProperty(id, "Height", "f", 0.1f.ToString());
        VtkUnityWorkbenchPlugin.SetProperty(id, "Radius", "f", 0.1f.ToString());
        VtkUnityWorkbenchPlugin.SetProperty(id, "Resolution", "d", 200.ToString());

        Profiler.EndSample();

		VtkToUnityPlugin.VtkResource_AddActor(id, colour, false);

        //var coneHeight = VtkUnityWorkbenchPlugin.GetProperty<double>(id, "Height");
        //var coneRadius = VtkUnityWorkbenchPlugin.GetProperty<double>(id, "Radius");
        //var coneResolution = VtkUnityWorkbenchPlugin.GetProperty<int>(id, "Resolution");
        //var coneAngle = VtkUnityWorkbenchPlugin.GetProperty<double>(id, "Angle");
        //var coneCapping = VtkUnityWorkbenchPlugin.GetProperty<int>(id, "Capping");
        //var coneCenter = VtkUnityWorkbenchPlugin.GetProperty<Double3>(id, "Center");
        //var coneDirection = VtkUnityWorkbenchPlugin.GetProperty<Double3>(id, "Direction");

        //var descriptor = VtkUnityWorkbenchPlugin.GetDescriptor(id);

        //var coneFactory = new VtkConeSourceUIFactory();
        //VtkUnityWorkbenchPlugin.RegisterComponentFactory("vtkConeSource", coneFactory);

        //VtkUnityWorkbenchPlugin.ShowComponentFor("vtkConeSource");
    }

    void OnDestroy()
    {
        Debug.Log("ConeTestVtk::OnDestroy");
        foreach (var idPosition in _shapeIdPositions)
        {
            VtkToUnityPlugin.RemoveProp3D(idPosition.Id);
        }

        VtkUnityWorkbenchPlugin.DestroyComponentFor("vtkConeSource");
        Profiler.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion shapeRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Matrix4x4 transformMatrix = Matrix4x4.identity;

        Matrix4x4 parentTransformMatrix = transform.localToWorldMatrix;

        foreach (var idPosition in _shapeIdPositions)
        {
            Profiler.BeginSample("Cone Source Update");
            Vector3 scale = new Vector3(
                idPosition.PositionScale.w,
                idPosition.PositionScale.w,
                idPosition.PositionScale.w
            );

            Vector3 translation = new Vector3(
                idPosition.PositionScale.x,
                idPosition.PositionScale.y,
                idPosition.PositionScale.z
            );

            transformMatrix.SetTRS(translation, shapeRotation, scale);
            transformMatrix = parentTransformMatrix * transformMatrix;

            VtkToUnityPlugin.Float16 transformArray = VtkToUnityPlugin.UnityMatrix4x4ToFloat16(transformMatrix);
            VtkToUnityPlugin.SetProp3DTransform(idPosition.Id, transformArray);
            Profiler.EndSample();
        }
    }
}
