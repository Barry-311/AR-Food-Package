using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeBridge : MonoBehaviour
{
    [DllImport("mediapipe_jni")]
    private static extern void mp_api__SetFreeHGlobal();

    void Start()
    {
        Debug.Log("[JNI] Trying to call native function...");
        try
        {
            mp_api__SetFreeHGlobal(); // 自动加载 libmediapipe_jni.so
            Debug.Log("[JNI] Native library loaded and function call succeeded.");
        }
        catch (Exception e)
        {
            Debug.LogError("[JNI] Native function call failed: " + e.Message);
        }
    }
}
