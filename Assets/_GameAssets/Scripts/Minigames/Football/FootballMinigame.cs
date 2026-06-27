using System;
using TMPro;
using UnityEngine;

public class FootballMinigame : Minigame
{
    [SerializeField] private MinigameTimer timer;
    [SerializeField] private TeamGoal[] goals;
    [SerializeField] private PlayerSpawner[] playerSpawners;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void OnEnable()
    {
        foreach(var spawner in playerSpawners)
        {
            spawner.Spawn();
        }
    }

    public override void StartMinigame()
    {
        if(timer)
        {
            timer.ResetTime();
            timer.SetTimerRunning(true);
        }

        foreach(var goal in goals)
        {
            goal.OnStartMinigame();
        }
    }

    public override void EndMinigame()
    {
        if(timer)
        {
            timer.SetTimerRunning(false);
        }

        UpdateFinalScoreUI();

        foreach(var goal in goals)
        {
            goal.OnEndMinigame();
        }
    }

    private void OnGoalScored()
    {

    }

    private void UpdateFinalScoreUI()
    {
        if(finalScoreText)
        {
            //TODO: >2 teams
            finalScoreText.text = $"{goals[0].GoalsScored} - {goals[1].GoalsScored}";
        }
    }
}
