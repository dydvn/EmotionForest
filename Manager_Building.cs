using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;

using TMPro;

using Random = UnityEngine.Random;

class BuildingDataBackupClass
{
    public List<string> buildingDataList;
    public List<string> buildingInventoryList;
    public int endorphin;
    public BuildingDataBackupClass(List<string> _buildingDataList, List<string> _buildingInventoryList, int _dummyEndorphin)
    {
        buildingDataList = _buildingDataList;
        buildingInventoryList = _buildingInventoryList;
        endorphin = _dummyEndorphin;
    }
}

public class Manager_Building : MonoBehaviour
{
    private static Manager_Building instance;
    
    [Header("========== Script")]
    [SerializeField] Manager_MyRoom manager_MyRoom;
    [Header("========== UI")]
    [SerializeField] GameObject goBuildMenu;
    [SerializeField] GameObject goBuildingTabButton;
    [SerializeField] GameObject goBuildingTabButtonParent;
    [SerializeField] GameObject goBuildingEachGroup;
    [SerializeField] GameObject goBuildingEachGroupParent;
    [SerializeField] GameObject goBuildingGroup;
    [SerializeField] GameObject goBuildingCell;
    [SerializeField] GameObject goBuildingFollowUI_InstallMode;
    [SerializeField] GameObject goBuildingFollowUI_EditMode;
    [SerializeField] GameObject goHideWhenBuildMode;
    [SerializeField] TextMeshProUGUI tmpNumberOfHave;
    [SerializeField] Sprite sprtThemeTabPressed;
    [SerializeField] Sprite sprtThemeTabNonPressed;

    private GameObject buildTarget;
    private GameObject touchTarget;
    private GameObject goBuildingFollowUI_Cur;
    private BuildingDataBackupClass buildingDataBackupClass;
    private List<Tuple<GameObject, string>> buildingList = new List<Tuple<GameObject, string>>();
    private Dictionary<string, int> buildingInventory = new Dictionary<string, int>();
    private Dictionary<string, (GameObject, GameObject)> buildingGroupDict = new();
    private Dictionary<string, TextMeshProUGUI> numberOfHaveBuildingDict = new Dictionary<string, TextMeshProUGUI>();
    private RectTransform rtBuildingFollowUI_Cur;
    private Camera mainCam;
    private Vector3 buildStartPosition;
    private Vector3 buildStartRotation;
    private Vector3 buildStartScale;
    private float buildingCurScale;
    private Vector2 touchStartPos;
    private Vector2 touchCurPos;
    private Vector3 DragPosition = new Vector3(0, 0, 0);
    private Vector3 DragPosition_UI_Start = new Vector3(0, 0, 0);
    private Vector3 buildingDragStartPos;
    private string buildingTargetName;
    private bool buildingRotateL = false;
    private bool buildingRotateR = false;
    private bool buildingScaleDown = false;
    private bool buildingScaleUp = false;

    public GameObject goClickHoldGauge;

    public bool isBuildMode = false;
    public bool isBuildMode_Install = false;
    public bool isGrabedBuilding = false;
    public bool isTouchBuilding = false;
    public bool isTouchGrabedBuilding = false;
    public bool isTouchGrabedBuilding_UI = false;
    public bool isBuildable = true;
    public bool clickHoldCoroutine = false;

    public static Manager_Building Instance
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
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        mainCam = Camera.main;

