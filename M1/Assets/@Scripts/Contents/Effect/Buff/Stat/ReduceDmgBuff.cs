using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceDmgBuff : BuffBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void ApplyEffect()
    {
        base.ApplyEffect();
        AddModifier(Owner.ReduceDamageRate, this);
    }

    public override bool ClearEffect(Define.EEffectClearType clearType)
    {
        if (base.ClearEffect(clearType) == true)
            RemoveModifier(Owner.ReduceDamageRate, this);

        return true;
    }
}
