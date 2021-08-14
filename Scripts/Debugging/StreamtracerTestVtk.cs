using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

using ThreeDeeHeartPlugins;
using VtkUnityWorkbench;

class StreamTracerUIFractory : IComponentFactory
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

public class StreamtracerTestVtk : MonoBehaviour
{
	[Range(0.001f, 1.0f)]
	public float Scale = 0.2f;

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

		Vector3 scale = new Vector3(Scale, Scale, Scale);
		Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		Matrix4x4 transform = Matrix4x4.identity;

		VtkToUnityPlugin.Float4 colour = new VtkToUnityPlugin.Float4();
		colour.SetXYZW(0.0f, 0.0f, 1.0f, 1.0f);

		int reader = VtkToUnityPlugin.VtkResource_CallObject("vtkStructuredGridReader");
		int seeds = VtkToUnityPlugin.VtkResource_CallObject("vtkPointSource");
		int streamer = VtkToUnityPlugin.VtkResource_CallObject("vtkStreamTracer");
		int outline = VtkToUnityPlugin.VtkResource_CallObject("vtkStructuredGridOutlineFilter");

		IdPosition
			streamerPosition = new IdPosition(),
			outlinePosition = new IdPosition();

		streamerPosition.Id = streamer;
		streamerPosition.PositionScale.x = 0.0f;
		streamerPosition.PositionScale.y = 0.0f;
		streamerPosition.PositionScale.z = 0.0f;
		streamerPosition.PositionScale.w = Scale;

		outlinePosition.Id = outline;
		outlinePosition.PositionScale.x = 0.0f;
		outlinePosition.PositionScale.y = 0.0f;
		outlinePosition.PositionScale.z = 0.0f;
		outlinePosition.PositionScale.w = Scale;

		Vector3 translation = new Vector3(0.0f, 0.0f, 0.0f);
		transform.SetTRS(translation, rotation, scale);

		VtkToUnityPlugin.Float16 transformArray = VtkToUnityPlugin.UnityMatrix4x4ToFloat16(transform);
		VtkToUnityPlugin.SetProp3DTransform(streamer, transformArray);
		VtkToUnityPlugin.SetProp3DTransform(outline, transformArray);

		_shapeIdPositions.Add(streamerPosition);
		_shapeIdPositions.Add(outlinePosition);

		VtkToUnityPlugin.VtkResource_SetAttrFromString(reader, "FileName", "s", "Data/density.vtk");
		VtkToUnityPlugin.VtkResource_CallMethodAsVoid(reader, "Update", "", IntPtr.Zero);

		string center = VtkToUnityPlugin.VtkResource_CallMethodPipedAsString(
			reader,
			2,
			2,
			VtkUnityWorkbenchHelpers.MarshalStringArray(new string[] { "GetOutput", "GetCenter" }),
			VtkUnityWorkbenchHelpers.MarshalStringArray(new string[] { "", "" }),
			IntPtr.Zero);

		VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "Radius", "f", 3.0f.ToString());
		VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "Center", "f3", center);
		VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "NumberOfPoints", "d", 100.ToString());

		VtkToUnityPlugin.VtkResource_Connect("Input", reader, streamer);
		VtkToUnityPlugin.VtkResource_Connect("Source", seeds, streamer);
		VtkToUnityPlugin.VtkResource_SetAttrFromString(streamer, "MaximumPropagation", "d", 100.ToString());
		VtkToUnityPlugin.VtkResource_SetAttrFromString(streamer, "InitialIntegrationStep", "f", 0.1f.ToString());
		VtkToUnityPlugin.VtkResource_CallMethodAsVoid(streamer, "SetIntegrationDirectionToBoth", "", IntPtr.Zero);

		VtkToUnityPlugin.VtkResource_Connect("Input", reader, outline);

		VtkToUnityPlugin.VtkResource_AddActor(streamer, colour, false);
		VtkToUnityPlugin.VtkResource_AddActor(outline, colour, false);
	}

	void OnDestroy()
	{
		Debug.Log("ConeTestVtk::OnDestroy");
		foreach (var idPosition in _shapeIdPositions)
		{
			VtkToUnityPlugin.RemoveProp3D(idPosition.Id);
		}
	}

	// Update is called once per frame
	void Update()
	{
		Quaternion shapeRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		Matrix4x4 transformMatrix = Matrix4x4.identity;

		Matrix4x4 parentTransformMatrix = transform.localToWorldMatrix;

		foreach (var idPosition in _shapeIdPositions)
		{
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
		}
	}
}
