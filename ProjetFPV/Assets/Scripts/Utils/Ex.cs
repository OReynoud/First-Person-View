using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ex
{
    public const string Tag_Player = "Player";
    public const string Tag_Head = "Head";
    public const string Tag_Body = "Body";

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
    
    



}
