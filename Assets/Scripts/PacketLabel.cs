//Tommy Bui

using TMPro;
using UnityEngine;

public class PacketLabel : MonoBehaviour
{
    public TextMeshPro textLabel;

    public void SetLabel(string labelText)
    {
        if (textLabel != null)
        {
            textLabel.text = labelText;
        }
    }

    public static PacketLabel SetPacketLabel(GameObject packet, string labelText)
    {
        if (packet == null) return null;

        PacketLabel label = packet.GetComponent<PacketLabel>();

        if (label != null)
        {
            label.SetLabel(labelText);
        }

        return label;
    }
}