namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Waypoint
    {
        public string id;
        public string name;
        public string type;
        public GameObject obj;
        public List<Waypoint> adjacent = new List<Waypoint>();

        public Waypoint(string id, string name, string type)
        {
            this.id = id;
            this.name = name;
            this.type = type;
        }
    }
}