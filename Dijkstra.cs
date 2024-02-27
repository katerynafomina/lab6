using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace lab6
{
    public class Dijkstra
    {
        // FIELDS
        Graph graph;
        ConcurrentDictionary<int, int> openSet;
        ConcurrentDictionary<int, int> closedSet;
        List<int> path;

        // CONSTRUCTORS
        public Dijkstra(Graph graph)
        {
            this.graph = graph;
            this.openSet = new ConcurrentDictionary<int, int>();
            this.closedSet = new ConcurrentDictionary<int, int>();
            this.path = new List<int>();
        }

        // PROPERTIES
        public IEnumerable<int> Path => path;

        // METHODS
        public void Run(int from)
        {
            Reset();
            openSet[from] = 0; // Start position

            while (!openSet.IsEmpty)
            {
                int currentEdge = openSet.OrderBy(kv => kv.Value).First().Key;
                int currentWeight = openSet[currentEdge];
                int[] neighbourWeight = graph.GetWeightToHeighbour(currentEdge);

                Parallel.ForEach(
                    Partitioner.Create(0, graph.Size),
                    range =>
                    {
                        for (int edgeIndex = range.Item1; edgeIndex < range.Item2; ++edgeIndex)
                        {
                            if (CanBeBetter(edgeIndex, neighbourWeight) && WeightIsShorter(edgeIndex, currentWeight, neighbourWeight))
                            {
                                openSet.AddOrUpdate(edgeIndex, currentWeight + neighbourWeight[edgeIndex], (key, value) => currentWeight + neighbourWeight[key]);
                            }
                        }
                    }
                );

                int removedKey;
                openSet.TryRemove(currentEdge, out removedKey);
                closedSet[currentEdge] = currentWeight;
                path.Add(currentEdge);
            }
        }

        public void RunSequential(int from)
        {
            Reset();
            openSet[from] = 0; // Start position

            while (!openSet.IsEmpty)
            {
                int currentEdge = openSet.OrderBy(kv => kv.Value).First().Key;
                int currentWeight = openSet[currentEdge];
                int[] neighbourWeight = graph.GetWeightToHeighbour(currentEdge);

                for (int edgeIndex = 0; edgeIndex < graph.Size; ++edgeIndex)
                {
                    if (CanBeBetter(edgeIndex, neighbourWeight) && WeightIsShorter(edgeIndex, currentWeight, neighbourWeight))
                    {
                        openSet.AddOrUpdate(edgeIndex, currentWeight + neighbourWeight[edgeIndex], (key, value) => currentWeight + neighbourWeight[key]);
                    }
                }

                int removedKey;
                openSet.TryRemove(currentEdge, out removedKey);
                closedSet[currentEdge] = currentWeight;
                path.Add(currentEdge);
            }
        }

        private void Reset()
        {
            openSet.Clear();
            closedSet.Clear();
            path.Clear();
        }

        private bool CanBeBetter(int edgeIndex, int[] weights)
        {
            return weights[edgeIndex] != 0 && !closedSet.ContainsKey(edgeIndex);
        }

        private bool WeightIsShorter(int edgeIndex, int currentWeight, int[] weights)
        {
            int newWeight = currentWeight + weights[edgeIndex];
            if (openSet.TryGetValue(edgeIndex, out int existingWeight))
            {
                return newWeight < existingWeight;
            }
            return true;
        }
    }
}
