using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using TMPro;

using System;
using System.Collections.Generic;
using System.Collections;

using Google;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

using Firebase;
using Firebase.Auth;
using Firebase.Messaging;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;

using UnityEngine.SceneManagement;



public class Manager_Initial : MonoBehaviour
{
    
    // 각종 변수들..

    private void Start()
    {
        Init();


        StartCoroutine(CheckUnderMaintenance());
    }

    private void Update()
    {
        // ..
    }

    void Init()
    {
        isDone_CheckMaintenance = false;
        isDone_Login = false;
        isDone_ConstValue = false;
        isDone_Bundle = false;
        isDone_UserData = false;

        // 기기 언어 체크
        var kindOfLanguage = PlayerPrefs.GetString(nameof(PlayerPrefsCode.kindOfLanguage));
        if (kindOfLanguage == "" || kindOfLanguage == null)
        {
            var systemLanguage = Application.systemLanguage;
            switch (systemLanguage)
            {
                case SystemLanguage.Korean:
                    kindOfLanguage = nameof(LanguageDataBaseCode.kor);
                    break;
                case SystemLanguage.English:
                    kindOfLanguage = nameof(LanguageDataBaseCode.eng);
                    break;
                default:
                    kindOfLanguage = nameof(LanguageDataBaseCode.eng);
                    break;
            }
        }
        Manager_Master.Instance.kindOfLanguage = kindOfLanguage;

        imgGameTitle.sprite = sprtGameTitle[(int)Manager_Master.Instance.StringToEnum<LanguageDataBaseCode>(kindOfLanguage)];

        localAuth = FirebaseAuth.DefaultInstance;
        Manager_Master.Instance.db = FirebaseFirestore.DefaultInstance;
    }

    IEnumerator CheckUnderMaintenance()
    {
        WaitForSeconds waitTime = new WaitForSeconds(1);

        // 언어 다운
        log = "Set Language";
        StartCoroutine(Messaging());

        Manager_Master.Instance.SetLanguageDict();

        while (!isDone_Language)
        {
            yield return waitTime;
        }


        // 서버 점검중 확인
        log = "Check during maintenance";

        Manager_Master.Instance.CheckUnderMaintenance();

        while (!isDone_CheckMaintenance)
        {
            yield return waitTime;
        }


        // 점검중이 아니면
        if (!Manager_Master.Instance.masterCheck.isUnderMaintenance)
            StartCoroutine(InitFlow());
        else
        {
            log = "";
            Manager_Master.Instance.Warnning(Manager_Master.Instance.LanguageDict["LAN_TEXT_035"]);
        }
    }

    private void InitLanguage()
    {
        // ..
    }

    IEnumerator InitFlow()
    {
        // Debug.Log(Manager_Master.Instance.InternetCheck());

        WaitForSeconds waitTime = new WaitForSeconds(1);


        // ====================================================================== Const Value Set
        // ..고정 값 세팅 관련 코드

        // ====================================================================== Login Set
        // ..Log in 관련 코드

        // ====================================================================== Bundle Set
        log = "Preparing Bundle";
        StartCoroutine(BundleSizeCheck());

        while (!isDone_Bundle)
        {
            yield return waitTime;
        }
        Debug.Log("Bundle Set Success");


        // ====================================================================== UserData Set
        // ..유저 데이터 세팅 관련 코드

        
        yield return new WaitForSeconds(1);
        goGameStartBTN.SetActive(true);
    }
    
    // for Addressable
    IEnumerator BundleSizeCheck()
    {
        log = "Check download content";

        Addressables.GetDownloadSizeAsync(LabelForBundleDown).Completed += (AsyncOperationHandle<long> SizeHandle) =>
        {
            if (SizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(SizeHandle);

                long totalSize = SizeHandle.Result;
                string size = "";

                if (totalSize > 0)
                {
                    if (totalSize >= 1000000000)
                        size = string.Format($"{totalSize / 1000000000.0f:F2}") + " GB";
                    else if (totalSize >= 1000000)
                        size = string.Format($"{totalSize / 1000000.0f:F2}") + " MB";
                    else if (totalSize >= 1000)
                        size = string.Format($"{totalSize / 1000.0f:F2}") + " KB";
                    else
                        size = string.Concat(totalSize, " byte");

                    downloadSize.text = size;
                    goProgressBar.SetActive(true);
                    buttonDownload.onClick.RemoveAllListeners();
                    buttonDownload.onClick.AddListener(delegate { BTN_BundleDown(); });
                    goDownPopup.SetActive(true);
                }
                else
                {
                    downloadPercent.text = Manager_Master.Instance.LanguageDict["LAN_TEXT_002"];
                    isDone_Bundle = true;
                }
            }
        };

        yield return null;
    }
    public void BTN_BundleDown()
    {
        downloadHandle = Addressables.DownloadDependenciesAsync(LabelForBundleDown, true);

        StartCoroutine(BundleDownPercent());

        downloadHandle.Completed += (AsyncOperationHandle Handle) =>
        {
            if (Handle.Status == AsyncOperationStatus.Succeeded)
            {
                //다운로드가 끝나면 메모리 해제.
                Addressables.Release(Handle);

                StartCoroutine(Finish());
            }
            else
                Manager_Master.Instance.Warnning("error : bundle download failed");
        };

        IEnumerator Finish()
        {
            downloadPercent.text = Manager_Master.Instance.LanguageDict["LAN_TEXT_003"];
            slider.value = 1;
            yield return new WaitForSeconds(1);

            goProgressBar.SetActive(false);
            isDone_Bundle = true;
        }
    }
    IEnumerator BundleDownPercent()
    {
        while (downloadHandle.IsValid())
        {
            downloadPercent.text = string.Concat(downloadHandle.PercentComplete * 100, " %");
            slider.value = downloadHandle.PercentComplete;
            yield return null;
        }
    }
    // ~for Addressable

    void SetMood()
    {
       // ..
    }

    public void BTN_GameStart()
    {
       // ..
    }

    IEnumerator Messaging()
    {
        // ..
    }

    public void BTN_LinkToPolicyURL()
    {
        // ..
    }
}
