using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class QuestTask
{
    public QuestTaskData _questTaskData;
    public int Count { get; set; }

    public QuestTask(QuestTaskData questTaskData)
    {
        _questTaskData = questTaskData;
    }

    public bool IsCompleted()
    {
        //TODO
        return false;

    }

    public void OnHandleBroadcastEvent(EBroadcastEventType eventType, int value)
    {

    }
}



public class Quest
{
    public QuestSaveData SaveData { get; set; }
    public QuestData _questData;

    public List<QuestTask> _questTasks = new List<QuestTask>();        //�������� ����Ʈ�� óġ �� ���� �޸𸮿� ��� ���� count.


    public int TemplateID
    {
        get { return SaveData.TemplateId; }
        set { SaveData.TemplateId = value; }
    }

    public EQuestState State
    {
        get { return SaveData.State; }
        set { SaveData.State = value; }
    }

    public Quest(int templateId)
    {
        TemplateID = templateId;
        State = EQuestState.None;

        _questData = Managers.Data.QuestDic[templateId];

        _questTasks.Clear();

        foreach(QuestTaskData taskData in _questData.QuestTasks)
        {
            _questTasks.Add(new QuestTask(taskData));
        }
    }

    public bool IsCompleted()
    {
        for(int i = 0; i < _questData.QuestTasks.Count; i++)
        {
            if (i < SaveData.ProgressCount.Count)
                return false;

            QuestTaskData questTaskData = _questData.QuestTasks[i];

            int progressCount = SaveData.ProgressCount[i];
            if (progressCount < questTaskData.ObjectiveCount)
                return false;
        }

        return true;
    }

    public static Quest MakeQuest(QuestSaveData saveData)
    {
        if (Managers.Data.QuestDic.TryGetValue(saveData.TemplateId, out QuestData questData) == false)
            return null;

        Quest quest = null;

        //TODO 

        quest = new Quest(saveData.TemplateId);

        if(quest != null)
        {
            quest.SaveData = saveData;
        }

        return quest;

    }

    public void OnHandleBroadcastEvent(EBroadcastEventType eventType, int value)
    {
        switch(eventType)
        {
            case EBroadcastEventType.ChangeMeat:
                break;
            case EBroadcastEventType.ChangeWood:
                break;
            case EBroadcastEventType.ChangeMineral:
                break;
            case EBroadcastEventType.ChangeGold:
                break;
            case EBroadcastEventType.KillMonster:
                break;

        }
    }

}