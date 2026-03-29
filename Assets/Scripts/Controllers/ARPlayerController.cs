using System.Collections.Generic;                      // ✅ pour List<>
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Netcode;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlayerController : NetworkBehaviour
{
    private ARRaycastManager raycastManager;
    private SharedSpatialAnchor sharedAnchor;
    private bool anchorSet = false;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        sharedAnchor = FindFirstObjectByType<SharedSpatialAnchor>();  // ✅ corrigé
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!anchorSet && Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TrySetAnchor(Input.GetTouch(0).position);
        }
    }

    private void TrySetAnchor(Vector2 screenPos)
    {
        var hits = new List<ARRaycastHit>();            // ✅ fonctionne maintenant
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            sharedAnchor.SetAnchorFromAR(hits[0].pose);
            anchorSet = true;
        }
    }
}