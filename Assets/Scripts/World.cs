using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class World : MonoBehaviour
{
    /*Stores cell prefab, scale of the world, reference to the object that will hold all the cells of the world,
     reference to the object that holds a graph SCRIPT, reference to the current tact label*/
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float scale = 1.0f;
    [SerializeField] private RectTransform mapContainer;

    /*Stores externally new bacteria and creepers to simulate synchronous activity*/
    // private World tempWorld;
    
    /*Stores references to the cells, cell labels, cell data*/
    private GameObject[,] cellsMap;
    private TextMeshProUGUI[,] cellsMapBacteria;
    private TextMeshProUGUI[,] cellsMapCreepers;
    private Cell[,] cellsData;
    private Cell[,] tempData;
    
    /*Stores total amount of bacteria and creepers each tact*/
    private List<int> totalCreepers;
    private List<int> totalBacteria;
    
    /*Random generator*/
    private Random random;
    
    /*Additional parameters*/
    private float cellSize = 0;
    private int gridSize = 0;
    
    public List<int> TotalBacteria => totalBacteria;
    public List<int> TotalCreepers => totalCreepers;
    public int TotalBacteriaInTact(int i) => totalBacteria[i];
    public int TotalCreepersInTact(int i) => totalCreepers[i];
    public bool IsReady { get; set; }
    public Cell CellsData(int i, int j) => cellsData[i, j];

    public void Awake()
    {
        cellsMap = new GameObject[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsMapBacteria = new TextMeshProUGUI[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsMapCreepers = new TextMeshProUGUI[Init.WORLD_SIZE, Init.WORLD_SIZE];
        cellsData = new Cell[Init.WORLD_SIZE, Init.WORLD_SIZE];
        tempData = new Cell[Init.WORLD_SIZE, Init.WORLD_SIZE];
        // tempWorld.cellsData = new Cell[Init.WORLD_SIZE, Init.WORLD_SIZE];

        totalBacteria = new List<int>();
        totalCreepers = new List<int>();
        // tempWorld.totalBacteria = new List<int>();
        // tempWorld.totalCreepers = new List<int>();

        random = new Random();
        
        IsReady = false;
        gridSize = Init.WORLD_SIZE;
    }

    public GameObject InstantiateCell(Vector2 anchorePosition, Vector3 scale)
    {
        GameObject g = Instantiate(cellPrefab, mapContainer.transform, false);
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchoredPosition = anchorePosition;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        g.transform.localScale = scale;

        return g;
    }
    
    public void InstantiateWorld()
    {
        float mapHeight = mapContainer.sizeDelta.y;

        float cellHeight = ((RectTransform)cellPrefab.transform).rect.height * scale;
        float cellWidth = ((RectTransform) cellPrefab.transform).rect.width * scale;
        
        float verticalOffset = (((RectTransform)cellPrefab.transform).rect.height / 2) * scale;
        float horizontalOffset = (((RectTransform)cellPrefab.transform).rect.width / 2) * scale;

        // Instantiate cells
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            cellsMap[x, y] = InstantiateCell(new Vector2(x * cellWidth + horizontalOffset, mapHeight - y * cellHeight - verticalOffset), 
                new Vector3(scale, scale, 1.0f));

            // Get reference to Bacteria text objects
            cellsMapBacteria[x, y] = cellsMap[x, y].GetComponentsInChildren<TextMeshProUGUI>()[0];  
            
            // Get reference to Creeper text objects
            cellsMapCreepers[x, y] = cellsMap[x, y].GetComponentsInChildren<TextMeshProUGUI>()[1];
            
            //Store coordinates of the cell in the grid
            cellsData[x, y] = new Cell {X = x, Y = y};
            
            /*Stores temporarly new bacteria and creepers*/
            tempData[x, y] = new Cell {X = x, Y = y};
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
    
    public void UpdateCellsMap()
    {
        /*Assign values from cellsData to textMeshPro labels*/
        for (int i = 0; i < gridSize; i++)
        for (int j = 0; j < gridSize; j++)
        {
            cellsMapBacteria[i, j].text = "" + cellsData[i, j].Bacteria;
            cellsMapCreepers[i, j].text = "" + cellsData[i, j].CreepNum;
        }
    }

    public void CountTotals(int currentTact)
    { 
        totalBacteria.Add(0);
        totalCreepers.Add(0);
        
        for (int i = 0; i < Init.WORLD_SIZE; i++)
        for (int j = 0; j < Init.WORLD_SIZE; j++)
        {
            totalBacteria[currentTact] += cellsData[i, j].Bacteria;
            totalCreepers[currentTact] += cellsData[i, j].CreepNum;
        }
    }

    public void HandleCreeperBacteriaActions()
    {
        /*Prepares cells for random actions*/
        for (int i = 0; i < Init.WORLD_SIZE; i++)
            for (int j = 0; j < Init.WORLD_SIZE; j++)
                cellsData[i, j].IsVisited = false;
        
        /*Handles bacteria reproduction and creeper actions in each cell in a random order of cells,
         the order of reproduction and creeper action can be reversed with 50% chance*/
        for (int i = 0; i < Init.WORLD_SIZE * Init.WORLD_SIZE; i++)
        {
            Cell cell = cellsData[random.Next(0, Init.WORLD_SIZE), random.Next(0, Init.WORLD_SIZE)];

            if (!cell.IsVisited)
            { 
                if (random.NextDouble() > 0.5)
                {
                    cell.HandleCreepsAction(this, tempData);
                    cell.BactReproduction(this, tempData);
                }
                else
                {
                    cell.BactReproduction(this, tempData);
                    cell.HandleCreepsAction(this, tempData);
                }

                cell.IsVisited = true;
            }
        }
        /*Safe addition of new bacteria and creepers*/
        for (int i = 0; i < Init.WORLD_SIZE; i++)
        for (int j = 0; j < Init.WORLD_SIZE; j++) 
        {
            CellsData(i, j).AddBactNum(tempData[i, j].Bacteria);
            CellsData(i, j).AddCreepRange(tempData[i, j].Creeps);
            
            tempData[i, j] = new Cell();
            tempData[i, j].Creeps.Clear();
        }
    }

    public void Clear()
    {
        TotalBacteria.Clear();
        TotalCreepers.Clear();
        
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            /*Clear all cells*/
            cellsData[x, y] = new Cell {X = x, Y = y};
            tempData[x, y] = new Cell {X = x, Y = y};
            tempData[x, y].Creeps.Clear();
        }
    }
}
