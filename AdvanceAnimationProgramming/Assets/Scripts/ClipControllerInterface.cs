/*
	Advanced Animation Programming
	By Jake Ruth

    ClipControllerInterface.cs - Creates a GUI to allow input into a clip controller
*/

using UnityEngine;
using Random = UnityEngine.Random;

namespace AdvAnimation
{
    /// <summary>
    /// The interface to control multiple clip controllers
    /// </summary>
    public class ClipControllerInterface : MonoBehaviour
    {
        public ClipController[] controllers;
        private int _currentClipControllerIndex;
        public bool displayGUIControls = true;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Awake()
        {
            GenerateTestingData();
        }

        /// <summary>
        /// Used to display a simple GUI to the camera
        /// </summary>
        void OnGUI()
        {
            // get the current controller
            ClipController current = controllers[_currentClipControllerIndex];

            // Most of these numbers were chooses as good enough looking values to render a gui. 
            // All hard coded values in rects are used as they were large enough to display the respective value (i.e. text and buttons)

            const int margin = 10;
            const int padding = 5;
            const int rowHeight = 20;

            int width = 400;
            int height = 400;
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

            // Create the rects to hold the controller information
            Rect previousControllerButton = new Rect(margin + padding, margin + padding + 20, 20, rowHeight);
            Rect nextControllerButton = new Rect(previousControllerButton.xMax + 5, previousControllerButton.y,
                previousControllerButton.width, rowHeight);
            Rect currentControllerLabel =
                new Rect(nextControllerButton.xMax + 5, nextControllerButton.y, 240, rowHeight);

            // If pressed, cycle through the available controllers
            if (GUI.Button(previousControllerButton, "<"))
            {
                _currentClipControllerIndex--;
                if (_currentClipControllerIndex < 0)
                    _currentClipControllerIndex = controllers.Length - 1;
            }

            if (GUI.Button(nextControllerButton, ">"))
            {
                _currentClipControllerIndex++;
                if (_currentClipControllerIndex >= controllers.Length)
                    _currentClipControllerIndex = 0;
            }

            // display the current controller's name
            GUI.Label(currentControllerLabel, $"Current Clip Controller: {current.name}");

            // Create the rects to hold the playback controls
            Rect playbackReverse = new Rect(margin + padding, currentControllerLabel.yMax + padding, 65, rowHeight);
            Rect playbackPause = new Rect(playbackReverse.xMax + 5, playbackReverse.y, 65, rowHeight);
            Rect playbackForward = new Rect(playbackPause.xMax + 5, playbackReverse.y, 65, rowHeight);

            // if any toggle is pressed, set the respective playback control
            if (GUI.Toggle(playbackReverse, current.playback == PlaybackDirection.REVERSE, "Reverse"))
                current.playback = PlaybackDirection.REVERSE;
            if (GUI.Toggle(playbackPause, current.playback == PlaybackDirection.PAUSE, "Pause"))
                current.playback = PlaybackDirection.PAUSE;
            if (GUI.Toggle(playbackForward, current.playback == PlaybackDirection.FORWARD, "Forward"))
                current.playback = PlaybackDirection.FORWARD;

            // Create the rects to hold a playback speed multiplier
            Rect playbackMultiplierLabel = new Rect(margin + padding, playbackForward.yMax + padding, 180, rowHeight);
            Rect playbackMultiplierSlider = new Rect(playbackMultiplierLabel.xMax + padding,
                playbackMultiplierLabel.y + 5, width - playbackMultiplierLabel.width - padding * 3, rowHeight);

            // Display a label and slider to control the playback speed multiplier from 0 to 2
            GUI.Label(playbackMultiplierLabel, $"Playback speed multiplier: {current.playbackSpeed:F1}");
            current.playbackSpeed =
                GUI.HorizontalSlider(playbackMultiplierSlider, current.playbackSpeed, 0,
                    2); // a scale of 0 to 2 keeps normal speed in the middle of the slider
            current.playbackSpeed =
                Mathf.Round(current.playbackSpeed * 10) / 10; // round playback speed to closest tenth

            // Create the rects to hold clip controls
            Rect previousClipButton =
                new Rect(margin + padding, playbackMultiplierSlider.yMax + padding, 20, rowHeight);
            Rect nextClipButton = new Rect(previousClipButton.xMax + 5, previousClipButton.y, previousClipButton.width,
                rowHeight);
            Rect currentClipLabel = new Rect(nextClipButton.xMax + 5, nextClipButton.y, 240, rowHeight);

            // If pressed, cycle through the available clips
            if (GUI.Button(previousClipButton, "<"))
            {
                current.GoToPrevClip();
            }

            if (GUI.Button(nextClipButton, ">"))
            {
                current.GoToNextClip();
            }

            GUI.Label(currentClipLabel, $"Change Current Clip");

            // Display all info about the current clip
            Rect clipTimeLabel = new Rect(margin + padding, currentClipLabel.yMax + padding, 500, rowHeight * 4);
            GUI.Label(clipTimeLabel, $"Current Clip:\t[{current.clipIndex}]: '{current.GetCurrentClip().name}'\n" +
                                     $"Clip Duration:\t{current.GetCurrentClip().Duration:00.000}\n" +
                                     $"Clip Time:\t\t{current.clipTime:00.000}\n" +
                                     $"Clip Parameter:\t{current.clipParameter:0.000}");

            // Display all info about the current keyframe
            Rect keyframeTimeLabel = new Rect(margin + padding, clipTimeLabel.yMax + padding, 500, rowHeight * 4);
            GUI.Label(keyframeTimeLabel, $"Current keyframe:\t[{current.GetCurrentKeyframe().index:00.}]\n" +
                                         $"Value:\t\t{current.GetCurrentKeyframe().value}\n" +
                                         $"Keyframe Duration:\t{current.GetCurrentKeyframe().Duration:0.000}\n" +
                                         $"Keyframe Time:\t{current.keyframeTime:0.000}\n" +
                                         $"Keyframe Parameter:\t{current.keyframeParameter:0.000}");

            Rect evaluateRect = new Rect(margin + padding, keyframeTimeLabel.yMax + padding, 500, rowHeight * 4);
            GUI.Label(evaluateRect, "Evaluations\n" +
                                    $" - Step:\t\t{current.Evaluate(ClipController.EvaluationType.STEP)}\n" +
                                    $" - Nearest:\t\t{current.Evaluate(ClipController.EvaluationType.NEAREST)}\n" +
                                    $" - Lerp:\t\t{current.Evaluate(ClipController.EvaluationType.LERP)}\n" +
                                    $" - Catmull-Rom:\t{current.Evaluate(ClipController.EvaluationType.CATMULL_ROM)}");
        }