        InitGrass();
        BuildingInit();
        BuildingInventoryInit(true);
        InitBuildMenu();
    }

    private void Update()
    {
        // build mode일 때 건물 컨트롤
        if (isGrabedBuilding)
        {
            if (isTouchGrabedBuilding)
            {
                int maskLayer = 1 << LayerMask.NameToLayer("Ground");
                if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, maskLayer))
                {
                    if (isTouchGrabedBuilding_UI)
                    {
                        GetBuildingTarget().transform.position = buildingDragStartPos + hit.point - DragPosition_UI_Start;
                    }
                    else
                    {
                        DragPosition.x = hit.point.x;
                        DragPosition.z = hit.point.z;
                        GetBuildingTarget().transform.position = DragPosition;
                    }
                }
            }

            if (buildingRotateL)
            {
                buildTarget.transform.Rotate(Vector3.up * 90 * Time.deltaTime);
            }
            else if (buildingRotateR)
            {
                buildTarget.transform.Rotate(Vector3.up * -90 * Time.deltaTime);
            }

            if(buildingScaleUp)
            {
                float standardSize = float.Parse(Manager_Master.Instance.BuildingData[buildingTargetName][nameof(BuildingDataBaseCode.sizeInWorld)].ToString());
                float value = Math.Clamp(buildTarget.transform.localScale.x + standardSize * Time.deltaTime, standardSize * 0.5f, standardSize * 1.5f);
                buildingCurScale = value;
                buildTarget.transform.localScale = new Vector3(value, value, value);
            }
            else if(buildingScaleDown)
            {
                float standardSize = float.Parse(Manager_Master.Instance.BuildingData[buildingTargetName][nameof(BuildingDataBaseCode.sizeInWorld)].ToString());
                float value = Math.Clamp(buildTarget.transform.localScale.x - standardSize * Time.deltaTime, standardSize * 0.5f, standardSize * 1.5f);
                buildingCurScale = value;
                buildTarget.transform.localScale = new Vector3(value, value, value);
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (isGrabedBuilding)
            {
                var hits = Physics.RaycastAll(mainCam.ScreenPointToRay(Input.mousePosition));

                foreach (var obj in hits)
                {
                    if (obj.transform.gameObject == GetBuildingTarget())
                    {
                        isTouchGrabedBuilding = true;
                    }
                }

                if(!isTouchGrabedBuilding && Vector2.Distance(mainCam.WorldToScreenPoint(GetBuildingTarget().transform.position), Input.mousePosition) < 100)
                {
                    isTouchGrabedBuilding = true;
                }
            }
            else
            {
                int maskLayer = 1 << LayerMask.NameToLayer("Building");
                if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, maskLayer))
                {
                    touchStartPos = Input.mousePosition;
                    touchTarget = hit.transform.gameObject;
                    isTouchBuilding = true;

                    if (!isBuildMode)
                    {
                        clickHoldCoroutine = true;
                        StartCoroutine(nameof(ClickHold));
                    }
                }
            }
        }

        if(Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (isTouchBuilding && Vector2.Distance(Input.mousePosition, touchStartPos) > (int)ConstValue.touchDistance)
            {
                isTouchBuilding = false;
                clickHoldCoroutine = false;
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if (isBuildMode && isTouchBuilding && !isGrabedBuilding)
            {
                EnterBuildMode(touchTarget);
            }
            isTouchBuilding = false;
            isTouchGrabedBuilding = false;
            clickHoldCoroutine = false;
        }


#elif UNITY_ANDROID
        //여기다가 터치구현
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(0))
                    return;

                if (isGrabedBuilding)
                {
                    var hits = Physics.RaycastAll(mainCam.ScreenPointToRay(Input.GetTouch(0).position));

                    foreach (var obj in hits)
                    {
                        if (obj.transform.gameObject == GetBuildingTarget())
                            isTouchGrabedBuilding = true;
                    }

                    if (!isTouchGrabedBuilding && Vector2.Distance(mainCam.WorldToScreenPoint(GetBuildingTarget().transform.position), Input.GetTouch(0).position) < 100)
                    {
                        isTouchGrabedBuilding = true;
                    }
                }
                else
                {
                    int maskLayer = 1 << LayerMask.NameToLayer("Building");
                    if (Physics.Raycast(mainCam.ScreenPointToRay(Input.GetTouch(0).position), out RaycastHit hit, Mathf.Infinity, maskLayer))
                    {
                        touchStartPos = Input.GetTouch(0).position;
                        touchTarget = hit.transform.gameObject;
                        isTouchBuilding = true;

                        if (!isBuildMode)
                        {
                            clickHoldCoroutine = true;
                            StartCoroutine(nameof(ClickHold));
                        }
                    }
                }
            }

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                if (EventSystem.current.IsPointerOverGameObject(0))
                    return;

                if (isTouchBuilding && Vector2.Distance(Input.GetTouch(0).position, touchStartPos) > (int)ConstValue.touchDistance)
                {
                    isTouchBuilding = false;
                    clickHoldCoroutine = false;
                }
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (isBuildMode && isTouchBuilding && !isGrabedBuilding)
                {
                    EnterBuildMode(touchTarget);
                }
                isTouchBuilding = false;
                isTouchGrabedBuilding = false;
                clickHoldCoroutine = false;
            }

        }

