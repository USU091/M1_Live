using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : InitBase
{
    //SkillComponent Ŭ���� ��� ����
    //��� ��ų���� ��� ������, �װ� ������Ʈ ������ �ٸ� ������Ʈ�� ���� �� �ֵ��� ����� ��


    public List<SkillBase> SkillList { get; } = new List<SkillBase>();

    Creature _owner;        //��ų�� ��� �ִ� ��ü

    public void SetInfo(Creature owner, List<int> skillTemplateIDs)
    {
        _owner = owner;

        foreach (int skillTemplateID in skillTemplateIDs)
            AddSkill(skillTemplateID);
    }

    public void AddSkill(int skillTemplateID = 0)
    {
        string className = Managers.Data.SkillDic[skillTemplateID].ClassName;

        SkillBase skill = gameObject.AddComponent(Type.GetType(className)) as SkillBase;
        if (skill == null)
            return;

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);

    }
}