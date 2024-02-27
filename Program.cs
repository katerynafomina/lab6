using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

class Graph
{
    private int V;
    private List<(int, int)>[] adj;

    public Graph(int vertices)
    {
        V = vertices;
        adj = new List<(int, int)>[V];
        for (int i = 0; i < V; i++)
        {
            adj[i] = new List<(int, int)>();
        }
    }

    public void AddEdge(int u, int v, int weight)
    {
        adj[u].Add((v, weight));
    }

    public void DijkstraParallel(int source, int numThreads)
    {
        var shortestPaths = new ConcurrentBag<(int, int)>(); // Контейнер для зберігання найкоротших шляхів
        var minDistances = new int[V]; // Масив для зберігання мінімальних відстаней до кожної вершини
        for (int i = 0; i < V; i++)
        {
            minDistances[i] = int.MaxValue; // Ініціалізуємо мінімальні відстані як нескінченність
        }

        minDistances[source] = 0; // Відстань від джерела до себе ж завжди 0

        // Використовуємо масив атомарних булевих значень для відстеження відвіданих вершин
        var visited = new bool[V];
        var atomicVisited = new bool[V];

        // Використовуємо паралельний цикл для розподілу обчислень між потоками
        Parallel.For(0, numThreads, i =>
        {
            while (true)
            {
                int u = -1;

                // Атомарно визначаємо новий u, який не був відвіданий і має мінімальну відстань
                for (int j = 0; j < V; j++)
                {
                    if (!atomicVisited[j] && (u == -1 || minDistances[j] < minDistances[u]))
                    {
                        u = j;
                    }
                }

                if (u == -1)
                    break;

                atomicVisited[u] = true; // Позначаємо вершину як відвідану

                // Обробка сусідів вибраної вершини
                foreach (var edge in adj[u])
                {
                    int v = edge.Item1; // Сусідна вершина
                    int weight = edge.Item2; // Вага ребра між u та v

                    // Перевіряємо, чи можемо оновити відстань до вершини v через u
                    if (!atomicVisited[v])
                    {
                        var newDist = minDistances[u] + weight;

                        // Атомарно оновлюємо мінімальну відстань
                        if (newDist < minDistances[v])
                        {
                            Interlocked.Exchange(ref minDistances[v], newDist);
                            shortestPaths.Add((v, newDist));
                        }
                    }
                }
            }
        });

        // Виводимо результати можуть бути виведені за необхідності
        //PrintShortestPaths(shortestPaths, source);
    }

    private void PrintShortestPaths(ConcurrentBag<(int, int)> shortestPaths, int source)
    {
        Console.WriteLine($"Shortest paths from vertex {source}:");

        foreach (var path in shortestPaths)
        {
            if (path.Item1 != source)
            {
                Console.WriteLine($"To vertex {path.Item1}, Distance: {path.Item2}");
            }
        }
    }

    public void Dijkstra(int source)
    {
        int[] dist = new int[V]; // Масив для зберігання відстаней від джерела до кожної вершини
        bool[] visited = new bool[V]; // Масив, який вказує, чи була відвідана кожна вершина

        // Ініціалізація даних
        for (int i = 0; i < V; i++)
        {
            dist[i] = int.MaxValue; // Встановлюємо початкові відстані як нескінченні
            visited[i] = false;      // Жодна вершина не була відвідана
        }

        dist[source] = 0; // Відстань від джерела до себе ж завжди 0

        // Основний цикл алгоритму Дейкстри
        for (int i = 0; i < V - 1; i++)
        {
            int u = MinDistance(dist, visited); // Знаходимо вершину з найменшою відстанню
            visited[u] = true; // Позначаємо вершину як відвідану

            // Обробка сусідів вибраної вершини
            foreach (var edge in adj[u])
            {
                int v = edge.Item1; // Сусідня вершина
                int weight = edge.Item2; // Вага ребра між u та v

                // Перевіряємо, чи можемо оновити відстань до вершини v через u
                if (!visited[v] && dist[u] != int.MaxValue && dist[u] + weight < dist[v])
                {
                    dist[v] = dist[u] + weight; // Оновлюємо відстань
                }
            }
        }
        // Результати можуть бути виведені за необхідності
        //PrintShortestPaths(dist, source);
    }

    private int MinDistance(int[] dist, bool[] visited)
    {
        int minDist = int.MaxValue; // Ініціалізуємо мінімальну відстань як нескінченність
        int minIndex = -1; // Ініціалізуємо індекс мінімальної вершини як -1

        // Проходимо всі вершини графа
        for (int v = 0; v < V; v++)
        {
            // Перевіряємо, чи вершина не була відвідана і відстань до неї менша або рівна поточній мінімальній відстані
            if (!visited[v] && dist[v] <= minDist)
            {
                // Оновлюємо мінімальну відстань та індекс вершини
                minDist = dist[v];
                minIndex = v;
            }
        }
        return minIndex;
    }

    private void PrintShortestPaths(int[] dist, int source)
    {
        Console.WriteLine($"Shortest paths from vertex {source}:");

        for (int i = 0; i < V; i++)
        {
            if (i != source)
            {
                Console.WriteLine($"To vertex {i}, Distance: {dist[i]}");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        int sourceVertex = 0; // Вершина, від якої шукаємо найкоротші шляхи
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Graph graph2 = GenerateRandomGraph(1000, 1500);
        Console.WriteLine("\nRandom Graph 2 (1000 vertices, 1500 edges):");
        Console.WriteLine("Sequential Execution:");
        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        graph2.Dijkstra(sourceVertex);
        stopwatch.Stop();
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
        int minThreads =0;
        double minTime =100;
        for(int k2 = 2;k2<=20;++k2)
        {
            Console.WriteLine($"\nParallel Execution with {k2} Threads:");
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            graph2.DijkstraParallel(sourceVertex, k2);
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            if(stopwatch.ElapsedMilliseconds<minTime)
            {
                minThreads = k2;
                minTime = stopwatch.ElapsedMilliseconds;
            }
        }
        Console.WriteLine($"Best result is with {minThreads} threads: {minTime}");

    }

    public static Graph GenerateRandomGraph(int numVertices, int numEdges)
    {
        Random random = new Random();
        Graph graph = new Graph(numVertices);

        for (int i = 0; i < numEdges; i++)
        {
            int u = random.Next(numVertices);
            int v = random.Next(numVertices);
            int weight = random.Next(-1000, 1000); 

            graph.AddEdge(u, v, weight);
        }

        return graph;
    }
}
