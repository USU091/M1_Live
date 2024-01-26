using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager 
{
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
	public HashSet<Env> Envs { get; } = new HashSet<Env>();
	public HeroCamp Camp { get; private set; }
	public HashSet<SkillBase> Skills { get; } = new HashSet<SkillBase>();

	#region Roots
	public Transform GetRootTransform(string name)
	{
		GameObject root = GameObject.Find(name);
		if (root == null)
			root = new GameObject { name = name };

		return root.transform;
	}

	public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
	public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
	public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }

	#endregion

	public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
    {
		string prefabName = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate(prefabName);
		go.name = prefabName;
		go.transform.position = position;

		BaseObject obj = go.GetComponent<BaseObject>();

		if (obj.ObjectType == EObjectType.Creature)
		{

			//Data Check
			if(templateID != 0 && Managers.Data.CreatureDic.TryGetValue(templateID, out Data.CreatureData data) == false)
            {
				Debug.LogError($"ObjectManager Spawn Crature Failed! TryGetValue TemplateID : {templateID}");
				return null;
            }

			Creature creature = go.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case ECreatureType.Hero:
					obj.transform.parent = HeroRoot;
					Hero hero = creature as Hero;
					Heroes.Add(hero);
					break;
				case ECreatureType.Monster:
					obj.transform.parent = MonsterRoot;
					Monster monster = creature as Monster;
					Monsters.Add(monster);
					break;
			}
			creature.SetInfo(templateID);		//���⼭ ���ø�IDȣ������

		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			// TODO
		}
		else if (obj.ObjectType == EObjectType.Env)
		{
			// Data Check
			if(templateID != 0 && Managers.Data.EnvDic.TryGetValue(templateID, out Data.EnvData data) == false )
            {
				Debug.LogError($"ObjectManager Spawn Env Failed !  TryGetValue TemplateID : {templateID}");
				return null;

            }

			obj.transform.parent = EnvRoot;
			Env env = go.GetComponent<Env>();
			Envs.Add(env);

			env.SetInfo(templateID);

		}
		else if(obj.ObjectType == EObjectType.HeroCamp)
        {
			Camp = go.GetComponent<HeroCamp>();
        }
        else if (obj.ObjectType == EObjectType.Skill)
        {
            if (Managers.Data.SkillDic.TryGetValue(templateID, out Data.SkillData skillData) == false)
            {
                Debug.LogError($"ObjectManager Spawn Skill Failed!{templateID}");
                return null;
            }
            GameObject sgo = Managers.Resource.Instantiate(skillData.AniName, pooling: false);
            sgo.transform.position = position;

        }
        else
		{ }
		return obj as T;
	}


	public void Despawn<T>(T obj) where T : BaseObject
	{
		EObjectType objectType = obj.ObjectType;

		if (obj.ObjectType == EObjectType.Creature)
		{
			Creature creature = obj.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case ECreatureType.Hero:
					Hero hero = creature as Hero;
					Heroes.Remove(hero);
					break;
				case ECreatureType.Monster:
					Monster monster = creature as Monster;
					Monsters.Remove(monster);
					break;
			}
		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			// TODO
		}
		else if (obj.ObjectType == EObjectType.Env)
		{
			// TODO
			Env env = obj as Env;
			Envs.Remove(env);
		}
		else if (obj.ObjectType == EObjectType.HeroCamp)
		{
			Camp = null;
		}
		else
        {

        }
		Managers.Resource.Destroy(obj.gameObject);
	}
}
