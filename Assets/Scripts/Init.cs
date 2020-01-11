using System.Collections.Generic;

public struct Init
{
    // Default: 10
    /*Worlds size*/
    public static int WORLD_SIZE = 10;

    // Default: 100
    /*Total amount of tacts*/
    public static int NUM_TACT = 100;

    // Default: 500
    /*Starting amount of creepers*/
    public static int START_NUM_CREEPS = 500;

    // Default: 500
    /*Starting amount of bacteria*/
    public static int START_NUM_BACT = 500;

    // Default: 1
    /*The amount of energy needed to create new creeper*/
    public static int CREEP_CREATION_ENERGY = 1;

    // Default: 1
    /*Initial energy of a newborn creeper*/
    public static int CREEP_INITIAL_ENERGY = 1; 

    // Default: 4
    /*The amount of energy a creeper stores before he can create new creeper*/
    public static int CREEP_ENERGY_RESERVE = 4;
    
    // Default: 1 (does not exist, estimated from code)
    /*How much energy creeper gets from consuming single bacteria*/
    public static int ENERGY_FROM_BACT_CONSUMPTION = 1;
    
    // Default: 1 (does not exist, estimated from code)
    /*How much energy creeper wastes without food*/
    public static int ENERGY_ON_WAITING = 1;
    
    // Default: 1 (does not exist, estimated from code)
    /*How much energy creeper wastes by moving to another cell*/
    public static int ENERGY_ON_MOVING = 1;

    // Default: 5   
    /*Max amount of creepers that single creeper can create in a single tact.
     A creeper can't save too much energy, thus when it has more than 
     CREEP_CREATION_ENERGY + CREEPER_ENERGY_RESERVE, next tact new creeper will be created 
     and initial creeper's energy will be reduced by CREEP_CREATION_ENERGY per each created creeper.*/
    public static int MAX_CREEP_NUM_BORN_PER_TACT = 5;

    // Default: 15
    /*Max amount of bacteria consumed by creeper each tact*/
    public static int MAX_BACT_EATEN_BY_CREEP = 15;

    // Default: 0.8
    /*Each tact bacteria  will produce bacteria amount * BACT_MULTIPLICATION_RATE in each cell */
    public static float BACT_MULTIPLICATION_RATE = 0.8f;

    // Default: 0.5
    /*Describes how many bacteria will stay in a cell and how many will move
    ACCEPTABLE RANGE: 0 TO 1
    0.7 means, that 70% will stay in the same cell and 30% will move to neighbour cells*/
    public static float BACT_STAY_RATE = 0.5f;

    // Default: 1 000 000
    /*World bacteria limit. If surpassed simulation stops*/
    public static int BACT_NUM_LIMIT = 1000000; 

    // Stores offset for the neighbouring cells
    public static List<Position> Neighbours => new List<Position>
        {
            new Position(-1, 0),
            new Position(1, 0),
            new Position(0, -1),
            new Position(0, 1),
            new Position(-1, -1),
            new Position(1, 1),
            new Position(-1, 1),
            new Position(1, -1)
        };
    
    public struct Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}