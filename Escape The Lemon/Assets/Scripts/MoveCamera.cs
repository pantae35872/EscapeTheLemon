using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : GameplayMonoBehaviour
{
    public Transform cameraPosition;
    private Transform cacheTransform;

    private void Awake()
    {
        cacheTransform = transform;
    }

    protected override void PauseableUpdate()
    {
        cacheTransform.rotation = Quaternion.Euler(0, 0, 0);
        cacheTransform.position = cameraPosition.position;
    }
}
