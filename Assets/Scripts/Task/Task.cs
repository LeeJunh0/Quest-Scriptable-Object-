using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum TaskState
{
    Inactive,
    Running,
    Complete
}

[CreateAssetMenu(menuName = "Quest/Task/Task",fileName = "Task_")]
public class Task : ScriptableObject
{
    #region Event
    public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState);
    public delegate void SuccessChangedHandler(Task task, int currentSuccess, int prevSuccess);
    #endregion

    [Header("Category")]
    [SerializeField]
    Category category;

    [Header("Text")]
    [SerializeField]
    string codeName;
    [SerializeField]
    string description;

    [Header("Action")]
    [SerializeField]
    TaskAction action;

    [Header("Target")]
    [SerializeField]
    TaskTarget[] targets;

    [Header("Setting")]
    [SerializeField]
    InitialSccuessValue initialSccuessValue;
    [SerializeField]
    int needSuccessToComplete;
    [SerializeField]
    bool canReceiveReport;

    TaskState state;
    int currentSuccess;
    public event StateChangedHandler onStateChanged;
    public event SuccessChangedHandler onSuccessChanged;
    public Category Category => category;
    public int CurrentSuccess
    {
        get => currentSuccess;
        set
        {
            int prevSuccess = currentSuccess;
            currentSuccess = Mathf.Clamp(value, 0, needSuccessToComplete);
            if(currentSuccess != prevSuccess)
            {
                State = currentSuccess == needSuccessToComplete ? TaskState.Complete : TaskState.Running;
                onSuccessChanged?.Invoke(this, currentSuccess, prevSuccess);
            }
        }
    }
    public string CodeName => codeName;
    public string Description => description;
    public int NeedSuccessToComplete => needSuccessToComplete;
    public TaskState State
    {
        get => state;
        set
        {
            var prevState = state;
            state = value;
            onStateChanged?.Invoke(this, state, prevState);
        }
    }
    public bool IsComplete => State == TaskState.Complete;
    public Quest Owner { get; private set; }

    public void Setup(Quest owner)
    {
        Owner = owner;
    }

    public void Start()
    {
        State = TaskState.Running;
        if (initialSccuessValue == null)
            return;

        currentSuccess = initialSccuessValue.GetValue(this);
    }

    public void End()
    {
        onStateChanged = null;
        onSuccessChanged = null;
    }

    public void ReceiveReport(int successCount)
    {
        CurrentSuccess = action.Run(this, CurrentSuccess,successCount);
    }

    public void Complete()
    {
        CurrentSuccess = needSuccessToComplete;
    }

    public bool IsTarget(string category,object target)
    {
        return Category == category && targets.Any(x => x.IsEqual(target)) && (IsComplete != true || (IsComplete && canReceiveReport));
    }
}
