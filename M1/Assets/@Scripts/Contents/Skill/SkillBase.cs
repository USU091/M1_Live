using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillBase : BaseObject
{
    public float CoolTime { get;  set; } = 1.0f;
    public Creature Owner { get; set; }
    public Data.SkillData SkillData { get; protected set; }

    //public SkillType SkillType { get; set; } = Define.SkillType.None;
   
    ////������ ���ڷ� Ÿ�� �޾���
    //public SkillBase(SkillType type)
    //{
    //    SkillType = type;
    //}

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Skill;

        return true;
    }


    public virtual void ActionSkill(Action callback = null)
    {
        //�����Լ��� ��밡���ϰ�


    }


}
