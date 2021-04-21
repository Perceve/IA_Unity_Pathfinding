using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DirectionOperations;

public class BreadthFirstMind : AbstractPathMind
{
    private bool _isPathCalculated = false;
    private CellInfo _endPoint;
    private List<CellInfo> _pathToFollow;
    private int _pathToFollowIndex = 1;


    public override void Repath()
    {
        throw new System.NotImplementedException();
    }

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        //Nos aseguramos de que se calcula el camino una sola vez al comienzo, comprobando si el camino ya se ha calculado
        if (!this._isPathCalculated)
        {
            this._isPathCalculated = true;
            this._pathToFollow = ReconstructPath(SolveGraph(currentPos, boardInfo));
        }

        Locomotion.MoveDirection tempDirection = DirectionOperations.CalculateDirectionToAdjacentCell( currentPos, this._pathToFollow[this._pathToFollowIndex]);
        this._pathToFollowIndex++;

        return tempDirection;
    }

    private List<CellAndFather> SolveGraph(CellInfo startingCell, BoardInfo boardInfo)
    {
        var q = new Queue<CellInfo>();
        q.Enqueue(startingCell);

        var parentList = new List<CellAndFather>();
        parentList.Add(new CellAndFather(startingCell, null));

        while (q.Count != 0)
        {
            var node = q.Dequeue();
            var neighbours = node.WalkableNeighbours(boardInfo);


            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null)
                {
                    parentList.Add(new CellAndFather( neighbours[i], node));

                    if (neighbours[i].ItemInCell != null)
                    {
                        if (neighbours[i].ItemInCell.Tag == "Goal")
                        {
                            Debug.Log("Found goal at coordinates: " + neighbours[i].CellId);
                            Debug.Log("Goal found after iterating " + parentList.Count + " times");

                            this._endPoint = neighbours[i];

                            return parentList;
                        }
                    }

                    q.Enqueue(neighbours[i]);
                    neighbours[i].ChangeToNoWalkable();

                }
            }
        }

        Debug.Log("No goal found");
        return new List<CellAndFather>();
    }

    //Construye el camino a seguir en base a la lista con las celdas y los padres desde las que se llegaron a dichas celdas
    private List<CellInfo> ReconstructPath(List<CellAndFather> parentCells)
    {
        var path = new List<CellInfo>();

        for (CellInfo i = this._endPoint; i != null; i = parentCells.Find(CellParent => CellParent.Cell == i).Father)
        {
            path.Add(i);
        }

        path.Reverse();

        return path;
    }

    //Estructura que contiene referencia a una celda y a la celda desde la que se encontro
    private struct CellAndFather
    {
        public CellInfo Cell { get; private set; }
        public CellInfo Father { get; private set; }

        public CellAndFather(CellInfo cell , CellInfo cellParent)
        {
            this.Cell = cell;
            this.Father = cellParent;
        }
    }

}
