using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] private List<Quest> quests;
    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string id) => quests.FirstOrDefault(x => x.ID == id);

#if UNITY_EDITOR

    [ContextMenu("FindQuests")]
    private void FindQuests()
    {
        FindQuestsBy<Quest>();
    }

    [ContextMenu("FindAchievements")]
    private void FindAchievements()
    {
        FindQuestsBy<Achievement>();
    }
    
    private void FindQuestsBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

        AssetDatabase.StartAssetEditing();
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Quest quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (quest.GetType() == typeof(T))
                quests.Add(quest);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        AssetDatabase.StopAssetEditing();
    }
#endif
}