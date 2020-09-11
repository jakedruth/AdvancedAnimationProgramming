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
        void Start()
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
            // All hard coded values in rects are used as they were large enough to display the respective data (i.e. text and buttons)

            const int margin = 10;
            const int padding = 5;
            const int rowHeight = 20;

            int width = 400;
            int height = 300;
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
            if (GUI.Button(new Rect(160, margin + 1, 100, 20), hideOrShowText, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft}))
            {
                displayGUIControls = !displayGUIControls;
            }

            // return if the displays are not being shown
            if (!displayGUIControls)
                return;

            // Create the rects to hold the controller information
            Rect previousControllerButton = new Rect(margin + padding, margin + padding + 20, 20, rowHeight);
            Rect nextControllerButton = new Rect(previousControllerButton.xMax + 5, previousControllerButton.y, previousControllerButton.width, rowHeight);
            Rect currentControllerLabel = new Rect(nextControllerButton.xMax + 5, nextControllerButton.y, 240, rowHeight);

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
            Rect playbackMultiplierSlider = new Rect(playbackMultiplierLabel.xMax + padding, playbackMultiplierLabel.y + 5, width - playbackMultiplierLabel.width - padding * 3, rowHeight);

            // Display a label and slider to control the playback speed multiplier from 0 to 2
            GUI.Label(playbackMultiplierLabel, $"Playback speed multiplier: {current.playbackSpeed:F1}");
            current.playbackSpeed = GUI.HorizontalSlider(playbackMultiplierSlider, current.playbackSpeed, 0, 2); // a scale of 0 to 2 keeps normal speed in the middle of the slider
            current.playbackSpeed = Mathf.Round(current.playbackSpeed * 10) / 10; // round playback speed to closest tenth

            // Create the rects to hold clip controls
            Rect previousClipButton = new Rect(margin + padding, playbackMultiplierSlider.yMax + padding, 20, rowHeight);
            Rect nextClipButton = new Rect(previousClipButton.xMax + 5, previousClipButton.y, previousClipButton.width, rowHeight);
            Rect currentClipLabel = new Rect(nextClipButton.xMax + 5, nextClipButton.y, 240, rowHeight);

            // If pressed, cycle through the available clips
            if (GUI.Button(previousClipButton, "<"))
            {
                int index = current.clipIndex - 1;
                if (index < 0)
                    index = current.clipPool.Count - 1;

                current.SetCurrentClip(index);
            }
            if (GUI.Button(nextClipButton, ">"))
            {
                int index = current.clipIndex + 1;
                if (index > current.clipPool.Count - 1)
                    index = 0;

                current.SetCurrentClip(index);
            }
            GUI.Label(currentClipLabel, $"Change Current Clip");

            // Display all data about the current clip
            Rect clipTimeLabel = new Rect(margin + padding, currentClipLabel.yMax + padding, 500, rowHeight * 4);
            GUI.Label(clipTimeLabel, $"Current Clip:\t[{current.clipIndex}]: '{current.GetCurrentClip().name}'\n" +
                                     $"Clip Duration:\t{current.GetCurrentClip().Duration:00.000}\n" +
                                     $"Clip Time:\t\t{current.clipTime:00.000}\n" +
                                     $"Clip Parameter:\t{current.clipParameter:0.000}");

            // Display all data about the current keyframe
            Rect keyframeTimeLabel = new Rect(margin + padding, clipTimeLabel.yMax + padding, 500, rowHeight * 4);
            GUI.Label(keyframeTimeLabel, $"Current keyframe:\t[{current.GetCurrentKeyframe().index:00.}]\n" +
                                         $"Data:\t\t{current.GetCurrentKeyframe().data}\n" +
                                         $"Keyframe Duration:\t{current.GetCurrentKeyframe().Duration:0.000}\n" +
                                         $"Keyframe Time:\t{current.keyframeTime:0.000}\n" +
                                         $"Keyframe Parameter:\t{current.keyframeParameter:0.000}");
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
        /// Used to create random data for testing
        /// </summary>
        private void GenerateTestingData()
        {
            // Create 26 keyframes with a random duration of 0.5, 0.75, 1.0, 1.25, & 1.50
            // also generate a random float to be the data stored
            Keyframe frameA = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameB = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameC = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameD = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameE = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameF = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameG = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameH = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameI = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameJ = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameK = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameL = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameM = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameN = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameO = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameP = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameQ = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameR = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameS = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameT = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameU = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameV = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameW = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameY = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameX = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));
            Keyframe frameZ = new Keyframe(0.5f + Random.Range(0, 5) * 0.25f, Random.Range(-10f, 10f));

            // Create a keyframe pool to hold all of the frames
            KeyframePool poolFloat = new KeyframePool(frameA, frameB, frameC, frameD, frameE, frameF,
                frameG, frameH, frameI, frameJ, frameK, frameL, frameM, frameN, frameO, frameP,
                frameQ, frameR, frameS, frameT, frameU, frameV, frameW, frameX, frameY, frameZ);

            // generate 5 clips that have some overlapping keyframes
            Clip clipA = new Clip("Idle",   poolFloat, 0,  13);
            Clip clipB = new Clip("Walk",   poolFloat, 14, 25);
            Clip clipC = new Clip("Run",    poolFloat, 0,  25);
            Clip clipD = new Clip("Jump",   poolFloat, 10, 20);
            Clip clipE = new Clip("Crouch", poolFloat, 0,  1);

            // create multiple pools for multiple controllers
            ClipPool poolA = new ClipPool(clipA, clipB, clipC);
            ClipPool poolB = new ClipPool(clipB, clipC, clipD);
            ClipPool poolC = new ClipPool(clipC, clipD, clipE);

            controllers = new[]
            {
                new ClipController("Controller A", poolA, 0),       // Can set the starting clip by index,
                new ClipController("Controller B", poolB, "Run"),   // Or by name
                new ClipController("Controller C", poolC, 1),
            };

            // set the current controller to the first one, Controller A
            _currentClipControllerIndex = 0;
        }
    }
}
