namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class WaypointManager : MonoBehaviour
    {
        public Text DebugText;

        public PersistentCloudAnchorsController Controller;
        public GameObject ArrowPrefab;
        public GameObject DestPrefab;
        public Transform cameraTransform;
        public List<Vertice> vertices = new List<Vertice>();
        public List<Waypoint> waypoints = new List<Waypoint>();
        public List<Waypoint> path = new List<Waypoint>();
        public Waypoint destination;

        private Waypoint _current;
        private bool _path_found = false;
        private bool _path_marked = false;
        private List<GameObject> _pointers = new List<GameObject>();
        private GameObject _destObj;


        public void OnEnable()
        {
            _path_found = false;
            _path_marked = false;
            if (_destObj != null)
            {
                Destroy(_destObj);
            }
        }

        public void OnDisable()
        {
            vertices.Clear();
            waypoints.Clear();
            path.Clear();
            destination = null;
            foreach (GameObject o in _pointers)
            {
                Destroy(o);
            }
            _pointers.Clear();
            if (_destObj != null)
            {
                Destroy(_destObj);
            }
        }

        public void Update()
        {
            // Find closest anchor
            float min_dist = float.PositiveInfinity;
            foreach (Waypoint point in waypoints)
            {
                if (point.obj != null)
                {
                    float dist = Vector3.Distance(cameraTransform.position, point.obj.transform.position);
                    if (dist < min_dist)
                    {
                        _current = point;
                        _path_found = false;
                        _path_marked = false;
                        if (_current.id == destination.id)
                        {
                            _destObj = Instantiate(DestPrefab, point.obj.transform);
                        }
                    }
                }
            }

            // Find shortest path
            if (!_path_found && _current != null)
            {
                path = FindPath(_current, destination);
                _path_found = true;
            }

            // Show path
            if (!_path_marked)
            {
                foreach (GameObject o in _pointers)
                {
                    Destroy(o);
                }
                _pointers.Clear();

                for (int i = 1; i < path.Count; i++)
                {
                    if (path[i - 1].obj != null && path[i].obj != null)
                    {
                        GameObject go = Instantiate(ArrowPrefab, path[i - 1].obj.transform.position, Quaternion.LookRotation(path[i].obj.transform.position - path[i - 1].obj.transform.position));
                        _pointers.Add(go);
                    }
                }
                _path_marked = true;
            }
        }

        private List<Waypoint> FindPath(Waypoint start, Waypoint destination)
        {
            Queue<Waypoint> queue = new Queue<Waypoint>();
            queue.Enqueue(start);


            Dictionary<Waypoint, Waypoint> previous = new Dictionary<Waypoint, Waypoint>();
            previous[start] = null;

            while (queue.Count > 0)
            {
                Waypoint current = queue.Dequeue();
                if (current == destination)
                {
                    // Build the path
                    List<Waypoint> path1 = new List<Waypoint>();
                    while (current != null)
                    {
                        path1.Add(current);
                        current = previous[current];
                    }
                    path1.Reverse();
                    return path1;
                }

                foreach (Waypoint next in current.adjacent)
                {
                    if (!previous.ContainsKey(next))
                    {
                        queue.Enqueue(next);
                        previous[next] = current;
                    }
                }
            }

            return null;
        }

        public void MakeGraph()
        {
            CloudAnchorHistoryCollection cloud_anchors_history = Controller.LoadCloudAnchorHistory();
            PairHistoryCollection pairs_history = Controller.LoadPairHistory();

            waypoints = new List<Waypoint>();
            foreach (var data in cloud_anchors_history.Collection)
            {
                waypoints.Add(new Waypoint(data.Id, data.Name, data.Type));
            }

            vertices = new List<Vertice>();
            foreach (var data in pairs_history.Collection)
            {
                Vertice vert = new Vertice(data.Id1, data.Id2);
                this.vertices.Add(vert);
            }

            foreach (Vertice vertice in vertices)
            {
                Waypoint point1 = null;
                Waypoint point2 = null;
                foreach (Waypoint point in waypoints)
                {
                    if (point.id == vertice.nodes[0])
                    {
                        point1 = point;
                    }
                    if (point.id == vertice.nodes[1])
                    {
                        point2 = point;
                    }
                }
                point1.adjacent.Add(point2);
                point2.adjacent.Add(point1);
            }

        }

        public List<Waypoint> GetDestinations()
        {
            List<Waypoint> destinations = new List<Waypoint>();
            foreach (Waypoint point in waypoints)
            {
                if (point.type == "destination")
                {
                    destinations.Add(point);
                }
            }
            return destinations;
        }

        internal void SetAnchorObject(string anchor_id, GameObject anchor_obj)
        {
            foreach (Waypoint point in waypoints)
            {
                if (point.id == anchor_id)
                {
                    point.obj = anchor_obj;
                    _path_marked = false;
                }
            }
        }
    }
}
