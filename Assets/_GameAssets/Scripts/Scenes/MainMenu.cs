using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private List<MinigameSelectUIPanel> sceneSelectionButtons;

    private SceneTransitionManager sceneTransitionManager;

    private void OnEnable()
    {
        sceneTransitionManager = SceneTransitionManager.Inst;
        if(!sceneTransitionManager)
        {
            Debug.LogError($"No instance of {nameof(SceneTransitionManager)} found!");
        }

        foreach(var button in sceneSelectionButtons)
        {
            button.onMinigameSelected += OnMinigameSelected;
        }
    }

    private void OnDisable()
    {
        foreach(var button in sceneSelectionButtons)
        {
            button.onMinigameSelected -= OnMinigameSelected;
        }
    }

    private void OnMinigameSelected(int minigameSceneBuildIdx)
    {
        if(sceneTransitionManager)
        {
            sceneTransitionManager.LoadSceneWithLoadingScreen(minigameSceneBuildIdx);
        }
    }
}
