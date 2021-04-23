using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DirectionOperations;

public class ASTAR_Mind : AbstractPathMind
{
    bool _isPathCalculated = false;
    private List<CellInfoPlus> _pathToFollow;
    private int _pathToFollowIndex = 1;

    public GameObject openListPrefab;

    public GameObject closedListPrefab;

    public override void Repath()
    {
        throw new System.NotImplementedException();
    }
    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        if(!this._isPathCalculated)
        {
            AStarSearch(currentPos, boardInfo);
            _isPathCalculated = true;
        }

        Locomotion.MoveDirection tempDirection = DirectionOperations.CalculateDirectionToAdjacentCell(currentPos, this._pathToFollow[this._pathToFollowIndex].GetCell());
        this._pathToFollowIndex++;
        return tempDirection;
    }

    private void ReconstructPath(CellInfoPlus _startCell, CellInfoPlus _endCell)
    {
        List<CellInfoPlus> path = new List<CellInfoPlus>();
        CellInfoPlus currentCell = _endCell;

        while (currentCell != _startCell)
        {
            
            path.Add(currentCell);
            currentCell = currentCell.GetParent();
            GameObject.Instantiate(openListPrefab).transform.position = currentCell.GetCell().GetPosition;
        }
        path.Reverse();

        _pathToFollow = path;

        _isPathCalculated = true;

    }

    bool AStarSearch(CellInfo currentPos, BoardInfo boardInfo)
    {
        CellInfoPlus root = new CellInfoPlus(0,0,0,currentPos);
        CellInfoPlus endCell = new CellInfoPlus(0,0,0,boardInfo.Exit);
        GameObject.Instantiate(openListPrefab).transform.position = currentPos.GetPosition;
        GameObject.Instantiate(openListPrefab).transform.position = boardInfo.Exit.GetPosition;


        List<CellInfoPlus> openSet = new List<CellInfoPlus>();                        //MANTENEMOS EL TRACKING DE LOS NODOS A EXPLORAR
        List<CellInfoPlus> closedSet = new List<CellInfoPlus>();                      //MANTENEMOS EL TRACKING DE LAS EXPLORADAS

        //Se crean los datos del nodo de inicio y se añade a la lista abierta.
        float gCost = 0;                                                                //COSTE DEL NODO ACTUAL A OTRO NODO
        float hCost = Distance(root, endCell);                                          //COSTE DEL NODO ACTUAL AL NODO META
        float fCost = gCost + hCost;                                                    //COSTE TOTAL
        openSet.Add(new CellInfoPlus(gCost, hCost, fCost, root.GetCell()));
        //


        while(openSet.Count > 0)
        {
            GameObject.Instantiate(openListPrefab).transform.position = openSet[0].GetCell().GetPosition;

            //Nos aseguramos que siempre esté la lista en orden, según el coste total F.
            //openList.Sort((x, y) => x.GetFCost().CompareTo(y.GetFCost()));
            CellInfoPlus currentCell = openSet[0];

            //Cogemos el nodo con menos coste F total
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].GetFCost() <= currentCell.GetFCost())
                {
                    //Esto es para el caso en el que dos nodos tengan mismo F, pero distinto H
                    //De esta forma, siempre cogemos el que esté más cerca al nodo meta, al menos según la heurística
                    if (openSet[i].GetHCost() < currentCell.GetHCost())
                    {
                        currentCell = openSet[i];
                    }
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            //Si la celda actual es la celda meta
            if (currentCell.GetCell() == endCell.GetCell())
            {
                ReconstructPath(root, endCell);
                //Se ha encontrado solución
                return true;
            }

            CellInfo[] neighbours = currentPos.WalkableNeighbours(boardInfo);
            print("NEIGHBOURS COUNT: " + neighbours.Length);

        restart:
            for (int i = 0; i < neighbours.Length; i++)
            {
                //Se comprueba si es nulo, ya que es posible que se encuentre en un lado o esquina, siendo nulos los vecinos de fuera del board.
                if (neighbours[i] != null)
                {
                    print("NO NULOS: " + neighbours[i].GetPosition);


                    // Si el vecino no es "walkable" (es un obstáculo) o ya se encuentra en la lista de exploradas, evaluar el siguiente vecino.
                    if (!neighbours[i].Walkable)
                    {
                        continue;
                    }  
                    foreach(CellInfoPlus cellPlus in closedSet)
                    {
                        if(cellPlus.GetCell() == neighbours[i]) goto restart;
                    }

                    float _gCost = Distance(neighbours[i], root.GetCell());                                       //COSTE DEL NODO ACTUAL A OTRO NODO
                    float _hCost = Distance(neighbours[i], endCell.GetCell());                                    //COSTE DEL NODO ACTUAL AL NODO META
                    float _fCost = _gCost + _hCost;
                    CellInfoPlus neighbourPlus = new CellInfoPlus(_gCost, _hCost, _fCost, neighbours[i]);


                    GameObject.Instantiate(closedListPrefab).transform.position = neighbours[i].GetPosition;

                    print("myCurrentF: " + hCost);
                    print("FutureF: " + _hCost);

                    if (fCost < _fCost || !openSet.Contains(neighbourPlus))
                    {
                        neighbourPlus.SetGCost(_gCost);
                        neighbourPlus.SetHCost(_hCost);
                        neighbourPlus.SetFCost(_fCost);
                        neighbourPlus.SetParent(currentCell);


                        if(!openSet.Contains(neighbourPlus))
                        {
                            print("HOLA");
                            //openSet.Add(neighbourPlus);
                        }
                    }


                }
            }


        }
        return false;
    }

    
    float Distance(CellInfo startingCell, CellInfo endCell)
    {
        return Vector2.Distance(startingCell.GetPosition, endCell.GetPosition);
    }
    float Distance(CellInfoPlus startingCell, CellInfoPlus endCell)
    {
        return Vector2.Distance(startingCell.GetCell().GetPosition, endCell.GetCell().GetPosition);
    }

    class CellInfoPlus
    {
        float gCost;                                                    //COSTE DEL NODO ACTUAL A OTRO NODO
        float hCost;                                                    //COSTE DEL NODO ACTUAL AL NODO META
        float fCost;                                                    //COSTE TOTAL
        CellInfo cell;                                                  //CELDA REFERENCIA
        CellInfoPlus parent = null;                                     //CELDA DE LA QUE PROVIENE

        public CellInfoPlus(float gCost, float hCost, float fCost, CellInfo cell)
        {
            this.gCost = gCost;
            this.hCost = hCost;
            this.fCost = fCost;
            this.cell = cell;
        }

        public CellInfo GetCell()
        {
            return cell;
        }
        public float GetGCost()
        {
            return gCost;
        }
        public float GetHCost()
        {
            return hCost;
        }
        public float GetFCost()
        {
            return fCost;
        }
        public CellInfoPlus GetParent()
        {
            return parent;
        }
        public void SetGCost(float g)
        {
            gCost = g;
        }
        public void SetHCost(float h)
        {
            hCost = h;
        }
        public void SetFCost(float f)
        {
            fCost = f;
        }
        public void SetParent(CellInfoPlus p)
        {
            parent = p;
        }
    }
}
