using System.Runtime.InteropServices;
using CG5.Classes;
using OpenTK.Mathematics;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG5;

public class Cylinder : IRenderable
{
    public Matrix4 ModelMatrix { get; set; }
    public Mesh Mesh { get; }
    
    public Cylinder(float radius, float height, int vertexCount)
    {
        var (topVertices, topIndices) = GenerateCircle(radius, height / 2, vertexCount, new Vector2(0.25f, 0.75f));
        var (sideVertices, sideIndices) = GenerateSides(radius, height, vertexCount);
        var (bottomVertices, bottomIndices) = GenerateCircle(radius, -height / 2, vertexCount, new Vector2(0.75f, 0.75f));

        // Vertices
        var vertices = topVertices.ToList();
        vertices.AddRange(sideVertices);
        vertices.AddRange(bottomVertices);
        
        var vertexBuffer = new VertexBuffer(vertices.ToArray(), vertices.Count * Marshal.SizeOf<Vertex>(),
            vertices.Count, BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3) /* positions */,
            new VertexBuffer.Attribute(1, 2) /* texture coordinates */,
            new VertexBuffer.Attribute(2, 3) /* normal */);
        
        // Indices
        var indices = topIndices.ToList();
        indices.AddRange(sideIndices.Select(i => (uint)(i + topVertices.Length)));
        indices.AddRange(bottomIndices.Select(i => (uint)(i + topVertices.Length + sideVertices.Length)));
        
        var indexBuffer = new IndexBuffer(indices.ToArray(), indices.Count * sizeof(uint),
            DrawElementsType.UnsignedInt, indices.Count);
        
        Mesh = new Mesh(PrimitiveType.Triangles, indexBuffer, vertexBuffer);
        ModelMatrix = Matrix4.Identity;
    }
    
    private static (Vertex[], uint[]) GenerateCircle(float radius, float height, int vertexCount, Vector2 textureCenter)
    {
        // Vertex generation
        var vertices = new Vertex[vertexCount + 2];
        
        var center = new Vertex(
            new Vector3(0, height, 0),
            textureCenter
        );
        
        vertices[0] = center;
        
        var f = 2 * MathF.PI / vertexCount;
        var cylindrical = new Vector3(radius, height, 0);
        var normalYDirection = height / height;
        for (var i = 1; i <= vertexCount + 1; i++)
        {
            var position = OpenGLUtils.CylindricalToCartesian(cylindrical);
            var uv = OpenGLUtils.PolarToCartesian(textureCenter, 0.25f, cylindrical.Z);

            vertices[i] = new Vertex(position, uv, new Vector3(0, normalYDirection, 0));
            
            cylindrical.Z += f;
        }
        
        // Index generation
        var indices = new uint[3 * vertexCount];

        for (uint i = 0; i < vertexCount; i++)
        {
            indices[i * 3 + 0] = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = i + 2;
        }

        return (vertices, indices);
    }
    

    private static (Vertex[], uint[]) GenerateSides(float radius, float height, int vertexCount)
    {
        // Vertex generation
        var vertices = new Vertex[2 * vertexCount + 2];
        
        var rotationChange = 2 * MathF.PI / vertexCount;
        var currentCylindricalPosition = new Vector3(radius, -height / 2, 0);
        
        for (var i = 0; i <= vertexCount; i++)
        {
            var position = OpenGLUtils.CylindricalToCartesian(currentCylindricalPosition);
            var uv = new Vector2((float)i / vertexCount, 0.5f);

            vertices[i * 2] = new Vertex(position, uv, Vector3.Normalize(position));
            var upperPosition = position with { Y = height / 2 };
            vertices[i * 2 + 1] = new Vertex(upperPosition, uv with { Y = 0.0f }, Vector3.Normalize(upperPosition));
            
            currentCylindricalPosition.Z += rotationChange;
        }

        // Index generation
        var indices = new uint[vertexCount * 6];

        var j = 0;
        for (uint i = 0; i < vertexCount * 2; i += 2)
        {
            indices[j++] = i + 0; 
            indices[j++] = i + 1;
            indices[j++] = i + 2;
            
            indices[j++] = i + 1;
            indices[j++] = i + 2;
            indices[j++] = i + 3;
        }
        
        return (vertices, indices);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}