        /// <summary>
        /// Update is called once per frame 
        /// </summary>
        void Update()
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Used to create random value for testing
        /// </summary>
        private void GenerateTestingData()
        {
            const int numKeyframes = 64;
            const float deltaTime = 0.1f;

            Keyframe[] frames = new Keyframe[numKeyframes];

            for (int i = 0; i < numKeyframes; i++)
            {
                float start = i * deltaTime;
                float end = (i + 1) * deltaTime;

                float data = i;

                frames[i] = new Keyframe(start, end, data);
            }

            KeyframePool poolFloat = new KeyframePool(frames);

            ClipPool pool = new ClipPool(new[]
            {
                new Clip("rowA", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowB"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowH")),
                new Clip("rowB", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowC"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowA")),
                new Clip("rowC", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowD"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowB")),
                new Clip("rowD", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowE"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowC")),
                new Clip("rowE", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowF"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowD")),
                new Clip("rowF", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowG"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowE")),
                new Clip("rowG", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowH"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowF")),
                new Clip("rowH", poolFloat, 0, 7, new Transition(TransitionType.FORWARD_PLAYBACK, "rowA"), new Transition(TransitionType.BACKWARD_PLAYBACK, "rowG")),
            });

            controllers = new[]
            {
                new ClipController("Controller A", pool, 0),
                new ClipController("Controller B", pool, 0),
                new ClipController("Controller C", pool, 0),
            };

            // set the current controller to the first one, Controller A
            _currentClipControllerIndex = 0;
        }
    }
}
