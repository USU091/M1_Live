using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class StatModifier
{
    //변화량을 의미하는 클래스, Stat에 영향을 주는 행위 자체(Debuff, Buff, Item++, Item-- 등 Stat에 ++,-- 되는 행위

    public readonly float Value;
    public readonly EStatModType Type;
    public readonly int Order;      //Order는 중요도의 역할을 함, 연산 시 (+, * 의 seq...등 어떤 연산을 먼저 할 것인지에 대한 중요도를 의미함)
    public readonly object Source;

    public StatModifier(float value, EStatModType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;      
        Source = source;
    }

    public StatModifier(float value, EStatModType type) : this(value, type, (int)type, null) { }

    public StatModifier(float value, EStatModType type, int order) : this(value, type, order, null) { }

    public StatModifier(float value, EStatModType type, object source) : this(value, type, (int)type, source) { }

}
