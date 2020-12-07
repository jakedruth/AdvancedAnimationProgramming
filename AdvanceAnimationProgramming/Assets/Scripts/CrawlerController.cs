using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AdvAnimation
{
    public class CrawlerController : MonoBehaviour
    {
        public IKComponent[] ikComponents;

        private void Awake()
        {
        }

        private void Update()
        {
            for (int i = 0; i < ikComponents.Length; i++)
            {
                //if ((i < 4 && i % 2 == 0) || (i >= 4 && i % 2 == 1))
                    ikComponents[i].UpdateIK();
            }
        }
    }

}