using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Mood_MyRoom : MonoBehaviour
{
    public int friendCode = -1;

    private NavMeshAgent agent;
    private Animator anim;

    private int animTriggerID_Walk = Animator.StringToHash("walk");
    private int animTriggerID_Touch = Animator.StringToHash("touch");
    private int animTriggerID_Idle = Animator.StringToHash("idle");
    private string moodStrIdx;
    private string moodName;

    private void Start()
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        SetNewDestination();

        moodStrIdx = gameObject.name.Split('_')[0];
        moodName = Manager_MyRoom.Instance.GetMoodNameFromIdx(moodStrIdx);
    }

    private void Update()
    {
        // 멈췄을 때
        if (agent.remainingDistance < 0.1f && !agent.isStopped && !agent.pathPending)
        {
            agent.isStopped = true;
            StartCoroutine(ArriveAtDestination());
        }
    }

    private void OnMouseUpAsButton()
    {
#if UNITY_EDITOR
        if (EventSystem.current.IsPointerOverGameObject())
            return;
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
                return;
        }
#endif

        StopCoroutine(nameof(MoodTouch));
        StartCoroutine(nameof(MoodTouch));
    }
    private IEnumerator MoodTouch()
    {
        agent.isStopped = true;
        anim.SetTrigger(animTriggerID_Touch);

        if (friendCode > -1)
        {
            ChangeNickName();
        }

        string text = Manager_Master.Instance.InsertValueToText("MSG000070", string.Format($"{moodName}"));
        Manager_MyRoom.Instance.PopupMalpoongsun(text, gameObject, moodStrIdx);

        yield return new WaitForSeconds(1);

        SetNewDestination();
    }

    private void SetNewDestination()
    {
        agent.SetDestination(new Vector3(Random.Range((int)MinValue.moodMoveRangeX, (int)MaxValue.moodMoveRangeX), 0.3f, Random.Range((int)MinValue.moodMoveRangeZ, (int)MaxValue.moodMoveRangeZ)));
        anim.SetTrigger(animTriggerID_Walk);
        agent.isStopped = false;
    }

    IEnumerator ArriveAtDestination()
    {
        anim.SetTrigger(animTriggerID_Idle);

        yield return new WaitForSeconds(Random.Range(2.0f, 5.0f));

        SetNewDestination();
    }

    private void ChangeNickName()
    {
        string text = Manager_Master.Instance.InsertValueToText(
            "MSG000069",
            string.Format($"{Manager_MyRoom.Instance.friendDic[friendCode][nameof(FriendCode.strNickname)]}"),
            string.Format($"{Manager_MyRoom.Instance.GetMoodNameFromIdx(moodStrIdx)}")
            );
        moodName = text;
    }
}
