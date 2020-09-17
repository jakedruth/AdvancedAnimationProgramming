using System.Collections;
using System.Collections.Generic;
using AdvAnimation;
using UnityEngine;

public class SpriteHandler : MonoBehaviour
{
    public ClipController controller;
    public ClipController.EvaluationType stepType;

    void Start()
    {
        ClipControllerInterface controllerInterface = FindObjectOfType<ClipControllerInterface>();
        controller = controllerInterface.controllers[0];
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.y = controller.Evaluate(stepType);
        transform.position = pos;
    }
}
