using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using Firebase.Firestore;

using TMPro;


public class Manager_OGC : MonoBehaviour
{
    [Header("== 난이도 조절용 ==")]
    public int moodCount;
    public int TermX;
    public int TermY;
    public int safeZone;
    public int treeDetectRange;
    public int stoneDetectRange;
    public int grassDetectRange;
    public int treeGenCount;
    public int stoneGenCount;
    public int grassGenCount;


    [Header("== object ==")]
    public GameObject goRewardUI;
    public GameObject goUnlockUI;
    public GameObject goUnlockParent;
    public GameObject goPopup_Text;
    public GameObject goUnlockCell;
    [SerializeField] private GameObject moodParent;
    public TextMeshProUGUI txtEndorphin;
    public TextMeshProUGUI txtEXP;

    [Header("== Language ==")]
    [SerializeField] TextMeshProUGUI txtRewardPopup;
    [SerializeField] TextMeshProUGUI txtUnlockPopup;

    private static Manager_OGC instance;

    private int pastLevel;
    private List<GameObject> unlockedBuildings = new ();
    private List<Vector3> safePointList = new List<Vector3>();
    private List<GameObject> goList = new List<GameObject>();
    private List<GameObject> goMoodList = new List<GameObject>();
    private List<GameObject> goTreeList = new List<GameObject>();
    private List<GameObject> goStoneList = new List<GameObject>();
    private List<GameObject> goGrassList = new List<GameObject>();
    private List<string> moodNameList = new List<string>();
    private List<string> hatNameList = new List<string>();

