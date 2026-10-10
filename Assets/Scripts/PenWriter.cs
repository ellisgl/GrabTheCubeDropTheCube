using UnityEngine;
using Oculus.Interaction;

public class PenWriter : MonoBehaviour
{
    // The writing end of the pen
    [SerializeField] private Transform tip;
    // Surfaces this pen can write on
    [SerializeField] private WritingSurface[] surfaces;
    [SerializeField] private Material inkMaterial;
    [SerializeField] private float lineWidth = 0.0012f;
    // Minimum distance the tip has to move before the stroke is extended
    [SerializeField] private float minPointSpacing = 0.0008f;
    // How far above the surface the tip still writes
    [SerializeField] private float hoverDistance = 0.002f;
    // How far the tip can be pushed through the surface and still write
    [SerializeField] private float maxPenetration = 0.02f;
    // Only write while the pen is being held
    [SerializeField] private bool onlyWhileGrabbed = true;

    private Grabbable grabbable;
    private WritingSurface activeSurface;
    private LineRenderer stroke;
    private Vector3 lastInkPosition;

    void Awake()
    {
        grabbable = GetComponent<Grabbable>();
    }

    void OnDisable()
    {
        EndStroke();
    }

    // LateUpdate, so the pen has already been moved by the hand this frame
    void LateUpdate()
    {
        if (tip == null || surfaces == null)
        {
            return;
        }

        if (onlyWhileGrabbed && (grabbable == null || grabbable.SelectingPointsCount == 0))
        {
            EndStroke();
            return;
        }

        foreach (WritingSurface surface in surfaces)
        {
            if (surface == null || !surface.TryGetContact(tip.position, hoverDistance, maxPenetration, out Vector3 inkPosition))
            {
                continue;
            }

            Write(surface, inkPosition);
            return;
        }

        EndStroke();
    }

    void Write(WritingSurface surface, Vector3 inkPosition)
    {
        if (stroke == null || surface != activeSurface)
        {
            activeSurface = surface;
            stroke = surface.BeginStroke(inkPosition, inkMaterial, lineWidth);
            lastInkPosition = inkPosition;
            return;
        }

        if (Vector3.Distance(inkPosition, lastInkPosition) < minPointSpacing)
        {
            return;
        }

        surface.AddPoint(stroke, inkPosition);
        lastInkPosition = inkPosition;
    }

    void EndStroke()
    {
        stroke = null;
        activeSurface = null;
    }
}
