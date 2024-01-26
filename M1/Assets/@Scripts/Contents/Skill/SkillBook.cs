using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;


public class SkillBook : MonoBehaviour
{
    public List<SkillBase> Skills { get; } = new List<SkillBase>();


    public T AddSkill<T>(Vector3 position, Transform parent = null) where T : SkillBase
    {
        System.Type type = typeof(T);

        if (type == typeof(Melee))
        {
            var melee = gameObject.GetOrAddComponent<T>();
            Skills.Add(melee as Melee);
            gameObject.GetComponent<Melee>().ActionSkill();
            
            

            return null;
        }


        return null;
    }


}
