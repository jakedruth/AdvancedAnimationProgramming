/*
	Advanced Animation Programming
	By Jake Ruth

    ClipController.cs - Controls a group of clips
*/

using System;
using System.Collections.Generic;
using UnityEngine;

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
        public ClipPool clipPool;

        public int clipIndex;
        public float clipTime;
        public float clipParameter;

        public int keyframeIndex;
        public float keyframeTime;
        public float keyframeParameter;

        public PlaybackDirection playback;
        public float playbackSpeed;

        public Dictionary<string, object> transitionParameters;

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
            clipTime = 0;
            clipParameter = 0;
            
            keyframeIndex = clipPool[clipIndex].firstKeyframe;
            keyframeTime = 0;
            keyframeParameter = 0;

            playback = PlaybackDirection.FORWARD;
            playbackSpeed = 1;

            transitionParameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create a Clip Controller
        /// </summary>
        /// <param name="name">The name of the controller</param>
        /// <param name="pool">The pool to reference when animating</param>
        /// <param name="startingClip">The name of the first clip</param>
        public ClipController(string name, ClipPool pool, string startingClip) : this(name, pool, pool.GetClipIndexByName(startingClip)) { }

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
            keyframeTime += increment;

            if (playback == PlaybackDirection.FORWARD)
            {
                // handle if the keyframe has gone past it's duration
                if (keyframeTime >= clipPool[clipIndex][keyframeIndex].Duration)
                {
                    keyframeTime -= clipPool[clipIndex][keyframeIndex].Duration;

                    // handle a transition if the last keyframe is active
                    if (keyframeIndex >=clipPool[clipIndex].lastKeyframe)
                    {
                        clipPool[clipIndex].forwardTransition.HandleTransition(clipPool,
                            ref clipIndex, ref keyframeIndex, ref keyframeTime, ref playback);
                    }
                    else
                    {
                        keyframeIndex++;
                    }
                }
            }
            else if (playback == PlaybackDirection.REVERSE)
            {
                // handle if the keyframe has gone below 0s
                if (keyframeTime < 0)
                {
                    // handle a transition if the first keyframe is active
                    if (keyframeIndex <= clipPool[clipIndex].firstKeyframe)
                    {
                        clipPool[clipIndex].backwardTransition.HandleTransition(clipPool,
                            ref clipIndex, ref keyframeIndex, ref keyframeTime, ref playback);
                    }
                    else
                    {
                        keyframeIndex--;
                        keyframeTime = clipPool[clipIndex][keyframeIndex].Duration - keyframeTime;
                    }
                }
            }

            // calculate the parameters
            keyframeParameter = keyframeTime / clipPool[clipIndex][keyframeIndex].Duration;
            clipTime = clipPool[clipIndex][keyframeIndex].time                       // The start time of the current keyframe
                       - clipPool[clipIndex][clipPool[clipIndex].firstKeyframe].time // The start time of the first key frame
                       + keyframeTime;                                               // The current keyframe time
            clipParameter = clipTime / clipPool[clipIndex].Duration;
        }

        /// <summary>
        /// Get the current clip being controlled
        /// </summary>
        /// <returns>The current clip</returns>
        public Clip GetCurrentClip()
        {
            return clipPool[clipIndex];
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
            keyframeIndex = clipPool[clipIndex].firstKeyframe;
            keyframeTime = 0;
        }

        /// <summary>
        /// Set the current clip by name
        /// </summary>
        /// <param name="clipName">The Clip name</param>
        public void SetCurrentClip(string clipName)
        {
            SetCurrentClip(clipPool.GetClipIndexByName(clipName));
        }

        public void GoToNextClip()
        {
            int index = clipIndex + 1;
            if (index > clipPool.Count - 1)
                index = 0;

            SetCurrentClip(index);
        }

        public void GoToPrevClip()
        {
            int index = clipIndex - 1;
            if (index < 0)
                index = clipPool.Count - 1;

            SetCurrentClip(index);
        }

        public void SetTransitionParameterValue(string parameterName, object value)
        {
            if (transitionParameters.ContainsKey(parameterName))
                transitionParameters[parameterName] = value;
            else
                transitionParameters.Add(parameterName, value);
        }

        public T GetTransitionParameterValue<T>(string parameterName)
        {
            return (T)transitionParameters[parameterName];
        }

        public object GetTransitionParameterValue(string parameterName)
        {
            return transitionParameters[parameterName];
        }

        public enum EvaluationType
        {
            STEP,
            NEAREST,
            LERP,
            CATMULL_ROM,
            CUBIC
        }

        public float Evaluate(EvaluationType evaluationType = EvaluationType.STEP)
        {
            //TODO: Fix once transitions are implemented
            //      Getting the next and previous keyframe value will change if
            //      there is a transition to another clip.... OR the start of 
            //      this clip............

            switch (evaluationType)
            {
                case EvaluationType.CUBIC:       // Not implemented yet
                case EvaluationType.STEP:
                    return clipPool[clipIndex][keyframeIndex].value;
                case EvaluationType.NEAREST:
                {
                    int k0 = keyframeIndex;
                    int k1 = keyframeIndex + 1;
                    if (k1 > clipPool[clipIndex].lastKeyframe)
                        k1 = clipPool[clipIndex].firstKeyframe;

                    int k = keyframeParameter < 0.5f ? k0 : k1;

                    return clipPool[clipIndex][k].value;
                }
                case EvaluationType.LERP:
                {
                    int k0 = keyframeIndex;
                    int k1 = keyframeIndex + 1;
                    if (k1 > clipPool[clipIndex].lastKeyframe)
                        k1 = clipPool[clipIndex].firstKeyframe;

                    float v0 = clipPool[clipIndex][k0].value;
                    float v1 = clipPool[clipIndex][k1].value;

                    return (v0 + (v1 - v0) * keyframeParameter);
                }
                case EvaluationType.CATMULL_ROM:
                {
                    int k0 = keyframeIndex;
                    int k1 = keyframeIndex + 1;
                    
                    if (k1 > clipPool[clipIndex].lastKeyframe)
                        k1 = clipPool[clipIndex].firstKeyframe;
                    
                    int kP = k0 - 1;
                    int kN = k1 + 1;
                    
                    if (kP < 0)
                        kP = clipPool[clipIndex].lastKeyframe;

                    if (kN > clipPool[clipIndex].lastKeyframe)
                        kN = clipPool[clipIndex].firstKeyframe;

                    float u = keyframeParameter;
                    float uu = u * u;
                    float uuu = uu * u;

                    float qP = -uuu + 2 * uu - u;
                    float q0 = +3 * uuu - 5 * uu + 2;
                    float q1 = -3 * uuu + 4 * uu + u;
                    float qN = uuu - uu;

                    float vP = clipPool[clipIndex][kP].value;
                    float v0 = clipPool[clipIndex][k0].value;
                    float v1 = clipPool[clipIndex][k1].value;
                    float vN = clipPool[clipIndex][kN].value;

                    return (vP * qP + 
                           v0 * q0 + 
                           v1 * q1 + 
                           vN * qN) * 0.5f;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(evaluationType), evaluationType, null);
            }
        }

        public static float BlendControllerEvaluations(ClipController a, ClipController b,
            EvaluationType evaluationType, float t)
        {
            return Mathf.Lerp(a.Evaluate(evaluationType), b.Evaluate(evaluationType), t);
        }
    }
}
