using System;
using Navigation.World;
using Navigation.Interfaces;
using System.Collections.Generic;

namespace GrupoB
{
    public class SearchAlgorithm : INavigationAlgorithm
    {
        private class Node
        {
            public float GCost; // Coste desde el punto de partida hasta el nodo actual.
            public float HCost; // Coste estimado desde el nodo actual hasta el destino.
            public float FCost; // Coste total estimado de la ruta completa.

            public Node Parent; // Referencia al nodo padre del actual.
            public CellInfo Cell; // Información de la celda representada por este nodo.

            public Node(Node parent, CellInfo cell, float g, float h)
            {
                GCost = g;
                HCost = h;
                FCost = g + h;

                Cell = cell;
                Parent = parent;
            }
        }

        private WorldInfo _world;

        public void Initialize(WorldInfo worldInfo)
        {
            _world = worldInfo;
        }

        public CellInfo[] GetPath(CellInfo start, CellInfo target)
        {
            // Creación de listas para mantener un seguimiento de los nodos durante el proceso de búsqueda de rutas:
            
            List<Node> openedList = new List<Node>();
            List<CellInfo> closedList = new List<CellInfo>();

            // Creación del nodo inicial:

            Node startNode = new Node(null, start, 0, Manhattan(start, target));
            openedList.Add(startNode);

            // Bucle principal del algoritmo A*:

            while (openedList.Count > 0)
            {
                // Inicialización del nodo actual con el primer elemento de la lista de nodos por explorar:

                Node currentNode = openedList[0];

                // Selección del nodo con menor coste total estimado,
                // o FCost, dentro de la lista de nodos por explorar:

                foreach (var node in openedList)
                {
                    if (node.FCost < currentNode.FCost)
                    {
                        currentNode = node;
                    }
                }

                // Verificación de llegada al destino y reconstrucción de la ruta:

                if (currentNode.Cell == target)
                {
                    return ReconstructPath(currentNode, start);
                }

                // Actualización de las listas de seguimiento de nodos:

                openedList.Remove(currentNode);
                closedList.Add(currentNode.Cell);

                // Iteración sobre los nodos vecinos al actual:

                foreach (var neighbour in GetNeighbours(currentNode.Cell))
                {
                    // Verificación de la validez de los nodos vecinos al actual:

                    if (neighbour == null || !neighbour.Walkable || closedList.Contains(neighbour))
                    {
                        continue;
                    }

                    // Cálculo del nuevo coste estimado al objetivo:

                    float newGCost = currentNode.GCost + 1;

                    // Comprobación de la existencia previa de un nodo vecino en la lista de nodos por explorar: 

                    Node existingNode = openedList.Find(node => node.Cell == neighbour);

                    // Inserción del nodo vecino en caso de no existir previamente en la lista de nodos por explorar:

                    if (existingNode == null)
                    {
                        Node neighborNode = new Node(currentNode, neighbour, newGCost, Manhattan(neighbour, target));
                        openedList.Add(neighborNode);
                    }

                    // Actualización de costes y referencia al nodo padre en caso de encontrar un camino más eficiente:

                    else if (newGCost < existingNode.GCost)
                    {
                        existingNode.GCost = newGCost;
                        existingNode.Parent = currentNode;
                        existingNode.FCost = newGCost + existingNode.HCost;
                    }
                }
            }

            // Retorno de la celda inicial en caso de no encontrar ninguna ruta posible:

            return new CellInfo[] { start };
        }

        private float Manhattan(CellInfo a, CellInfo b)
        {
            // Cálculo de la distancia de Manhattan sumando las diferencias
            // absolutas entre las coordenadas correspondientes a los dos puntos:

            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        private CellInfo[] ReconstructPath(Node currentNode, CellInfo start)
        {
            // Creación de la lista de celdas que conforman la ruta:

            List<CellInfo> path = new List<CellInfo>();

            // Bucle encargado de trazar el recorrido hacia atrás desde
            // el destino hasta el punto de partida, excluyendo el nodo inicial:

            while (currentNode?.Parent != null)
            {
                path.Add(currentNode.Cell);
                currentNode = currentNode.Parent;
            }

            // Inversión del orden de la lista para obtener la ruta en el sentido correcto:

            path.Reverse();

            // Conversión y retorno de la ruta en formato de vector:

            return path.ToArray();
        }

        public IEnumerable<CellInfo> GetNeighbours(CellInfo current)
        {
            // Retorno de los nodos vecinos adyacentes al actual:

            yield return _world[current.x, current.y - 1];
            yield return _world[current.x + 1, current.y];
            yield return _world[current.x, current.y + 1];
            yield return _world[current.x - 1, current.y];
        }
    }
}
    