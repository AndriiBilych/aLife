using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class WorldMap : MonoBehaviour
{
    // Stores cell prefab
    public GameObject cellPrefab;

    [SerializeField]
    private float verticalOffset = 0;
    [SerializeField]
    private float horizontalOffset = 0;
    
    // Stores all information about cells of the map
    private GameObject[,] cellsMap;
    private TextMeshProUGUI[,] cellsMapBacteria;
    private TextMeshProUGUI[,] cellsMapCreepers;
    private Cell[,] cellsData;
    
    // Store total amount of bacteria and creepers on each tact
    private List<int> totalCreepersInTact;
    private List<int> totalBacteriaInTact;
    
    // Additional parameters
    private float cellSize = 0;
    private float canvasZeroPoint = 0;
    private int gridSize = 0;

    // Random generator
    private Random random;

    // Parameter accessors
    public bool IsReady { get; set; }
    public int TotalBacteriaInTact(int tact) => totalBacteriaInTact[tact];
    public List<int> TotalBacteria => totalBacteriaInTact;
    public GameObject CellsMap(int i, int j) => cellsMap[i, j];
    public Cell CellsData(int i, int j) => cellsData[i, j];

    public void Awake()
    {
        cellsMap = new GameObject[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsMapBacteria = new TextMeshProUGUI[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsMapCreepers = new TextMeshProUGUI[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsData = new Cell[Init.WORLD_SIZE, Init.WORLD_SIZE];

        // For the initial values (0 values)
        totalBacteriaInTact = new List<int>();
        totalCreepersInTact = new List<int>();

        IsReady = false;
        
        random = new Random();
        
        InstantiateWorld();
    }

    public void InstantiateWorld()
    {
        // Set additional parameters
        cellSize = ((RectTransform)cellPrefab.transform).rect.width;
        canvasZeroPoint = 50f;
        gridSize = Init.WORLD_SIZE;
        
        // Instantiate cells
        for (int i = 0; i < gridSize; i++)
        for (int j = 0; j < gridSize; j++)
        {
            cellsMap[i, j] = Instantiate(cellPrefab, new Vector3(canvasZeroPoint + (i * cellSize) + horizontalOffset, canvasZeroPoint + (j * cellSize) + verticalOffset), Quaternion.identity, transform);
            
            // Get reference to Bacteria text objects
            cellsMapBacteria[i, j] = cellsMap[i, j].GetComponentsInChildren<TextMeshProUGUI>()[0];  
            
            // Get reference to Creeper text objects
            cellsMapCreepers[i, j] = cellsMap[i, j].GetComponentsInChildren<TextMeshProUGUI>()[1];

            //Store coordinates of the cell in the grid
            cellsData[i, j] = new Cell {X = i, Y = j};
        }
        
        IsReady = true;
    }

    public void PopulateWorld()
    {
        SowBact();
        SowCreeps();
        UpdateCellsMap();
        CountTotals(0);
    }

    public void CountTotals(int currentTact)
    { 
        totalBacteriaInTact.Add(0);
        totalCreepersInTact.Add(0);
        
        for (int i = 0; i < Init.WORLD_SIZE; i++)
        for (int j = 0; j < Init.WORLD_SIZE; j++)
        {
            totalBacteriaInTact[currentTact] += cellsData[i, j].Bacteria;
            totalCreepersInTact[currentTact] += cellsData[i, j].CreepNum;
        }
    }

    public void SowBact()
    {    
        /*Sow bacteria to random cells*/
        for (int totalNumBact = 0, randomNumBact; totalNumBact < Init.START_NUM_BACT; totalNumBact += randomNumBact)
        {
            randomNumBact = random.Next(0, Init.WORLD_SIZE);
            cellsData[random.Next(0, Init.WORLD_SIZE), random.Next(0, Init.WORLD_SIZE)].AddBactNum(randomNumBact);
        }
    }

    public void SowCreeps()
    {
        /*Sow creepers to random cells*/
        for (int totalNumCreep = 0; totalNumCreep < Init.START_NUM_CREEPS; totalNumCreep++)
            cellsData[random.Next(0, Init.WORLD_SIZE), random.Next(0, Init.WORLD_SIZE)].AddCreep(Init.CREEP_INITIAL_ENERGY);
    }
    
    // Assign values from cellsData to textMeshpro components
    public void UpdateCellsMap()
    {
        for (int i = 0; i < gridSize; i++)
        for (int j = 0; j < gridSize; j++)
        {
            cellsMapBacteria[i, j].text = "" + cellsData[i, j].Bacteria;
            cellsMapCreepers[i, j].text = "" + cellsData[i, j].CreepNum;
        }
    }

    public void HandleCreeperBacteriaActions()
    {
        // poniżej przygotowanie komórek do losowych odwiedzin
        for (int i = 0; i < Init.WORLD_SIZE; i++)
            for (int j = 0; j < Init.WORLD_SIZE; j++)
                cellsData[i, j].IsVisited = false;
        
        // poniżej pętla losowego wyboru komórek świata
        // z wywołaniem akcji creepersAction() dla jednej komórki i rozmnożeniem się pozostałych bakterii
        // w tej komórce lub najpierw rozmnożeniem się bakterii w tej komórce, a następnie
        // wywołaniem akcji creepersAction() dla jednej komórki (kolejność określana jest losowo
        // z prawdopodobieństwem 50%)
        for (int i = 0; i < Init.WORLD_SIZE * Init.WORLD_SIZE; i++)
        {
            Cell cell = cellsData[random.Next(0, Init.WORLD_SIZE), random.Next(0, Init.WORLD_SIZE)];

            if (!cell.IsVisited)
            { 
                if (new Random().NextDouble() > 0.5)
                {
                    // cell.HandleCreepsAction(this);
                    cell.BactReproduction(this);
                }
                else
                {
                    cell.BactReproduction(this);
                    // cell.HandleCreepsAction(this);
                }

                cell.IsVisited = true;
            }
        }
    }

    public void DisplayWorldInConsole()
    {
        // Write amount of bacteria and creepers of each cell in console
        for (int i = 0; i < Init.WORLD_SIZE; i++)
            for (int j = 0; j < Init.WORLD_SIZE; j++)
                Debug.Log("X: " + (i + 1) + " Y: " + (j + 1) + " Bacteria: " + cellsData[i, j].Bacteria + " Creepers: " + cellsData[i, j].CreepNum);
    }

    public void DisplayTotalOrganismsInConsole()
    {
        // Write total amount of bacteria and creepers in the world on each tact
        for (int i = 0; i < totalBacteriaInTact.Count; i++)
            Debug.Log("Tact: " + i + " Bacteria: " + totalBacteriaInTact[i] + " Creepers: " + totalCreepersInTact[i]);
    }

    //TODO: review
    /*public void clear()
    {
        cellsData = new Cell[Init.SIZE_WORLD, Init.SIZE_WORLD];
        for (int i = 0; i < Init.SIZE_WORLD; i++)
        {
            for (int j = 0; j < Init.SIZE_WORLD; j++)
            {
                cellsData[i, j] = new Cell();
            }
        }
    }*/
}
