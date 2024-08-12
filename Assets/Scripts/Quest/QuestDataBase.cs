using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Quest/QuestDataBase")]
public class QuestDataBase : ScriptableObject
{
    [SerializeField]
    List<Quest> quests;

    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string codename)
    {
        return quests.FirstOrDefault(x => x.CodeName == codename);
    }

#if UNITY_EDITOR
    [ContextMenu("FindQuests")]
    void FindQuest()
    {
        FindQuestBy<Quest>();
    }

    [ContextMenu("FindAchievement")]
    void FindAchievement()
    {
        FindQuestBy<Achievement>();
    }

    void FindQuestBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (quest.GetType() == typeof(T))
                quests.Add(quest);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}
