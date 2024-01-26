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


	#region Skill
	[Serializable]
	public class SkillData
	{
		public int SkillID;
		public int Damage;
		public int Buff;
		public string AniName;
		public int LayerOrder;  //스킬 이펙트 중 먼저 그려질 순서
		public float CoolTime;
		public int RepeatYN;	//Repeat형인지 한 번 그려질 것인지
		public float Duration;  //Repeat타입이라면 몇 초 후 다시 그려질 것인지
		public float Speed;		//스킬실행하는 스피드
	}

	public class SkillDataLoader : ILoader<int, SkillData>
    {
		public List<SkillData> skills = new List<SkillData>();
		public Dictionary<int, SkillData> MakeDict()
        {
			Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
			foreach (SkillData skill in skills)
				dict.Add(skill.SkillID, skill);
			return dict;
        }
    }

	#endregion
}