using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WritingSurface : MonoBehaviour
{
    // How far the ink sits above the surface, to avoid z-fighting
    [SerializeField] private float inkOffset = 0.0005f;
    // Oldest strokes are erased once this many points have been drawn
    [SerializeField] private int maxPoints = 6000;

    // Mesh data cached in the surface's local space
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] triangleNormals;
    // Parent for all strokes, unscaled and with its Z axis along the surface normal
    private Transform strokeRoot;
    private readonly List<LineRenderer> strokes = new List<LineRenderer>();
    private int pointCount;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (vertices != null)
        {
            return;
        }

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // The side the mesh normals point to is the side that can be written on
        Vector3 facing = Vector3.zero;
        foreach (Vector3 normal in mesh.normals)
        {
            facing += normal;
        }

        Vector3 average = Vector3.zero;
        triangleNormals = new Vector3[triangles.Length / 3];
        for (int i = 0; i < triangleNormals.Length; i++)
        {
            Vector3 a = vertices[triangles[i * 3]];
            Vector3 b = vertices[triangles[i * 3 + 1]];
            Vector3 c = vertices[triangles[i * 3 + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(normal, facing) < 0f)
            {
                normal = -normal;
            }

            triangleNormals[i] = normal;
            average += normal;
        }

        strokeRoot = new GameObject("Ink").transform;
        strokeRoot.SetParent(transform, false);
        strokeRoot.localRotation = Quaternion.LookRotation(average.normalized);
        Vector3 scale = transform.lossyScale;
        strokeRoot.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
    }

    /// <summary>
    /// Checks whether a world position is touching the surface.
    /// </summary>
    /// <param name="worldPosition">The position to test, e.g. the tip of a pen.</param>
    /// <param name="hoverDistance">How far above the surface still counts as touching.</param>
    /// <param name="maxPenetration">How far below the surface still counts as touching.</param>
    /// <param name="inkPosition">The world position on the surface where ink should be placed.</param>
    public bool TryGetContact(Vector3 worldPosition, float hoverDistance, float maxPenetration, out Vector3 inkPosition)
    {
        Initialize();
        inkPosition = Vector3.zero;

        float scale = transform.lossyScale.x;
        Vector3 point = transform.InverseTransformPoint(worldPosition);

        for (int i = 0; i < triangleNormals.Length; i++)
        {
            Vector3 a = vertices[triangles[i * 3]];
            Vector3 b = vertices[triangles[i * 3 + 1]];
            Vector3 c = vertices[triangles[i * 3 + 2]];
            Vector3 normal = triangleNormals[i];

            float distance = Vector3.Dot(point - a, normal) * scale;
            if (distance > hoverDistance || distance < -maxPenetration)
            {
                continue;
            }

            Vector3 projected = point - normal * (distance / scale);
            if (!IsInsideTriangle(projected, a, b, c))
            {
                continue;
            }

            inkPosition = transform.TransformPoint(projected + normal * (inkOffset / scale));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Starts a new stroke at the given world position.
    /// </summary>
    public LineRenderer BeginStroke(Vector3 worldPosition, Material material, float width)
    {
        Initialize();

        // Make room by erasing the oldest strokes
        while (pointCount > maxPoints && strokes.Count > 0)
        {
            pointCount -= strokes[0].positionCount;
            Destroy(strokes[0].gameObject);
            strokes.RemoveAt(0);
        }

        GameObject strokeObject = new GameObject("Stroke");
        strokeObject.transform.SetParent(strokeRoot, false);

        LineRenderer stroke = strokeObject.AddComponent<LineRenderer>();
        stroke.useWorldSpace = false;
        stroke.alignment = LineAlignment.TransformZ;
        stroke.sharedMaterial = material;
        stroke.widthMultiplier = width;
        stroke.numCapVertices = 3;
        stroke.numCornerVertices = 3;
        stroke.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        stroke.receiveShadows = false;
        stroke.positionCount = 0;
        strokes.Add(stroke);

        // Two points close together, so that a single touch leaves a dot
        Vector3 start = strokeRoot.InverseTransformPoint(worldPosition);
        stroke.positionCount = 2;
        stroke.SetPosition(0, start);
        stroke.SetPosition(1, start + Vector3.right * (width * 0.1f));
        pointCount += 2;

        return stroke;
    }

    /// <summary>
    /// Extends a stroke to the given world position.
    /// </summary>
    public void AddPoint(LineRenderer stroke, Vector3 worldPosition)
    {
        stroke.positionCount++;
        stroke.SetPosition(stroke.positionCount - 1, strokeRoot.InverseTransformPoint(worldPosition));
        pointCount++;
    }

    /// <summary>
    /// Erases everything that has been drawn on the surface.
    /// </summary>
    public void Clear()
    {
        foreach (LineRenderer stroke in strokes)
        {
            if (stroke != null)
            {
                Destroy(stroke.gameObject);
            }
        }

        strokes.Clear();
        pointCount = 0;
    }

    static bool IsInsideTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = point - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denominator = d00 * d11 - d01 * d01;
        if (Mathf.Approximately(denominator, 0f))
        {
            return false;
        }

        float v = (d11 * d20 - d01 * d21) / denominator;
        float w = (d00 * d21 - d01 * d20) / denominator;
        return v >= 0f && w >= 0f && v + w <= 1f;
    }
}
