/*
	Advanced Animation Programming
	By Jake Ruth

    Lab04Interface.cs - Handles the interface for lab 04
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvAnimation
{

    public class Lab04Interface : MonoBehaviour
    {
        private InputNode _input;
        public ClipController controller;
        public bool displayGUIControls = true;

        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<InputNode>();
            InitControllerData();
        }

        public void InitControllerData()
        {
            KeyframePool idle = new KeyframePool(
                new Keyframe(0.0f, 0.1f, +0),
                new Keyframe(0.1f, 0.2f, +1));

            KeyframePool rightStart = new KeyframePool(
                new Keyframe(0.0f, 0.5f, +0),
                new Keyframe(0.5f, 1.0f, +1));
            
            KeyframePool rightStep = new KeyframePool(
                new Keyframe(0.0f, 0.5f, -1),
                new Keyframe(0.5f, 1.0f, +1));

            KeyframePool rightStop = new KeyframePool(
                new Keyframe(0.0f, 0.5f, -1),
                new Keyframe(0.5f, 1.0f, +0));

            KeyframePool leftStart = new KeyframePool(
                new Keyframe(0.0f, 0.5f, +0),
                new Keyframe(0.5f, 1.0f, +1));
            
            KeyframePool leftStep = new KeyframePool(
                new Keyframe(0.0f, 0.5f, -1),
                new Keyframe(0.5f, 1.0f, +1));

            KeyframePool leftStop = new KeyframePool(
                new Keyframe(0.0f, 0.5f, -1),
                new Keyframe(0.5f, 1.0f, +0));

            KeyframePool jump = new KeyframePool(
                new Keyframe(0, 1, 0),
                new Keyframe(1, 2, 1));

            KeyframePool falling = new KeyframePool(
                new Keyframe(0, 1, 1),
                new Keyframe(1, 2, 0));
            
            // TODO: Add idle

            ClipPool clipPool = new ClipPool(
                new Clip("idle",       idle,       0, 1, new Transition(TransitionType.FORWARD),               new Transition(TransitionType.BACKWARD)),    // 0  
                new Clip("rightStart", rightStart, 0, 1, new Transition(TransitionType.FORWARD, "leftStop"),   new Transition(TransitionType.PAUSE)),       // 1
                new Clip("rightStep",  rightStep,  0, 1, new Transition(TransitionType.FORWARD, "leftStop"),   new Transition(TransitionType.PAUSE)),       // 2
                new Clip("rightStop",  rightStop,  0, 1, new Transition(TransitionType.FORWARD, "idle"),       new Transition(TransitionType.PAUSE)),       // 3
                new Clip("leftStart",  leftStart,  0, 1, new Transition(TransitionType.FORWARD, "rightStop"),  new Transition(TransitionType.PAUSE)),       // 4
                new Clip("leftStep",   leftStep,   0, 1, new Transition(TransitionType.FORWARD, "rightStop"),  new Transition(TransitionType.PAUSE)),       // 5
                new Clip("leftStop",   leftStop,   0, 1, new Transition(TransitionType.FORWARD, "idle"),       new Transition(TransitionType.PAUSE)),       // 6
                new Clip("jump",       jump,       0, 1, new Transition(TransitionType.FORWARD, "falling"),    new Transition(TransitionType.PAUSE)),       // 7
                new Clip("falling",    falling,    0, 1, new Transition(TransitionType.FORWARD, "idle"),       new Transition(TransitionType.PAUSE))        // 8
            );

            controller = new ClipController("Character Controller", clipPool, 0);
            controller.SetTransitionParameterValue("jumpKey", new Trigger());
            controller.SetTransitionParameterValue("speed", 0f);

            for (int i = 0; i < controller.clipPool.Count - 2; i++) // Don't allow jump and falling to transition to jump
            {
                Clip clip = controller.clipPool[i];
                clip.forwardTransition.SetController(controller);
                clip.forwardTransition.AddCondition("jumpKey", "jump", obj => ((Trigger)obj).Get());
            }

            const float speedThreshold = 0.1f;
            controller.clipPool[0].forwardTransition.AddCondition("speed", "rightStart", obj => (float) obj > speedThreshold);
            controller.clipPool[1].forwardTransition.AddCondition("speed", "leftStep",   obj => (float) obj > speedThreshold);
            controller.clipPool[2].forwardTransition.AddCondition("speed", "leftStep",   obj => (float) obj > speedThreshold);
            controller.clipPool[3].forwardTransition.AddCondition("speed", "rightStart", obj => (float) obj > speedThreshold);
            controller.clipPool[4].forwardTransition.AddCondition("speed", "rightStep",  obj => (float) obj > speedThreshold);
            controller.clipPool[5].forwardTransition.AddCondition("speed", "rightStep",  obj => (float) obj > speedThreshold);
            controller.clipPool[6].forwardTransition.AddCondition("speed", "leftStart",  obj => (float) obj > speedThreshold);
        }

        // Update is called once per frame
        void Update()
        {
            controller.Update(Time.deltaTime);
            if (_input.jumpKeyDown)
            {
                controller.SetTransitionParameterValue("jumpKey", new Trigger(true));
            }

            Vector3 rightStick = _input.rawInputLeft;
            controller.SetTransitionParameterValue("speed", rightStick.sqrMagnitude);
        }

        private void OnGUI()
        {
            const int margin = 10;
            const int padding = 5;
            const int rowHeight = 20;

            int width = 400;
            int height = 460;
            if (!displayGUIControls)
            {
                width = 210;
                height = 27;
            }

            // Create GUI panel
            GUIStyle style = new GUIStyle(GUI.skin.box) {alignment = TextAnchor.UpperLeft};
            GUI.Box(new Rect(margin, margin, width, height), "Clip Controller Interface", style);

            // A button to toggle the displays
            string hideOrShowText = displayGUIControls ? "[Hide]" : "[Expand]";
            if (GUI.Button(new Rect(160, margin + 1, 100, 20), hideOrShowText,
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft}))
            {
                displayGUIControls = !displayGUIControls;
            }

            // return if the displays are not being shown
            if (!displayGUIControls)
                return;

            // display the current controller's name
            Rect clipControllerRect = new Rect(margin + padding, margin + padding + 20, 240, rowHeight);
            GUI.Label(clipControllerRect, $"Current Clip Controller: {controller.name}");

            // display conditional parameters
            KeyValuePair<string, object>[] parameters = controller.transitionParameters.ToArray();
            Rect transitionParametersRect = new Rect(clipControllerRect.x, clipControllerRect.yMax + padding, 500, rowHeight * (1 + parameters.Length));
            string parameterText = "Transition Parameters:\n";
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterText += $"{parameters[i].Key}: \t\t{parameters[i].Value}\n";
            }
            GUI.Label(transitionParametersRect, parameterText);


            // Display all info about the current clip
            Rect clipDataRect = new Rect(transitionParametersRect.x, transitionParametersRect.yMax + padding, 500, rowHeight * 4);
            GUI.Label(clipDataRect, $"Current Clip:\t[{controller.clipIndex}]: '{controller.GetCurrentClip().name}'\n" +
                                    $"Clip Duration:\t{controller.GetCurrentClip().Duration:00.000}\n" +
                                    $"Clip Time:\t\t{controller.clipTime:00.000}\n" +
                                    $"Clip Parameter:\t{controller.clipParameter:0.000}");

            Rect inputRect = new Rect(clipDataRect.x, clipDataRect.yMax + padding, 500, rowHeight * 16);
            GUI.Label(inputRect, "Controls:\n" +
                                 "\tCharacter Locomotion:\n" +
                                 "\t\tLeft Stick\n" +
                                 "\t\tW A S D\n" +
                                 $"\tLocomotion Type: {_input.positionControlType}\n" +
                                 "\t\tD-Pad left and right\n" +
                                 "\t\tQ E\n" +
                                 "\tCharacter Orientation:\n" +
                                 "\t\tRight Stick X-Axis\n" +
                                 "\t\tJ L\n" +
                                 $"\tOrientation Type: {_input.rotationControlType}\n" +
                                 "\t\tD-Pad up and down\n" +
                                 "\t\tU O\n" +
                                 "\tJump (condition transition):\n" +
                                 "\t\tA-Button\n" +
                                 "\t\tSpace Bar");
        }
    }
}