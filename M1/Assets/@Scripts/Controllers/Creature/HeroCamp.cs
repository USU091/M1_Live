using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroCamp : BaseObject
{
    Vector2 _moveDir = Vector2.zero;

    public float Speed { get; set; } = 5.0f;

    public Transform Pivot { get; private set; }
    public Transform Destination { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        // unity 2023에서 새로 추가된 기능. 그 밑 버전 사용불가
        Collider.includeLayers = (1 << (int)Define.ELayer.Obstacle);
        Collider.excludeLayers = (1 << (int)Define.ELayer.Monster) | (1 << (int)Define.ELayer.Hero);

        ObjectType = Define.EObjectType.HeroCamp;

        Pivot = Util.FindChild<Transform>(gameObject, "Pivot", true);
        Destination = Util.FindChild<Transform>(gameObject, "Destination", true);

        return true;
    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * Speed);
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;

        if(dir != Vector2.zero)
        {
            //아크탄젠트로 로테이션 계산. 뒷부분은 라디안에서 디그리 변환으로 넣어줌
            float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;
            Pivot.eulerAngles = new Vector3(0, 0, angle);
        }
    }

}
