//Tommy Bui

using UnityEngine;

public class PasswordStrengthVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    public Transform strengthSphere;

    [Header("Sphere Sizes")]
    public float[] sizeByStrength =
    {
        0.5f,   // Weak
        2.0f,   // Medium
        20.0f,  // Strong
        100.0f  // Very Strong
    };

    private Vector3 originalPosition;

    private void Start()
    {
        if (strengthSphere != null)
        {
            originalPosition = strengthSphere.position;
        }
    }

    public void ShowPasswordStrength(int strengthIndex)
    {
        float size = sizeByStrength[strengthIndex];

        strengthSphere.localScale = Vector3.one * size;

        float radius = size * 0.5f;

        strengthSphere.position =
            originalPosition + Vector3.forward * radius;
    }
}