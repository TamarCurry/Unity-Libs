using UnityEngine;
using System.Collections;

public static class TweenFuncs
{
    public static Vector3 GetLocalPosition(Transform t)
    {
        RectTransform rt = t as RectTransform;
        if (rt != null) {
            return rt.anchoredPosition;
        }
        if (t != null) {
            return t.localPosition;
        }
        return Vector3.zero;
    }

    public static void SetLocalPosition(Transform t, Vector3 value)
    {
        RectTransform rt = t as RectTransform;
        if (rt != null) {
            rt.anchoredPosition = value;
        }
        else if (t != null) {
            t.localPosition = value;
        }
    }
}
