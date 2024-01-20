using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{

    private BaseObject _target;
    public BaseObject Target
    {
        get { return _target; }
        set { _target = value; }
    }

    public override bool Init()
    {
        if( base.Init() == false)
            return false;

        Camera.main.orthographicSize = 15.0f;

        return true;
    }

    //일반적인 오브젝트와 다르게 카메라는 다른애들이 동작한 후에 마지막에 업뎃해줘야 부들부들 떨리는 현상을 막아줌
    void LateUpdate()
    {
        //매 프레임마다 주시하는 캐릭터를 따라가게끔 설정

        if (Target == null)
            return;

        //TODO
        Vector3 targetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);
        transform.position = targetPosition;

    }
}
