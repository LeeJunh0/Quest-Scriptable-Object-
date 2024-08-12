using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
public class QuestSystem : MonoBehaviour
{
    #region Event
    public delegate void QuestRegisterHandler(Quest newQuest);
    public delegate void QuestCompletedHandler(Quest quest);
    public delegate void QuestCanceledHandler(Quest quest);
    #endregion

    static QuestSystem instance = null;
    static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if(isApplicationQuitting == false && instance == null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if(instance == null)
                {
                    instance = new QuestSystem();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    List<Quest> activeQuests = new List<Quest>();
    List<Quest> completeQuests = new List<Quest>();
    
    List<Quest> activeAchievements = new List<Quest>();
    List<Quest> completeAchievements = new List<Quest>();

    QuestDataBase questDatabase;
    QuestDataBase achievementDatabase;

    public event QuestRegisterHandler onQuestRegistered;
    public event QuestCompletedHandler onQuestCompleted;
    public event QuestCanceledHandler onQuestCanceled;

    public event QuestRegisterHandler onAchievementRegistered;
    public event QuestCompletedHandler onAchievementCompleted;

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompleteQuests => completeQuests;
    public IReadOnlyList<Quest> ActiveAchievements => activeAchievements;
    public IReadOnlyList<Quest> CompleteAchievements => completeAchievements;

    private void Awake()
    {
        questDatabase = Resources.Load<QuestDataBase>("QuestDataBase");
        achievementDatabase = Resources.Load<QuestDataBase>("AchievementDataBase");

        foreach (var achievement in achievementDatabase.Quests) 
        {
            Register(achievement);
        }
    }

    public Quest Register(Quest quest)
    {
        var newquest = quest.Clone();

        if(newquest is Achievement)
        {
            newquest.onCompleted += OnAchievementCompleted;
            activeAchievements.Add(newquest);

            newquest.OnRegister();
            onAchievementRegistered?.Invoke(newquest);
        }
        else
        {
            newquest.onCompleted += OnQuestCompleted;
            newquest.onCanceled += OnQuestCanceld;

            activeQuests.Add(newquest);

            newquest.OnRegister();
            onQuestRegistered?.Invoke(newquest);
        }
        return newquest;
    }

    void ReceiveReport(List<Quest> quests, string category, object target, int successCount)
    {
        foreach(var quest in quests.ToArray())
        {
            quest.ReceiveReport(category,target, successCount);
        }
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        ReceiveReport(activeQuests, category, target, successCount);
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    public void Receivereport(Category category, TaskTarget target, int successCount) 
    {
        ReceiveReport(category.CodeName, target.Value, successCount);
    }

    public bool ContainsInActiveQuests(Quest quest) { return activeQuests.Any(x => x.CodeName == quest.CodeName); }
    public bool ContainsInCompleteQuests(Quest quest) { return completeQuests.Any(x => x.CodeName == quest.CodeName); }
    public bool ContainsInActiveAchievements(Quest quest) { return activeAchievements.Any(x => x.CodeName == quest.CodeName); }
    public bool ContainsInCompleteAchievements(Quest quest) { return completeAchievements.Any(x => x.CodeName == quest.CodeName); }

    #region CallBack
    void OnQuestCompleted(Quest quest)
    {
        activeQuests.Remove(quest);
        completeQuests.Add(quest);
    }

    void OnQuestCanceld(Quest quest)
    {
        activeQuests.Remove(quest);
        onQuestCanceled?.Invoke(quest);

        Destroy(quest, Time.deltaTime);
    }

    void OnAchievementCompleted(Quest achievement)
    {
        activeAchievements.Remove(achievement);
        completeAchievements.Add(achievement);

        onAchievementCompleted?.Invoke(achievement);
    }
    #endregion
}
