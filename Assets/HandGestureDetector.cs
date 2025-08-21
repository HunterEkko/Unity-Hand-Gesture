using UnityEngine;
using TMPro;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.Events;
using UnityEngine.XR.Hands;

// 手勢偵測元件，當手停在某個動作或姿勢超過指定時間後觸發事件
public class HandGestureDetector : MonoBehaviour
{
    [SerializeField]
    XRHandTrackingEvents m_HandTrackingEvents; // 接收手部關節資料更新事件的元件

    [SerializeField]
    ScriptableObject m_HandShapeOrPose; // 可被設定成 XRHandShape 或 XRHandPose 的 ScriptableObject

    [SerializeField]
    Transform m_TargetTransform; // 如果使用 XRHandPose，這個是目標 Transform，判斷相對旋轉用

    [SerializeField]
    string textObjectName = "GestureText";

    [SerializeField]
    UnityEvent m_GesturePerformed; // 當手勢成功完成時要執行的事件

    [SerializeField]
    UnityEvent m_GestureEnded; // 當手勢結束時要執行的事件

    [SerializeField]
    float m_MinimumHoldTime = 0.2f; // 停在指定手勢超過這個時間，才會觸發事件

    [SerializeField]
    float m_GestureDetectionInterval = 0.1f; // 每隔多久檢查一次是否達成手勢

    static TMP_Text gestureText;
    XRHandShape m_HandShape; // 內部使用：轉型後存放 XRHandShape
    XRHandPose m_HandPose; // 內部使用：轉型後存放 XRHandPose
    bool m_WasDetected; // 上一幀是否有偵測到符合的手勢
    bool m_PerformedTriggered; // 是否已經觸發 m_GesturePerformed
    float m_TimeOfLastConditionCheck; // 上次檢查手勢條件的時間
    float m_HoldStartTime; // 開始保持手勢的時間

    void Awake()
    {
        // 自動尋找場景中的 TMP_Text（只找一次）
        if (gestureText == null)
            gestureText = GameObject.Find(textObjectName)?.GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        m_HandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

        // 嘗試把 ScriptableObject 轉型為 XRHandShape 或 XRHandPose
        m_HandShape = m_HandShapeOrPose as XRHandShape;
        m_HandPose = m_HandShapeOrPose as XRHandPose;

        // 如果是 XRHandPose，並且它有相對方向條件，設定其目標 Transform
        if (m_HandPose != null && m_HandPose.relativeOrientation != null)
            m_HandPose.relativeOrientation.targetTransform = m_TargetTransform;
    }

    void OnDisable()
    {
        m_HandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
    }

    // 每次關節更新時呼叫，檢查是否符合手勢條件
    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        // 如果太快檢查（小於設定間隔時間），就跳過這次
        if (!isActiveAndEnabled || Time.timeSinceLevelLoad < m_TimeOfLastConditionCheck + m_GestureDetectionInterval)
            return;

        // 檢查是否有偵測到手勢
        var detected =
            m_HandTrackingEvents.handIsTracked && // 手部有追蹤到
            m_HandShape != null && m_HandShape.CheckConditions(eventArgs) || // 手勢符合 shape 條件
            m_HandPose != null && m_HandPose.CheckConditions(eventArgs); // 或者符合 pose 條件

        // 若前一幀沒偵測到，這幀偵測到，代表剛開始 → 記錄時間
        if (!m_WasDetected && detected)
        {
            m_HoldStartTime = Time.timeSinceLevelLoad;
        }
        // 若上一幀偵測到但這幀沒偵測到 → 重設狀態、觸發結束事件
        else if (m_WasDetected && !detected)
        {
            m_PerformedTriggered = false;
            m_GestureEnded?.Invoke();
        }

        // 更新上一幀的偵測狀態
        m_WasDetected = detected;

        // 如果還沒觸發過執行事件，且目前偵測到
        if (!m_PerformedTriggered && detected)
        {
            // 計算保持這個手勢的時間
            var holdTimer = Time.timeSinceLevelLoad - m_HoldStartTime;

            // 如果保持超過設定時間，觸發手勢事件
            if (holdTimer > m_MinimumHoldTime)
            {
                if (gestureText != null)
                {
                    if (m_HandShape != null) gestureText.text = m_HandShape.name;
                    else if (m_HandPose != null) gestureText.text = m_HandPose.name;
                }
                m_GesturePerformed?.Invoke();
                m_PerformedTriggered = true; // 記得已經觸發過了
            }
        }

        // 更新這次檢查的時間
        m_TimeOfLastConditionCheck = Time.timeSinceLevelLoad;
    }
}
