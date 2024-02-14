using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class SkillComponent : InitBase
{
    //SkillComponent 클래스 기능 설명
    //모든 스킬들을 들고 있으며, 그걸 컴포넌트 식으로 다른 오브젝트에 붙일 수 있도록 기능할 것


    public List<SkillBase> SkillList { get; } = new List<SkillBase>();
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase>();      //사용 가능한 준비된 스킬


    public SkillBase DefaultSkill { get; private set; }
    public SkillBase EnvSkill { get; private set; }
    public SkillBase ASkill { get; private set; }
    public SkillBase BSkill { get; private set; }


    //사용하려는 스킬이 목록에 없다면 기본스킬을 사용하고, 있다면 랜덤으로 사용하도록 인공지능으로 관리
    public SkillBase CurrentSkill
    {
        get
        {
            if (ActiveSkills.Count == 0)
                return DefaultSkill;

            int randomIndex = Random.Range(0, ActiveSkills.Count);
            return ActiveSkills[randomIndex];
        }
    }


    Creature _owner;        //스킬을 들고 있는 주체


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void SetInfo(Creature owner, CreatureData creatureData)
    {
        _owner = owner;

        AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
        AddSkill(creatureData.EnvSkillId, ESkillSlot.Env);
        AddSkill(creatureData.SkillAId, ESkillSlot.A);
        AddSkill(creatureData.SkillBId, ESkillSlot.B);
    }

    public void AddSkill(int skillTemplateID, Define.ESkillSlot skillSlot)
    {
        if (skillTemplateID == 0)
            return;

        if (Managers.Data.SkillDic.TryGetValue(skillTemplateID, out var data) == false)
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;
        if (skill == null)
            return;

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);

        switch (skillSlot)
        {
            case Define.ESkillSlot.Default:
                DefaultSkill = skill;
                break;
            case Define.ESkillSlot.Env:
                EnvSkill = skill;
                break;
            case Define.ESkillSlot.A:
                ASkill = skill;
                ActiveSkills.Add(skill);
                break;
            case Define.ESkillSlot.B:
                BSkill = skill;
                ActiveSkills.Add(skill);
                break;
        }

    }
}

