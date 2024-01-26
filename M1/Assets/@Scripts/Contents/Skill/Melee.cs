using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public  class Melee : SkillBase
{
    Rigidbody2D _rb;
    Coroutine _coroutine;

    ////생성자 인자로 타입 받아줌
    //public Melee() : base(Define.SkillType.Sequence)
    //{
    //}

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }
    public override void ActionSkill(Action callback = null)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(CoStartSkill(callback));
    }

    IEnumerator CoStartSkill(Action callback = null)
    {
        _rb = GetComponent<Rigidbody2D>();

        Managers.Data.SkillDic.TryGetValue((int)ESkillID.Melee, out SkillData skillData);

        yield return new WaitForSeconds(skillData.CoolTime);

//        GetComponent<Animator>().Play(skillData.AniName);
        PlayAnimation(0, skillData.AniName, true);

        callback?.Invoke();
    }
}