#endif
    }

    private void LateUpdate()
    {
        // 건물 설치할때 ui 갱신
        if (isGrabedBuilding)
            BuildUISync();
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    // 서버에 저장돼있는 건물 배치 정보 참고해서 재설치하는 코드
    private void BuildingInit()
    {
        var buildingInfoList = Manager_Master.Instance.userData.buildingInfoList;
        for (int i = 0, count = buildingInfoList.Count; i < count; ++i)
        {
            var code = buildingInfoList[i];
            var codeSplit = code.Split(',');
            var name = codeSplit[(int)UserBuildingCode.name];
            var x = float.Parse(codeSplit[(int)UserBuildingCode.x]);
            var y = float.Parse(codeSplit[(int)UserBuildingCode.y]);
            var z = float.Parse(codeSplit[(int)UserBuildingCode.z]);
            var rotate = float.Parse(codeSplit[(int)UserBuildingCode.rotate]);


            GameObject goResult = Addressables.InstantiateAsync(name, new Vector3(x, y, z), Quaternion.Euler(0, rotate, 0)).WaitForCompletion();

            // 나중에 추가된거니까 임시로 개수 검사해줘야됨
            if (codeSplit.Length >= (int)UserBuildingCode.count)
            {
                var scale = float.Parse(codeSplit[(int)UserBuildingCode.scale]);
                goResult.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                code += "," + goResult.transform.localScale.x;
            }

            buildingList.Add(new Tuple<GameObject, string>(goResult, code));
            HideNearGrass(goResult);
        }
    }
    private void InitBuildMenu()
    {
        // 서버에서 테마 종류 받아와서 그 종류만큼 Tab과 Group 생성하는 코드
        var themeSet = Manager_Master.Instance.BuildingThemeSet;
        bool isfirstTab = true;
        foreach (var theme in themeSet)
        {
            GameObject goTab = Instantiate(goBuildingTabButton, goBuildingTabButtonParent.transform, false);
            goTab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Manager_Master.Instance.LanguageDict[theme];
            Button button = goTab.GetComponent<Button>();
            string temp = theme;
            button.onClick.AddListener(delegate { BTN_BuildTap(temp); });

            GameObject goEachGroup = Instantiate(goBuildingEachGroup, goBuildingEachGroupParent.transform, false);
            buildingGroupDict.Add(theme, (goEachGroup, goTab));
            if (isfirstTab)
            {
                goEachGroup.SetActive(true);
                button.onClick.Invoke();
                isfirstTab = false;
                goBuildingEachGroupParent.transform.parent.GetComponent<ScrollRect>().content = buildingGroupDict[theme].Item1.GetComponent<RectTransform>();
            }
        }


        // create cell
        var userLevel = Manager_Master.Instance.GetCurLevel();
        var BuildingData = Manager_Master.Instance.BuildingData;

        foreach(var element in BuildingData)
        {
            var name = element.Key;
            var theme = element.Value[nameof(BuildingDataBaseCode.theme)].ToString();
            var price = int.Parse(element.Value[nameof(BuildingDataBaseCode.price)].ToString());
            var unlockLv = int.Parse(element.Value[nameof(BuildingDataBaseCode.unlockLv)].ToString());
            var sizeInCell = int.Parse(element.Value[nameof(BuildingDataBaseCode.sizeInCell)].ToString());

            GameObject goCell = Instantiate(goBuildingCell, buildingGroupDict[theme].Item1.transform, false);
            GameObject resultObj = Addressables.InstantiateAsync(name, goCell.transform.GetChild(1).transform, false).WaitForCompletion();
            resultObj.transform.localPosition = new Vector3(0, 0, 0);
            resultObj.transform.localEulerAngles = new Vector3(0, 0, 0);
            resultObj.transform.localScale = new Vector3(sizeInCell, sizeInCell, sizeInCell);
            Destroy(resultObj.GetComponent<Building>());
            Destroy(resultObj.GetComponent<Rigidbody>());
            Destroy(resultObj.GetComponent<BoxCollider>());
            Manager_Master.Instance.ChangeLayer(resultObj.transform, "UI");
            resultObj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;


            goCell.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format($"{price:#,0}");
            numberOfHaveBuildingDict.Add(name, goCell.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>());
            if (userLevel < unlockLv)
            {
                goCell.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format($"{ unlockLv} Lv");
                goCell.transform.GetChild(4).gameObject.SetActive(true);
            }

            string str = name;
            goCell.GetComponent<Button>().onClick.AddListener(delegate { BTN_BuildingCell(str, unlockLv, price); });
        }

        RefreshNumberOfHave();
    }

    private void BuildingInventoryInit(bool _isFirst = false)
    {
        if (_isFirst)
        {
            var sortedBuildingData = Manager_Master.Instance.BuildingData;

            foreach(var element in sortedBuildingData)
            {
                buildingInventory.Add(element.Key, 0);
            }

            if (Manager_Master.Instance.userData.buildingInventoryList.Count == 0)
                Manager_Master.Instance.userData.buildingInventoryList = GetNewBuildingInventoryList().ToList();
        }

        var buildingInventoryList = Manager_Master.Instance.userData.buildingInventoryList;

        for (int i = 0, count = buildingInventoryList.Count; i < count; ++i)
        {
            var codeSplit = buildingInventoryList[i].Split(',');
            buildingInventory[codeSplit[(int)UserInventoryCode.name]] = int.Parse(codeSplit[(int)UserInventoryCode.numberOfHave]);
        }
    }
    private void InitGrass()
    {
        GameObject goGrass = Addressables.LoadAssetAsync<GameObject>("Grass_06").WaitForCompletion();

        int xMax = (int)MaxValue.moodMoveRangeX;
        int xMin = (int)MinValue.moodMoveRangeX;
        int zMax = (int)MaxValue.moodMoveRangeZ;
        int zMin = (int)MinValue.moodMoveRangeZ;

        int xT = (xMax - xMin) / 9;
        int zT = (zMax - zMin) / 10;

        for (int x = xMin; x < xMax; x += xT)
        {
            for (int z = zMin; z < zMax; z += zT)
            {
                GameObject grass = Instantiate(goGrass, new Vector3(Random.Range(x + 0.0f, x + xT + 0.0f), 0.0f, Random.Range(z + 0.0f, z + zT + 0.0f)), Quaternion.Euler(0, Random.Range(0, 360), 0));
                Destroy(grass.GetComponent<Building>());
            }
        }
    }

    public void UserLevelupEvent(int _beforeLevel)
    {
        if (Manager_Master.Instance.GetCurLevel() <= _beforeLevel)
            return;

        var userLevel = Manager_Master.Instance.GetCurLevel();
        var idxDict = new Dictionary<string, int>();

        foreach(var element in Manager_Master.Instance.BuildingThemeSet)
            idxDict.Add(element, 0);

        var unlockObjName = new Dictionary<string, string>();
        
        var hatData = Manager_Master.Instance.HatData;
        foreach (var element in hatData)
        {
            var unlockLv = int.Parse(element.Value[nameof(HatDataBaseCode.unlockLv)].ToString());
            if ((Manager_Master.Instance.userData.userClass != nameof(UserClass.VVIP) || element.Value[nameof(HatDataBaseCode.hatClass)].ToString() != nameof(HatClass.Special)) && _beforeLevel < unlockLv && unlockLv <= userLevel)
                unlockObjName.Add(element.Key, nameof(FirebaseCollectionKey.hatData));
        }

        var buildingData = Manager_Master.Instance.BuildingData;
        foreach(var element in buildingData)
        {
            var theme = element.Value[nameof(BuildingDataBaseCode.theme)].ToString();
            var unlockLv = int.Parse(element.Value[nameof(BuildingDataBaseCode.unlockLv)].ToString());

            Transform _trasformBuildingUI = buildingGroupDict[theme].Item1.transform.GetChild(idxDict[theme]);
            TextMeshProUGUI lockText = _trasformBuildingUI.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>();
            if (userLevel >= unlockLv && lockText.text != "")
            {
                _trasformBuildingUI.GetChild(4).gameObject.SetActive(false);
                lockText.text = "";

                unlockObjName.Add(element.Key, nameof(FirebaseCollectionKey.buildingData));
            }

            idxDict[theme]++;
        }

        

        if (unlockObjName.Count > 0)
            manager_MyRoom.PopupUnlock(unlockObjName);
    }

    public void SetBuildTarget(GameObject target)
    {
        buildTarget = target;
        buildingTargetName = target.name.Replace("(Clone)", "");
    }
    public GameObject GetBuildingTarget()
    {
        return buildTarget;
    }

    public void BuildUISync()
    {
        rtBuildingFollowUI_Cur.localPosition = GetViewportPointFromWorld(buildTarget);
    }
    
    public void OnOffBuildingFollowUI(bool active)
    {
        isGrabedBuilding = active;
        goBuildingFollowUI_Cur.SetActive(active);
        goBuildMenu.SetActive(!active);
    }

    public void SetBuildStartPosition(GameObject _go)
    {
        buildStartPosition = _go.transform.position;
        buildStartRotation = _go.transform.eulerAngles;
        buildStartScale = _go.transform.localScale;
    }
    public void GetBuildStartPosition(ref GameObject _go)
    {
        _go.transform.position = buildStartPosition;
        _go.transform.eulerAngles = buildStartRotation;
        _go.transform.localScale = buildStartScale;
    }
    public void UpdateBuildingList(bool _isInstall, GameObject _target)
    {
        var code = string.Format($"{buildingTargetName},{_target.transform.position.x},{_target.transform.position.y},{_target.transform.position.z},{_target.transform.eulerAngles.y},{buildingCurScale}");

        if (_isInstall)
        {
            buildingList.Add(new Tuple<GameObject, string>(_target, code));
        }
        else
        {
            for(int i = 0, count = buildingList.Count; i < count; ++i)
            {
                if (buildingList[i].Item1 == _target)
                {
                    buildingList[i] = new Tuple<GameObject, string>(_target, code);
                    break;
                }
            }
        }
    }
    // building cell 눌러서 들어 왔을 때
    private void EnterBuildMode(string _buildingName, Vector3 _pos, Quaternion _rotate, bool isCell = false)
    {
        buildStartPosition = Vector3.zero; // only install mode
        SetBuildTarget(Addressables.InstantiateAsync(_buildingName, _pos, _rotate).WaitForCompletion()); // only install mode
        buildTarget.transform.position = _pos; // only install mode
        if(isCell)
        {
            buildingCurScale = buildTarget.transform.localScale.x;
        }
        else // 연속 설치로 들어올 때
        {
            buildTarget.transform.localScale = new Vector3(buildingCurScale, buildingCurScale, buildingCurScale);
        }

        goBuildingFollowUI_Cur = goBuildingFollowUI_InstallMode;
        rtBuildingFollowUI_Cur = goBuildingFollowUI_Cur.GetComponent<RectTransform>();
        RefreshNumberOfHave();
        isBuildMode_Install = true; // only install mode

        OnOffBuildingFollowUI(true);
    }
    // build mode가 아닌데 건물 꾹 눌러서 들어왔을 때
    public void EnterBuildMode(GameObject _goBuilding)
    {
        if (_goBuilding == null)
            return;
        if (!isBuildMode)
            buildingDataBackupClass = new BuildingDataBackupClass(GetBuildingDataList().ToList(), Manager_Master.Instance.userData.buildingInventoryList, Manager_Master.Instance.userData.endorphin);
        goBuildingFollowUI_Cur = goBuildingFollowUI_EditMode;
        rtBuildingFollowUI_Cur = goBuildingFollowUI_Cur.GetComponent<RectTransform>();
        isBuildMode = true;
        isTouchGrabedBuilding = true;
        SetBuildTarget(_goBuilding);
        SetBuildStartPosition(_goBuilding);
        goHideWhenBuildMode.SetActive(false);

        OnOffBuildingFollowUI(true);
    }
    private void RefreshNumberOfHave()
    {
        if (buildingTargetName != null && buildingTargetName != "")
        {
            var price = Manager_Master.Instance.BuildingData[buildingTargetName][nameof(BuildingDataBaseCode.price)].ToString();
            if(buildingInventory[buildingTargetName] > 0)
            {
                tmpNumberOfHave.text = buildingInventory[buildingTargetName].ToString();
                tmpNumberOfHave.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                tmpNumberOfHave.text = string.Format($"{Manager_Master.Instance.LanguageDict["LAN_TEXT_004"]} : {price} Pin");
                tmpNumberOfHave.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        foreach (var element in numberOfHaveBuildingDict)
            element.Value.text = string.Format($"{buildingInventory[element.Key]:#,0}");
    }
    private List<string> GetBuildingDataList()
    {
        var result = new List<string>();

        foreach (var str in buildingList)
            result.Add(str.Item2);

        return result;
    }
    private List<string> GetNewBuildingInventoryList()
    {
        var result = new List<string>();

        foreach(var element in buildingInventory)
            result.Add(string.Format($"{element.Key},{element.Value}"));

        return result;
    }
    public void HideNearGrass(GameObject _go)
    {
        var cols = Physics.OverlapSphere(_go.transform.position, 10.0f);
        foreach (var obj in cols)
        {
            if (obj.gameObject.CompareTag("Grass"))
            {
                obj.gameObject.SetActive(false);
                _go.GetComponent<Building>().grassList.Add(obj.gameObject);
            }
        }
    }
    public void AppeartNearGrass(GameObject _go)
    {
        if (_go.GetComponent<Building>().grassList != null)
        {
            var grassList = _go.GetComponent<Building>().grassList;
            foreach (var item in grassList)
            {
                item.SetActive(true);
            }
        }
    }

    public Vector3 GetViewportPointFromWorld(GameObject _target)
    {
        return new Vector3((mainCam.WorldToViewportPoint(_target.transform.position).x - 0.5f) * Screen.width, (mainCam.WorldToViewportPoint(_target.transform.position).y - 0.5f) * Screen.height, 0);
    }
    public IEnumerator ClickHold()
    {
        float clickHoldTime = 0;
        int maskLayer = 1 << LayerMask.NameToLayer("Building");
        Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, maskLayer);
        if (hit.transform.gameObject == null)
            yield break;
        goClickHoldGauge.transform.localPosition = GetViewportPointFromWorld(hit.transform.gameObject) + new Vector3(0, 200, 0);
        var gauge = goClickHoldGauge.transform.GetChild(0).GetComponent<Slider>();
        gauge.value = 0;


        float standardTime = 0.5f;

        while (clickHoldTime < 1 && clickHoldCoroutine)
        {
            clickHoldTime += 1 * Time.deltaTime;

            if (clickHoldTime > standardTime)
            {
                goClickHoldGauge.SetActive(true);
                gauge.value = (clickHoldTime - standardTime) / (1 - standardTime);
            }
            yield return null;
        }

        if (clickHoldTime >= 1)
        {
            EnterBuildMode(hit.transform.gameObject);
        }
        goClickHoldGauge.SetActive(false);
    }





    // Button Function
    public void BTN_EnterBuildMode()
    {
        goBuildMenu.SetActive(true);
        goHideWhenBuildMode.SetActive(false);

        buildingDataBackupClass = new BuildingDataBackupClass(GetBuildingDataList().ToList(), Manager_Master.Instance.userData.buildingInventoryList, Manager_Master.Instance.userData.endorphin);
        Manager_Tutorial.Instance.InitTutorial(TutorialKey.build);

        isBuildMode = true;
    }
    public void BTN_ExitBuildMode(bool isApply)
    {
        // BuildMode 진입 후 변경사항 없으면 그냥 exit
        if (Enumerable.SequenceEqual(buildingDataBackupClass.buildingDataList, GetBuildingDataList()))
        {
            goBuildMenu.SetActive(false);
            goHideWhenBuildMode.SetActive(true);
            isBuildMode = false;
            return;
        }

        if(isApply)
            manager_MyRoom.PopupYesNo(Manager_Master.Instance.LanguageDict["LAN_TEXT_005"], BuildModeApply);
        else
            manager_MyRoom.PopupYesNo(Manager_Master.Instance.LanguageDict["LAN_TEXT_006"], BuildModeCancle);
    }
    private void BuildModeApply()
    {
        //우주로 간거 검사해서 날리기
        for(int i = 0, count = buildingList.Count; i < count; ++i)
        {
            if (buildingList.Count <= i)
                break;

            if (buildingList[i].Item1.transform.position.x > 800)
            {
                var go = buildingList[i].Item1;
                Addressables.ReleaseInstance(go);
                buildingList.RemoveAt(i);
                i--;
            }
        }

        isBuildMode = false;
        Manager_Master.Instance.userData.buildingInfoList = GetBuildingDataList().ToList();
        Manager_Master.Instance.userData.buildingInventoryList = GetNewBuildingInventoryList().ToList();
        manager_MyRoom.UpdateBuildingData();
        manager_MyRoom.RefreshEndorphin(true);

        goBuildMenu.SetActive(false);
        goHideWhenBuildMode.SetActive(true);
    }
    private void BuildModeCancle()
    {
        //위치 바꾼 건물 원위치
        for (int i = 0, count = buildingDataBackupClass.buildingDataList.Count; i < count; ++i)
        {
            var go = buildingList[i].Item1;
            var code = buildingDataBackupClass.buildingDataList[i];
            var codeSplit = code.Split(',');

            go.transform.position = new Vector3(float.Parse(codeSplit[(int)UserBuildingCode.x]), float.Parse(codeSplit[(int)UserBuildingCode.y]), float.Parse(codeSplit[(int)UserBuildingCode.z]));
            go.transform.rotation = Quaternion.Euler(go.transform.eulerAngles.x, float.Parse(codeSplit[(int)UserBuildingCode.rotate]), go.transform.eulerAngles.z);

            // 나중에 추가된거니까 임시로 개수 검사진행해줘야돼
            if(codeSplit.Length >= (int)UserBuildingCode.count)
            {
                float scale = float.Parse(codeSplit[(int)UserBuildingCode.scale]);
                go.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        //우주로 간거 검사해서 날리기
        for (int i = 0, count = buildingList.Count; i < count; ++i)
        {
            if (buildingList.Count <= i)
                break;

            if (buildingList[i].Item1.transform.position.x > 800)
            {
                var go = buildingList[i].Item1;
                Addressables.ReleaseInstance(go);
                buildingList.RemoveAt(i);
                i--;
            }
        }

        //새로 건설한 건물 삭제
        for (int i = buildingDataBackupClass.buildingDataList.Count, count = buildingList.Count; i < count; ++i)
        {
            if (buildingList.Count <= i)
                break;

            var go = buildingList[i].Item1;
            Addressables.ReleaseInstance(go);
            buildingList.RemoveAt(i);
            i--;
        }

        // 백업데이터 불러와서 원본 데이터에 다시 쓰기
        for(int i = 0, count = buildingDataBackupClass.buildingDataList.Count; i < count; ++i)
            buildingList[i] = new Tuple<GameObject, string>(buildingList[i].Item1, buildingDataBackupClass.buildingDataList[i]);

        // 인벤토리 개수 롤백
        BuildingInventoryInit();
        RefreshNumberOfHave();

        // 소비한 돈이 있으면 돌려주기
        Manager_Master.Instance.userData.endorphin = buildingDataBackupClass.endorphin;
        manager_MyRoom.RefreshEndorphin();

        // status 설정
        isBuildMode = false;

        // UI 설정
        goBuildMenu.SetActive(false);
        goHideWhenBuildMode.SetActive(true);
    }

    public void BTN_BuildOk()
    {
        if (!isBuildable)
        {
            manager_MyRoom.PopupTextMyRoom(Manager_Master.Instance.LanguageDict["LAN_TEXT_007"]);
            return;
        }

        if (isBuildMode_Install)
        {
            var price = int.Parse(Manager_Master.Instance.BuildingData[buildingTargetName][nameof(BuildingDataBaseCode.price)].ToString());


            // 새로 설치하면 보유량 개수 판단해서 보유량을 깔지, 금액을 깔지 결정
            if (buildingInventory[buildingTargetName] < 1)
            {
                if (!Manager_Master.Instance.CheckPrice(price * -1))
                {
                    return;
                }
                Manager_Master.Instance.AddEndorphin(-price);
            }
            else
            {
                buildingInventory[buildingTargetName] -= 1;
            }

            Vector3 newPos = buildTarget.transform.position;
            Quaternion newRot = buildTarget.transform.rotation;
            float tempValx = buildTarget.GetComponent<BoxCollider>().size.x * buildTarget.transform.localScale.x;
            float tempValz = buildTarget.GetComponent<BoxCollider>().size.z * buildTarget.transform.localScale.z;
            newPos.x += tempValx;
            newPos.z -= tempValz;
            // ok를 누를 때마다 tuple List 데이터 변경
            buildTarget.GetComponent<Building>().StartInstall();
            buildTarget.transform.position = new Vector3(buildTarget.transform.position.x, 0, buildTarget.transform.position.z);
            UpdateBuildingList(isBuildMode_Install, buildTarget); // EnterBuildMode보다 위에있어야함
            EnterBuildMode(buildingTargetName, newPos, newRot);
            RefreshNumberOfHave();
        }
        else
        {
            AppeartNearGrass(buildTarget);
            UpdateBuildingList(isBuildMode_Install, buildTarget);
            buildTarget.transform.position = new Vector3(buildTarget.transform.position.x, 0, buildTarget.transform.position.z);
            OnOffBuildingFollowUI(false);
        }


        HideNearGrass(buildTarget);
    }

    public void BTN_BuildCancle()
    {
        isBuildMode_Install = false;

        if (buildStartPosition != Vector3.zero)
            GetBuildStartPosition(ref buildTarget);
        else
            Addressables.ReleaseInstance(buildTarget);

        OnOffBuildingFollowUI(false);
    }
    public void BTN_BuildingRotateL(bool isDown)
    {
        buildingRotateL = isDown;
    }
    public void BTN_BuildingRotateR(bool isDown)
    {
        buildingRotateR = isDown;
    }
    public void BTN_BuildingMove(bool isDown)
    {
        isTouchGrabedBuilding = isDown;
        isTouchGrabedBuilding_UI = isDown;
        buildingDragStartPos = GetBuildingTarget().transform.position;

        if (isDown)
        {
            int maskLayer = 1 << LayerMask.NameToLayer("Ground");
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, maskLayer))
            {
                DragPosition_UI_Start.x = hit.point.x;
                DragPosition_UI_Start.z = hit.point.z;
            }
        }
    }
    public void BTN_BuildingScaleDown(bool isDown)
    {
        buildingScaleDown = isDown;
    }
    public void BTN_BuildingScaleUp(bool isDown)
    {
        buildingScaleUp = isDown;
    }
    public void BTN_BuildingStorage()
    {
        // 건물 회수시 inventory 갯수 증가
        buildingInventory[buildingTargetName] = Math.Clamp(++buildingInventory[buildingTargetName], (int)MinValue.buildingCount, (int)MaxValue.buildingCount);
        
        if (buildingInventory[buildingTargetName] == (int)MaxValue.buildingCount)
            Manager_MyRoom.Instance.PopupTextMyRoom(Manager_Master.Instance.LanguageDict["MSG000117"]);
            
        buildTarget.transform.position = new Vector3(999, 999, 999);
        UpdateBuildingList(false, buildTarget);

        RefreshNumberOfHave();
        OnOffBuildingFollowUI(false);
    }
    public void BTN_BuildingSale()
    {
        var price = int.Parse(Manager_Master.Instance.BuildingData[buildingTargetName][nameof(BuildingDataBaseCode.price)].ToString());

        Manager_Master.Instance.AddEndorphin(price);
        buildTarget.transform.position = new Vector3(999, 999, 999);
        UpdateBuildingList(false, buildTarget);

        RefreshNumberOfHave();
        OnOffBuildingFollowUI(false);
    }

    private void BTN_BuildTap(string _themeName)
    {
        foreach (var element in buildingGroupDict)
        {
            if(element.Key == _themeName)
            {
                goBuildingEachGroupParent.transform.parent.GetComponent<ScrollRect>().content = buildingGroupDict[element.Key].Item1.GetComponent<RectTransform>();
                element.Value.Item1.SetActive(true);
                element.Value.Item2.GetComponent<Image>().sprite = sprtThemeTabPressed;
            }
            else
            {
                element.Value.Item1.SetActive(false);
                element.Value.Item2.GetComponent<Image>().sprite = sprtThemeTabNonPressed;
            }
        }
    }

    private void BTN_BuildingCell(string _buildingName, int _unlockLv, int _price)
    {
        if (buildingInventory[_buildingName] < 1 && !Manager_MyRoom.Instance.IsPossibleBuy(_price, _unlockLv))
            return;

        // Vector3 initVector = Vector3.zero;

        int maskLayer = 1 << LayerMask.NameToLayer("Ground");
        isBuildable = true;
        if (Physics.Raycast(mainCam.ScreenPointToRay(new Vector3((int)ConstValue.screenWidth * 0.5f, (int)ConstValue.screenHeight * 0.5f, 0)), out RaycastHit hit, Mathf.Infinity, maskLayer))
        {
            EnterBuildMode(_buildingName, hit.point, Quaternion.Euler(0, 45, 0), true);
        }
    }
}
