using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Data
{
	#region CreatureData

	[Serializable]
	public class CreatureData
	{
		public int DataId;
		public string DescriptionTextID;
		public string PrefabLabel;
		public float ColliderOffsetX;
		public float ColliderOffstY;
		public float ColliderRadius;
		public float Mass;
		public float MaxHp;
		public float MaxHpBonus;
		public float Atk;
		public float AtkRange;
		public float AtkBonus;
		public float Def;
		public float MoveSpeed;
		public float TotalExp;
		public float HpRate;
		public float AtkRate;
		public float DefRate;
		public float MoveSpeedRate;
		public string IconImage;		//새로 추가. 스킬이나 캐릭터를 대표하는 sprite이미지 등 추가
		public string SkeletonDataID;
		public string AnimatorName;
		public List<int> SkillIdList = new List<int>();
		public int DropItemId;
	}

	[Serializable]
	public class CreatureDataLoader : ILoader<int, CreatureData>
	{
		public List<CreatureData> creatures = new List<CreatureData>();

		public Dictionary<int, CreatureData> MakeDict()
		{
			Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
			foreach (CreatureData creature in creatures)
				dict.Add(creature.DataId, creature);
			return dict;
		}

	}
	#endregion


	#region Monster
	[Serializable]
	public class MonsterData : CreatureData
    {
		//public int DropItem;
    }

	[Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
		public List<MonsterData> monsters = new List<MonsterData>();

		public Dictionary<int, MonsterData> MakeDict()
		{
			Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
			foreach (MonsterData monster in monsters)
				dict.Add(monster.DataId, monster);
			return dict;
		}
    }
    #endregion

    #region Hero
	[Serializable]
	public class HeroData : CreatureData
    {

    }
	[Serializable]
	public class HeroDataLoader : ILoader<int, HeroData>
    {
		public List<HeroData> heroes = new List<HeroData>();

		public Dictionary<int, HeroData> MakeDict()
        {
			Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
			foreach (HeroData hero in heroes)
				dict.Add(hero.DataId, hero);
			return dict;
        }
	}

	#endregion


	#region SkillData
	[Serializable]
	public class SkillData
	{
		public int DataId;
		public string Name;
		public string ClassName;
		public string ComponentName;
		public string Description;
		public int ProjectileId;
		public string PrefabLabel;
		public string IconLabel;
		public string AnimName;
		public float CoolTime;
		public float DamageMultiplier;
		public float Duration;
		public float NumProjectiles;
		public string CastingSound;
		public float AngleBetweenProj;
		public float SkillRange;
		public float RotateSpeed;
		public float ScaleMultiplier;
		public float AngleRange;
	}

	[Serializable]
	public class SkillDataLoader : ILoader<int, SkillData>
    {
		public List<SkillData> skills = new List<SkillData>();

		public Dictionary<int, SkillData> MakeDict()
        {
			Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
			foreach (SkillData skill in skills)
				dict.Add(skill.DataId, skill);
			return dict;
        }
    }

	#endregion

	#region Env
	[Serializable]
	public class EnvData
    {
		public int DataId;
		public string DescriptionTextID;
		public string PrefabLabel;
		public float MaxHp;
		public int ResourceAmount;
		public float RegenTime;
		public List<String> SkeletonDataIDs = new List<String>();
		public int DropItemId;
    }

	public class EnvDataLoader : ILoader<int, EnvData>
    {
		public List<EnvData> envs = new List<EnvData>();
		public Dictionary<int, EnvData> MakeDict()
        {
			Dictionary<int, EnvData> dict = new Dictionary<int, EnvData>();
			foreach (EnvData env in envs)
				dict.Add(env.DataId, env);
			return dict;
        }
    }
    #endregion

}