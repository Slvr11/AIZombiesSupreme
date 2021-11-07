using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;

namespace AIZombiesSupreme
{
    class pathfinding : BaseScript
    {
        private static List<pathNode> _nodes = new List<pathNode>();

        public class pathNode
        {
            public List<pathNode> visibleNodes = new List<pathNode>();
            public Vector3 location;

            public pathNode(Vector3 point)
            {
                location = point;
                _nodes.Add(this);
            }

            public pathNode getClosestPathNodeToEndNode(pathNode node)
            {
                float dis = 999999999;
                pathNode closest = this;

                foreach (pathNode p in node.visibleNodes)
                {
                    if (p.location.DistanceTo2D(location) < dis) closest = p;
                }

                return closest;
            }

            public List<pathNode> getPathToNode(pathNode node)
            {
                List<pathNode> path = new List<pathNode>();
                //End = node
                float currentDistance = 999999;
                pathNode currentNode = node;
                while (currentDistance > 256)//Risky business...
                {
                    currentNode = currentNode.getClosestPathNodeToEndNode(this);
                    currentDistance = location.DistanceTo2D(currentNode.location);
                    path.Add(currentNode);
                }
                return path;
            }
        }

        public pathNode getClosestNode(Vector3 pos)
        {
            float dis = 999999999;
            pathNode closest = null;
            foreach (pathNode p in _nodes)
            {
                if (p.location.DistanceTo2D(pos) < dis) closest = p;
            }
            return closest;
        }
    }
}
