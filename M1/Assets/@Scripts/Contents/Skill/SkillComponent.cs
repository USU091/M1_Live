using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : InitBase
{
    //SkillComponent 클래스 기능 설명
    //모든 스킬들을 들고 있으며, 그걸 컴포넌트 식으로 다른 오브젝트에 붙일 수 있도록 기능할 것


    public List<SkillBase> SkillList { get; } = new List<SkillBase>();

    Creature _owner;        //스킬을 들고 있는 주체


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

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

    public SkillBase GetReadySkill()
    {
        // TEMP
        return SkillList[0];
    }
}
