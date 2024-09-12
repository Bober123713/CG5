using System.Runtime.InteropServices;
using CG5.Classes;
using OpenTK.Mathematics;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG5;

public class Cuboid : IRenderable
{
    public Matrix4 ModelMatrix { get; set; }

    public Mesh Mesh { get; }

    public Cuboid(float width, float height, float depth)
    {
        var halfWidth = width / 2;
        var halfHeight = height / 2;
        var halfDepth = depth / 2;
        
        Vertex[] vertices = 
        [
            // Front
            new Vertex(new Vector3(halfWidth, -halfHeight, halfDepth), new Vector2(1f/3, 1f/2), new Vector3(0, 0, 1)),
            new Vertex(new Vector3(-halfWidth, -halfHeight, halfDepth), new Vector2(0, 1f/2), new Vector3(0, 0, 1)),
            new Vertex(new Vector3(-halfWidth, halfHeight, halfDepth), new Vector2(0, 0), new Vector3(0, 0, 1)),
            new Vertex(new Vector3(halfWidth, halfHeight, halfDepth), new Vector2(1f/3, 0), new Vector3(0, 0, 1)),

            // Top
            new Vertex(new Vector3(halfWidth, -halfHeight, -halfDepth), new Vector2(2f/3, 1), new Vector3(0, -1, 0)),
            new Vertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), new Vector2(1f/3, 1), new Vector3(0, -1, 0)),
            new Vertex(new Vector3(-halfWidth, -halfHeight, halfDepth), new Vector2(1f/3, 1f/2), new Vector3(0, -1, 0)),
            new Vertex(new Vector3(halfWidth, -halfHeight, halfDepth), new Vector2(2f/3, 1f/2), new Vector3(0, -1, 0)),
            
            // Right
            new Vertex(new Vector3(halfWidth, -halfHeight, -halfDepth), new Vector2(2f/3, 1f/2), new Vector3(1, 0, 0)),
            new Vertex(new Vector3(halfWidth, -halfHeight, halfDepth), new Vector2(1f, 1f/2), new Vector3(1, 0, 0)),
            new Vertex(new Vector3(halfWidth, halfHeight, halfDepth), new Vector2(1f, 0), new Vector3(1, 0, 0)),
            new Vertex(new Vector3(halfWidth, halfHeight, -halfDepth), new Vector2(2f/3, 0), new Vector3(1, 0, 0)),

            // Back
            new Vertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), new Vector2(0, 1), new Vector3(0, 0, -1)),
            new Vertex(new Vector3(halfWidth, -halfHeight, -halfDepth), new Vector2(1f/3, 1), new Vector3(0, 0, -1)),
            new Vertex(new Vector3(halfWidth, halfHeight, -halfDepth), new Vector2(1f/3, 1f/2), new Vector3(0, 0, -1)),
            new Vertex(new Vector3(-halfWidth, halfHeight, -halfDepth), new Vector2(0, 1f/2), new Vector3(0, 0, -1)),

            // Bottom
            new Vertex(new Vector3(-halfWidth, halfHeight, halfDepth), new Vector2(1f/3, 1f/2), new Vector3(0, 1, 0)),
            new Vertex(new Vector3(halfWidth, halfHeight, halfDepth), new Vector2(2f/3, 1f/2), new Vector3(0, 1, 0)),
            new Vertex(new Vector3(halfWidth, halfHeight, -halfDepth), new Vector2(2f/3, 0), new Vector3(0, 1, 0)),
            new Vertex(new Vector3(-halfWidth, halfHeight, -halfDepth), new Vector2(1f/3, 0), new Vector3(0, 1, 0)),

            // Left
            new Vertex(new Vector3(-halfWidth, -halfHeight, halfDepth), new Vector2(2f/3, 1), new Vector3(-1, 0, 0)),
            new Vertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), new Vector2(1, 1), new Vector3(-1, 0, 0)),
            new Vertex(new Vector3(-halfWidth, halfHeight, -halfDepth), new Vector2(1, 1f/2), new Vector3(-1, 0, 0)),
            new Vertex(new Vector3(-halfWidth, halfHeight, halfDepth), new Vector2(2f/3, 1f/2), new Vector3(-1, 0, 0))
        ];
        
        short[] indices =
        [
            // Front
            0, 1, 2, 
            0, 2, 3,

            // Back
            4, 5, 6, 
            4, 6, 7,

            // Left
            8, 9, 10, 
            8, 10, 11,

            // Right
            12, 13, 14,
            12, 14, 15,

            // Top
            16, 17, 18, 
            16, 18, 19,

            // Bottom
            20, 21, 22, 
            20, 22, 23
        ];

        var indexBuffer = new IndexBuffer(
            indices, 
            indices.Length * sizeof(short), 
            DrawElementsType.UnsignedShort,
            indices.Length
            );

        var vertexBuffer = new VertexBuffer(
            vertices,
            vertices.Length * Marshal.SizeOf<Vertex>(),
            vertices.Length,
            BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3), /* positions */
            new VertexBuffer.Attribute(1, 2), /* texture coordinates */
            new VertexBuffer.Attribute(2, 3) /* normal */
            );

        Mesh = new Mesh(PrimitiveType.Triangles, indexBuffer, vertexBuffer);
        ModelMatrix = Matrix4.Identity;
    }
    

    public void Dispose()
    {
        Mesh.Dispose();
    }
}