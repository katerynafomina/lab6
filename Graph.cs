using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6
{
    public class Graph
    {
        int[][] graph;
        int size;

        public int this[int i, int j]
        {
            get => graph[i][j];
            set => graph[i][j] = value;
        }
        public int Size => size;


        public Graph(int size)
        {
            this.size = size;
            this.graph = new int[size][];
            for (int i = 0; i < size; ++i)
            {
                graph[i] = new int[size];
            }
        }
        public Graph Generate(float noPathRate = 0.35F)
        {
            System.Random rand = new System.Random(System.DateTime.Now.Millisecond);
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (rand.NextDouble() < noPathRate) graph[i][j] = 0;
                    else graph[i][j] = rand.Next(-100, 100);
                }
            }
            return this;
        }
        public int[] GetWeightToHeighbour(int edgeIndex) => graph[edgeIndex];
    }
}
