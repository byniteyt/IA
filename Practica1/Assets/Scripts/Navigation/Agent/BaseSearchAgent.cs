#region Copyright
// MIT License
// 
// Copyright (c) 2023 David María Arribas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System.Collections.Generic;
using System.Data.Common;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace Navigation.Agent
{
    public class BaseSearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective { get; private set; }
        public Vector3 CurrentDestination { get; private set; }
        public int NumberOfDestinations { get; private set; }

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private CellInfo[] _objectives;
        private Queue<CellInfo> _path;
        private int _index = 1;
        
        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            if (_objectives == null) // Cargamos los objetivos
            {
                _objectives = GetDestinations();
                Debug.LogWarning(_objectives);
                CurrentObjective = _objectives[_objectives.Length - 1];
                NumberOfDestinations = _objectives.Length;
            }
            
            if (_path == null || _path.Count==0) // La ruta no se ha inicializado o calculado
            {
                CellInfo currentPosition = _worldInfo.FromVector3(position);
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
                Debug.LogWarning($"Next destination: {CurrentObjective}");
                _path = new Queue<CellInfo>(path); 
            }

            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue();
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }
            else
            {
                _index++;
                CurrentObjective = _objectives[_objectives.Length - _index];
            }
                return CurrentDestination;
        }

        private void ReorderTreasures(int a, int b, List<CellInfo> targets)
        {
            Debug.Log($"-------------Reorganizando {a} y {b}--------------");
            CellInfo temp = targets[a];
            targets[a] = targets[b];
            targets[b] = temp;
        }

        private float DistanceToPlayer(CellInfo cellInfo)
        {
            Vector3 pos = GameObject.Find("Agent").transform.position;
            return (Mathf.Abs(pos.x - cellInfo.x) + Mathf.Abs(pos.y - cellInfo.y));
        }

        private CellInfo[] GetDestinations()
        {
            List<CellInfo> targets = new List<CellInfo>();

            targets.Add(_worldInfo.Exit);
            for (int i = 0; i < _worldInfo.Targets.Length; i++)
            {
                targets.Add(_worldInfo.Targets[i]);
            }
            for (int i = 0; i < targets.Count - 1; i++)
            {
                
                for (int j = i+1; j < targets.Count; j++)
                {
                    if (DistanceToPlayer(targets[i]) < DistanceToPlayer(targets[j]))
                    {
                        Debug.Log($"Dist_{i} <  Dist_{j} -------> " +
                            $"{DistanceToPlayer(targets[i])} > {DistanceToPlayer(targets[j])}");
                        ReorderTreasures(i, j, targets);
                    }
                }
                Debug.Log($"Dist tesoro {i} = {DistanceToPlayer(targets[i])}");
            }
            for (int i = 0; i < targets.Count; i++)
            {
                Debug.LogWarning($"La posicion {i} es de {targets[i]} ");
            }
            return targets.ToArray();
        }
    }
}