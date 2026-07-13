//Nils Klein

using UnityEngine;
using TMPro;

public class DoorKeypad : MonoBehaviour
{
    [Header("Mesh Render")]
    public MeshRenderer mesh1;
    public MeshRenderer mesh2;

    [Header("Code")]
    public string correctCode = "1234";

    [Header("Display")]
    private string currentInput = "";

    [Header("Button Label")]
    public TextMeshPro displayText;

    public Transform door;
    public Vector3 openPosition;

    private bool isOpen = false;

    public void ShowMeshes()
    {
        if (mesh1 != null)
            mesh1.enabled = true;

        if (mesh2 != null)
            mesh2.enabled = true;
    }

    public void AddNumber(string number)
    {
        //ignore any new input after unlock
        if (isOpen)
            return;

        //prevent more than 4 digits
        if (currentInput.Length >= 4)
            return;

        currentInput += number;
        UpdateDisplay();

        if (currentInput.Length == 4)
        {
            CheckCode();
        }
    }

    void UpdateDisplay()
    {
        displayText.text = currentInput;
    }

    void CheckCode()
    {
        if (currentInput == correctCode)
        {
            Debug.Log("Correct code !");
            OpenDoor();

            //keep correct code permanently displayed
            displayText.text = correctCode;
            ShowMeshes();
        }
        else
        {
            Debug.Log("Wrong code !");

            //Only clear if wrong
            Invoke(nameof(ClearInput), 1f);
        }
    }

    void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }

    void OpenDoor()
    {
        if (!isOpen)
        {
            door.position = openPosition;
            isOpen = true;
        }
    }
}