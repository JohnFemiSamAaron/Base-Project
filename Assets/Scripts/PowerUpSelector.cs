using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Thirdweb;
using System.Threading.Tasks;
using Thirdweb.Examples;
public class PowerUpSelector : MonoBehaviour
{
    private List<string> _selectedPowerUps = new List<string>();
    private int maxSelection = 3;

    public List<Button> powerUpButtons;
    public TextMeshProUGUI feedbackText;

    [SerializeField] private GameObject Confirmation;

    private Scene _currentScene;
    

    void Start()
    {
        _currentScene = SceneManager.GetActiveScene();

        foreach (Button button in powerUpButtons)
        {
            button.onClick.AddListener(() => OnPowerUpButtonClicked(button));
        }
    }

    void OnPowerUpButtonClicked(Button button)
    {
        string powerUpName = button.GetComponentInChildren<TextMeshProUGUI>().text;

        if (_selectedPowerUps.Contains(powerUpName))
        {
            _selectedPowerUps.Remove(powerUpName);
            button.GetComponent<Image>().color = Color.white; // Deselect
            feedbackText.text = ""; // Clear feedback
        }
        else
        {
            if (_selectedPowerUps.Count < maxSelection)
            {
                _selectedPowerUps.Add(powerUpName);
                button.GetComponent<Image>().color = Color.green; // Select
                feedbackText.text = ""; // Clear feedback
            }
            else
            {
                feedbackText.text = "You must select " + maxSelection + " power-ups.";
            }
        }

        Debug.Log("Selected Power-Ups: " + string.Join(", ", _selectedPowerUps));
    }

    public void SaveSelectedPowerUps()
    {
        // Convert the list to a comma-separated string before saving
        PlayerPrefs.SetString("PowerUps", string.Join(",", _selectedPowerUps));
        Debug.Log(PlayerPrefs.GetString("PowerUps"));
    }

    public void LoadNextScene()
    {
        if (_currentScene.name != "PowerUp-Select-Scene")
        {
            int nextSceneIndex = _currentScene.buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex, LoadSceneMode.Single);
        }
        else
        {
            if (_selectedPowerUps.Count < 3)
            {
                feedbackText.text = "You must select " + maxSelection + " power-ups.";
                return;
            }

            if (feedbackText.text == "")
            {
                SaveSelectedPowerUps();
                int nextSceneIndex = _currentScene.buildIndex + 1;
                SceneManager.LoadScene(nextSceneIndex, LoadSceneMode.Single);
            }
        }
    }

    public void CheckingSomething()
    {
        Confirmation.SetActive((true));
    }
    
}
