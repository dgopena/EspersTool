using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance;

    [SerializeField] private GameObject notifPrefab;

    private Queue<NotifSet> notifQueue;

    [Header("Settings")]
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float lifeTime = 5f;

    [Space(10f)]
    [SerializeField] private float notifSpacing = 10f;
    [SerializeField] private float notifHeight = 125f;

    private float pushingTime;
    private float startPushAt;

    private class NotifSet
    {
        public RectTransform notifElem;
        public CanvasGroup notifAlpha;
        public float targetHeight;

        public float startHeight;

        public float birthTime;

        public void AddToHeight(float val)
        {
            targetHeight += val;
        }

        public void SetAlpha(float val)
        {
            notifAlpha.alpha = val;
        }

        public void RefreshStartHeight()
        {
            startHeight = notifElem.anchoredPosition.y;
        }

        public void ApplyFracReHeight(float t)
        {
            Vector3 v = new Vector3();
            v.y = Mathf.Lerp(startHeight, targetHeight, t);
            notifElem.anchoredPosition = v;
        }

        public void ApplyFracReAlpha(float t)
        {
            notifAlpha.alpha = Mathf.Lerp(1f, 0f, t);
        }
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        notifQueue = new Queue<NotifSet>();

        //StartCoroutine(TestRoutine());
    }

    private IEnumerator TestRoutine()
    {
        yield return new WaitForSeconds(2f);

        PushNotification("one");

        yield return new WaitForSeconds(1f);

        PushNotification("two"); 

        yield return new WaitForSeconds(0.5f);

        PushNotification("three"); 
        
        yield return new WaitForSeconds(3f);

        PushNotification("four");

        yield return new WaitForSeconds(7f);

        PushNotification("last");
    }

    private void FixedUpdate()
    {
        //movement
        if (pushingTime > 0f)
        {
            pushingTime -= Time.deltaTime;

            float t = 1 - (pushingTime / startPushAt);

            foreach (NotifSet set in notifQueue)
            {
                set.ApplyFracReHeight(t);
            }
        }

        int callDequeue = 0;
        //alpha lifetime
        foreach(NotifSet set in notifQueue)
        {
            float diff = (Time.time - set.birthTime) - lifeTime;
            float t = 0;
            if (diff > 0f && diff < fadeTime)
                t = diff / fadeTime;
            else if (diff >= fadeTime)
            {
                t = 1f;
                callDequeue++;
            }

            set.ApplyFracReAlpha(t);
        }

        for (int i = callDequeue; i > 0; i--)
        {
            NotifSet set = notifQueue.Dequeue();
            Destroy(set.notifElem.gameObject);
        }
    }

    public void PushNotification(string notice)
    {
        GameObject nuNotice = Instantiate<GameObject>(notifPrefab, transform);
        RectTransform noticeRT = nuNotice.GetComponent<RectTransform>();

        noticeRT.GetChild(0).GetComponent<TextMeshProUGUI>().text = notice;
        noticeRT.sizeDelta = new Vector2(noticeRT.sizeDelta.x, notifHeight);

        Vector3 posPoint = Vector3.zero;
        posPoint.y = -1f * notifHeight;

        noticeRT.anchoredPosition = posPoint;

        pushingTime += moveTime;

        foreach(NotifSet set in notifQueue)
        {
            set.AddToHeight(notifSpacing + notifHeight);
            set.RefreshStartHeight();
        }

        nuNotice.SetActive(true);

        NotifSet nuSet = new NotifSet() { birthTime = Time.time, notifAlpha = nuNotice.GetComponent<CanvasGroup>(), notifElem = noticeRT, targetHeight = notifSpacing };
        nuSet.RefreshStartHeight();
        notifQueue.Enqueue(nuSet);

        startPushAt = pushingTime;
    }
}
