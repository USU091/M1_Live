using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataManager 
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

	public Dictionary<int, Data.TestData> TestDic { get; private set; } = new Dictionary<int, Data.TestData>();

	public void Init()
	{
		TestDic = LoadJson<Data.TestDataLoader, int, Data.TestData>("TestData").MakeDict();
	}

	private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
	{
		TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
		return JsonConvert.DeserializeObject<Loader>(textAsset.text);
	}
}
