using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;

public enum QuestState
{
    Inactive,
    Running,
    Complete,
    Cancel,
    WaitingForCompletion
}

[CreateAssetMenu(menuName = "Quest/Quest", fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region Event
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CancelHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    #endregion
    [SerializeField]
    Category category;
    [SerializeField]
    Sprite icon;

    [Header("Text")]
    [SerializeField]
    string codeName;
    [SerializeField]
    string displayName;
    [SerializeField,TextArea]
    string description;

    [Header("Task")]
    [SerializeField]
    TaskGroup[] taskGroups;

    [Header("Option")]
    [SerializeField]
    bool useAutoComplete;

    int currentTaskGroupIndex;
    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsCompletable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;

    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CompletedHandler onCancelled;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister()
    {
        Debug.Assert(IsRegistered == false, "또 할당하는중");

        foreach(var taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (var task in taskGroup.Tasks)
                task.onSuccessChanged += OnSuccessChanged;
        }
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(IsRegistered == false, "또 할당하는중");
        Debug.Assert(IsCancel == false, "This quest has been canceled");
        if (IsComplete == true)
            return;

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;
                if (useAutoComplete == true)
                    Complete();
            }
            else
            {
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];
                prevTaskGroup.End();
                CurrentTaskGroup.Start();
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        else
            State = QuestState.Running;
    }

    public void Complete()
    {
        Debug.Assert(IsRegistered == false, "또 할당하는중");
        Debug.Assert(IsCancel == false, "This quest has been canceled");
        Debug.Assert(IsCompletable == false, "THis quest has already been completed");

        foreach(var taskGroup in taskGroups)
            taskGroup.Complete();

        State = QuestState.Complete;
        onCompleted?.Invoke(this);

        onTaskSuccessChanged = null;
        onCancelled = null;
        onNewTaskGroup = null;
        onCompleted = null;
    }

    public void Cancel()
    {

    }

    public void OnSuccessChanged(Task task, int currentSuccess,int prevSuccess)
    {
        onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);
    }
}
