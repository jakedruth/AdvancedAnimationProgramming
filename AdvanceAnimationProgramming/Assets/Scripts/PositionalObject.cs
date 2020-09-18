/*
	Advanced Animation Programming
	By Jake Ruth

    PositionObject.cs - A script used to demonstrate the different interpolations
*/

using UnityEngine;
using AdvAnimation;
using Keyframe = AdvAnimation.Keyframe;

public class PositionalObject : MonoBehaviour
{
    private ClipController _clipControllerX;
    private ClipController _clipControllerY;

    public ClipController.EvaluationType stepType;

    private Vector3 _startPos;

    // Start is called before the first frame update
    void Start()
    {
        // Get the starting position to use as an offset
        _startPos = transform.position;

        // create the x values
        Keyframe x0 = new Keyframe(0f, 1f, -4f);
        Keyframe x1 = new Keyframe(1f, 2f, -4f);
        Keyframe x2 = new Keyframe(1f, 2f, +4f);
        Keyframe x3 = new Keyframe(1f, 2f, +4f);

        // create the y values
        Keyframe y0 = new Keyframe(0f, 1f, -4f);
        Keyframe y1 = new Keyframe(1f, 2f, +4f);
        Keyframe y2 = new Keyframe(1f, 2f, +4f);
        Keyframe y3 = new Keyframe(1f, 2f, -4f);

        // create a pool for each channel
        KeyframePool keyframePoolX = new KeyframePool(x0, x1, x2, x3);
        KeyframePool keyframePoolY = new KeyframePool(y0, y1, y2, y3);

        // create a clip for each channel
        Clip clipX = new Clip("channelX", keyframePoolX, 0, 3, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD));
        Clip clipY = new Clip("channelY", keyframePoolY, 0, 3, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD));

        // create a pool for each channel
        ClipPool clipPoolX = new ClipPool(clipX);
        ClipPool clipPoolY = new ClipPool(clipY);

        // initialize the 2 controllers
        _clipControllerY = new ClipController("Controller Y", clipPoolY, 0);
        _clipControllerX = new ClipController("Controller X", clipPoolX, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // update both controllers
        _clipControllerY.Update(Time.deltaTime);
        _clipControllerX.Update(Time.deltaTime);

        // Get the position values from the controllers
        Vector3 pos = Vector3.zero;
        pos.x = _clipControllerX.Evaluate(stepType);
        pos.y = _clipControllerY.Evaluate(stepType);

        // set the position of the sphere object offset from it's starting position
        transform.position = _startPos + pos;
    }
}
