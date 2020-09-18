/*
	Advanced Animation Programming
	By Jake Ruth

    SpriteHandler.cs - A simple script used to set the texture coordinates of a sprite
*/


using AdvAnimation;
using UnityEngine;

public class SpriteHandler : MonoBehaviour
{
    private const int ROW_COUNT = 8;
    private const float ROW_COUNT_INVERSE = 1f / ROW_COUNT;

    public ClipController controller;
    public string controllerName;
    public ClipController.EvaluationType stepType;

    private Material _mainTex;

    void Start()
    {
        // Get the Controller to be referenced
        ClipControllerInterface controllerInterface = FindObjectOfType<ClipControllerInterface>();
        controller = controllerInterface.GetClipControllerByName(controllerName);

        // Get the material that will be manipulated by the controller
        _mainTex = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        // Convert the evaluation of the controller from a 1D data point to a 2D value
        Vector2 uvOffset;
        int index = Mathf.RoundToInt(controller.Evaluate(stepType));
        uvOffset.x = (index % ROW_COUNT) * ROW_COUNT_INVERSE;
        uvOffset.y = (index / ROW_COUNT) * ROW_COUNT_INVERSE; // invert Y so it counts from the top left of the image

        // adjust the texture's offest to animate it
        _mainTex.SetTextureOffset("_MainTex", uvOffset);
    }
}
