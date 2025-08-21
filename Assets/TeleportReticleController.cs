using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[DisallowMultipleComponent]
public class TeleportReticleController : MonoBehaviour
{
    [SerializeField] XRRayInteractor ray;               // 同物件上的 XRRayInteractor
    [SerializeField] XRInteractorLineVisual lineVisual; // 同物件上的 XRInteractorLineVisual

    void Awake()
    {
        if (!ray) ray = GetComponent<XRRayInteractor>();
        if (!lineVisual) lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    void LateUpdate()
    {
        if (!ray || !lineVisual) return;

        // 只有打到 TeleportationArea/Anchor 才顯示 reticle
        bool canTeleport = IsHittingTeleportSurface();

        var r = lineVisual.reticle;          // 正常 reticle
        var br = lineVisual.blockedReticle;   // 阻擋 reticle（傳送用不到）

        if (r) r.SetActive(canTeleport);
        if (br) br.SetActive(false);          // 永遠關掉阻擋樣式，避免視覺殘留
    }

    void OnDisable()
    {
        // 保險：物件被關時順便把 reticle 關掉，避免場景中殘留
        if (lineVisual)
        {
            if (lineVisual.reticle) lineVisual.reticle.SetActive(false);
            if (lineVisual.blockedReticle) lineVisual.blockedReticle.SetActive(false);
        }
    }

    bool IsHittingTeleportSurface()
    {
        // 沒有 3D 命中就不可傳送
        if (!ray.TryGetCurrent3DRaycastHit(out var hit))
            return false;

        var go = hit.collider ? hit.collider.gameObject : null;
        if (!go) return false;

        // 命中 TeleportationArea 或 TeleportationAnchor 才算可傳送
        return
            go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>() != null ||
            go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>() != null;
    }
}
