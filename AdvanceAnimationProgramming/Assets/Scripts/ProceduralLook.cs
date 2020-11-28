using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLook : MonoBehaviour
{
    public Transform bone;
    private Quaternion _startRotation;
    public Transform lookAtTransform;

    void Start()
    {
        _startRotation = bone.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!bone || !lookAtTransform)
            return;

        Debug.DrawLine(bone.position, lookAtTransform.position);
        Vector3 lookDisplacement = lookAtTransform.position - bone.position;
        Vector3 lookDir = lookDisplacement.normalized;
        
        bone.rotation = Quaternion.LookRotation(lookDisplacement, Vector3.up) * _startRotation;
    }
}
