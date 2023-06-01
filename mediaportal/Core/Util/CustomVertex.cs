using SharpDX;
using SharpDX.Direct3D9;
using System.Runtime.InteropServices;

namespace MediaPortal.Util
{
  /// <summary>
  /// Defines various custom fixed-format vertex types. This class is a container of structures. See the individual structures for more information.
  /// </summary>
  public sealed class CustomVertex
  {
    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Transformed
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
      /// </summary>
      public float Rhw;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.PositionRhw;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.Transformed" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(TransformedTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector4 Position
      {
        get
        {
          return new Vector4(X, Y, Z, Rhw);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
          Rhw = value.W;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.Transformed" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector4" /> object that contains the position.</param>
      public Transformed(Vector4 value)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Rhw = value.W;
      }

      /// <summary>Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.Transformed" /> class.</summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      public Transformed(float xvalue, float yvalue, float zvalue, float rhwvalue)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Rhw = rhwvalue;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedTextured
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
      /// </summary>
      public float Rhw;

      /// <summary>
      /// Retrieves or sets the u component of the texture coordinate.
      /// </summary>
      public float Tu;

      /// <summary>
      /// Retrieves or sets the v component of the texture coordinate.
      /// </summary>
      public float Tv;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Texture1 | VertexFormat.PositionRhw;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedTextured" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(TransformedTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector4 Position
      {
        get
        {
          return new Vector4(X, Y, Z, Rhw);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
          Rhw = value.W;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedTextured" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector4" /> object that contains the position.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedTextured.#ctor" /> component of the texture coordinate.</param>
      public TransformedTextured(Vector4 value, float u, float v)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Rhw = value.W;
        Tu = u;
        Tv = v;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedTextured" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="rhwvalue">Floating-point value that represents the reciprocal homogeneous w (RHW) component of the transformed vertex.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedTextured.#ctor" /> component of the texture coordinate.</param>
      public TransformedTextured(float xvalue, float yvalue, float zvalue, float rhwvalue, float u, float v)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Rhw = rhwvalue;
        Tu = u;
        Tv = v;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices and color information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedColored
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
      /// </summary>
      public float Rhw;

      /// <summary>
      /// Retrieves or sets the vertex color.
      /// </summary>
      public int Color;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Diffuse | VertexFormat.PositionRhw;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColored" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(TransformedColored);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector4 Position
      {
        get
        {
          return new Vector4(X, Y, Z, Rhw);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
          Rhw = value.W;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColored" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector4" /> object that contains the position.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      public TransformedColored(Vector4 value, int c)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Rhw = value.W;
        Color = c;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColored" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="rhwvalue">Floating-point value that represents the reciprocal homogeneous w (RHW) component of the transformed vertex.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      public TransformedColored(float xvalue, float yvalue, float zvalue, float rhwvalue, int c)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Rhw = rhwvalue;
        Color = c;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices, color, and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedColoredTextured
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
      /// </summary>
      public float Rhw;

      /// <summary>
      /// Retrieves or sets the vertex color.
      /// </summary>
      public int Color;

      /// <summary>
      /// Retrieves or sets the u component of the texture coordinate.
      /// </summary>
      public float Tu;

      /// <summary>
      /// Retrieves or sets the v component of the texture coordinate.
      /// </summary>
      public float Tv;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Texture1 | VertexFormat.Diffuse | VertexFormat.PositionRhw;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColoredTextured" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(TransformedColoredTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector4 Position
      {
        get
        {
          return new Vector4(X, Y, Z, Rhw);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
          Rhw = value.W;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColoredTextured" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector4" /> object that contains the position.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedColoredTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedColoredTextured.#ctor" /> component of the texture coordinate.</param>
      public TransformedColoredTextured(Vector4 value, int c, float u, float v)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Rhw = value.W;
        Tu = u;
        Tv = v;
        Color = c;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.TransformedColoredTextured" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="rhwvalue">Floating-point value that represents the reciprocal homogeneous w (RHW) component of the transformed vertex.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedColoredTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.TransformedColoredTextured.#ctor" /> component of the texture coordinate.</param>
      public TransformedColoredTextured(float xvalue, float yvalue, float zvalue, float rhwvalue, int c, float u, float v)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Rhw = rhwvalue;
        Tu = u;
        Tv = v;
        Color = c;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains only position data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionOnly
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Position;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionOnly" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionOnly);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionOnly" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex position.</param>
      public PositionOnly(Vector3 value)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionOnly" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      public PositionOnly(float xvalue, float yvalue, float zvalue)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position and normal data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormal
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the nx component of the vertex normal.
      /// </summary>
      public float Nx;

      /// <summary>
      /// Retrieves or sets the ny component of the vertex normal.
      /// </summary>
      public float Ny;

      /// <summary>
      /// Retrieves or sets the nz component of the vertex normal.
      /// </summary>
      public float Nz;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.PositionRhw;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormal" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionNormal);
        }
      }

      public Vector3 Normal
      {
        get
        {
          return new Vector3(Nx, Ny, Nz);
        }
        set
        {
          Nx = value.X;
          Ny = value.Y;
          Nz = value.Z;
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormal" /> class.
      /// </summary>
      /// <param name="pos">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex position.</param>
      /// <param name="nor">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex normal data.</param>
      public PositionNormal(Vector3 pos, Vector3 nor)
      {
        X = pos.X;
        Y = pos.Y;
        Z = pos.Z;

        Nx = nor.X;
        Ny = nor.Y;
        Nz = nor.Z;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormal" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="nxvalue">Floating-point value that represents the nx coordinate of the vertex normal.</param>
      /// <param name="nyvalue">Floating-point value that represents the ny coordinate of the vertex normal.</param>
      /// <param name="nzvalue">Floating-point value that represents the nz coordinate of the vertex normal.</param>
      public PositionNormal(float xvalue, float yvalue, float zvalue, float nxvalue, float nyvalue, float nzvalue)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Nx = nxvalue;
        Ny = nyvalue;
        Nz = nzvalue;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position and color information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColored
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the vertex color.
      /// </summary>
      public int Color;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Diffuse | VertexFormat.Position;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColored" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionColored);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColored" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector3" /> object that contains the position.</param>
      /// <param name="c">Integer that represents the diffuse color value.</param>
      public PositionColored(Vector3 value, int c)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Color = c;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColored" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="c">Integer that represents the diffuse color value.</param>
      public PositionColored(float xvalue, float yvalue, float zvalue, int c)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Color = c;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position, color, and normal data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalColored
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the nx component of the vertex normal.
      /// </summary>
      public float Nx;

      /// <summary>
      /// Retrieves or sets the ny component of the vertex normal.
      /// </summary>
      public float Ny;

      /// <summary>
      /// Retrieves or sets the nz component of the vertex normal.
      /// </summary>
      public float Nz;

      /// <summary>
      /// Retrieves or sets the vertex color.
      /// </summary>
      public int Color;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Diffuse;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalColored" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionNormalColored);
        }
      }

      /// <summary>
      /// Retrieves or sets the vertex normal data.
      /// </summary>
      public Vector3 Normal
      {
        get
        {
          return new Vector3(Nx, Ny, Nz);
        }
        set
        {
          Nx = value.X;
          Ny = value.Y;
          Nz = value.Z;
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalColored" /> class.
      /// </summary>
      /// <param name="pos">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex position.</param>
      /// <param name="nor">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex normal data.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      public PositionNormalColored(Vector3 pos, Vector3 nor, int c)
      {
        X = pos.X;
        Y = pos.Y;
        Z = pos.Z;

        Nx = nor.X;
        Ny = nor.Y;
        Nz = nor.Z;

        Color = c;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalColored" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="nxvalue">Floating-point value that represents the nx coordinate of the vertex normal.</param>
      /// <param name="nyvalue">Floating-point value that represents the ny coordinate of the vertex normal.</param>
      /// <param name="nzvalue">Floating-point value that represents the nz coordinate of the vertex normal.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      public PositionNormalColored(float xvalue, float yvalue, float zvalue, float nxvalue, float nyvalue, float nzvalue, int c)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Nx = nxvalue;
        Ny = nyvalue;
        Nz = nzvalue;
        Color = c;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position, normal data, and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalTextured
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the nx component of the vertex normal.
      /// </summary>
      public float Nx;

      /// <summary>
      /// Retrieves or sets the ny component of the vertex normal.
      /// </summary>
      public float Ny;

      /// <summary>
      /// Retrieves or sets the nz component of the vertex normal.
      /// </summary>
      public float Nz;

      /// <summary>
      /// Retrieves or sets the u component of the texture coordinate.
      /// </summary>
      public float Tu;

      /// <summary>
      /// Retrieves or sets the v component of the texture coordinate.
      /// </summary>
      public float Tv;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalTextured" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionNormalTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the vertex normal data.
      /// </summary>
      public Vector3 Normal
      {
        get
        {
          return new Vector3(Nx, Ny, Nz);
        }
        set
        {
          Nx = value.X;
          Ny = value.Y;
          Nz = value.Z;
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalTextured" /> class.
      /// </summary>
      /// <param name="pos">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex position.</param>
      /// <param name="nor">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex normal data.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionNormalTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionNormalTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionNormalTextured(Vector3 pos, Vector3 nor, float u, float v)
      {
        X = pos.X;
        Y = pos.Y;
        Z = pos.Z;

        Nx = nor.X;
        Ny = nor.Y;
        Nz = nor.Z;

        Tu = u;
        Tv = v;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionNormalTextured" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="nxvalue">Floating-point value that represents the nx coordinate of the vertex normal.</param>
      /// <param name="nyvalue">Floating-point value that represents the ny coordinate of the vertex normal.</param>
      /// <param name="nzvalue">Floating-point value that represents the nz coordinate of the vertex normal.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionNormalTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionNormalTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionNormalTextured(float xvalue, float yvalue, float zvalue, float nxvalue, float nyvalue, float nzvalue, float u, float v)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Nx = nxvalue;
        Ny = nyvalue;
        Nz = nzvalue;
        Tu = u;
        Tv = v;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionTextured
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the u component of the texture coordinate.
      /// </summary>
      public float Tu;

      /// <summary>
      /// Retrieves or sets the v component of the texture coordinate.
      /// </summary>
      public float Tv;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Texture1 | VertexFormat.Position;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionTextured" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionTextured" /> class.
      /// </summary>
      /// <param name="pos">A <see cref="T:SharpDX.Vector3" /> object that contains the vertex position.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionTextured(Vector3 pos, float u, float v)
      {
        X = pos.X;
        Y = pos.Y;
        Z = pos.Z;
        Tu = u;
        Tv = v;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionTextured" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionTextured(float xvalue, float yvalue, float zvalue, float u, float v)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Tu = u;
        Tv = v;
      }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains position, color, and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColoredTextured
    {
      /// <summary>
      /// Retrieves or sets the x component of the position.
      /// </summary>
      public float X;

      /// <summary>
      /// Retrieves or sets the y component of the position.
      /// </summary>
      public float Y;

      /// <summary>
      /// Retrieves or sets the z component of the position.
      /// </summary>
      public float Z;

      /// <summary>
      /// Retrieves or sets the vertex color.
      /// </summary>
      public int Color;

      /// <summary>
      /// Retrieves or sets the u component of the texture coordinate.
      /// </summary>
      public float Tu;

      /// <summary>
      /// Retrieves or sets the v component of the texture coordinate.
      /// </summary>
      public float Tv;

      /// <summary>
      /// Retrieves the <see cref="T:SharpDX.Direct3D9.VertexFormat" /> for the current custom vertex.
      /// </summary>
      public const VertexFormat Format = VertexFormat.Texture1 | VertexFormat.Diffuse | VertexFormat.Position;

      /// <summary>
      /// Retrieves the size of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColoredTextured" /> structure.
      /// </summary>
      public unsafe static int StrideSize
      {
        get
        {
          return sizeof(PositionColoredTextured);
        }
      }

      /// <summary>
      /// Retrieves or sets the transformed position.
      /// </summary>
      public Vector3 Position
      {
        get
        {
          return new Vector3(X, Y, Z);
        }
        set
        {
          X = value.X;
          Y = value.Y;
          Z = value.Z;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColoredTextured" /> class.
      /// </summary>
      /// <param name="value">A <see cref="T:SharpDX.Vector3" /> object that contains the position.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionColoredTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionColoredTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionColoredTextured(Vector3 value, int c, float u, float v)
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        Color = c;
        Tu = u;
        Tv = v;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:MediaPortal.Util.CustomVertex.PositionColoredTextured" /> class.
      /// </summary>
      /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
      /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
      /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
      /// <param name="c">Integer that represents the vertex color value.</param>
      /// <param name="u">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionColoredTextured.#ctor" /> component of the texture coordinate.</param>
      /// <param name="v">Floating-point value that represents the <see cref="M:MediaPortal.Util.CustomVertex.PositionColoredTextured.#ctor" /> component of the texture coordinate.</param>
      public PositionColoredTextured(float xvalue, float yvalue, float zvalue, int c, float u, float v)
      {
        X = xvalue;
        Y = yvalue;
        Z = zvalue;
        Color = c;
        Tu = u;
        Tv = v;
      }
    }
  }
}
