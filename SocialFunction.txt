// 친구 감정 생성 
void CreateFriendMood(int _class)
{
    CheckFriendExist(_class);

    void CheckFriendExist(int _class)
    {
        string friendCode = friendDic[_class][nameof(FriendCode.strCode)].ToString();

        if (friendCode == null || friendCode == "")
            return;

        Query friendCodeQuery = db.Collection(nameof(FirebaseCollectionKey.users)).WhereEqualTo(nameof(FirebaseUserKey.userCode), friendCode);
        friendCodeQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                return;
            }
            else if (task.IsFaulted)
            {
                return;
            }

            QuerySnapshot friendCodeQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in friendCodeQuerySnapshot.Documents)
            {
                Dictionary<string, object> Dic = documentSnapshot.ToDictionary();
                UserData friendData = documentSnapshot.ConvertTo<UserData>();

                if (friendData.friendNormal[nameof(FriendCode.strCode)].ToString() == Manager_Master.Instance.userData.userCode
                || friendData.friendVIP[nameof(FriendCode.strCode)].ToString() == Manager_Master.Instance.userData.userCode
                || friendData.friendVVIP[nameof(FriendCode.strCode)].ToString() == Manager_Master.Instance.userData.userCode)
                {
                    friendDic[_class][nameof(FriendCode.isIlchone)] = true;
                }
                else
                {
                    friendDic[_class][nameof(FriendCode.isIlchone)] = false;
                }

                UpdateUserData();

                CreateMood(Dic[nameof(FirebaseUserKey.previousSelectedMood)].ToString(), _class);
            }
        });
    }

    void CreateMood(string _moodCode, int _class)
    {
        Vector3 spawnPosition;

        var splitCode = _moodCode.Split(',');
        var name = splitCode[(int)MoodCode.name];
        var hatName = splitCode[(int)MoodCode.hatName];

        spawnPosition = new Vector3(Random.Range(-28.0f, 28.0f), 0, Random.Range(-15.0f, 55.0f));
        GameObject goResult = Addressables.InstantiateAsync(name, spawnPosition, new Quaternion(0, 180, 0, 1)).WaitForCompletion();
        goResult.AddComponent<Mood_MyRoom>();
        goResult.GetComponent<Mood_MyRoom>().friendCode = _class;
        NicknameBoard nicknameBoardScript = goMoodFollowUpUI[_class].GetComponent<NicknameBoard>();
        nicknameBoardScript.target = goResult;
        nicknameBoardScript.SetNickname(friendDic[_class][nameof(FriendCode.strNickname)].ToString());
        goMoodFollowUpUI[_class].SetActive(true);

        // 친구용 이펙트
        if (bool.Parse(friendDic[_class][nameof(FriendCode.isIlchone)].ToString()))
        {
            Instantiate(goParticle_SpecialFriend, goResult.transform, false);
        }
        else
        {
            Instantiate(goParticle_Friend, goResult.transform, false);
        }

        // 모자생성
        if (hatName != "null")
        {
            Addressables.InstantiateAsync(hatName, goResult.transform.GetChild(2), false);
        }

        if (bool.Parse(friendDic[_class][nameof(FriendCode.isPossibleDaily)].ToString()))
        {
            var heart = Instantiate(goDailyHeart_Friend, goResult.transform, false);
            heart.GetComponent<DailyReward>().rewardType = _class;
        }



        goFriendMood[_class] = goResult;
    }
}



// 친구 창 
public void InitSocialMenu(bool isStart = false)
{
    txtUserCode.text = string.Format($"{Manager_Master.Instance.userData.userCode}");

    // 친구 코드 갱신
    txtFriendCodeInputField[(int)UserClass.normal].text = Manager_Master.Instance.userData.friendNormal[nameof(FriendCode.strNickname)].ToString();
    txtFriendCodeInputField[(int)UserClass.VIP].text = Manager_Master.Instance.userData.friendVIP[nameof(FriendCode.strNickname)].ToString();
    txtFriendCodeInputField[(int)UserClass.VVIP].text = Manager_Master.Instance.userData.friendVVIP[nameof(FriendCode.strNickname)].ToString();

    for (int userClass = 0, count = (int)UserClass.count; userClass < count; ++userClass)
    {
        if (friendDic[userClass][nameof(FriendCode.strCode)].ToString() != "")
        {
            // 일촌 표시 갱신
            if (bool.Parse(friendDic[userClass][nameof(FriendCode.isIlchone)].ToString()))
                goIlchon[userClass].GetComponent<Image>().sprite = spriteIlchoneFill;
            else
                goIlchon[userClass].GetComponent<Image>().sprite = spriteIlchoneEmpty;

            // 입력 버튼 갱신 (설정버전)
            txtFriendCodeInputField[userClass].transform.GetChild(2).gameObject.SetActive(false);
            txtFriendCodeInputField[userClass].transform.GetChild(3).gameObject.SetActive(true);
            txtFriendCodeInputField[userClass].readOnly = true;
            var image = txtFriendCodeInputField[userClass].GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
        else
        {
            // 일촌 표시 갱신
            goIlchon[userClass].GetComponent<Image>().sprite = spriteIlchoneNull;

            // 입력 버튼 갱신 (설정버전)
            txtFriendCodeInputField[userClass].transform.GetChild(2).gameObject.SetActive(true);
            txtFriendCodeInputField[userClass].transform.GetChild(3).gameObject.SetActive(false);
            txtFriendCodeInputField[userClass].readOnly = false;
            var image = txtFriendCodeInputField[userClass].GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
    }

    // 입력 버튼 갱신 (구매버전)
    // VIP
    if (Manager_Master.Instance.userData.userClass == nameof(UserClass.normal))
    {
        btn_Connect_VIP.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        btn_Connect_VIP.gameObject.transform.GetChild(1).gameObject.SetActive(true);

        goCover_VIP.SetActive(true);
        txtFriendCodeInputField[(int)UserClass.VIP].readOnly = true;
    }
    else
    {
        goCover_VIP.SetActive(false);
    }

    // VVIP
    if (Manager_Master.Instance.userData.userClass != nameof(UserClass.VVIP))
    {
        btn_Connect_VVIP.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        btn_Connect_VVIP.gameObject.transform.GetChild(1).gameObject.SetActive(true);

        goCover_VVIP.SetActive(true);
        txtFriendCodeInputField[(int)UserClass.VVIP].readOnly = true;
    }
    else
    {
        goCover_VVIP.SetActive(false);
    }
}
