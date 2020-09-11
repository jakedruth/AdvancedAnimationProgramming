﻿/*
	Advanced Animation Programming
	By Jake Ruth

    ClipController.cs - Controls a group of clips
*/

namespace AdvAnimation
{
    /// <summary>
    /// Various options used to control the direction of time
    /// </summary>
    public enum PlaybackDirection
    {
        REVERSE = -1,
        PAUSE,
        FORWARD
    }

    public class ClipController
    {
        public string name;

        public int clipIndex;
        public float clipTime;
        public float clipParameter;

        public int keyframeIndex;
        public float keyframeTime;
        public float keyframeParameter;

        public PlaybackDirection playback = PlaybackDirection.FORWARD;
        public float playbackSpeed = 1;

        public ClipPool clipPool;

        /// <summary>
        /// Create a Clip Controller
        /// </summary>
        /// <param name="name">The name of the controller</param>
        /// <param name="pool">The pool to reference when animating</param>
        /// <param name="startingClip">The index of the first clip</param>
        public ClipController(string name, ClipPool pool, int startingClip)
        {
            this.name = name;
            clipPool = pool;
            clipIndex = startingClip;

            keyframeIndex = GetCurrentClip().firstKeyframe;
        }

        /// <summary>
        /// Create a Clip Controller
        /// </summary>
        /// <param name="name">The name of the controller</param>
        /// <param name="pool">The pool to reference when animating</param>
        /// <param name="startingClip">The name of the first clip</param>
        public ClipController(string name, ClipPool pool, string startingClip) : this(name, pool, pool.GetClip(startingClip)) { }

        /// <summary>
        /// Used to update the controller by some amount of time
        /// </summary>
        /// <param name="deltaTime">The time dilatation since the last call</param>
        public void Update(float deltaTime)
        {
            // nothing to update if the clip is paused, or if the playback speed is set to 0 (or negative)
            if (playback == PlaybackDirection.PAUSE || playbackSpeed <= 0)
                return;

            // increment both clip time and keyframe time
            float increment = deltaTime * playbackSpeed * (int)playback;
            clipTime += increment;
            keyframeTime += increment;

            // get the current clip and keyframe
            Clip currentClip = GetCurrentClip();
            Keyframe currentKeyframe = GetCurrentKeyframe();

            if (playback == PlaybackDirection.FORWARD)
            {
                // handle if the keyframe time has gone past it's duration
                if (keyframeTime >= currentKeyframe.Duration)
                {
                    keyframeTime -= currentKeyframe.Duration;
                    keyframeIndex++;

                    // if the keyframe has gone past the last keyframe
                    if (keyframeIndex > currentClip.lastKeyframe)
                    {
                        // loop back to the begging of the clip and adjust clip time
                        keyframeIndex = currentClip.firstKeyframe;
                        clipTime = keyframeTime;
                    }
                }
            }
            else if (playback == PlaybackDirection.REVERSE)
            {
                // handle if the keyframe time has gone below 0
                if (keyframeTime < 0)
                {
                    float delta = -keyframeParameter;
                    keyframeIndex--;

                    // if the keyframe has gone past the first keyframe
                    if (keyframeIndex < currentClip.firstKeyframe)
                    {
                        // loop back to the end of the clip and adjust the clip time
                        keyframeIndex = currentClip.lastKeyframe;
                        clipTime = GetCurrentClip().Duration - delta;
                    }

                    // adjust keyframe time last, so the duration can be set based on the new keyframes duration
                    keyframeTime = GetCurrentKeyframe().Duration - delta;
                }
            }

            // normalize clip time and keyframe time
            clipParameter = clipTime / GetCurrentClip().Duration;
            keyframeParameter = keyframeTime / GetCurrentKeyframe().Duration;
        }

        /// <summary>
        /// Get the current clip being controlled
        /// </summary>
        /// <returns>The current clip</returns>
        public Clip GetCurrentClip()
        {
            return clipPool.GetClip(clipIndex);
        }

        /// <summary>
        /// Get the current keyframe being controlled
        /// </summary>
        /// <returns>the current keyframe</returns>
        public Keyframe GetCurrentKeyframe()
        {
            return GetCurrentClip().GetKeyframe(keyframeIndex);
        }

        /// <summary>
        /// Set the current Clip by index
        /// </summary>
        /// <param name="i">The Clip index</param>
        public void SetCurrentClip(int i)
        {
            clipIndex = i;
            clipTime = 0;
            keyframeTime = 0;
        }

        /// <summary>
        /// Set the current clip by name
        /// </summary>
        /// <param name="clipName">The Clip name</param>
        public void SetCurrentClip(string clipName)
        {
            SetCurrentClip(clipPool.GetClip(clipName));
        }
    }
}
