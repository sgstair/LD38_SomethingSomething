using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    class PathFinder
    {
        GameEngine Eng;
        public PathFinder(GameEngine engine)
        {
            Eng = engine;
        }


        class Node
        {
            public Point Location;
            public int Depth;
        }

        Point[] FinishTiles;
        Node FinishNode;

        SortedSet<int> Depths = new SortedSet<int>();
        Dictionary<int, List<Node>> InvestigateNodes = new Dictionary<int, List<Node>>();
        Dictionary<Point, Point> ReversePath = new Dictionary<Point, Point>();
        HashSet<Point> CheckedPoints = new HashSet<Point>();

        void QueueNode(Point from, Point to, int depth)
        {
            Node n = new Node() { Location = to, Depth = depth };
            ReversePath.Add(to, from);

            // finish condition
            if(FinishTiles.Contains(to))
            {
                FinishNode = n;
            }

            if(!InvestigateNodes.ContainsKey(depth))
            {
                InvestigateNodes.Add(depth, new List<Node>());
            }
            if (InvestigateNodes[depth].Count == 0) Depths.Add(depth);
            InvestigateNodes[depth].Add(n);
            CheckedPoints.Add(to);
        }

        Node DequeueNextNode()
        {
            if (Depths.Count == 0) return null;
            int depth = Depths.Min;

            List<Node> nodes = InvestigateNodes[depth];
            int count = nodes.Count;
            int index = 0;
            if(count > 1)
            {
                // Pick randomly
                index = Eng.PathFinderRandom.Next(count);
            }

            Node n = nodes[index];
            nodes.RemoveAt(index);
            if (nodes.Count == 0) Depths.Remove(depth);

            return n;
        }

        void ProcessNewNode(Point location, int depth)
        {
            foreach(Point other in Eng.Map.PathableTiles(location))
            {
                if (CheckedPoints.Contains(other)) continue;
                QueueNode(location, other, depth + 1);
            }
        }


        Point TileFromLocation(Vector2 loc)
        {
            return new Point((int)Math.Floor(loc.X), (int)Math.Floor(loc.Y));
        }

        
        public Route FindRouteToNearestLocation(Vector2 startPoint, params Vector2[] locations)
        {
            CheckedPoints.Clear();
            InvestigateNodes.Clear();
            Depths.Clear();
            ReversePath.Clear();
            FinishNode = null;

            FinishTiles = locations.Select(l => TileFromLocation(l)).ToArray();
            Point start = TileFromLocation(startPoint);
            CheckedPoints.Add(start);
            if (FinishTiles.Contains(start))
            {
                foreach(Vector2 loc in locations)
                {
                    if(TileFromLocation(loc) == start) return SingleTileRoute(startPoint, loc);
                }
                
            }

            ProcessNewNode(start, 0);
            while (FinishNode == null)
            {
                Node newNode = DequeueNextNode();
                if(newNode == null)
                {
                    // No path can be found!
                    return null;
                }
                ProcessNewNode(newNode.Location, newNode.Depth);
            }

            // We have a path back from the finish node now.
            List<Point> trace = new List<Point>();
            Point p = FinishNode.Location;
            trace.Add(p);
            while(ReversePath.TryGetValue(p, out p))
            {
                trace.Add(p);
            }
            trace.Reverse();

            Vector2 finishLoc = Vector2.Zero;
            foreach (Vector2 loc in locations)
            {
                if (TileFromLocation(loc) == FinishNode.Location) { finishLoc = loc; break; }
            }

            // Generate route through tiles
            Route r = new Route(startPoint, finishLoc);
            foreach(Point routePoint in trace)
            {
                r.RoutePath.Add(new RouteTile(routePoint));
            }
            r.RoutePath[0].PathSteps.Add(startPoint);
            // Find center points between tiles and fill in the routes.
            for(int i=1;i<r.RoutePath.Count;i++)
            {
                Vector2 centerPt = (TileCenter(r.RoutePath[i - 1].TileLocation) +
                                    TileCenter(r.RoutePath[i].TileLocation)) * 0.5f;

                r.RoutePath[i - 1].PathSteps.Add(centerPt);
                r.RoutePath[i].PathSteps.Add(centerPt);
            }
            r.RoutePath.Last().PathSteps.Add(finishLoc);
            foreach(var path in r.RoutePath) { path.ComputeLength(); }
            r.ComputeLength();

            return r;
        }

        Vector2 TileCenter(Point location)
        {
            return new Vector2(location.X + 0.5f, location.Y + 0.5f);
        }


        Route SingleTileRoute(Vector2 startPoint, Vector2 endPoint)
        {
            RouteTile t = new RouteTile(TileFromLocation(startPoint));
            t.PathSteps.Add(startPoint);
            t.PathSteps.Add(endPoint);
            t.ComputeLength();

            Route r = new Route(startPoint, endPoint);
            r.RoutePath.Add(t);
            r.ComputeLength();
            return r;
        }
    }

    class Route
    {
        public Route(Vector2 start, Vector2 end)
        {
            CurrentPosition = start;
            TargetPosition = end;
        }
        public Vector2 CurrentPosition;
        public Vector2 TargetPosition;
        public float PathLength; // Future, go all integer to reduce uncertainty & rounding errors.
        public List<RouteTile> RoutePath = new List<RouteTile>();

        public void ComputeLength()
        {
            PathLength = RoutePath.Sum(t => t.Length);
        }

        public void Advance(float distance)
        {
            while(distance > 0)
            {
                if(RoutePath.Count == 0)
                {
                    CurrentPosition = TargetPosition;
                    PathLength = 0;
                    return;
                }
                if(RoutePath[0].Length < distance)
                {
                    distance -= RoutePath[0].Length;
                    RoutePath.RemoveAt(0);
                }
                else
                {
                    RoutePath[0].Advance(distance);
                    CurrentPosition = RoutePath[0].PathSteps[0];
                    RoutePath[0].ComputeLength();
                    ComputeLength();
                    return;
                }
            }
        }

        public override string ToString()
        {
            return $"Route({CurrentPosition} => {TargetPosition} Length {PathLength})";
        }
    }

    class RouteTile
    {
        public RouteTile(Point location) { TileLocation = location; }
        public Point TileLocation;
        public float Length;
        public List<Vector2> PathSteps = new List<Vector2>();
        public void ComputeLength()
        {
            Length = 0;
            for(int i=1;i<PathSteps.Count; i++)
            {
                Length += (PathSteps[i - 1] - PathSteps[i]).Length();
            }
        }

        public void Advance(float length)
        {
            while(length > 0)
            {
                if (PathSteps.Count < 2) return;
                float segmentLength = (PathSteps[0] - PathSteps[1]).Length();
                if(length >= segmentLength)
                {
                    length -= segmentLength;
                    PathSteps.RemoveAt(0);
                }
                else
                {
                    // Advance this path based on the length
                    float movepercent = length / segmentLength;
                    PathSteps[0] = PathSteps[0] * (1 - movepercent) + PathSteps[1] * movepercent;
                    return;
                }
            }
        }
    }

}
