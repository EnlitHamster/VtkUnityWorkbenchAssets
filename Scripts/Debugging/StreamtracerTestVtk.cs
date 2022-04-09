using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

using ThreeDeeHeartPlugins;
using VtkUnityWorkbench;
using System.Diagnostics;

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

    [Range(10, 1000)]
    public int nPoints = 100;

    float avgFramerate, timer, refresh;
    StringBuilder fpsBuffer;

    private static StringBuilder perfBuffer = new StringBuilder();

    private static T time<T>(string name, Func<T> f)
    {
        Stopwatch sw = Stopwatch.StartNew();
        T r = f();
        sw.Stop();
        double ns = ((double) sw.ElapsedTicks / (double) Stopwatch.Frequency);
        perfBuffer.Append(string.Format("{0},{1};", name, ns.ToString()));
        return r;
    }

    private static void time(string name, Action a)
    {
        Stopwatch sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        double ns = ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
        perfBuffer.Append(string.Format("{0},{1};", name, ns.ToString()));
    }
    

	struct IdPosition
	{
		public int Id;
		public VtkToUnityPlugin.Float4 PositionScale;
	}

	private List<IdPosition> _shapeIdPositions = new List<IdPosition>();

	// Start is called before the first frame update
	void Start()
	{
        fpsBuffer = new StringBuilder();

		// Setting correct localization
		System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
		customCulture.NumberFormat.NumberDecimalSeparator = ".";
		System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

		Vector3 scale = new Vector3(Scale, Scale, Scale);
		Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		Matrix4x4 transform = Matrix4x4.identity;

		VtkToUnityPlugin.Float4 colour = new VtkToUnityPlugin.Float4();
		colour.SetXYZW(0.0f, 0.0f, 1.0f, 1.0f);

        Profiler.logFile = "streamTracer_VR";
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;

        Profiler.BeginSample("Cone Source Creation");

        int reader = time("reader_inst", () => VtkToUnityPlugin.VtkResource_CallObject("vtkStructuredGridReader"));
		int seeds = time("seeds_inst", () => VtkToUnityPlugin.VtkResource_CallObject("vtkPointSource"));
		int streamer = time("streamer_inst", () => VtkToUnityPlugin.VtkResource_CallObject("vtkStreamTracer"));
		int outline = time("outline_inst", () => VtkToUnityPlugin.VtkResource_CallObject("vtkStructuredGridOutlineFilter"));

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

		time("reader_setfile", () => VtkToUnityPlugin.VtkResource_SetAttrFromString(reader, "FileName", "s", "Data/density.vtk"));
		time("reader_update", () => VtkToUnityPlugin.VtkResource_CallMethodAsVoid(reader, "Update", "", IntPtr.Zero));

		string center = time("reader_getcenter", () => VtkToUnityPlugin.VtkResource_CallMethodPipedAsString(
			reader,
			2,
			2,
			VtkUnityWorkbenchHelpers.MarshalStringArray(new string[] { "GetOutput", "GetCenter" }),
			VtkUnityWorkbenchHelpers.MarshalStringArray(new string[] { "", "" }),
			IntPtr.Zero));

		time("seeds_setradius", () =>VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "Radius", "f", 3.0f.ToString()));
		time("seeds_setcenter", () =>VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "Center", "f3", center));
		time("seeds_setnumberofpoints", () =>VtkToUnityPlugin.VtkResource_SetAttrFromString(seeds, "NumberOfPoints", "d", nPoints.ToString()));

		time("streamer_reader_connect", () => VtkToUnityPlugin.VtkResource_Connect("Input", reader, streamer));
		time("streamer_seeds_connect", () => VtkToUnityPlugin.VtkResource_Connect("Source", seeds, streamer));
		time("streamer_setmaximumpropagation", () => VtkToUnityPlugin.VtkResource_SetAttrFromString(streamer, "MaximumPropagation", "d", 100.ToString()));
		time("streamer_setinitialintegrationstep", () => VtkToUnityPlugin.VtkResource_SetAttrFromString(streamer, "InitialIntegrationStep", "f", 0.1f.ToString()));
		time("streamer_setinitegrationdirection", () => VtkToUnityPlugin.VtkResource_CallMethodAsVoid(streamer, "SetIntegrationDirectionToBoth", "", IntPtr.Zero));

		time("outline_reader_connect", () => VtkToUnityPlugin.VtkResource_Connect("Input", reader, outline));

		time("streamer_actor", () => VtkToUnityPlugin.VtkResource_AddActor(streamer, colour, false));
		time("outline_actor", () => VtkToUnityPlugin.VtkResource_AddActor(outline, colour, false));

        Profiler.EndSample();
    }

	void OnDestroy()
	{
        UnityEngine.Debug.Log("ConeTestVtk::OnDestroy");
		foreach (var idPosition in _shapeIdPositions)
		{
			VtkToUnityPlugin.RemoveProp3D(idPosition.Id);
		}

        File.WriteAllLines("fps.csv", fpsBuffer.ToString().Split(';'));
        File.WriteAllLines("perf.csv", perfBuffer.ToString().Split(';'));
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
            time("update", () =>
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
            });
            Profiler.EndSample();
        }

        // Updating FPS
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if (timer <= 0) avgFramerate = (int)(1f / timelapse);
        fpsBuffer.Append(string.Format("{0},{1},{2},{3};", avgFramerate.ToString(), timer.ToString(), refresh.ToString(), timelapse.ToString()));
	}
}
