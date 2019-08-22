﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;
using Stahle.Utility;

public class DebugRenderer : Singleton<DebugRenderer>
{
    Device device;
    BodyTracker tracker;
    Skeleton skeleton;
    GameObject[] debugObjects;
    public Renderer renderer;

    List<Skeleton> skeletons = new List<Skeleton>();

    private void OnEnable()
    {
        this.device = Device.Open(0);
        var config = new DeviceConfiguration
        {
            ColorResolution = ColorResolution.r720p,
            ColorFormat = ImageFormat.ColorBGRA32,
            DepthMode = DepthMode.NFOV_Unbinned
        };
        device.StartCameras(config);

        var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);
        this.tracker = BodyTracker.Create(calibration);
        debugObjects = new GameObject[(int)JointId.Count];

        for (var i = 0; i < (int)JointId.Count; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = Enum.GetName(typeof(JointId), i);
            cube.transform.localScale = Vector3.one * 0.4f;
            debugObjects[i] = cube;
        }
    }

    private void OnDisable()
    {
		Debug.Log("Disabling");
        if (tracker != null)
        {
            tracker.Dispose();
        }
        if (device != null)
        {
            device.Dispose();
        }
    }

    void Update()
    {
        using (Capture capture = device.GetCapture())
        {
            tracker.EnqueueCapture(capture);
            var color = capture.Color;
            if (color.WidthPixels > 0)
            {
                Texture2D tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                tex.LoadRawTextureData(color.GetBufferCopy());
                tex.Apply();
                renderer.material.mainTexture = tex;
            }
        }
        
        using (var frame = tracker.PopResult())
        {
            //Debug.LogFormat("{0} bodies found.", frame.NumBodies);
            if (frame.NumBodies > 0)
            {
                var bodyId = frame.GetBodyId(0);
                //Debug.LogFormat("bodyId={0}", bodyId);
                this.skeleton = frame.GetSkeleton(0);
                skeletons.Add(this.skeleton);
                for (var i = 0; i < (int)JointId.Count; i++)
                {
                    var joint = this.skeleton.Joints[i];
                    var pos = joint.Position;
					Debug.Log("pos: " + (JointId)i + " " + pos[0] + " " + pos[1] + " " + pos[2]); // Length 3
					//foreach (float t in pos)
					//{
					//	Debug.Log(t + "" + (JointId)i); //ex. rcvd: -484.9375EarLeft...6decimal pts rcvd...todo cap float
					//}

                    var rot = joint.Orientation;
					Debug.Log("rot " + (JointId)i + " " + rot[0] + " " + rot[1] + " " + rot[2] + " " + rot[3]); // Length 4
					var v = new Vector3(pos[0], -pos[1], pos[2]) * 0.004f;
                    var r = new Quaternion(rot[1], rot[2], rot[3], rot[0]);
                    var obj = debugObjects[i];
                    obj.transform.SetPositionAndRotation(v, r);

                    if (skeletons.Count > 50)
                    {
                        Debug.Log("we have enough skeletons");
						Debug.Log(System.DateTime.Now);
						//this.device.StopCameras();
						//NativeMethods.k4a_device_stop_cameras(handle);
						//this.device.Dispose();
						Debug.Break();
                    }
                }
                
            }
		}
    }
}
