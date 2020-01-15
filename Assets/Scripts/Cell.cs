using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Cell
{
    // Stores amount of bacteria
    private int bacteria;
    
    // Stores all creepers in a cell and energy of each creeper
    // Energy is used for existence, reproduction and movement
    private List<int> creeps;

    private Random random;
    
    // Auto properties and property accessors
    public bool IsVisited { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Bacteria => bacteria;
    public void AddBactNum(int num) => bacteria += num;
    public int CreepNum => creeps.Count;
    public void AddCreep(int creep) => creeps.Add(creep);
    public List<int> Creeps => creeps;
    public void AddCreepRange(List<int> creepsRange) => creeps.AddRange(creepsRange);
    public bool HasEnergy(int index) => creeps[index] > 0;
    private bool HasEnergyToReproduce(int index) => creeps[index] >= Init.CREEP_CREATION_ENERGY + Init.CREEP_ENERGY_RESERVE;

    public Cell()
    {
        creeps = new List<int>();
        bacteria = 0;
        random = new Random();
    }

    public void ReduceBactNum(int num)
    {
        if (num < bacteria)
        {
            bacteria = 0;
            // Debug.LogError("WARNING - trying to remove exceeding amount of bacteria! X: " + X + " Y: " + Y);
        } else bacteria -= num;
    }

    public void RemoveCreep(int index)
    {
        if (creeps?.Count != 0)
            creeps.RemoveAt(index);
        else
            Debug.LogError("WARNING - trying to remove creeper from a cell without any creepers! X: " + X + " Y: " + Y);
    }
    
    public void HandleCreepsAction(World w, Cell[,] tempData)
    {
        /*Each tact a creeper executes one of 5 actions
         a) Birth of at least one creeper and losing energy
         b) Consuming of bacteria and increasing energy
         c) Moving to neighbouring cell with the biggest amount of bacteria and losing energy
         d) Waiting and losing energy(if neighbouring cells don't have bacteria)
         e) Death(if creeper has no energy left and no bacteria to eat)*/
        
        for (int index = creeps.Count - 1; index >= 0; index--)
        {
            if (HasEnergyToReproduce(index))
            {
                /*a) If creeper has enough energy, than creates at least one another creeper and his energy decreases
                 corresponding to CREEPER_ENERGY_PRO_LIFE per one created creeper*/
                var bornCreepersNum = 0;
                while (HasEnergyToReproduce(index) && bornCreepersNum < Init.MAX_CREEP_NUM_BORN_PER_TACT)
                {
                    creeps.Add(Init.CREEP_INITIAL_ENERGY);
                    creeps[index] -= Init.CREEP_CREATION_ENERGY;
                    bornCreepersNum++;
                }
            }
            else
            {
                /*b) If creeper doen't have enough energy, than he eats bacteria in current cell,
                 but no more than MAX_BACT_EATEN_BY_CREEPER and gains corresponding to Init.ENERGY_FROM_BACT_CONSUMPTION energy*/
                if (bacteria > 0)
                {
                    if (bacteria >= Init.MAX_BACT_EATEN_BY_CREEP)
                    {
                        // If there is more bacteria in a cell than a creeper can eat
                        creeps[index] += Init.MAX_BACT_EATEN_BY_CREEP * Init.ENERGY_FROM_BACT_CONSUMPTION;
                        ReduceBactNum(Init.MAX_BACT_EATEN_BY_CREEP);
                    }
                    else
                    {
                        // If there is less bacteria in a cell than a creeper can eat
                        creeps[index] += bacteria * Init.ENERGY_FROM_BACT_CONSUMPTION;
                        ReduceBactNum(bacteria);
                    }
                }
                else
                {
                    /*If there is no bacteria in a cell, three more options are left:
                     move to another cell, wait, die*/
                    if (HasEnergy(index))
                    {
                        var bestCell = CheckNeighbourCells(w);
                        
                        if (bestCell.bacteria > 0)
                        {
                            // c) Move to the cell with highest number of bacteria
                            creeps[index] -= Init.ENERGY_ON_MOVING;  
                            
                            // Move creeper to another cell
                            tempData[bestCell.X, bestCell.Y].AddCreep(creeps[index]);
                            RemoveCreep(index);
                        }
                        else
                        {
                            // d) Neighbour cells don't have any bacteria, wait
                            creeps[index] -= Init.ENERGY_ON_WAITING; 
                        }
                    }
                    else 
                        // e) Creeper dies
                        RemoveCreep(index); 
                }
            }
        }
    }

    private Cell CheckNeighbourCells(World w)
    {
        /*Check if neighbouring cells have any bacteria, if yes return cell with the highest number*/
        
        var neighbours = Init.Neighbours;
        Cell best = new Cell{X = 0, Y = 0};

        int newX, newY;

        // Loop will check neighbours until there is no more positions left in the list, giving the ability to increase the amount of neighbours
        for (int i = 0; i < neighbours.Count; i++)
        {
            newX = X + neighbours[i].X;
            newY = Y + neighbours[i].Y;
            
            if (newX >= 0 && newY >= 0 && newX < Init.WORLD_SIZE && newY < Init.WORLD_SIZE) 
                if (w.CellsData(newX, newY).bacteria > 0)
                    if (w.CellsData(newX, newY).bacteria > best.Bacteria)
                       best = w.CellsData(newX, newY);
        }
        return best;
    }
    
    public void BactReproduction(World w, Cell[,] tempData)
    {
        /*Newborn bacteria can't be eaten in the same tact, thus are stored in tempWorld.
         Thanks to this the simulation is more stable*/
        int newBact, newBactStayingInCell, newBactMovingToNewCells;

        int tempCell = bacteria;

        if (bacteria > 0)
        {
            // How many bacteria have been born
            newBact = (int) Math.Round(bacteria * Init.BACT_MULTIPLICATION_RATE);

            // How many bacteria have stayed
            newBactStayingInCell = (int) (newBact * Init.BACT_STAY_RATE);
            
            // Keep newborn bacteria safe in separate cell
            tempData[X, Y].bacteria += newBactStayingInCell;

            // Remaining bacteria that moves to new cells
            newBactMovingToNewCells = newBact - newBactStayingInCell;

            GoToNewCells(tempData, newBactMovingToNewCells);
        }
    }

    private void GoToNewCells(Cell[,] tempData, int bactMovinToNewCells)
    {
        // Stores bacteria in existent neighbours
        List<Cell> neighbours = new List<Cell>();
        
        int newX, newY, partBact;

        // Stores only existent neighbour cells
        for (int i = 0; i < Init.Neighbours.Count; i++)
        {
            newX = X + Init.Neighbours[i].X;
            newY = Y + Init.Neighbours[i].Y;
            
            if (newX >= 0 && newY >= 0 && newX < Init.WORLD_SIZE && newY < Init.WORLD_SIZE) 
                neighbours.Add(new Cell{X = newX, Y = newY});
        }

        // TODO: review randomization algorithm
        // Randomization without repetition
        while (neighbours.Count > 0 && bactMovinToNewCells > 0)
        {
            int index = random.Next(0, neighbours.Count);

            if (neighbours.Count > 1)
            {
                // Random amount of bacteria which is added to neighbour cells
                partBact = random.Next(0, bactMovinToNewCells);
                tempData[ neighbours[index].X, neighbours[index].Y].AddBactNum(partBact);

                // Subtract moved bacteria
                bactMovinToNewCells -= partBact;
            }
            else
            { 
                // Add remaining bacteria
                tempData[neighbours[index].X, neighbours[index].Y].AddBactNum(bactMovinToNewCells);

                bactMovinToNewCells = 0;
            }

            neighbours.RemoveAt(index);
        }
    }

    public void Clear()
    {
        bacteria = 0;
        creeps.Clear();
    }
}