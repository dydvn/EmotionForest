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

        string buildingPath = Path.Combine(folderPath, "BuildingData.json");
        if (File.Exists(buildingPath) && userData.docVersion == masterCheck.docVersion && userData.docLastWriteTimeUTCBuilding == new FileInfo(buildingPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"))
        {
            // building 파일 유무 확인 완료, 문서 버전 확인 완료, 수정 여부 확인 완료 후 들어옴
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
                buildingDic[nameof(BuildingDataBaseCode.sizeInWorld)] = ele.Value.sizeInWorld;
                BuildingData.Add(ele.Key, buildingDic);
                BuildingThemeSet.Add(buildingDic[nameof(BuildingDataBaseCode.theme)].ToString());
            }

            //언락 레벨로 정렬 후 가격으로 정렬
            BuildingData = BuildingData.OrderBy(item => item.Value[nameof(BuildingDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(BuildingDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);

            isDone_BuildingData = true;
        }
        else
        {
            // building 파일 조건 만족 못해서 들어옴
            // --------- buildingDataBase에서 데이터 불러와서 BuildingData 만드는 부분
            Query allBuildingDataQuery = db.Collection(nameof(FirebaseCollectionKey.buildingData)); // Collection의 모든 문서 가져오기
                                                                                                    // Query allBuildingDataQuery = db.Collection("buildingData").WhereEqualTo("theme", "00S"); // 문서내 필드 중 원하는 필드의 값들을 필터링해서 걸리는 것들만 가져오기
            allBuildingDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }

                // json
                Dictionary<string, BuildingTest> tempBuildingDict = new();



                QuerySnapshot allBuildingDataQuerySnapshot = task.Result;
                foreach (DocumentSnapshot documentSnapshot in allBuildingDataQuerySnapshot.Documents)
                {
                    Dictionary<string, object> buildingDic = documentSnapshot.ToDictionary();
                    BuildingData.Add(documentSnapshot.Id, buildingDic);
                    BuildingThemeSet.Add(buildingDic[nameof(BuildingDataBaseCode.theme)].ToString());

                    // json
                    BuildingTest buildingTest = new();
                    buildingTest.price = int.Parse(buildingDic[nameof(BuildingDataBaseCode.price)].ToString());
                    buildingTest.sizeInCell = int.Parse(buildingDic[nameof(BuildingDataBaseCode.sizeInCell)].ToString());
                    buildingTest.theme = buildingDic[nameof(BuildingDataBaseCode.theme)].ToString();
                    buildingTest.unlockLv = int.Parse(buildingDic[nameof(BuildingDataBaseCode.unlockLv)].ToString());
                    buildingTest.sizeInWorld = float.Parse(buildingDic[nameof(BuildingDataBaseCode.sizeInWorld)].ToString());

                    tempBuildingDict[documentSnapshot.Id] = buildingTest;
                }

                //언락 레벨로정렬 후 가격으로 정렬
                BuildingData = BuildingData.OrderBy(item => item.Value[nameof(BuildingDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(BuildingDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);

                // json file 만들면서 userData에 각종 정보 저장
                string json = DictionaryJsonUtility.ToJson(tempBuildingDict, true);
                File.WriteAllText(buildingPath, json);

                userData.docLastWriteTimeUTCBuilding = new FileInfo(buildingPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                var update = new Dictionary<string, object>
                {
                    { nameof(FirebaseUserKey.docLastWriteTimeUTCBuilding), userData.docLastWriteTimeUTCBuilding }
                };
                docRef.SetAsync(update, SetOptions.MergeAll);

                isDone_BuildingData = true;
            });
        }

        // json 파일이 존재한다면 json의 정보 사용
        string hatPath = Path.Combine(folderPath, "HatData.json");
        if (File.Exists(hatPath) && userData.docVersion == masterCheck.docVersion && userData.docLastWriteTimeUTCHat == new FileInfo(hatPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"))
        {
            // hat 파일 유무 확인 완료, 문서 버전 확인 완료, 수정 여부 확인 완료 후 들어옴
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
            // hat 파일 조건 만족 못해서 들어옴

            // --------- hatDataBase에서 데이터 불러와서 HatData 만드는 부분
            Query allHatDataQuery = db.Collection(nameof(FirebaseCollectionKey.hatData)); // Collection의 모든 문서 가져오기
            allHatDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }
                
                Dictionary<string, HatTest> tempHatDict = new();

                QuerySnapshot allHatDataQuerySnapshot = task.Result;
                foreach (DocumentSnapshot documentSnapshot in allHatDataQuerySnapshot.Documents)
                {
                    Dictionary<string, object> hatDic = documentSnapshot.ToDictionary();
                    HatData.Add(documentSnapshot.Id, hatDic);
                    
                    HatTest hatTest = new();
                    hatTest.price = int.Parse(hatDic[nameof(HatDataBaseCode.price)].ToString());
                    hatTest.sizeInCell = int.Parse(hatDic[nameof(HatDataBaseCode.sizeInCell)].ToString());
                    hatTest.hatClass = hatDic[nameof(HatDataBaseCode.hatClass)].ToString();
                    hatTest.unlockLv = int.Parse(hatDic[nameof(HatDataBaseCode.unlockLv)].ToString());

                    tempHatDict[documentSnapshot.Id] = hatTest;
                }

                //언락 레벨로정렬 후 가격으로 정렬
                HatData = HatData.OrderBy(item => item.Value[nameof(HatDataBaseCode.unlockLv)]).ThenBy(item => item.Value[nameof(HatDataBaseCode.price)]).ToDictionary(x => x.Key, x => x.Value);


                // json file 만들면서 userData에 각종 정보 저장
                string json = DictionaryJsonUtility.ToJson(tempHatDict, true);
                File.WriteAllText(hatPath, json);

                userData.docLastWriteTimeUTCHat = new FileInfo(hatPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");

                var update = new Dictionary<string, object>
                {
                    { nameof(FirebaseUserKey.docLastWriteTimeUTCHat), userData.docLastWriteTimeUTCHat }
                };
                docRef.SetAsync(update, SetOptions.MergeAll);


                isDone_HatData = true;
            });
        }

        string constPath = Path.Combine(folderPath, "ConstValue.json");
        if (File.Exists(constPath) && userData.docVersion == masterCheck.docVersion && userData.docLastWriteTimeUTCConst == new FileInfo(constPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"))
        {
            // const 파일 유무 확인 완료, 문서 버전 확인 완료, 수정 여부 확인 완료 후 들어옴
            string str = File.ReadAllText(constPath);

            constValue = JsonUtility.FromJson<ConstValueTest>(str).Convert();

            isDone_ConstValueData = true;
        }
        else
        {
            // const 파일 조건 만족 못해서 들어옴
            // --------- Set ConstValue
            var constValueDocRef = db.Collection(nameof(FirebaseCollectionKey.constValue)).Document("data");
            constValueDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    constValue = snapshot.ConvertTo<ConstValueFromServer>();
                }
                else
                {
                    // Debug.log("ConstValue load error");
                }

                // json file 만들면서 userData에 각종 정보 저장
                ConstValueTest constValueTest = constValue.Convert();
                string json = JsonUtility.ToJson(constValueTest, true);
                File.WriteAllText(constPath, json);

                userData.docLastWriteTimeUTCConst = new FileInfo(constPath).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                var update = new Dictionary<string, object>
                {
                    { nameof(FirebaseUserKey.docLastWriteTimeUTCConst), userData.docLastWriteTimeUTCConst }
                };
                docRef.SetAsync(update, SetOptions.MergeAll);

                isDone_ConstValueData = true;
            });
        }

        while (!isDone_BuildingData || !isDone_HatData || !isDone_ConstValueData)
            yield return null;

        if (userData.docVersion != masterCheck.docVersion)
        {
            userData.docVersion = masterCheck.docVersion;
            var update = new Dictionary<string, object>
            {
                { nameof(FirebaseUserKey.docVersion), userData.docVersion }
            };
            docRef.SetAsync(update, SetOptions.MergeAll);
        }
        Manager_Initial.isDone_ConstValue = true;
    }
}
