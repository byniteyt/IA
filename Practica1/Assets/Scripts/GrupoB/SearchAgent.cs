using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace GrupoB
{
    public class SearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective => throw new System.NotImplementedException();

        public Vector3 CurrentDestination => throw new System.NotImplementedException();

        public int NumberOfDestinations => throw new System.NotImplementedException();

        public Vector3? GetNextDestination(Vector3 currentPosition)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            throw new System.NotImplementedException();
        }
    }
}
