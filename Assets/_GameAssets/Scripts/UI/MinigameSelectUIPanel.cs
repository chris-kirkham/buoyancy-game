using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelectUIPanel : MonoBehaviour
{
    [SerializeField] private Image gameThumbnail;
    [SerializeField] private string minigameName;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button selectGameButton;
    [SerializeField] private int minigameSceneBuildIdx;

    public event Action<int> onMinigameSelected;

    private void OnEnable()
    {
        selectGameButton.onClick.AddListener(OnMinigameSelected);
        if(nameText)
        {
            nameText.text = minigameName;
        }
    }

    private void OnDisable()
    {
        selectGameButton.onClick.RemoveAllListeners();
    }

    private void OnMinigameSelected()
    {
        onMinigameSelected?.Invoke(minigameSceneBuildIdx);
    }
}
