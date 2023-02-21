namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class WaypointManager : MonoBehaviour
    {
        public PersistentCloudAnchorsController Controller;
        public GameObject ArrowPrefab;
        public Transform cameraTransform;
        public List<Vertice> vertices;
        public List<Waypoint> waypoints;
        public List<Waypoint> path;
        public Waypoint destination;

        private Waypoint _current;
        private bool _path_found = false;
        private bool _path_marked = false;
        private List<GameObject> _pointers = new List<GameObject>();


        public void OnEnable()
        {
            _path_found = false;
            _path_marked = false;
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
                    List<Waypoint> path = new List<Waypoint>();
                    while (current != null)
                    {
                        path.Add(current);
                        current = previous[current];
                    }
                    path.Reverse();
                    return path;
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
            //TODO: Load waypoints, vertices from firebase instead
            CloudAnchorHistoryCollection cloud_anchors_history = Controller.LoadCloudAnchorHistory();
            PairHistoryCollection pairs_history = Controller.LoadPairHistory();

            foreach (var data in cloud_anchors_history.Collection)
            {
                waypoints.Add(new Waypoint(data.Id, data.Name, data.Type));
            }

            foreach (var data in pairs_history.Collection)
            {
                vertices.Add(new Vertice(data.Id1, data.Id2));
            }

            //TODO: make graph
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
                }
            }
        }
    }
}
