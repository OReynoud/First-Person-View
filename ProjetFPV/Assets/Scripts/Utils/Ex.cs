using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ex
{
    /// <summary>
    /// RETURN THE ANGLE FROM A GIVEN DIRECTION
    /// </summary>
    public static float GetAngleFromVector(this Vector2 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;

        if (angle > 360)
            angle -= 360;

        return angle;
    }


    /// <summary>
    /// ROTATES A VECTOR FROM A CERTAIN ANGLE
    /// </summary>
    // public static Vector2 RotateDirection(this Vector2 originalDirection, float addedAngle)
    // {
    //     float currentAngle = GetAngleFromVector(originalDirection);
    //
    //     currentAngle += addedAngle;
    //
    //     return GetVectorFromAngle(currentAngle);
    // }
}
