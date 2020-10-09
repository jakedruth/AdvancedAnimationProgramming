using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace AdvAnimation
{
    public class HTRParser
    {
        public class MyNode : HierarchyNode
        {
            public SpacialPose spacialPose;

            public MyNode(string name, int index, int parentIndex) : base(name, index, parentIndex)
            {
                spacialPose = new SpacialPose();
            }
        }

        public string fullPath;
        public string fileType = "htr";
        public string dataType = "HTRS";
        public int fileVersion = 1;
        public int numSegments = -1;
        public int numFrames = -1;
        public float dataFrameRate = -1;
        public string eulerRotationOrder = "ZYX";
        public string calibrationUnits;
        public string rotationUnits = "Degrees";
        public string globalAxisOfGravity = "Y";
        public string bonLengthAxis = "Y";
        public float scaleFactor = 1f;

        public HTRParser(string relativePath)
        {
            fullPath = $"{Application.dataPath}/Resources/{relativePath}";
        }

        public void ParseFile(out MyNode[] nodePool)
        {
            nodePool = new MyNode[0];
            StreamReader sr = new StreamReader(fullPath);
            string currentSection = "";
            int index = 0;

            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine();
                if (line == null)
                    continue;

                // remove any comments from the line
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                {
                    //Debug.Log("comment found on line: " + line);
                    line = line.Substring(0, commentIndex);
                    //Debug.Log("Line after comment removal: " + line);
                }

                // Stop if the entire line was a comment
                if (string.IsNullOrEmpty(line))
                    continue;

                // Trim line end
                line = line.TrimEnd('\t');

                if (line[0] == '[') // Switch Section!
                {
                    currentSection = line.Substring(1, line.Length - 2);
                }
                else
                {
                    List<string> args = line.Split(null).ToList();
                    for (int i = args.Count - 1; i >= 0; i--)
                    {
                        if (string.IsNullOrWhiteSpace(args[i]))
                        {
                            args.RemoveAt(i);
                        }
                    }

                    switch (currentSection)
                    {
                        case "Header":
                        {
                            switch (args[0])
                            {
                                case "FileType":
                                    fileType = args[1];
                                    break;
                                case "DataType":
                                    dataType = args[1];
                                    break;
                                case "FileVersion":
                                    fileVersion = int.Parse(args[1]);
                                    break;
                                case "NumSegments":
                                    numSegments = int.Parse(args[1]);
                                    nodePool = new MyNode[numSegments];
                                    break;
                                case "NumFrames":
                                    numFrames = int.Parse(args[1]);
                                    break;
                                case "DataFrameRate":
                                    dataFrameRate = int.Parse(args[1]);
                                    break;
                                case "EulerRotationOrder":
                                    eulerRotationOrder = args[1];
                                    break;
                                case "CalibrationUnits":
                                    calibrationUnits = args[1];
                                    break;
                                case "RotationUnits":
                                    rotationUnits = "Degrees";
                                    break;
                                case "GlobalAxisofGravity":
                                    globalAxisOfGravity = args[1];
                                    break;
                                case "BoneLengthAxis":
                                    bonLengthAxis = args[1];
                                    break;
                                case "ScaleFactor":
                                    scaleFactor = float.Parse(args[1]);
                                    break;
                                default:
                                    Debug.LogWarning($"Warning! Could not parse parameter: {args[0]}");
                                    break;
                            }
                            break;
                        }

                        case "SegmentNames&Hierarchy":
                        {
                            int parentIndex = -1;
                            if (args[1] != "GLOBAL")
                            {
                                for (int i = 0; i < nodePool.Length; i++)
                                {
                                    MyNode n = nodePool[i];
                                    if (n.name != args[1]) 
                                        continue;
                                    
                                    parentIndex = n.index;
                                    break;
                                }
                            }
                            nodePool[index] = new MyNode(args[0], index, parentIndex);
                            index++;
                            break;
                        }

                        case "BasePosition":
                        {
                            MyNode node = nodePool.First(n => n.name == args[0]);

                            node.spacialPose.translation = new Vector3(
                                float.Parse(args[1]),
                                float.Parse(args[2]),
                                float.Parse(args[3]));

                            if (calibrationUnits == "mm")
                                node.spacialPose.translation *= 0.001f;

                            node.spacialPose.orientation = new Vector3(
                                float.Parse(args[4]),
                                float.Parse(args[5]),
                                float.Parse(args[6]));

                            node.spacialPose.scale = Vector3.up * float.Parse(args[7]);

                            break;
                        }
                        default: // Segment Name frame animation
                            break;
                    }
                }
            }
        }
    }
}