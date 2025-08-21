using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[DisallowMultipleComponent]
public class HandTeleport : MonoBehaviour
{
    [Header("References")]
    [SerializeField] XRRayInteractor teleportRay;            // 常駐啟用
    [SerializeField] TeleportationProvider provider;         // 自動抓
    [SerializeField] InputActionReference pinchTeleport;     // Button(Press)

    [Header("Options")]
    [SerializeField] bool faceTravelDirection = true;        // 落地面向移動方向（XROrigin -> 命中點）

    void Awake()
    {
        if (!teleportRay) teleportRay = GetComponent<XRRayInteractor>();

        // 自動抓 provider：先從父物件找，再全場景找
        if (!provider)
        {
            provider = GetComponentInParent<TeleportationProvider>();
#if UNITY_2023_1_OR_NEWER
            if (!provider) provider = FindFirstObjectByType<TeleportationProvider>();
#else
            if (!provider) provider = FindObjectOfType<TeleportationProvider>();
#endif
        }
    }

    void OnEnable()
    {
        if (pinchTeleport && pinchTeleport.action != null)
        {
            var a = pinchTeleport.action;
            a.Enable();
            // 按下就傳送
            a.performed += OnPinchPressed;
        }
    }

    void OnDisable()
    {
        if (pinchTeleport && pinchTeleport.action != null)
            pinchTeleport.action.performed -= OnPinchPressed;
    }

    void OnPinchPressed(InputAction.CallbackContext _)
    {
        TryTeleportAtRayHit();
    }

    void TryTeleportAtRayHit()
    {
        if (!teleportRay || !provider) return;

        // 命中資訊
        if (!teleportRay.TryGetHitInfo(out var hitPos, out _, out _, out _))
            return;

        // 只允許 TeleportationArea / TeleportationAnchor
        if (!teleportRay.TryGetCurrent3DRaycastHit(out var hit)) return;
        var hitGo = hit.collider ? hit.collider.gameObject : null;
        if (!hitGo) return;

        bool isTeleportable =
            hitGo.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>()  != null ||
            hitGo.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>() != null;

        if (!isTeleportable) return;

        // 計算落地朝向
        var rot = Quaternion.identity;
        if (faceTravelDirection)
        {
            var from = provider.transform.position; // XROrigin
            var dir  = hitPos - from;
            dir.y = 0f;

            if (dir.sqrMagnitude < 1e-4f)
            {
                dir = teleportRay.transform.forward; // 退而求其次：手的水平前方
                dir.y = 0f;
            }
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;

            rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        var req = new TeleportRequest
        {
            destinationPosition = hitPos,
            destinationRotation = rot,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };
        provider.QueueTeleportRequest(req);
    }
}