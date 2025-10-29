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

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            if (_objectives == null)
            {
                _objectives = GetDestinations();
                CurrentObjective = _objectives[0];
                NumberOfDestinations = _objectives.Length;
            }

            if (_path == null || _path.Count == 0)
            {
                CellInfo currentPosition = _worldInfo.FromVector3(position);
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
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
                CurrentObjective = _objectives[_index];
            }

            return CurrentDestination;
        }

        private CellInfo[] GetDestinations()
        {
            List<CellInfo> targets = new List<CellInfo>();

            List<CellInfo> treasures = new List<CellInfo>();
            treasures.AddRange(_worldInfo.Targets);

            CellInfo current = _worldInfo.FromVector3( GameObject.Find("Agent").transform.position);
            int idx = 0;
            while (treasures.Count > 0)
            {
                CellInfo nearest = treasures[0];
                //float bestDistance = Mathf.Abs(current.x - nearest.x) + Mathf.Abs(current.y - nearest.y);
                int bestDistance = _navigationAlgorithm.GetPath(current, nearest).Length; 

                foreach (var treasure in treasures)
                {
                    //float newDistance = Mathf.Abs(current.x - treasure.x) + Mathf.Abs(current.y - treasure.y);
                    int newDistance = _navigationAlgorithm.GetPath(current, treasure).Length;

                    if (newDistance < bestDistance)
                    {
                        nearest = treasure;
                        bestDistance = newDistance;
                    }
                }
                Debug.Log($"Cofre {idx} esta a {bestDistance} ");
                idx++;
                targets.Add(nearest);
                treasures.Remove(nearest);
                current = nearest;
            }

            //targets.AddRange(_worldInfo.Targets);
            targets.Add(_worldInfo.Exit);
            return targets.ToArray();
        }
    }
}
