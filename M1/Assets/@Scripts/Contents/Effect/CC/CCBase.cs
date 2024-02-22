using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CCBase : EffectBase
{
    protected ECreatureState lateState;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        EffectType = EEffectType.CrowdControl;
        return true;
    }

    public override void ApplyEffect()
    {
        base.ApplyEffect();

        lateState = Owner.CreatureState;
        if (lateState == ECreatureState.OnDamaged)
            return;

        Owner.CreatureState = ECreatureState.OnDamaged;
    }

    public override bool ClearEffect(EEffectClearType clearType)
    {
        if (base.ClearEffect(clearType) == true)
            Owner.CreatureState = lateState;

        return  true;
    }
}
