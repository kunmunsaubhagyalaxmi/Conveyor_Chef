using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// For a full-screen picture fitted by <see cref="DesignFrame"/>: on a screen shaped unlike
    /// the design phone the picture is enlarged to cover it and its edges are cut off. By default
    /// it grows around its centre; this keeps another part in place instead, e.g. vertical 0 keeps
    /// the bottom (the loading picture has words painted near the bottom, right above the loading
    /// bar, so only sky is cut off at the top).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DesignFrameCoverAlign : MonoBehaviour
    {
        [Range(0f, 1f)] public float horizontal = 0.5f;   // 0 left, 0.5 centre, 1 right
        [Range(0f, 1f)] public float vertical = 0.5f;     // 0 bottom, 0.5 centre, 1 top
    }
}
