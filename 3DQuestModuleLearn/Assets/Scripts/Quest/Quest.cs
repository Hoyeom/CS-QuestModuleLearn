using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive,
    Running,
    Complete,
    Cancel,
    WaitingForCompletion
}

[CreateAssetMenu(menuName = "Quest/Quest",fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region Events

    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CanceledHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);

    #endregion
    
    [SerializeField] private Category category;
    [SerializeField] private Sprite icon;

    [Header("Text")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea] private string description;

    [Header("Task")]
    [SerializeField] private TaskGroup[] taskGroups;

    [Header("Reward")] 
    [SerializeField] private Reward[] rewards;
    
    [Header("Option")]
    [SerializeField] private bool useAutoComplete;
    [SerializeField] private bool isCancelable;

    [Header("Condition")]
    [SerializeField] private Condition[] acceptionConditions;
    [SerializeField] private Condition[] cancelConditions;
    
    private int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon;
    public string ID => id;
    public string DisplayName => displayName;
    public string Description => description;
    
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;

    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsComplatable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;
    public bool IsCancelable => isCancelable && cancelConditions.All(x => x.IsPass(this));
    public bool IsAcceptable => acceptionConditions.All(x => x.IsPass(this));
    
    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CanceledHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, "This quest has already been registered.");

        foreach (TaskGroup taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (Task task in taskGroup.Tasks)
                task.onSuccessChanged += OnSuccessChanged;
        }
        
        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }
    
    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(!IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        
        if(IsComplete)
            return;

        CurrentTaskGroup.ReceiveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;
                if (useAutoComplete)
                    Complete();
            }
            else
            {
                TaskGroup prevTaskGroup = taskGroups[currentTaskGroupIndex++];
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
        CheckIsRunning();

        foreach (TaskGroup taskGroup in taskGroups)
            taskGroup.Complete();

        State = QuestState.Complete;
        
        foreach (Reward reward in rewards)
            reward.Give(this);
        
        onCompleted?.Invoke(this);

        onTaskSuccessChanged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
    }

    public void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(isCancelable, "This quest can't be canceled");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);
    }

    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);

    [Conditional("UNITY_EDITOR")]
    private void CheckIsRunning()
    {
        Debug.Assert(!IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        Debug.Assert(!IsComplatable,"This quest has already been completed.");
    }
}