using System.Drawing;
using System.Runtime.InteropServices;
using CG5.Classes;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CG5;

public class Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    private bool IsLoaded { get; set; }
    private Shader Shader { get; set; } = null!;
    private ImGuiController ImGuiController { get; set; } = null!;
    private IRenderable CurrentModel { get; set; } = null!;
    private Camera Camera { get; set; } = null!;
    private Texture Texture { get; set; } = null!;
    private DebugProc DebugProcCallback { get; } = OnDebugMessage;

    public static void Main(string[] _)
    {
        var gwSettings = GameWindowSettings.Default;
        var nwSettings = NativeWindowSettings.Default;
        nwSettings.NumberOfSamples = 16;

        using var program = new Program(gwSettings, nwSettings);
        program.Title = "CG5";
        program.Size = new Vector2i(1280, 800);
        program.Run();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.DebugMessageCallback(DebugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        
        Shader = new Shader(
            paths:
            [
                ("shader.vert", ShaderType.VertexShader), 
                ("shader.frag", ShaderType.FragmentShader)
            ]
        );
        
        Shader.LoadFloat3("objectColor", new Vector3(1, 0, 0));
        Shader.LoadFloat3("lightColor", new Vector3(1, 1, 1));
        Shader.LoadFloat3("lightPos", new Vector3(2, 2, 2));
        
        ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        Camera = new Camera(new OrbitingControl(5 * Vector3.UnitZ, Vector3.Zero), new PerspectiveProjection());

        CurrentModel = new Cuboid(1, 1, 1);
        
        Texture = new Texture("texture.jpg");
        
        GL.ClearColor(Color.DarkSlateGray);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        IsLoaded = true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        CurrentModel.Dispose();
        ImGuiController.Dispose();
        Texture.Dispose();
        Shader.Dispose();

        IsLoaded = false;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (!IsLoaded) return;

        base.OnResize(e);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        ImGuiController.OnWindowResized(ClientSize.X, ClientSize.Y);
        Camera.Aspect = (float)ClientSize.X / ClientSize.Y;
    }

    private float _time;

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        _time += (float)args.Time;

        ImGuiController.Update((float)args.Time);
        Camera.Update((float)args.Time);
        
        CurrentModel.ModelMatrix = Matrix4.CreateRotationY(_time * 0.25f);
        
        if (ImGui.GetIO().WantCaptureMouse) return;

        var keyboard = KeyboardState.GetSnapshot();
        var mouse = MouseState.GetSnapshot();

        Camera.HandleInput((float)args.Time, keyboard, mouse);

        if (keyboard.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Shader.Use();
        
        var mvp = CurrentModel.ModelMatrix * Camera.ProjectionViewMatrix;
        
        Shader.LoadMatrix4("model", CurrentModel.ModelMatrix);
        Shader.LoadMatrix4("view", Camera.ViewMatrix);
        Shader.LoadMatrix4("projection", Camera.ProjectionMatrix);
        
        Shader.LoadFloat3("viewPos", Camera.Position);
        
        Shader.LoadFloat("ambientStr", _ambientStr);
        Shader.LoadFloat("specularStr", _specularStr);
        Shader.LoadInteger("specularCoeff", _specularCoeff);
        
        Texture.Bind();
        Shader.LoadInteger("texture1", 0);
        
        CurrentModel.Mesh.Bind();
        CurrentModel.Mesh.RenderIndexed();
        
        DebugMatrix(CurrentModel.ModelMatrix, "Model Matrix");
        DebugMatrix(Camera.ViewMatrix, "View Matrix");
        DebugMatrix(Camera.ProjectionMatrix, "Projection Matrix");
        DebugMatrix(mvp, "MVP Matrix");

        RenderGui();

        Context.SwapBuffers();
    }
    
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        ImGuiController.OnKey(e, true);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);

        ImGuiController.OnKey(e, false);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        ImGuiController.OnMouseButton(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        ImGuiController.OnMouseButton(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        ImGuiController.OnMouseMove(e);
    }

    private void DebugMatrix(Matrix4 matrix, string name)
    {
        ImGui.Begin(name, ImGuiWindowFlags.AlwaysAutoResize);
        var c0 = new System.Numerics.Vector4(matrix.Column0.X, matrix.Column0.Y, matrix.Column0.Z, matrix.Column0.W);
        var c1 = new System.Numerics.Vector4(matrix.Column1.X, matrix.Column1.Y, matrix.Column1.Z, matrix.Column1.W);
        var c2 = new System.Numerics.Vector4(matrix.Column2.X, matrix.Column2.Y, matrix.Column2.Z, matrix.Column2.W);
        var c3 = new System.Numerics.Vector4(matrix.Column3.X, matrix.Column3.Y, matrix.Column3.Z, matrix.Column3.W);
        ImGui.PushID($"{name}_c0"); ImGui.InputFloat4("", ref c0); ImGui.PopID();
        ImGui.PushID($"{name}_c1"); ImGui.InputFloat4("", ref c1); ImGui.PopID();
        ImGui.PushID($"{name}_c2"); ImGui.InputFloat4("", ref c2); ImGui.PopID();
        ImGui.PushID($"{name}_c3"); ImGui.InputFloat4("", ref c3); ImGui.PopID();
        ImGui.End();
    }
    
    private void RenderGui()
    {
        // Camera
        ImGui.Begin("Camera", ImGuiWindowFlags.AlwaysAutoResize);
        if (ImGui.CollapsingHeader("Control"))
        {
            ImGui.Indent(10);
            if (ImGui.RadioButton("No Control", ref _control, 0))
                Camera.Control = new NoControl(Camera.Control);
            if (ImGui.RadioButton("Orbital Control", ref _control, 1))
                Camera.Control = new OrbitingControl(Camera.Position, Vector3.Zero);
            if (ImGui.RadioButton("FlyBy Control", ref _control, 2))
                Camera.Control = new FlyByControl(Camera.Control);

            ImGui.Indent(-10);
        }

        if (ImGui.CollapsingHeader("Projection"))
        {
            ImGui.Indent(10);
            if (ImGui.RadioButton("Perspective", ref _projection, 0))
                Camera.Projection = new PerspectiveProjection { Aspect = Camera.Aspect };
            if (ImGui.RadioButton("Orthographic", ref _projection, 1))
                Camera.Projection = new OrthographicProjection { Aspect = Camera.Aspect, Height = 5 };
            ImGui.Indent(-10);
        }

        if (ImGui.CollapsingHeader("Details"))
        {
            ImGui.Indent(10);
            var position = new System.Numerics.Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
            var front = new System.Numerics.Vector3(Camera.Front.X, Camera.Front.Y, Camera.Front.Z);
            var right = new System.Numerics.Vector3(Camera.Right.X, Camera.Right.Y, Camera.Right.Z);
            var up = new System.Numerics.Vector3(Camera.Up.X, Camera.Up.Y, Camera.Up.Z);
            ImGui.InputFloat3("Camera position", ref position);
            ImGui.InputFloat3("Camera front", ref front);
            ImGui.InputFloat3("Camera right", ref right);
            ImGui.InputFloat3("Camera up", ref up);
            ImGui.Indent(-10);
        }

        ImGui.End();
        
        // Model
        ImGui.Begin("Model", ImGuiWindowFlags.AlwaysAutoResize);

        if (ImGui.RadioButton("Cuboid", ref _model, 0))
            UpdateCuboid();
        
        if (ImGui.RadioButton("Cylinder", ref _model, 1))
            UpdateCylinder();
        
        if (ImGui.RadioButton("Sphere", ref _model, 2))
            UpdateSphere();
        
        if (ImGui.CollapsingHeader("Parameters"))
        {
            ImGui.Indent(10);
            var update = false;
            switch (_model)
            {
                case 0:
                    // Cuboid
                    update |= ImGui.SliderFloat("Width", ref _cuboidWidth, 0.1f, 5.0f);
                    update |= ImGui.SliderFloat("Height", ref _cuboidHeight, 0.1f, 5.0f);
                    update |= ImGui.SliderFloat("Depth", ref _cuboidDepth, 0.1f, 5.0f);
                    if (update)
                        UpdateCuboid();
                    break;
                case 1:
                    // Cylinder
                    update |= ImGui.SliderFloat("Radius", ref _cylinderRadius, 0.1f, 5.0f);
                    update |= ImGui.SliderFloat("Height", ref _cylinderHeight, 0.1f, 5.0f);
                    update |= ImGui.SliderInt("Sections", ref _cylinderResolution, 4, 512);
                    if (update)
                        UpdateCylinder();
                    break;
                case 2:
                    // Sphere
                    update |= ImGui.SliderFloat("Radius", ref _sphereRadius, 0.1f, 5.0f);
                    update |= ImGui.SliderInt("Latitude", ref _sphereLatitudeSections, 3, 512);
                    update |= ImGui.SliderInt("Longitude", ref _sphereLongitudeSections, 3, 512);
                    if (update)
                        UpdateSphere();
                    break;
            }
            ImGui.Indent(-10);
        }
        
        if (ImGui.CollapsingHeader("Shader"))
        {
            ImGui.Indent(10);
            ImGui.SliderFloat("Ambient Str", ref _ambientStr, 0, 10);
            ImGui.SliderFloat("Specular Str", ref _specularStr, 0, 10);
            ImGui.SliderInt("Specular Coeff", ref _specularCoeff, 0, 1024);
            ImGui.Indent(-10);
        }
        ImGui.End();
        
        // Display
        ImGui.Begin("Display", ImGuiWindowFlags.AlwaysAutoResize);
        if(ImGui.RadioButton("Fill", ref _displayMode, 0))
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        if(ImGui.RadioButton("Wireframe", ref _displayMode, 1))
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        ImGui.End();
        
        ImGuiController.Render();
    }
    
    private int _displayMode = 0;
    private int _model = 0;
    private float _cuboidWidth = 1.0f;
    private float _cuboidHeight = 1.0f;
    private float _cuboidDepth = 1.0f;
    private void UpdateCuboid()
    {
        CurrentModel = new Cuboid(_cuboidWidth, _cuboidHeight, _cuboidDepth);
    }

    private float _cylinderRadius = 1.0f;
    private float _cylinderHeight = 1.0f;
    private int _cylinderResolution = 20;
    private void UpdateCylinder()
    {
        CurrentModel = new Cylinder(_cylinderRadius, _cylinderHeight, _cylinderResolution);
    }

    private float _sphereRadius = 1.0f;
    private int _sphereLatitudeSections = 20;
    private int _sphereLongitudeSections = 20;
    private void UpdateSphere()
    {
        CurrentModel = new Sphere(_sphereRadius, _sphereLatitudeSections, _sphereLongitudeSections);
    }

    private float _ambientStr = 0.2f;
    private float _specularStr = 0.5f;
    private int _specularCoeff = 32;
    
    private int _control = 1;
    private int _projection;
    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        ImGuiController.OnPressedChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        ImGuiController.OnMouseScroll(e);
    }

    private static void OnDebugMessage(
        DebugSource source,     // Source of the debugging message.
        DebugType type,         // Type of the debugging message.
        int id,                 // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length,             // Length of the string in pMessage.
        IntPtr pMessage,        // Pointer to message string.
        IntPtr pUserParam)      // The pointer you gave to OpenGL.
    {
        var message = Marshal.PtrToStringAnsi(pMessage, length);

        var log = $"[{severity} source={source} type={type} id={id}] {message}";

        Console.WriteLine(log);
    }
}

public struct Vertex(Vector3 position, Vector2 textureCoordinate, Vector3? normal = null)
{
    public Vector3 Position = position;
    public Vector2 TextureCoordinate = textureCoordinate;
    public Vector3 Normal = normal ?? Vector3.Zero;
}