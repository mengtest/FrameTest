using Core.Res;
using UnityEngine;

public class ChessManager
{
    private static ChessManager instance;
    public static ChessManager Instance
    {
        get {
            if (instance == null)
                instance = new ChessManager();
            return instance;
        }
    }

    Chess[,] chesses = new Chess[9, 10];

    private Chess selectedChess = null;

    public void ChangeSelectedChess(Chess chess)
    {
        if (selectedChess == chess)
            return;
        selectedChess.OnUnSelected.Invoke();
        selectedChess = chess;
        chess.OnSelected.Invoke();
    }

    public void AddChess(int x, int y, Chess chess)
    {
        if (chesses[x, y] == null)
        {
            chesses[x, y] = chess;
        }
        else
        {
            UnityEngine.Debug.LogErrorFormat("重复添加位置，x %d, y %d", x, y);
        }
    }

    void RemoveChess(int x, int y)
    {
        if (chesses[x, y] != null)
        {
            chesses[x, y] = null;
        }
    }

    public void Eat(int x, int y, Chess chess)
    {
        RemoveChess(x, y);
        AddChess(x, y, chess);
    }

}

public class Main : MonoBehaviour {

    public GameObject ChessInstance;
    public GameObject selectObj;
    public Transform bd;
    // Use this for initialization
    void Start () {
        //InitChess();
        AssetLoaderManager.Instance.Initialize();
        //异步 Test
        AssetLoaderManager.Instance.LoadAssetAsync<GameObject>("cube", DownLoadComplete);

        //同步 Test
        //UnityEngine.Object obj = AssetLoaderManager.Instance.LoadAsset<GameObject>("cube");
        //DownLoadComplete(obj, null);
    }

    void DownLoadComplete(UnityEngine.Object obj, object[] args)
    {
        if (obj != null)
        {
            GameObject.Instantiate(obj as GameObject);
        }
    }

	// Update is called once per frame
	void Update () {
        AssetLoaderManager.Instance.OnUpdate();

    }

    string[] chessNames = new string[] { "jiang", "shi","xiang","ma",  "ju",  "pao","bing" };
    //public List<Chess> Chesses = new List<Chess>();

    void InitChess()
    {
        for (int column = -5; column <= 5; column++)
        {
            for (int row = 0; row < 5; row++)
            {
                Chess chessL = null;
                Chess chessR = null;
                if (System.Math.Abs(column) == 2)
                {
                    if (row == 0 || row == 2 || row == 4)
                    {
                        chessL = new Chess();
                        chessL.x = row;
                        chessL.y = column;
                        chessL.name = chessNames[6];

                        if (row != 0)
                        {
                            chessR = new Chess();
                            chessR.x = -row;
                            chessR.y = column;
                            chessR.name = chessNames[6];
                        }
                    }
                }
                else if (System.Math.Abs(column) == 3)
                {
                    if (row == 3)
                    {
                        chessL = new Chess();
                        chessL.x = row;
                        chessL.y = column;
                        chessL.name = chessNames[5];

                        {
                            chessR = new Chess();
                            chessR.x = -row;
                            chessR.y = column;
                            chessR.name = chessNames[5];
                        }
                    }
                }
                else if (System.Math.Abs(column) == 5)
                {
                    chessL = new Chess();
                    chessL.x = row;
                    chessL.y = column;
                    chessL.name = chessNames[row];
                    if (row != 0)
                    {
                        chessR = new Chess();
                        chessR.x = -row;
                        chessR.y = column;
                        chessR.name = chessNames[row];
                    }
                }

                if (null != chessL)
                {
                    chessL.isRed = column > 0 ? 1 : -1;
                    //chessL.LoadSprite();
                    chessL.Create(ChessInstance, bd);
                    chessL.selectedObj = selectObj;
                    ChessManager.Instance.AddChess(chessL.x, chessL.y, chessL);
                }


                if (null != chessR)
                {
                    chessR.isRed = column > 0 ? 1 : -1;
                    //chessR.LoadSprite();
                    chessR.Create(ChessInstance, bd);
                    chessR.selectedObj = selectObj;
                    ChessManager.Instance.AddChess(chessR.x, chessR.y, chessR);
                }
            }
        }
       
    }
}


[System.Serializable]
public class Chess
{
  
    public int x,y;
    public string name;
    public AssetBundle assetBundle;
    public Sprite sprite;
    public GameObject gameObject;
    public UnityEngine.UI.Image image;
    public int isRed;
    public GameObject selectedObj;

    public UnityEngine.Events.UnityAction OnSelected;
    public UnityEngine.Events.UnityAction OnUnSelected;

    public string AbName()
    {
        return "imgs." + (isRed == 1 ? "red." : "black.") + name + ".unity3d";
    }

   

    public void Create(GameObject ChessInstance, Transform transform)
    {
        gameObject = GameObject.Instantiate(ChessInstance);
        image = gameObject.GetComponent<UnityEngine.UI.Image>();
       
        gameObject.transform.SetParent(transform);
        gameObject.transform.localPosition = GetRealPostion();
        gameObject.transform.localScale = Vector3.one;
        image.sprite = sprite;
        image.SetNativeSize();
        gameObject.SetActive(true);
        gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(
            () => {
                //selectedObj.transform.SetParent(gameObject.transform);
                //selectedObj.transform.localPosition = Vector3.zero;
                //selectedObj.SetActive(true);
                ChessManager.Instance.ChangeSelectedChess(this);
            }
            );
    }

    public Vector2 GetRealPostion()
    {
        Vector2 vector2 = new Vector2();
        vector2.y = y* 40 - 20 * isRed;
        vector2.x = 40 * x;
        return vector2;
    }
}





