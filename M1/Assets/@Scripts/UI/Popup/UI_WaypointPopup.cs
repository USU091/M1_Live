using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class UI_WaypointPopup : UI_Popup
{
    enum GameObjects
    {
        WaypointList
    }

    enum Buttons
    {
        CloseButton,
    }

    List<UI_StageItem> _items = new List<UI_StageItem>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        Refresh();
        return true;
    }

    public void SetInfo()
    {
        Refresh();
    }


    //Refresh()함수는  Update()함수와의 차이가 무엇인가 ?
    // 어쨌든 Refresh()함수의 호출 이유는 팝업을 열었을 때 인벤토리창에 아이템들이 보일 때, 운영자가 아이템을
    //넣어주는 경우 갱신되어야 하니까 넣어준다고 함.
    void Refresh()
    {
        if (_init == false)
            return;

        _items.Clear();

        GameObject parent = GetObject((int)GameObjects.WaypointList);

        foreach (var stage in Managers.Map.StageTransition.Stages)
        {
            UI_StageItem item = Managers.UI.MakeSubItem<UI_StageItem>(parent.transform);

            item.SetInfo(stage, () =>
            {
                Managers.UI.ClosePopupUI(this);
            });

            _items.Add(item);
        }
    }

    void OnClickCloseButton(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI(this);
    }
}

