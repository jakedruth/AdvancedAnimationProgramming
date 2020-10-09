using System.Collections;
using System.Collections.Generic;
using AdvAnimation;
using UnityEngine;

public class CreatePoseData : MonoBehaviour
{
    public string filePath;
    public HTRParser.MyNode[] nodes;
    public HTRParser.MyNode[] worldSpaceNodes;

    void Start()
    {
        HTRParser parser =  new HTRParser(filePath);
        parser.ParseFile(out nodes);

        if (nodes != null) 
            worldSpaceNodes = nodes.Clone() as HTRParser.MyNode[];

        if (worldSpaceNodes != null)
            for (int i = 0; i < worldSpaceNodes.Length; i++)
            {
                HTRParser.MyNode current = worldSpaceNodes[i];
                HTRParser.MyNode parent = current.parentIndex != -1 ? worldSpaceNodes[current.parentIndex] : null;

                if (parent != null)
                {
                    current.spacialPose.translation = parent.spacialPose.translation +
                                                      Quaternion.Euler(parent.spacialPose.orientation) * current.spacialPose.translation;//Quaternion.Euler(parent.spacialPose.orientation) * parent.spacialPose.translation; //+ Quaternion.Euler(parent.spacialPose.orientation) * Vector3.up * parent.spacialPose.scale.y;
                    current.spacialPose.orientation += parent.spacialPose.orientation;
                }

                GameObject go = new GameObject(current.name);
                go.transform.position = current.spacialPose.translation;
                go.transform.rotation = Quaternion.Euler(current.spacialPose.orientation);
                go.transform.SetParent(transform);
            }
    }

    private void OnDrawGizmos()
    {
        //if (worldSpaceNodes != null && worldSpaceNodes.Length > 0)
        //{
            
        //    for (int i = 0; i < worldSpaceNodes.Length; i++)
        //    {
        //        Gizmos.color = Color.Lerp(Color.green, Color.red, (float)i / worldSpaceNodes.Length);
        //        Gizmos.DrawCube(worldSpaceNodes[i].spacialPose.translation, Vector3.one * 0.1f);
        //    }

        //}

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.red, (float)i / transform.childCount);
                Gizmos.DrawCube(transform.GetChild(i).position, Vector3.one * 0.05f);
            }
        }
    }


}
