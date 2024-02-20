using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]  //나중에 유니티에서 보기 위해 붙임
public class CreatureStat
{
    public float BaseValue { get; private set; }        //원본 값

    private bool _isDirty = true;       //값이 변했는지 여부의 boolean 타입 변수


    //변화되는 Stat값
    [SerializeField]
    private float _value;
    public virtual float Value
    {
        get
        {
            if(_isDirty)
            {
                _value = CalculateFinalValue();
                _isDirty = false;
            }
            return _value;
        } 

        private set { _value = value; }
    }

    public List<StatModifier> StatModifiers = new List<StatModifier>();     //스탯마다 어떠한 변화를 입었는지 기록하는 용도의 List선언. 어떤 애 때문에 곱셈연산, 덧셈 두 번 등을 기록하는 용도

    public CreatureStat()
    {
    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    //Buff, Item++등 Stat에 변화를 주는 StatModifier클래스를 생성하여 호출할 때 리스트에 추가함
    public virtual void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        StatModifiers.Add(modifier);
    }


    //Debuff, Item--등 Stat에 변화를 주는 StatModifier클래스를 생성하여 호출할 때 리스트에 추가함
    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if(StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }

        return false;
    }


    //ex)어떤 Buff관련해서 ++ or -- 된 것들을 다 지워야 한다면 이 함수를 통해 리스트에서 삭제처리됨
    public virtual bool ClearModifiersFromSource(object source)
    {
        int numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

        if(numRemovals > 0)
        {
            _isDirty = true;
            return true;
        }
        return true;
    }

    private int CompareOrder(StatModifier a, StatModifier b)
    {
        if (a.Order == b.Order)
            return 0;

        return (a.Order < b.Order) ? -1 : 1;
    }

    //처음부터 Stat관련된 부분을 연산하는 함수
    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < StatModifiers.Count; i++)
        {
            StatModifier modifier = StatModifiers[i];

            switch (modifier.Type)
            {
                case Define.EStatModType.Add:
                    finalValue += modifier.Value;
                    break;
                case Define.EStatModType.PercentAdd:
                    sumPercentAdd += modifier.Value;
                    if(i == StatModifiers.Count - 1 || StatModifiers[i+1].Type != Define.EStatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                    break;
                case Define.EStatModType.PercentMult:
                    finalValue *= 1 + modifier.Value;
                    break;

            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
