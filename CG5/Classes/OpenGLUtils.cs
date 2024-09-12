using System.Diagnostics;
using OpenTK.Mathematics;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace CG5.Classes;

public static class OpenGLUtils
{
    [Conditional("DEBUG")]
    public static void CheckError()
    {
        ErrorCode error;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Debug.Print($"Error: {error.ToString()}({(int)error})");
        }
    }
    
    public static Vector2 PolarToCartesian(Vector2 center, float radius, float angle)
    {
        var x = center.X + radius * MathF.Cos(angle);
        var y = center.Y + radius * MathF.Sin(angle);
        
        return new Vector2(x, y);
    }
    
    public static Vector3 CylindricalToCartesian(Vector3 cylindrical)
    {
        var r = cylindrical.X;
        var theta = cylindrical.Z;
        var y = cylindrical.Y;
        
        var x = r * MathF.Cos(theta);
        var z = r * MathF.Sin(theta);

        return new Vector3(x, y, z);
    }
}