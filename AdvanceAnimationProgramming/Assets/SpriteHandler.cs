using System.Collections;
using System.Collections.Generic;
using AdvAnimation;
using UnityEngine;

public class SpriteHandler : MonoBehaviour
{
    private const int ROW_COUNT = 8;
    private const float ROW_COUNT_INVERSE = 1f / ROW_COUNT;

    public ClipController controller;
    public ClipController.EvaluationType stepType;

    private Material _mainTex;

    void Start()
    {
        ClipControllerInterface controllerInterface = FindObjectOfType<ClipControllerInterface>();
        controller = controllerInterface.controllers[0];

        _mainTex = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        Vector2 uvOffset;
        int index = Mathf.RoundToInt(controller.Evaluate(ClipController.EvaluationType.STEP));

        uvOffset.x = (index % ROW_COUNT) * ROW_COUNT_INVERSE;
        uvOffset.y = (index / ROW_COUNT) * ROW_COUNT_INVERSE; // invert Y so it counts from the top left of the image

        _mainTex.SetTextureOffset("_MainTex", uvOffset);
    }
}
