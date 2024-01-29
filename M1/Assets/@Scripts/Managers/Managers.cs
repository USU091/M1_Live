using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
	public static bool Initialized { get; set; } = false;
	private static Managers s_instance;
	private static Managers Instance { get { Init(); return s_instance; } }
	//게임이 종료될 때 s_instance가 null이 되고, Instance를 사용하려고 하면, s_instance가 널이기 때문에
	//다시 생성하려고 할 수 있다. 그 부분을 피하기 위해서 Initialized 변수를 둬서 두 번 생성되지 않게방지

    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
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


	public static void Init()
	{
		if (s_instance == null && Initialized == false)
		{
			Initialized = true;		//모든 클래스가 Destroy되면서 혹시나 또 클래스를 생성시킬 때를 방지하기 위한 불리언 함수 추가

			GameObject go = GameObject.Find("@Managers");
			if (go == null)
			{
				go = new GameObject { name = "@Managers" };
				go.AddComponent<Managers>();
			}

			DontDestroyOnLoad(go);
			// 초기화
			s_instance = go.GetComponent<Managers>();
		}
	}


}
