using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIController : MonoBehaviour
{
    public static UIController Instance;
    
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject languageSelectionPanel;
    [SerializeField] private GameObject warningText;

    [SerializeField] private TMP_Text successName;
    [SerializeField] private TMP_InputField inputLoginTxt;
    [SerializeField] private TMP_InputField inputChangeNameTxt;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowLoading()
    {
        HideEverything();
        loginPanel.SetActive(false);
        loadingText.SetActive(true);
    }

    public void ShowLoginPanel()
    {
        HideEverything();
        loginPanel.SetActive(true);
    }

    public void ShowSuccessPanel()
    {
        HideEverything();
        loginPanel.SetActive(false);
        successPanel.SetActive(true);
        successName.text = $"Welcome back, {PlayerPrefs.GetString("localUsername")}";
    }
    
    public void ShowLanguageSelectionPanel()
    {
        HideEverything();
        languageSelectionPanel.SetActive(true);
    }

    public async void CheckInputField()
    {
        if (string.IsNullOrEmpty(inputLoginTxt.text))
        {
            warningText.SetActive(true);
        }
        else
        {
            await UserProfileController.Instance.NewLogin(inputLoginTxt.text);
            ShowLoginPanel();
        }
    }

    public void ChangeName()
    {
        // UserProfileController.Instance.ChangeName(inputChangeNameTxt.text);
    }

    private void HideEverything()
    {
        warningText.SetActive(false);
        loadingText.SetActive(false);
        successPanel.SetActive(false);
        languageSelectionPanel.SetActive(false);
    }
}
