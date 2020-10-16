using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvAnimation
{
    public class Lab03Interface : MonoBehaviour
    {
        public List<SpacialPose> poses;
        private int _offset;
        private ClipController[] _clipControllers;

        void Start()
        {
            InitPoseData();
        }

        public void InitPoseData()
        {
            _clipControllers = new[]
            {
                new ClipController("Pose Controller 1",
                    new ClipPool(new Clip("0 to 1", new KeyframePool(new Keyframe(0, 1, 0), new Keyframe(1, 2, 1)), 0,
                        1, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD))), 0),
                new ClipController("Pose Controller 2",
                    new ClipPool(new Clip("3, 4, 5, 6", new KeyframePool(new Keyframe(0, 1, 0), new Keyframe(1, 2, 1), new Keyframe(2, 3, 2), new Keyframe(3, 4, 3)), 0, 
                        3, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD))), 0),
            };
                _offset = 3;
            poses = new List<SpacialPose>();

            poses.Add(MathAA.Identity());                                                                   // 0
            poses.Add(MathAA.Construct(new Vector3(10, 20, 45), Vector3.one, new Vector3(1, 0.5f, 1.5f)));  // 1
            poses[1] = MathAA.Scale(poses[1], 2);
            poses.Add(poses[0].Clone());                                                                    // 2 Animates
            poses.Add(MathAA.Construct(Vector3.zero, Vector3.one, new Vector3(5, 0, 0)));                   // 3
            SpacialPose a = MathAA.Construct(new Vector3(0, 0, 90f), Vector3.one, new Vector3(3, 0, 0));    
            poses.Add(MathAA.Concat(poses[3], a));                                                          // 4
            poses.Add(MathAA.Copy(poses[4]));                                                               // 5
            poses[5].translation.y += 3;
            poses[5].orientation.z += 90f;
            poses.Add(MathAA.DeConcat(poses[5], a));                                                        // 6
            poses[6].orientation.z = 270f;
            poses.Add(MathAA.Identity());                                                                   // 7 Animates
            SpacialPose b = MathAA.Identity();
            b.translation.x = 10;
            poses.Add(MathAA.Concat(poses[3], b));                                                          // 8
            poses.Add(MathAA.Concat(poses[4], b));                                                          // 9
            poses.Add(MathAA.Concat(poses[5], b));                                                          // 10
            poses.Add(MathAA.Concat(poses[6], b));                                                          // 11
            poses.Add(MathAA.Identity());                                                                   // 12 Animates
        }

        void Update()
        {
            for (int i = 0; i < _clipControllers.Length; i++)
            {
                _clipControllers[i].Update(Time.deltaTime);
            }
            float u1 = _clipControllers[0].Evaluate(ClipController.EvaluationType.LERP);
            int uA = (int) _clipControllers[1].Evaluate();
            poses[2] = MathAA.LerpCompositeOne(poses[0], poses[1], u1);
            poses[7] = MathAA.Lerp(poses[3 + uA], poses[3 + (uA + 1) % 4], _clipControllers[1].keyframeParameter);
            poses[12] = MathAA.Cubic(poses[8 + (uA + 3) % 4], 
                                     poses[8 + (uA + 0) % 4],
                                     poses[8 + (uA + 1) % 4],
                                     poses[8 + (uA + 2) % 4],
                                     _clipControllers[1].keyframeParameter);
        }

        private void OnDrawGizmos()
        {
            if (poses == null)
                return;

            Gizmos.color = Color.green;
            for (int i = 0; i < poses.Count; i++)
            {
                SpacialPose pose = poses[i];
                if (pose == null)
                    continue;

                Vector3 pos = pose.translation;
                Gizmos.DrawWireSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, pos + Quaternion.Euler(pose.orientation) * Vector3.right * pose.scale.x);
            }
        }
    }
}
