using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.DataStructures;

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
        if (!this._isPathCalculated)
        {
            this._isPathCalculated = true;
            this._pathToFollow = ReconstructPath( currentPos, SolveGraph(currentPos, boardInfo));
        }

        Locomotion.MoveDirection tempDirection = CalculateNextDirection( currentPos, _pathToFollow[_pathToFollowIndex]);
        _pathToFollowIndex++;

        return tempDirection;
    }

    private List<CellParent> SolveGraph(CellInfo startingCell, BoardInfo boardInfo)
    {
        var q = new Queue<CellInfo>();
        q.Enqueue(startingCell);

        var parentList = new List<CellParent>();
        parentList.Add(new CellParent(startingCell, null));

        while (q.Count != 0)
        {
            var node = q.Dequeue();
            var neighbours = node.WalkableNeighbours(boardInfo);


            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null)
                {
                    parentList.Add(new CellParent( neighbours[i], node));

                    if (neighbours[i].ItemInCell != null)
                    {
                        if (neighbours[i].ItemInCell.Tag == "Goal")
                        {
                            print("Found goal at coordinates: " + neighbours[i].CellId);
                            _endPoint = neighbours[i];

                            print("Goal found after iterating " + parentList.Count + " times");

                            return parentList;
                        }
                    }

                    q.Enqueue(neighbours[i]);
                    neighbours[i].ChangeToNoWalkable();

                }
            }
        }

        Debug.Log("No goal found");
        return new List<CellParent>();
    }

    private List<CellInfo> ReconstructPath(CellInfo startingCell, List<CellParent> parentCells)
    {
        var path = new List<CellInfo>();

        for (CellInfo i = _endPoint; i != null; i = parentCells.Find(CellParent => CellParent.Cell == i).CellFather)
        {
            path.Add(i);
        }

        path.Reverse();

        return path;
    }

    private struct CellParent
    {
        public CellInfo Cell { get; private set; }
        public CellInfo CellFather { get; private set; }

        public CellParent(CellInfo cell , CellInfo cellParent)
        {
            this.Cell = cell;
            this.CellFather = cellParent;
        }
    }

    private Locomotion.MoveDirection CalculateNextDirection( CellInfo currentPosition, CellInfo nextPosiiton)
    {
        Vector2 vectorDifference = nextPosiiton.GetPosition - currentPosition.GetPosition;

        if (vectorDifference.x == 1)
        {
            return Locomotion.MoveDirection.Right;
        }
        else if (vectorDifference.x == -1)
        {
            return Locomotion.MoveDirection.Left;
        }
        if (vectorDifference.y == 1)
        {
            return Locomotion.MoveDirection.Up;
        }
        else if (vectorDifference.y == -1)
        {
            return Locomotion.MoveDirection.Down;
        }

        return Locomotion.MoveDirection.Up;
    }
}
