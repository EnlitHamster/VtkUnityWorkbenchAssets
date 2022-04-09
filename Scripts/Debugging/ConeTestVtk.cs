using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

using ThreeDeeHeartPlugins;
using VtkUnityWorkbench;
using System.Diagnostics;
using System.IO;

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

    struct Preset
    {
        public float Height;
        public float Radius;
        public int Resolution;
    }

    private bool useDefault = true;
    private Preset defaultPreset = new Preset();
    private Preset transformedPreset = new Preset();

    IEnumerator TransformCone()
    {
        for(;;)
        {

            foreach (var idPosition in _shapeIdPositions)
            {
                Preset preset = useDefault ? defaultPreset : transformedPreset;
                int id = idPosition.Id;

                time("set_height", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Height", "f", preset.Height.ToString()));
                time("set_radius", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Radius", "f", preset.Radius.ToString()));
                time("set_resolution", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Resolution", "d", preset.Resolution.ToString()));

            }
            useDefault = !useDefault;
            yield return new WaitForSeconds(2.0f);
        }
    }

    float avgFramerate, timer, refresh;
    StringBuilder fpsBuffer;

    private static StringBuilder perfBuffer = new StringBuilder();

    private static T time<T>(string name, Func<T> f)
    {
        Stopwatch sw = Stopwatch.StartNew();
        T r = f();
        sw.Stop();
        double ns = ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
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

    private List<IdPosition> _shapeIdPositions = new List<IdPosition>();

    // Start is called before the first frame update
    void Start()
    {
        // Create cone value presets
        defaultPreset.Height = 0.1f;
        defaultPreset.Radius = 0.1f;
        defaultPreset.Resolution = 200;

        transformedPreset.Height = 0.2f;
        transformedPreset.Radius = 0.05f;
        transformedPreset.Resolution = 500;

        fpsBuffer = new StringBuilder();

        // Setting correct localization
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
		customCulture.NumberFormat.NumberDecimalSeparator = ".";
		System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
		
        Vector3 scale = new Vector3(ConeScale, ConeScale, ConeScale);
        Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Matrix4x4 transform = Matrix4x4.identity;

        VtkToUnityPlugin.Float4 colour = new VtkToUnityPlugin.Float4();
        colour.SetXYZW(0.0f, 0.0f, 1.0f, 1.0f);

        Profiler.logFile = "coneTestWithTransforms";
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;

        Profiler.BeginSample("Cone Source Creation");

        int id = time("cone_inst", () => VtkToUnityPlugin.VtkResource_CallObject("vtkConeSource"));

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

        time("set_height", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Height", "f", defaultPreset.Height.ToString()));
        time("set_radius", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Radius", "f", defaultPreset.Radius.ToString()));
        time("set_resolution", () => VtkUnityWorkbenchPlugin.SetProperty(id, "Resolution", "d", defaultPreset.Resolution.ToString()));

        Profiler.EndSample();

		VtkToUnityPlugin.VtkResource_AddActor(id, colour, false);

        StartCoroutine("TransformCone");
    }

    void OnDestroy()
    {
        UnityEngine.Debug.Log("ConeTestVtk::OnDestroy");
        foreach (var idPosition in _shapeIdPositions)
        {
            VtkToUnityPlugin.RemoveProp3D(idPosition.Id);
        }
        _shapeIdPositions.Clear();

        File.WriteAllLines("fpsConeWithTransforms.csv", fpsBuffer.ToString().Split(';'));
        File.WriteAllLines("perffpsConeWithTransforms.csv", perfBuffer.ToString().Split(';'));

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

        // Updating FPS
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if (timer <= 0) avgFramerate = (int)(1f / timelapse);
        fpsBuffer.Append(string.Format("{0},{1},{2},{3};", avgFramerate.ToString(), timer.ToString(), refresh.ToString(), timelapse.ToString()));
    }
}
