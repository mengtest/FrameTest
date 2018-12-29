using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SDebug
{
    public static void Debug(string msg, bool showStack = false)
    {
        UnityEngine.Debug.Log(msg);
    }

    public static void Error(string msg, bool showStack = false)
    {
        UnityEngine.Debug.LogError(msg);
    }
}
