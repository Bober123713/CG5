using CG5.Classes;
using OpenTK.Mathematics;

namespace CG5;

public interface IRenderable
{
    public Matrix4 ModelMatrix { get; set; }
    public Mesh Mesh { get; }
    public void Dispose();
}