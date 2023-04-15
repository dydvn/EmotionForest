// 서버의 데이터를 사용하여 인게임 감정선택 메뉴 초기화 하는 함수
private void InitMoodSelect()
{
    var handle = Addressables.LoadAssetsAsync<GameObject>("Mood",
        result =>
        {
            string resultName = result.name;
            GameObject goCell = Instantiate(goMoodCell, goMoodCellParent.transform, false);
            Sprite emoji = Addressables.LoadAssetAsync<Sprite>(resultName).WaitForCompletion();
            sprtMoodEmoji.Add(resultName.Split('_')[0], emoji);
            goCell.transform.GetChild(1).GetComponent<Image>().sprite = emoji;
            goCell.transform.GetChild(2).GetComponent<Image>().sprite = emoji;
            goCell.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = GetMoodNameFromIdx(resultName.Split('_')[0]);
            goCell.transform.GetChild(0).GetComponent<Image>().color = result.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].color;
            goCell.GetComponent<Button>().onClick.AddListener(delegate { BTN_SelectMood(resultName); });
        }).WaitForCompletion();

    Addressables.Release(handle);
}

// 서버의 데이터를 사용하여 인게임 색 선택 메뉴 초기화 하는 함수
private void InitRoomColorSelect()
{
    var handle = Addressables.LoadAssetsAsync<Material>("RoomColor",
        result =>
        {
            string resultName = result.name;
            GameObject goCell = Instantiate(goRoomOptionCell, goRoomColorCellParent.transform, false);
            goCell.GetComponent<Image>().color = result.color;
            goCell.GetComponent<Button>().onClick.AddListener(delegate { BTN_SelectRoomColor(resultName); });
        }).WaitForCompletion();

    Addressables.Release(handle);
}

// 서버의 데이터를 사용하여 인게임 효과 선택 메뉴 초기화 하는 함수
private void InitRoomEffectSelect()
{
    // none cell 하나 만들어주기
    Instantiate(goRoomOptionCell, goRoomEffectCellParent.transform, false).GetComponent<Button>().onClick.AddListener(delegate { BTN_SelectRoomEffect("none"); });

    var handle = Addressables.LoadAssetsAsync<GameObject>("RoomEffect",
        result =>
        {
            string resultName = result.name;
            GameObject goCell = Instantiate(goRoomOptionCell, goRoomEffectCellParent.transform, false);
            goCell.GetComponent<Button>().onClick.AddListener(delegate { BTN_SelectRoomEffect(resultName); });

            // 이펙트 이미지 추가
            goCell.transform.GetChild(0).GetComponent<Image>().sprite = Addressables.LoadAssetAsync<Sprite>(result.name.Replace("RE_", "RE_Img_")).WaitForCompletion();
            goCell.transform.GetChild(0).gameObject.SetActive(true);
        }).WaitForCompletion();

    Addressables.Release(handle);
}



// 서버의 데이터를 사용하여 인게임 건설 메뉴 초기화 하는 함수
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
