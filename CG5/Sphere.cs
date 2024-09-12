using System.Runtime.InteropServices;
using CG5.Classes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CG5;

public class Sphere : IRenderable
{
    public Matrix4 ModelMatrix { get; set; }
    public Mesh Mesh { get; }

    public Sphere(float radius, int latitudeSegments, int longitudeSegments)
    {
        // Vertex generation
        var vertices = new Vertex[(latitudeSegments + 1) * (longitudeSegments + 1)];

        var j = 0;
        for (var lat = 0; lat <= latitudeSegments; lat++)
        {
            var theta = MathF.PI * lat / latitudeSegments;
            var sinTheta = MathF.Sin(theta);
            var cosTheta = MathF.Cos(theta);

            for (var lon = 0; lon <= longitudeSegments; lon++)
            {
                var phi = 2 * MathF.PI * lon / longitudeSegments;
                var sinPhi = MathF.Sin(phi);
                var cosPhi = MathF.Cos(phi);

                var x = cosPhi * sinTheta;
                var y = cosTheta;
                var z = sinPhi * sinTheta;
                var position = new Vector3(x, y, z) * radius;
                
                var uv = new Vector2((float)lon / longitudeSegments, (float)lat / latitudeSegments);
                
                var normal = Vector3.Normalize(new Vector3(x, y, z));
                
                var vertex = new Vertex(position, uv, normal);
                vertices[j++] = vertex;
            }
        }
        
        var vertexBuffer = new VertexBuffer(
            vertices,
            vertices.Length * Marshal.SizeOf<Vertex>(),
            vertices.Length,
            BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3), /* positions */
            new VertexBuffer.Attribute(1, 2), /* texture coordinates */
            new VertexBuffer.Attribute(2, 3) /* normal */
        );
        
        // Index generation
        
        var indices = new uint[longitudeSegments * latitudeSegments * 6];

        j = 0;
        for (uint lat = 0; lat < latitudeSegments; lat++)
        {
            for (uint lon = 0; lon < longitudeSegments; lon++)
            {
                var current = (uint)(lat * (longitudeSegments + 1) + lon);
                var next = (uint)(current + longitudeSegments + 1);
                
                indices[j++] = current;
                indices[j++] = next;
                indices[j++] = current + 1;
                
                indices[j++] = current + 1;
                indices[j++] = next;
                indices[j++] = next + 1;
            }
        }
        
        var indexBuffer = new IndexBuffer(
            indices, 
            indices.Length * sizeof(uint), 
            DrawElementsType.UnsignedInt,
            indices.Length
        );

        Mesh = new Mesh(PrimitiveType.Triangles, indexBuffer, vertexBuffer);
    }
    
    public void Dispose()
    {
        Mesh.Dispose();
    }
}