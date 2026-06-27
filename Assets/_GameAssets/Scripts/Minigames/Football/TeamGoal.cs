using System;
using TMPro;
using UnityEngine;

public class TeamGoal : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int team;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreUIText;

    public int GoalsScored { get; private set; }
    public int Team => team;

    public event Action onGoalScored;

    public void OnGoalScored()
    {
        GoalsScored++;
        onGoalScored?.Invoke();
    }

    public void OnStartMinigame()
    {
        ResetGoalsScored();
    }

    public void OnEndMinigame()
    {
    }

    private void ResetGoalsScored()
    {
        GoalsScored = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == targetObject)
        {
            onGoalScored?.Invoke();
        }
    }
}
