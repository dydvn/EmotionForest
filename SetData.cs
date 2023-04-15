public void SetConstValue()
{
    StartCoroutine(flow());
    IEnumerator flow()
    {
        BuildingThemeSet = new SortedSet<string>();
        BuildingData = new Dictionary<string, Dictionary<string, object>>();
        HatData = new Dictionary<string, Dictionary<string, object>>();

        bool isDone_BuildingData = false;
        bool isDone_HatData = false;
        bool isDone_ConstValueData = false;

        // json 파일이 존재한다면 json의 정보 사용
        string folderPath;
#if UNIDY_EDITOR
        folderPath = Application.dataPath;
#elif UNITY_ANDROID
        folderPath = Application.persistentDataPath;
#endif
        string buildingPath = Path.Combine(folderPath, "testBuildingData.json");
        if (File.Exists(buildingPath))
        {
            Debug.Log("building 파일 있어서 들어옴");
            string str = File.ReadAllText(buildingPath);
            BuildingDict buildingDict = new BuildingDict();
            buildingDict.buildingDict = DictionaryJsonUtility.FromJson<string, BuildingTest>(str);

            foreach(var ele in buildingDict.buildingDict)
            {
                Dictionary<string, object> buildingDic = new Dictionary<string, object>();
                buildingDic[nameof(BuildingDataBaseCode.price)] = ele.Value.price;
                buildingDic[nameof(BuildingDataBaseCode.unlockLv)] = ele.Value.unlockLv;
                buildingDic[nameof(BuildingDataBaseCode.sizeInCell)] = ele.Value.sizeInCell;
                buildingDic[nameof(BuildingDataBaseCode.theme)] = ele.Value.theme;
                BuildingData.Add(ele.Key, buildingDic);
                BuildingThemeSet.Add(buildingDic[nameof(BuildingDataBaseCode.theme)].ToString());
            }

            //언락 레벨로정렬 후 가격으로 정렬
            BuildingData = BuildingData.OrderBy(item => item.Value[nameof(BuildingDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(BuildingDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);

            isDone_BuildingData = true;
        }
        else
        {
            Debug.Log("building 파일 없어서 들어옴");
            // --------- buildingDataBase에서 데이터 불러와서 BuildingData 만드는 부분
            Query allBuildingDataQuery = db.Collection(nameof(FirebaseCollectionKey.buildingData)); // Collection의 모든 문서 가져오기
                                                                                                    // Query allBuildingDataQuery = db.Collection("buildingData").WhereEqualTo("theme", "00S"); // 문서내 필드 중 원하는 필드의 값들을 필터링해서 걸리는 것들만 가져오기
            allBuildingDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("[Warning] BuildingData load Canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("[Warning] BuildingData load Faulted" + task.Exception);
                    return;
                }

                // json 연습
                Dictionary<string, BuildingTest> tempBuildingDict = new();



                QuerySnapshot allBuildingDataQuerySnapshot = task.Result;
                foreach (DocumentSnapshot documentSnapshot in allBuildingDataQuerySnapshot.Documents)
                {
                    Dictionary<string, object> buildingDic = documentSnapshot.ToDictionary();
                    BuildingData.Add(documentSnapshot.Id, buildingDic);
                    BuildingThemeSet.Add(buildingDic[nameof(BuildingDataBaseCode.theme)].ToString());

                    // json 연습
                    BuildingTest buildingTest = new();
                    buildingTest.price = int.Parse(buildingDic[nameof(BuildingDataBaseCode.price)].ToString());
                    buildingTest.sizeInCell = int.Parse(buildingDic[nameof(BuildingDataBaseCode.sizeInCell)].ToString());
                    buildingTest.theme = buildingDic[nameof(BuildingDataBaseCode.theme)].ToString();
                    buildingTest.unlockLv = int.Parse(buildingDic[nameof(BuildingDataBaseCode.unlockLv)].ToString());

                    tempBuildingDict[documentSnapshot.Id] = buildingTest;
                }

                //언락 레벨로정렬 후 가격으로 정렬
                BuildingData = BuildingData.OrderBy(item => item.Value[nameof(BuildingDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(BuildingDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);

                Debug.Log("레벨, 가격 정렬 완료");

                // json 연습
                string json = DictionaryJsonUtility.ToJson(tempBuildingDict, true);
                Debug.Log("json 생성 완료");
                File.WriteAllText(buildingPath, json);
                Debug.Log("WriteAllText 완료");

                isDone_BuildingData = true;
                Debug.Log("isDone_BuildingData : " + isDone_BuildingData);
            });
        }


        // json 연습
        // json 파일이 존재한다면 json의 정보 사용
        string hatPath = Path.Combine(folderPath, "testHatData.json");
        if (File.Exists(hatPath))
        {
            Debug.Log("hat 파일 있어서 들어옴");
            string str = File.ReadAllText(hatPath);
            HatDict hatDict = new();
            hatDict.hatDict = DictionaryJsonUtility.FromJson<string, HatTest>(str);

            foreach (var ele in hatDict.hatDict)
            {
                Dictionary<string, object> hatDic = new Dictionary<string, object>();
                hatDic[nameof(HatDataBaseCode.price)] = ele.Value.price;
                hatDic[nameof(HatDataBaseCode.unlockLv)] = ele.Value.unlockLv;
                hatDic[nameof(HatDataBaseCode.sizeInCell)] = ele.Value.sizeInCell;
                hatDic[nameof(HatDataBaseCode.hatClass)] = ele.Value.hatClass;
                HatData.Add(ele.Key, hatDic);
            }

            //언락 레벨로정렬 후 가격으로 정렬
            HatData = HatData.OrderBy(item => item.Value[nameof(BuildingDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(BuildingDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);

            isDone_HatData = true;
        }
        else
        {
            Debug.Log("hat 파일 없어서 들어옴");

            // --------- hatDataBase에서 데이터 불러와서 HatData 만드는 부분
            Query allHatDataQuery = db.Collection(nameof(FirebaseCollectionKey.hatData)); // Collection의 모든 문서 가져오기
            allHatDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("[Warning] HatData load Canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("[Warning] HatData load Faulted" + task.Exception);
                    return;
                }
                // json 연습
                Dictionary<string, HatTest> tempHatDict = new();

                QuerySnapshot allHatDataQuerySnapshot = task.Result;
                foreach (DocumentSnapshot documentSnapshot in allHatDataQuerySnapshot.Documents)
                {
                    Dictionary<string, object> hatDic = documentSnapshot.ToDictionary();
                    HatData.Add(documentSnapshot.Id, hatDic);
                    // json 연습
                    HatTest hatTest = new();
                    hatTest.price = int.Parse(hatDic[nameof(HatDataBaseCode.price)].ToString());
                    hatTest.sizeInCell = int.Parse(hatDic[nameof(HatDataBaseCode.sizeInCell)].ToString());
                    hatTest.hatClass = hatDic[nameof(HatDataBaseCode.hatClass)].ToString();
                    hatTest.unlockLv = int.Parse(hatDic[nameof(HatDataBaseCode.unlockLv)].ToString());

                    tempHatDict[documentSnapshot.Id] = hatTest;
                }

                //언락 레벨로정렬 후 가격으로 정렬
                HatData = HatData.OrderBy(item => item.Value[nameof(HatDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(HatDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);


                // json 연습
                string json = DictionaryJsonUtility.ToJson(tempHatDict, true);
                File.WriteAllText(hatPath, json);

                isDone_HatData = true;
                Debug.Log("isDone_HatData : " + isDone_HatData);
            });
        }


        string constPath = Path.Combine(folderPath, "testConstValue.json");
        if (File.Exists(constPath))
        {
            Debug.Log("const 파일 있어서 들어옴");
            string str = File.ReadAllText(constPath);

            constValue = JsonUtility.FromJson<ConstValueTest>(str).Convert();

            isDone_ConstValueData = true;
        }
        else
        {
            Debug.Log("const 파일 없어서 들어옴");
            // --------- Set ConstValue
            var constValueDocRef = db.Collection(nameof(FirebaseCollectionKey.constValue)).Document("data");
            constValueDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("[Warning] ConstValue set Canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("[Warning] ConstValue set Faulted" + task.Exception);
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    constValue = snapshot.ConvertTo<ConstValueFromServer>();
                }
                else
                {
                    Debug.Log("ConstValue load error");
                }

                // json 연습
                ConstValueTest constValueTest = constValue.Convert();
                string json = JsonUtility.ToJson(constValueTest, true);
                File.WriteAllText(constPath, json);

                isDone_ConstValueData = true;
                Debug.Log("isDone_ConstValueData : " + isDone_ConstValueData);
            });
        }

        while (!isDone_BuildingData || !isDone_HatData || !isDone_ConstValueData)
            yield return null;

        Manager_Initial.isDone_ConstValue = true;
    }
}