    public static Manager_OGC Instance
    {
        get
        {
            if (instance == null)
                return null;

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 감정 리스트 받아놓기
        var handle = Addressables.LoadAssetsAsync<GameObject>("Mood",
            result =>
            {
                moodNameList.Add(result.name);
            }).WaitForCompletion();

        Addressables.Release(handle);

        // 모자 리스트 받아놓기
        handle = Addressables.LoadAssetsAsync<GameObject>("Hat",
            result =>
            {
                hatNameList.Add(result.name);
            }).WaitForCompletion();

        Addressables.Release(handle);

        // 빈 모자 하나 추가해놓기
        hatNameList.Add("null");


        // 랜덤으로 나무 테마 정해서 받아놓기
        var enumValues = System.Enum.GetValues(enumType: typeof(TreeLabel));
        var randomEnum = (TreeLabel)enumValues.GetValue(Random.Range(0, enumValues.Length - 1));
        handle = Addressables.LoadAssetsAsync<GameObject>(randomEnum.ToString(),
            result =>
            {
                goTreeList.Add(result);
            }).WaitForCompletion();


        // 돌 받아놓기
        handle = Addressables.LoadAssetsAsync<GameObject>("Stone_A",
            result =>
            {
                goStoneList.Add(result);
            }).WaitForCompletion();


        // 풀 받아놓기
        handle = Addressables.LoadAssetsAsync<GameObject>("Grass_A",
            result =>
            {
                goGrassList.Add(result);
            }).WaitForCompletion();


        GenerateMap();
        GenerateMood();

        // 튜토리얼
        Manager_Tutorial.Instance.InitTutorial(TutorialKey.OGC);

        // 오감 확인
        CreateTodayMood();

        SetLanguage();
    }

    void SetLanguage()
    {
        txtRewardPopup.text = Manager_Master.Instance.LanguageDict["LAN_TEXT_009"];
        txtUnlockPopup.text = Manager_Master.Instance.LanguageDict["MSG000119"];
    }
    private void GenerateMap()
    {
        foreach(var obj in goList)
        {
            Destroy(obj);
        }
        safePointList.Clear();
        safePointList.Add(new Vector3(0, 5.3f, 69)); // 중앙 나무 위치

        int xMax = (int)MaxValue.moodMoveRangeX;
        int xMin = (int)MinValue.moodMoveRangeX;
        int zMax = (int)MaxValue.moodMoveRangeZ;
        int zMin = (int)MinValue.moodMoveRangeZ;

        int xT = (xMax - xMin) / TermX;
        int zT = (zMax - zMin) / TermY;

        for (int x = xMin; x < xMax; x += xT)
        {
            for (int z = zMin; z < zMax; z += zT)
            {
                StartCoroutine(GenerateTree(x, z, xT, zT));
                StartCoroutine(GenerateStone(x, z, xT, zT));
                StartCoroutine(GenerateGrass(x, z, xT, zT));
            }
        }
    }
    private void GenerateMood()
    {
        foreach (var obj in goMoodList)
        {
            Addressables.ReleaseInstance(obj);
        }
        goMoodList.Clear();

        // 오늘의 감정 정보 받아놓기
        var splitCode = Manager_Master.Instance.userData.previousSelectedMood.Split(',');
        var todayMoodName = splitCode[(int)MoodCode.name];
        var todayHatName = splitCode[(int)MoodCode.hatName];
        
        // 찐감정 생성
        GameObject goJJinMood = Addressables.InstantiateAsync(todayMoodName, new Vector3(Random.Range((int)MinValue.moodMoveRangeX, (int)MaxValue.moodMoveRangeX), 0, Random.Range((int)MinValue.moodMoveRangeZ, (int)MaxValue.moodMoveRangeZ)), new Quaternion(0, 180, 0, 1)).WaitForCompletion();
        goJJinMood.AddComponent<Mood_OGC>().isTodayMood = true;

        if (todayHatName != "null")
        {
            Addressables.InstantiateAsync(todayHatName, goJJinMood.transform.GetChild(2), false);
        }

        // 짭감정 생성
        for (int i = 0; i < moodCount; ++i)
        {
            var hatName = hatNameList[Random.Range(0, hatNameList.Count)];

            if(hatName == todayHatName)
            {
                --i;
                continue;
            }

            GameObject goResult = Addressables.InstantiateAsync(todayMoodName, new Vector3(Random.Range((int)MinValue.moodMoveRangeX, (int)MaxValue.moodMoveRangeX), 0, Random.Range((int)MinValue.moodMoveRangeZ, (int)MaxValue.moodMoveRangeZ)), new Quaternion(0, 180, 0, 1)).WaitForCompletion();
            goResult.AddComponent<Mood_OGC>();
            goMoodList.Add(goResult);

            // 모자생성
            if (hatName != "null")
            {
                Addressables.InstantiateAsync(hatName, goResult.transform.GetChild(2), false);
            }
        }
    }



    IEnumerator GenerateTree(int x, int z, int xT, int zT)
    {
        for (int i = 0; i < treeGenCount; ++i)
        {
            var treePos = GetPos(x, z, xT, zT, safeZone);
            while (!PossiblePosition(treePos, treeDetectRange))
            {
                treePos = GetPos(x, z, xT, zT, safeZone);
                // Debug.log("Tree 루프");
                yield return null;
            }

            var go = Instantiate(goTreeList[Random.Range(0, goTreeList.Count)], treePos, Quaternion.Euler(0, Random.Range(0, 361), 0));
            go.GetComponent<Building>().enabled = false;
            go.GetComponent<BoxCollider>().enabled = false;
            goList.Add(go);
            safePointList.Add(go.transform.position);
        }
    }
    IEnumerator GenerateStone(int x, int z, int xT, int zT)
    {
        for (int i = 0; i < stoneGenCount; ++i)
        {
            var stonePos = GetPos(x, z, xT, zT, safeZone);
            while (!PossiblePosition(stonePos, stoneDetectRange))
            {
                stonePos = GetPos(x, z, xT, zT, safeZone);
                yield return null;
            }

            var go = Instantiate(goStoneList[Random.Range(0, goStoneList.Count)], stonePos, Quaternion.Euler(0, Random.Range(0, 361), 0));
            go.GetComponent<Building>().enabled = false;
            go.GetComponent<BoxCollider>().enabled = false;
            goList.Add(go);
            safePointList.Add(go.transform.position);
        }
    }
    IEnumerator GenerateGrass(int x, int z, int xT, int zT)
    {
        for (int i = 0; i < grassGenCount; ++i)
        {
            var grassPos = GetPos(x, z, xT, zT, 2);
            while (!PossiblePosition(grassPos, grassDetectRange))
            {
                grassPos = GetPos(x, z, xT, zT, 2);
                yield return null;
            }

            var go = Instantiate(goGrassList[Random.Range(0, goGrassList.Count)], grassPos, Quaternion.Euler(0, Random.Range(0, 361), 0));
            go.GetComponent<Building>().enabled = false;
            go.GetComponent<BoxCollider>().enabled = false;
            goList.Add(go);
        }
    }

    private Vector3 GetPos(int x, int z, int xT, int zT, int _safeZone = 0)
    {
        return new Vector3(Random.Range(x + _safeZone + 0.0f, x + xT - _safeZone + 0.0f), 0.0f, Random.Range(z + _safeZone + 0.0f, z + zT - _safeZone + 0.0f));
    }

    private bool PossiblePosition(Vector3 _, int _detectRange = 0)
    {
        foreach(var element in safePointList)
        {
            if (element.x - _detectRange < _.x && element.x + _detectRange > _.x && element.z - _detectRange < _.z && element.z + _detectRange > _.z)
                return false;
        }

        return true;
    }

    public void BTN_GoToHome()
    {
        Manager_Loading.LoadScene("Scene_MyRoom");
    }
    private void PopupTextOGC(string _message)
    {
        goPopup_Text.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _message;
        goPopup_Text.SetActive(false);
        goPopup_Text.SetActive(true);
    }
    public void FindOG(Transform _targetTransform)
    {
        // camera followup 시작
        var cameraControl = Camera.main.GetComponent<CameraControl>();
        cameraControl.targetTransform = _targetTransform;
        cameraControl.isFollowUp = true;
        cameraControl.goTouchBlockCover.SetActive(true);
        // camera followup 끝

        StartCoroutine(flow());

        IEnumerator flow()
        {
            yield return new WaitForSeconds(1);
            if (!Manager_Master.Instance.userData.isPossibleDailyOGC)
            {
                PopupTextOGC(Manager_Master.Instance.LanguageDict["MSG000116"]);
                cameraControl.goTouchBlockCover.SetActive(false);
            }
            else
            {
                pastLevel = Manager_Master.Instance.GetCurLevel();

                txtEndorphin.text = string.Format($"{Manager_Master.Instance.constValue.OGCEndorphin * Manager_Master.Instance.UserClassCoefficient}");
                txtEXP.text = string.Format($"{Manager_Master.Instance.constValue.OGCEXP * Manager_Master.Instance.UserClassCoefficient}");

                Manager_Master.Instance.userData.exp += Manager_Master.Instance.constValue.OGCEXP * Manager_Master.Instance.UserClassCoefficient;
                Manager_Master.Instance.userData.endorphin = Mathf.Clamp(Manager_Master.Instance.userData.endorphin + Manager_Master.Instance.constValue.OGCEndorphin * Manager_Master.Instance.UserClassCoefficient, (int)MinValue.endorphin, (int)MaxValue.endorphin);
                Manager_Master.Instance.userData.isPossibleDailyOGC = false;

                Manager_Master.Instance.docRef.SetAsync(Manager_Master.Instance.userData, SetOptions.MergeAll);


                cameraControl.goTouchBlockCover.SetActive(false);
                goRewardUI.SetActive(true);

                if (Manager_Master.Instance.userData.userClass == nameof(UserClass.VVIP))
                    goRewardUI.transform.GetChild(4).gameObject.SetActive(true);
            }
        }
    }
    public void CheckUnlock()
    {
        if(Manager_Master.Instance.GetCurLevel() > pastLevel)
        {
            int curLevel = Manager_Master.Instance.GetCurLevel();

            var unlockObjName = new Dictionary<string, string>();

            var buildingData = Manager_Master.Instance.BuildingData;
            foreach (var element in buildingData)
            {
                var unlockLv = int.Parse(element.Value[nameof(BuildingDataBaseCode.unlockLv)].ToString());

                if (pastLevel < unlockLv && unlockLv <= curLevel)
                {
                    unlockObjName.Add(element.Key, nameof(FirebaseCollectionKey.buildingData));
                }
            }
            var hatData = Manager_Master.Instance.HatData;
            foreach (var element in hatData)
            {
                var unlockLv = int.Parse(element.Value[nameof(HatDataBaseCode.unlockLv)].ToString());

                if (pastLevel < unlockLv && unlockLv <= curLevel)
                {
                    unlockObjName.Add(element.Key, nameof(FirebaseCollectionKey.hatData));
                }
            }


            PopupUnlock(unlockObjName);
        }
    }

    public void PopupUnlock(Dictionary<string, string> _rewardKey)
    {
        if (_rewardKey == null)
            return;

        for (int i = 0, length = goUnlockParent.transform.childCount; i < length; ++i)
            goUnlockParent.transform.GetChild(i).gameObject.SetActive(false);

        ReleaseUnlockedBuilding();

        GameObject goCell;
        int idx = 0;

        foreach (var element in _rewardKey)
        {
            if (goUnlockParent.transform.childCount == idx)
                goCell = Instantiate(goUnlockCell, goUnlockParent.transform, false);
            else
                goCell = goUnlockParent.transform.GetChild(idx).gameObject;

            if (goCell.transform.GetChild(0).childCount > 0)
                Addressables.ReleaseInstance(goCell.transform.GetChild(0).GetChild(0).gameObject);

            goCell.transform.GetChild(1).gameObject.SetActive(false);
            goCell.transform.GetChild(2).gameObject.SetActive(false);
            goCell.transform.GetChild(3).gameObject.SetActive(false);

            if (element.Value == nameof(FirebaseCollectionKey.hatData))
            {
                if (Manager_Master.Instance.HatData[element.Key][nameof(HatDataBaseCode.hatClass)].ToString() == nameof(HatClass.Special))
                    goCell.transform.GetChild(1).gameObject.SetActive(true);
                GameObject go = Addressables.InstantiateAsync(element.Key, goCell.transform.GetChild(0).transform, false).WaitForCompletion();
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localEulerAngles = new Vector3(0, 0, 0);
                var size = int.Parse(Manager_Master.Instance.HatData[element.Key][nameof(HatDataBaseCode.sizeInCell)].ToString());
                go.transform.localScale = new Vector3(size, size, size);
                unlockedBuildings.Add(go);
                goCell.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (element.Value == nameof(FirebaseCollectionKey.buildingData))
            {
                GameObject go = Addressables.InstantiateAsync(element.Key, goCell.transform.GetChild(0).transform, false).WaitForCompletion();
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localEulerAngles = new Vector3(0, 0, 0);
                var size = int.Parse(Manager_Master.Instance.BuildingData[element.Key][nameof(BuildingDataBaseCode.sizeInCell)].ToString());
                go.transform.localScale = new Vector3(size, size, size);
                Destroy(go.GetComponent<Building>());
                Destroy(go.GetComponent<Rigidbody>());
                Destroy(go.GetComponent<BoxCollider>());
                unlockedBuildings.Add(go);
                goCell.transform.GetChild(3).gameObject.SetActive(true);
            }

            goCell.SetActive(true);
            idx++;
        }

        goUnlockUI.SetActive(true);
    }
    public void ReleaseUnlockedBuilding()
    {
        for (int i = 0, length = unlockedBuildings.Count; i < length; ++i)
        {
            if (unlockedBuildings[i] != null)
                Addressables.ReleaseInstance(unlockedBuildings[i]);
        }
        unlockedBuildings.Clear();
    }

    private void CreateTodayMood()
    {
        var splitCode = Manager_Master.Instance.userData.previousSelectedMood.Split(',');
        var name = splitCode[(int)MoodCode.name];
        var hatName = splitCode[(int)MoodCode.hatName];

        GameObject goTodayMood = Addressables.InstantiateAsync(name, moodParent.transform, false).WaitForCompletion();

        if (hatName != "null")
        {
            Addressables.InstantiateAsync(hatName, goTodayMood.transform.GetChild(2), false);
        }
    }
}
