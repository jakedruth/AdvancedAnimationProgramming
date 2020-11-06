/*
	Advanced Animation Programming
	By Jake Ruth

    Transition.cs - Holds info for a transition to another clip
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AdvAnimation
{
    public enum TransitionType
    {
        PAUSE,                  // Pause on current clip
        FORWARD,                // Play from the start of the first frame of the supplied clip
        FORWARD_PAUSE,          // Pause at start of the first frame of the supplied clip
        BACKWARD,               // Rewind from end of the last frame of the supplied clip
        BACKWARD_PAUSE,         // Pause at end of the last frame of the supplied clip
        FORWARD_SKIP,           // Play from end of the first frame of the supplied clip
        FORWARD_SKIP_PAUSE,     // Pause at the end of the first frame of supplied clip
        BACKWARD_SKIP,          // Rewind from the start of the last frame of the supplied clip
        BACKWARD_SKIP_PAUSE,    // pause at the start of the last frame of the supplied clip
    }

    public class Transition
    {
        public TransitionType transitionType;
        public string transitionClipName;

        private ClipController _controller;
        private readonly List<(string parameterName, string nextClip, Func<object, bool>)> _conditions;

        public Transition(TransitionType type, string clip = null)
        {
            transitionType = type;
            transitionClipName = clip;
            _controller = null;
            _conditions = new List<(string parameterName, string nextClip, Func<object, bool>)>();
        }

        public void SetController(ClipController controller)
        {
            _controller = controller;
        }

        public void AddCondition(string parameterName, string nextClip, Func<object, bool> function)
        {
            if (_controller == null)
                throw new Exception("Can not add transitional Conditions if the controller is null");
            _conditions.Add((parameterName, nextClip, function));
        }

        public void HandleTransition(ClipPool pool, ref int clipIndex, ref int keyframeIndex, 
            ref float keyframeTime, ref PlaybackDirection playbackDirection)
        {
            Clip nextClip = GetNextClip(pool);

            // If the next transitional clip is null, set it to the current clip
            if (nextClip == null)
                nextClip = pool[clipIndex];

            // Used to store how much over a duration or under the start of a keyframe
            float deltaOutOfBounds = Math.Abs(keyframeTime);

            // TODO: separate all the pauses from there non-pause counterparts

            switch (transitionType)
            {
                case TransitionType.PAUSE:
                    switch (playbackDirection)
                    {
                        case PlaybackDirection.FORWARD:
                            keyframeIndex = nextClip.lastKeyframe;
                            keyframeTime = nextClip[keyframeIndex].Duration;
                            break;
                        case PlaybackDirection.REVERSE:
                            keyframeIndex = nextClip.firstKeyframe;
                            keyframeTime = 0;
                            break;
                        case PlaybackDirection.PAUSE:
                            throw new Exception("A paused clip shouldn't be able to transition...");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(playbackDirection), playbackDirection, null);
                    }
                    playbackDirection = PlaybackDirection.PAUSE;
                    break;
                case TransitionType.FORWARD:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe;
                    keyframeTime = deltaOutOfBounds;
                    playbackDirection = PlaybackDirection.FORWARD;
                    break;
                case TransitionType.FORWARD_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe;
                    keyframeTime = 0;
                    playbackDirection = PlaybackDirection.PAUSE;
                    break;
                case TransitionType.BACKWARD:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe;
                    keyframeTime = nextClip[keyframeIndex].Duration - deltaOutOfBounds;
                    playbackDirection = PlaybackDirection.REVERSE;
                    break;
                case TransitionType.BACKWARD_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe;
                    keyframeTime = nextClip[keyframeIndex].Duration;
                    playbackDirection = PlaybackDirection.PAUSE;
                    break;
                case TransitionType.FORWARD_SKIP:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe + 1;
                    keyframeTime = deltaOutOfBounds;
                    playbackDirection = PlaybackDirection.FORWARD;
                    break;
                case TransitionType.FORWARD_SKIP_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe + 1;
                    keyframeTime = 0;
                    playbackDirection = PlaybackDirection.PAUSE;
                    break;
                case TransitionType.BACKWARD_SKIP:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe - 1;
                    keyframeTime = nextClip[keyframeIndex].Duration - deltaOutOfBounds;
                    playbackDirection = PlaybackDirection.REVERSE;
                    break;
                case TransitionType.BACKWARD_SKIP_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe - 1;
                    keyframeTime = nextClip[keyframeIndex].Duration;
                    playbackDirection = PlaybackDirection.REVERSE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected Clip GetNextClip(ClipPool pool)
        {
            // Check conditions to see if conditional transition occured
            for (int i = 0; i < _conditions.Count; i++)
            {
                (string parameterName, string nextClip, Func<object, bool> func) condition = _conditions[i];

                object obj = _controller.transitionParameters[condition.parameterName];

                if (condition.func(obj))
                {
                    return FindClip(pool, condition.nextClip);
                }
            }

            return FindClip(pool, transitionClipName);
        }

        protected Clip FindClip(ClipPool pool, string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
                return null;

            Clip nextClip = null;
            int index = pool.GetClipIndexByName(clipName);
            if (index >= 0)
            {
                nextClip = pool[index];
            }

            return nextClip;
        }
    }

    public class Trigger
    {
        private bool _value;

        public Trigger(bool valueIn = false)
        {
            _value = valueIn;
        }

        public static implicit operator Trigger(bool other)
        {
            return new Trigger(other);
        }

        public bool Get()
        {
            if (!_value) 
                return false;
            
            _value = false;
            return true;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
