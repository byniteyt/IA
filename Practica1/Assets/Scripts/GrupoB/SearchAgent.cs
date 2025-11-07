using UnityEngine;
using Navigation.World;
using Navigation.Interfaces;
using System.Collections.Generic;

namespace GrupoB
{
    public class SearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective { get; private set; }
        public Vector3 CurrentDestination { get; private set; }
        public int NumberOfDestinations { get; private set; }

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private CellInfo[] _objectives;
        private Queue<CellInfo> _path;

        private int _index;
        private const int HORIZON = 5;
        private bool _isEnemyPhase = false;


        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            // Localización de todos los elementos del
            // mundo antes de calcular una nueva ruta:

            _worldInfo.ResetObjects();

            // Verificación de inicialización de objetivos:
            
            if (_objectives == null)
            {
                // Inicialización de la lista de objetivos:

                _objectives = GetDestinations();
                CurrentObjective = _objectives[0];
                NumberOfDestinations = _objectives.Length;

                // Reinicio de los contadores y del estado de la fase:

                _index = 0;
                _isEnemyPhase = false;
            }

            // Comienzo de la fase de persecución de enemigos al terminar de recolectar cofres:
            
            if (!_isEnemyPhase && _index >= _objectives.Length - 1)
            {
                _isEnemyPhase = true;
            }

            // Inicialición de la fase de persecución de enemigos:

            if (_isEnemyPhase)
            {
                // Selección del zombie activo más cercano:

                CurrentObjective = GetNearestActiveEnemy();

                // En caso de no quedar más enemigos por capturar,
                // terminar la fase pertinente y partir a la meta:

                if (CurrentObjective == null)
                {
                    CurrentObjective = _worldInfo.Exit;
                    _isEnemyPhase = false;
                }

                // Reinicio de la ruta para calcular correctamente el camino hacia el objetivo:

                _path = null;
            }

            // Cálculo de la ruta a seguir en caso de haberla completado o alcanzado el horizonte:

            if (_path == null || _path.Count == 0 || (_isEnemyPhase && _path.Count <= HORIZON))
            {
                CellInfo currentPosition = _worldInfo.FromVector3(position);
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
                _path = new Queue<CellInfo>(path);
            }

            // Extracción del siguiente destino de la ruta prevista:
            
            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue();
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }

            // En caso de no quedar más nodos en la ruta, proceder al siguiente objetivo:

            else
            {
                _index++;
            
                // Verificación del número de objetivos por visitar:

                if (_index < _objectives.Length)
                {
                    CurrentObjective = _objectives[_index];
                }
            }

            return CurrentDestination;
        }

        private CellInfo[] GetDestinations()
        {
            // Creación de listas para almacenar todos los objetivos en orden:

            List<CellInfo> targets = new List<CellInfo>();
            List<CellInfo> treasures = new List<CellInfo>(_worldInfo.Targets);
            CellInfo current = _worldInfo.FromVector3(GameObject.Find("Agent").transform.position);

            // Selección iterativa del tesoro más cerca del agente hasta agotar la lista:

            while (treasures.Count > 0)
            {
                // Inicialización de las variables necesarias para
                // determinar y almacenar el tesoro más inmediato:

                CellInfo nearest = treasures[0];
                int bestDistance = _navigationAlgorithm.GetPath(current, nearest).Length;
            
                // Iteración sobre la lista de tesoros:

                foreach (var treasure in treasures)
                {
                    // Cálculo de la distancia al tesoro actual utilizando el algoritmo de navegación:

                    int newDistance = _navigationAlgorithm.GetPath(current, treasure).Length;

                    // Actualización del tesoro más próximo
                    // en caso de encontrar otro más cerca:

                    if (newDistance < bestDistance)
                    {
                        nearest = treasure;
                        bestDistance = newDistance;
                    }
                }

                // Actualización del punto de referencia y de
                // los objetivos a conseguir en ambas listas:
            
                targets.Add(nearest);
                treasures.Remove(nearest);
                current = nearest;
            }

            // Actualización final, conversión y
            // retorno de la lista de objetivos:

            targets.Add(_worldInfo.Exit);
            return targets.ToArray();
        }

        private CellInfo GetNearestActiveEnemy()
        {
            // Inicialización de las variables necesarias para
            // determinar y almacenar al zombie más inmediato:

            CellInfo nearest = null;
            int bestDistance = int.MaxValue;

            // Creación de lista de enemigos y obtención de la
            // posición del agente para calcular la distancia:

            List<CellInfo> enemies = new List<CellInfo>(_worldInfo.Enemies);
            CellInfo current = _worldInfo.FromVector3(GameObject.Find("Agent").transform.position);
            
            // Iteración sobre la lista de zombies:

            foreach (var enemy in enemies)
            {
                // Verificación de la existencia y estado activo del enemigo:

                if (enemy.GameObject == null || !enemy.GameObject.activeSelf)
                    continue;

                // Cálculo de la distancia al enemigo actual utilizando el algoritmo de navegación:

                int newDistance = _navigationAlgorithm.GetPath(current, enemy).Length;
                
                // Actualización del zombie más próximo
                // en caso de encontrar otro más cerca:

                if (newDistance < bestDistance)
                {
                    bestDistance = newDistance;
                    nearest = enemy;
                }
            }

            // Retorno del enemigo más cercano:

            return nearest;
        }
    }
}
