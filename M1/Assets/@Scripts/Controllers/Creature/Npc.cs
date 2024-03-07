using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Data;
using Spine.Unity;

public class Npc : BaseObject
{
    public NpcData Data { get; set; }

    private SkeletonAnimation _skeletonAnim;
    private UI_NpcInteraction _ui;
    public override bool Init()
    {
        if( base.Init() == false)
            return false;

        ObjectType = EObjectType.Npc;
        return true;
    }


    public void SetInfo(int dataId)
    {
        Data = Managers.Data.NpcDic[dataId];
        gameObject.name = $"{Data.DataId}_{Data.Name}";

        #region Spine Animation
        SetSpineAnimation(Data.SkeletonDataID, SortingLayers.NPC);
        PlayAnimation(0, AnimName.IDLE, true);
        #endregion

        //Npc ��ȣ�ۿ��� ���� ��ư
        GameObject button = Managers.Resource.Instantiate("UI_NpcInteraction", gameObject.transform);
        button.transform.localPosition = new Vector3(0f, 3f);
        _ui = button.GetComponent<UI_NpcInteraction>();
        _ui.SetInfo(DataTemplateID, this);
    }

}
