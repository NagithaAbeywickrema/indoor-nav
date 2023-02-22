namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Vertice
    {
        public List<string> nodes = new List<string>();

        public Vertice(string node1, string node2)
        {
            nodes.Add(node1);
            nodes.Add(node2);
        }
    }
}
