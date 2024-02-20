using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]  //���߿� ����Ƽ���� ���� ���� ����
public class CreatureStat
{
    public float BaseValue { get; private set; }        //���� ��

    private bool _isDirty = true;       //���� ���ߴ��� ������ boolean Ÿ�� ����


    //��ȭ�Ǵ� Stat��
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

    public List<StatModifier> StatModifiers = new List<StatModifier>();     //���ȸ��� ��� ��ȭ�� �Ծ����� ����ϴ� �뵵�� List����. � �� ������ ��������, ���� �� �� ���� ����ϴ� �뵵

    public CreatureStat()
    {
    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    //Buff, Item++�� Stat�� ��ȭ�� �ִ� StatModifierŬ������ �����Ͽ� ȣ���� �� ����Ʈ�� �߰���
    public virtual void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        StatModifiers.Add(modifier);
    }


    //Debuff, Item--�� Stat�� ��ȭ�� �ִ� StatModifierŬ������ �����Ͽ� ȣ���� �� ����Ʈ�� �߰���
    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if(StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }

        return false;
    }


    //ex)� Buff�����ؼ� ++ or -- �� �͵��� �� ������ �Ѵٸ� �� �Լ��� ���� ����Ʈ���� ����ó����
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

    //ó������ Stat���õ� �κ��� �����ϴ� �Լ�
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
