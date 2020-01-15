using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentTactLabel;
    [SerializeField] private Button RestartButton;
    
    public void UpdateCurrentTactLabel(int currentTact)
    {
        currentTactLabel.text = "Current Tact: " + currentTact;
    }

    public void DisplayWarning(string s)
    {
        currentTactLabel.text = s;
        Debug.Log(s);
    }

    public void SetRestartInteractable()
    {
        RestartButton.interactable = true;
        RestartButton.enabled = true;
    }
}
