using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
	public static bool Initialized { get; set; } = false;
	private static Managers s_instance;
	private static Managers Instance { get { Init(); return s_instance; } }
	//������ ����� �� s_instance�� null�� �ǰ�, Instance�� ����Ϸ��� �ϸ�, s_instance�� ���̱� ������
	//�ٽ� �����Ϸ��� �� �� �ִ�. �� �κ��� ���ϱ� ���ؼ� Initialized ������ �ּ� �� �� �������� �ʰԹ���

    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();
	private MapManager _map = new MapManager();
	private InventoryManager _inventory = new InventoryManager();


    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static InventoryManager Inventory { get { return Instance?._inventory; } }
    #endregion

    #region Core
    private DataManager _data = new DataManager();
	private PoolManager _pool = new PoolManager();
	private ResourceManager _resource = new ResourceManager();
	private SceneManagerEx _scene = new SceneManagerEx();
	private SoundManager _sound = new SoundManager();
	private UIManager _ui = new UIManager();

	public static DataManager Data { get { return Instance?._data; } }
	public static PoolManager Pool { get { return Instance?._pool; } }
	public static ResourceManager Resource { get { return Instance?._resource; } }
	public static SceneManagerEx Scene { get { return Instance?._scene; } }
	public static SoundManager Sound { get { return Instance?._sound; } }
	public static UIManager UI { get { return Instance?._ui; } }
	#endregion


	#region

	private static Define.ELanguage _language = Define.ELanguage.Korean;
	public static Define.ELanguage Language
    {
        get { return _language; }
		set
        {
			_language = value;
        }
    }

	public static  string GetText(string textId)
    {
		switch(_language)
        {
			case Define.ELanguage.Korean:
				return Managers.Data.TextDic[textId].KOR;
			case Define.ELanguage.English:
				break;
			case Define.ELanguage.French:
				break;
			case Define.ELanguage.SimplifiedChinese:
				break;
			case Define.ELanguage.TraditionalChinese:
				break;
			case Define.ELanguage.Japanese:
				break;
		}

		return "";
	}

    #endregion

    public static void Init()
	{
		if (s_instance == null && Initialized == false)
		{
			Initialized = true;		//��� Ŭ������ Destroy�Ǹ鼭 Ȥ�ó� �� Ŭ������ ������ų ���� �����ϱ� ���� �Ҹ��� �Լ� �߰�

			GameObject go = GameObject.Find("@Managers");
			if (go == null)
			{
				go = new GameObject { name = "@Managers" };
				go.AddComponent<Managers>();
			}

			DontDestroyOnLoad(go);
			// �ʱ�ȭ
			s_instance = go.GetComponent<Managers>();
		}
	}


}
