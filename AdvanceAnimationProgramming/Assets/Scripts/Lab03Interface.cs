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

        [Header("Bi-Lerp Values")]
        [Range(0, 1)] public float lerpU0;
        [Range(0, 1)] public float lerpU1;
        [Range(0, 1)] public float lerpU;

        [Header("Bi-Cubic Values")] 
        [Range(0, 1)] public float cubicU0; 
        [Range(0, 1)] public float cubicU1; 
        [Range(0, 1)] public float cubicU2;
        [Range(0, 1)] public float cubicU3;
        [Range(0, 1)] public float cubicU4; 
        [Range(0, 1)] public float cubicU;

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
            poses.Add(MathAA.Concat(poses[8], b));                                                          // 13
            poses.Add(MathAA.Copy(poses[13]));                                                              // 14
            poses[14].translation.x += 3;
            poses[14].translation.y += 1;
            poses.Add(MathAA.Copy(poses[14]));                                                              // 15
            poses[15].translation.x -= 3;
            poses[15].translation.y += 0.5f;
            poses[15].translation.z += 3;
            poses.Add(MathAA.Copy(poses[15]));                                                              // 16
            poses[16].translation.x += 3;
            poses[16].translation.y += 2.5f;
            poses.Add(MathAA.Identity());                                                                   // 17 Animates Manually
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 18 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 19 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 20 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 21 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 22 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 23 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 24 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 25 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 26 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 27 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 28 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 29 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 30 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 31 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 32 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 33 
            poses.Add(MathAA.Concat(poses[13], b));                                                         // 34 Animates Manually 
            poses[18].translation += new Vector3(0, 2, 0);
            poses[19].translation += new Vector3(2, 1.5f, 0);
            poses[20].translation += new Vector3(4, 1, 0);
            poses[21].translation += new Vector3(6, 2, 0);
            poses[22].translation += new Vector3(0, 4, 2);
            poses[23].translation += new Vector3(2, 0.5f, 2);
            poses[24].translation += new Vector3(4, 2, 2);
            poses[25].translation += new Vector3(6, 3, 2);
            poses[26].translation += new Vector3(0, 3.5f, 4);
            poses[27].translation += new Vector3(2, 2, 4);
            poses[28].translation += new Vector3(4, 4, 4);
            poses[29].translation += new Vector3(6, 1, 4);
            poses[30].translation += new Vector3(0, 1, 6);
            poses[31].translation += new Vector3(2, 2, 6);
            poses[32].translation += new Vector3(4, 4, 6);
            poses[33].translation += new Vector3(6, 3.5f, 6);

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
            poses[17] = MathAA.BiLerp(poses[13], poses[14], poses[15], poses[16], lerpU0, lerpU1, lerpU);
            poses[34] = MathAA.BiCubic(
                poses[18], poses[19], poses[20], poses[21],
                poses[22], poses[23], poses[24], poses[25],
                poses[26], poses[27], poses[28], poses[29],
                poses[30], poses[31], poses[32], poses[33],
                cubicU0, cubicU1, cubicU2, cubicU3, cubicU);
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

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(poses[13].translation, poses[14].translation);
            Gizmos.DrawLine(poses[15].translation, poses[16].translation);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                Vector3.Lerp(poses[13].translation, poses[14].translation, lerpU0),
                Vector3.Lerp(poses[15].translation, poses[16].translation, lerpU1));


            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Gizmos.DrawLine(poses[18 + i * 4 + j].translation, poses[18 + i * 4 + j + 1].translation);
                }    
            }

        }
    }
}
