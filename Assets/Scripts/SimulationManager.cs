using System;
using System.Collections;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private float timer = 0.0f;
    
    /*References to the corresponding scripts*/
    private World world;
    private Graph graph;
    private Controls controls;

    private int currentTact = 0;

    public void ExecuteOnTimer() => StartCoroutine(ExecuteSimulation()); 
    
    public void Awake()
    {
        world = gameObject.GetComponentInChildren<World>();
        graph = gameObject.GetComponentInChildren<Graph>();
        controls = gameObject.GetComponentInChildren<Controls>();
        
        world.InstantiateWorld();
    }

    // TODO: review
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
     public IEnumerator Start()
     {
         /*Wait until the world is initialized and instantiated*/
         while (!world.IsReady) yield return null;
     
         /*Sow bacteria and creepers, update map, draw and update graph, update current tact label*/
         world.PopulateWorld();
         graph.ShowGraph(world.TotalBacteria, world.TotalCreepers);
         controls.UpdateCurrentTactLabel(currentTact);
     }
     
     public IEnumerator ExecuteSimulation()
    {
        while (currentTact <= Init.NUM_TACT)
        {
            yield return new WaitForSeconds(timer);
            currentTact++;
            ExecuteNextTact();
            graph.ShowGraph(world.TotalBacteria, world.TotalCreepers);
            world.UpdateCellsMap();
        }
    }
    
    public void ExecuteNextTact()
    {
        if(currentTact <= Init.NUM_TACT)
        { 
            /*Simulates bacteria multiplication and creeper actions, calculates total amount of bacteria and creepers in this tact*/ 
            world.HandleCreeperBacteriaActions();
            world.CountTotals(currentTact);
            
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

            if (world.TotalBacteriaInTact(currentTact) > Init.BACT_NUM_LIMIT)
            {
                /*Check if the total amount of bacteria in the world exceeds the world's limit*/
                controls.DisplayWarning("Total amount of bacteria surpassed limit!\nCurrent tact: " + currentTact);
                currentTact = Init.NUM_TACT + 1;
                controls.SetRestartInteractable();
            }
            else if (world.TotalBacteriaInTact(currentTact) == 0)
            {
                /*Check if the total of bacteria in the world is 0*/
                controls.DisplayWarning("All bacteria died!\nCreepers: " + world.TotalCreepersInTact(currentTact) + "\nCurrent tact: " + currentTact);
                currentTact = Init.NUM_TACT + 1;
                controls.SetRestartInteractable();
            }
            else       
                controls.UpdateCurrentTactLabel(currentTact);
        }
        else
        {
            controls.DisplayWarning("The simulation ended succesfully!\nBacteria: " + world.TotalBacteriaInTact(Init.NUM_TACT) 
                                                                                    + "\nCreepers: " + world.TotalCreepersInTact(Init.NUM_TACT));
            controls.SetRestartInteractable();
        }
    }

    public void Restart()
    {
        world.Clear();

        currentTact = 0;
        
        controls.UpdateCurrentTactLabel(currentTact);
     
        /*Sow bacteria and creepers, update map, draw and update graph, update current tact label*/
        world.PopulateWorld();
        graph.ShowGraph(world.TotalBacteria, world.TotalCreepers);
        controls.UpdateCurrentTactLabel(currentTact);
    }
}
