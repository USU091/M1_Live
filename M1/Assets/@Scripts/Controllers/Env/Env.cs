using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Env : BaseObject
{
    private Data.EnvData _data;

    public EEnvState _envState = Define.EEnvState.Idle;
    public EEnvState EnvState
    {
        get { return _envState; }
        set
        {
            _envState = value;
            UpdateAnimation();
        }
    }

    #region Stat
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;

        return true;

    }

    public void SetInfo(int templateID)
    {
        DataTemplateID = templateID;
        _data = Managers.Data.EnvDic[templateID];

        //Stat
        Hp = _data.MaxHp;
        MaxHp = _data.MaxHp;

        //Spine
        string randSpine = _data.SkeletonDataIDs[Random.Range(0, _data.SkeletonDataIDs.Count)];
        SetSpineAnimation(randSpine, SortingLayers.ENV);
    }

    // ä���� ���� ���� �Ϲ����� �ִϸ��̼� ����� �ƴ�. �׷��Ƿ� �������̵� �ؼ� ���� ����������
    protected override void UpdateAnimation()
    {
        switch (EnvState)
        {
            case EEnvState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case EEnvState.OnDamaged:
                PlayAnimation(0, AnimName.DAMAGED, false);
                break;
            case EEnvState.Dead:
                PlayAnimation(0, AnimName.DEAD, false);
                break;
        }

    }
}