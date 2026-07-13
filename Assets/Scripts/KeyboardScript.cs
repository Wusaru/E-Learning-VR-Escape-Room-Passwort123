//Nils Klein

using UnityEngine;
using TMPro;

public class KeyboardScript : MonoBehaviour
{

    [Header("Password")]
    public string correctPassword = "Passwort123!";

    [Header("Limits")]
    public int maxPasswordLength = 20;

    private string currentInput = "";

    [Header("Displays")]
    public TextMeshPro displayText;
    public TextMeshPro resultText;

    [Header("Result")]
    public string successMessage = "Code is 1234";

    [Header("Button Labels")]
    public TextMeshPro capsButtonLabel;
    public TextMeshPro key1Label;

    private bool unlocked = false;
    private bool capsLock = false;

    public void AddCharacter(string character)
    {
        if (unlocked)
            return;

        if (currentInput.Length >= maxPasswordLength)
            return;

        if (capsLock)
        {
            if (character == "1")
                character = "!";
            else
                character = character.ToUpper();
        }

        currentInput += character;

        UpdateDisplay();

        //check when max length is reached
        if (currentInput.Length >= maxPasswordLength)
        {
            CheckPassword();
        }
    }

    public void ToggleCaps()
    {
        capsLock = !capsLock;

        UpdateCapsLabel();

        Debug.Log("Caps Lock: " + (capsLock ? "ON" : "OFF"));
    }

    void UpdateCapsLabel()
    {
        if (capsButtonLabel != null)
        {
            capsButtonLabel.text =
                capsLock ? "Caps\nON" : "Caps\nOFF";
        }

        if (key1Label != null)
        {
            key1Label.text = capsLock ? "!" : "1";
        }
    }

    public void Backspace()
    {
        if (currentInput.Length > 0)
        {
            currentInput =
                currentInput.Substring(0, currentInput.Length - 1);

            UpdateDisplay();
        }
    }

    public void Clear()
    {
        currentInput = "";
        UpdateDisplay();
    }

    public void Enter()
    {
        CheckPassword();
    }

    void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = currentInput;
    }

    void ShowSuccessMessage()
    {
        if (resultText != null)
            resultText.text = successMessage;
    }

    void CheckPassword()
    {
        if (currentInput == correctPassword)
        {
            Debug.Log("Correct Password !");

            unlocked = true;

            if (resultText != null)
                resultText.text = "Valid Password !";

            //show the revealed code after 2 seconds
            Invoke(nameof(ShowSuccessMessage), 2f);
        }
        else
        {
            Debug.Log("Wrong Password !");

            if (resultText != null)
                resultText.text = "Wrong Password !";

            currentInput = "";
            UpdateDisplay();
        }
    }
}