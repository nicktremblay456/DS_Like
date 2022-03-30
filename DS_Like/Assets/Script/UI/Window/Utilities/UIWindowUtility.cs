using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtility
{
    public static IEnumerator WaitRealTime (float i_WaitTime)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + i_WaitTime)
        {
            yield return null;
        }
    }
}
