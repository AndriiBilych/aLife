// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Services;
// using Google.Apis.Sheets.v4;
// using Google.Apis.Sheets.v4.Data;

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Program : MonoBehaviour
{
    //Google sheets variables
    /*static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    
    static readonly string ApplicationName = "aLifeCSharp";
    
    static readonly string SpreadsheetId = "1ZZ5zZkgYlniNOsYBLtw1idzzabBmWU16oazJePBSURA";
    
    static readonly string sheet = "Sheet2";
    
    static SheetsService service;
    */
    
    private WorldMap world;

    private TextMeshProUGUI displayCurrentTact;

    private int currentTact = 0;

    private bool isTactReady;

    [SerializeField] private float timer = 0f;
    
    [SerializeField] private GameObject graphContainer;

    private Graph graph;
    
    public void StartTimer() => StartCoroutine(ExecuteOnTimer()); 
    // public void StartOnCoroutine() => StartCoroutine(ExecuteOnTimer()); 
    
    public void Awake()
    {
        graph = graphContainer.GetComponent<Graph>();
        world = FindObjectOfType<Canvas>().GetComponent<WorldMap>();
        displayCurrentTact = GameObject.FindWithTag("CurrentTact").GetComponent<TextMeshProUGUI>();
        isTactReady = false;
        
        /*//Get the credentials from json
        GoogleCredential credential;
        using (var stream = new FileStream("aLife-5238216c8adf.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(Scopes);
        }

        // Create Google Sheets API service.
        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });*/
    }

    // TODO: review
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public IEnumerator Start()
    {
        // Waits until the worldMap is initialized and instantiated
        while (!world.IsReady) yield return null;
        
        // Sows Bacteria and creepers, updates map
        world.PopulateWorld();
        graph.ShowGraph(world.TotalBacteria);
        displayCurrentTact.text = "Current tact: " + currentTact;
    }

    public IEnumerator ExecuteOnTimer()
    {
        while (currentTact <= Init.NUM_TACT)
        {
            yield return new WaitForSeconds(timer);
            ExecuteNextTactOnTimer();
            Debug.Log("Tact: " + currentTact);
        }
    }

    public void ExecuteNextTactOnTimer()
    {
        currentTact++;
        
        /*Simulation loop*/
        if(currentTact <= Init.NUM_TACT)
        { 
            /*Simulate bacteria multiplication and creeper actions*/ 
            world.HandleCreeperBacteriaActions();

            /*Bacteria and creeper injections*/
            /*if (numTact >= 50)
            {
            if (totalBactNum <= 200) start_waitng_bacterias = true;
            if (start_waitng_bacterias && wait_num_tact_bacteria != 0) wait_num_tact_bacteria--;
            if (wait_num_tact_bacteria == 0)
            {
            world2.sowBacteries(Init.START_NUM_BACT);
            world2.totallyBacteria[world2.totallyBacteria.Count - 1] += Init.START_NUM_BACT;
            Console.WriteLine("BactAmount: " + world2.totallyBacteria[world2.totallyBacteria.Count - 1] + " Count: " + world2.totallyBacteria.Count);
            Console.WriteLine("Got to if wait_num_bact");
            start_waiting_creepers = true;
            }
            if (start_waiting_creepers && wait_num_tact_creepers != 0) wait_num_tact_creepers--;
            if (wait_num_tact_creepers == 0)
            {
            world2.sowCreepers(Init.START_NUM_CREEPERS);
            world2.totallyCreepers[world2.totallyCreepers.Count - 1] += Init.START_NUM_CREEPERS;
            Console.WriteLine("BactAmount: " +  world2.totallyCreepers[world2.totallyCreepers.Count - 1] + " Count: " + world2.totallyCreepers.Count);
            Console.WriteLine("Got to if wait_num_tact_creepers");
            }
            }*/
            
            /*Calculate total amount of bacteria and creepers in this tact*/
            world.CountTotals(currentTact);

            graph.ShowGraph(world.TotalBacteria);

            world.UpdateCellsMap();
            
            if (world.TotalBacteriaInTact(currentTact) > Init.BACT_NUM_LIMIT)
            {
                /*Check if the total of bacteria in the world is exceeds the world's limit*/
                Debug.Log("Total amount of bacteria surpassed the world limit! Limit:  " + Init.BACT_NUM_LIMIT + " Current tact: " + currentTact);
                currentTact = Init.NUM_TACT;
            }
            else if (world.TotalBacteriaInTact(currentTact) == 0)
            {
                /*Check if the total of bacteria in the world is 0*/
                Debug.Log("All bacteria died! Bacteria: 0 Current tact: " + currentTact);
                currentTact = Init.NUM_TACT;
            }
            else       
                displayCurrentTact.text = "Current tact: " + currentTact;
        }
        else
        {
            Debug.Log("The simulation ended! The number of tacts hit " + Init.NUM_TACT);
        }
    }
    
    public void DisableButton()
    {
        GameObject.FindWithTag("NextTactButton").GetComponent<Button>().interactable = false;
    }
    
    /*Google sheets stuff*/
    /*//Prepare data for Google sheets parsing
    public  List<IList<object>> GetWorldGridData(WorldMap w)
    {
        var oblist = new List<object>();
        List<IList<object>> values = new List<IList<object>>();
        
        for (int i = 0; i < Init.SIZE_WORLD; i++)
        {
            oblist.Clear();
            oblist.TrimExcess();

            for (int j = 0; j < Init.SIZE_WORLD; j++)
            {
                // consoleOutput += w.board[i, j].getBactNum() + " | " + w.board[i, j].getCreepersNum() + "\t";

                oblist.Add(w.CellsData(i, j).getBactNum() + " | " + w.CellsData(i, j).getCreepersNum());
            }
            // consoleOutput += "\n";
            values.Add(new List<object>(oblist));

        }
        // consoleOutput += "\n";

        return values;
    }

    //Google sheets method
    static void CreateEntry(List<IList<object>> values, string sheetRange)
    {
        var range = $"{sheet}!" + sheetRange;
        var valueRange = new ValueRange();
    
        valueRange.Values = values;
    
        var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        var appendReponse = appendRequest.Execute();
    }

    //Google sheets method
    static void UpdateEntry(List<IList<object>> values)
    {
        var valueRange = new ValueRange();
        
        valueRange.Values = values;

        var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, $"{sheet}!A1:J10");
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        var updateReponse = updateRequest.Execute();
    }

    //Google sheets method
    static void DeleteEntry()
    {
        var deleteRequest = service.Spreadsheets.Values.Clear(new ClearValuesRequest(), SpreadsheetId, $"{sheet}!A1:M1500");
        var deleteReponse = deleteRequest.Execute();
    }*/
}
