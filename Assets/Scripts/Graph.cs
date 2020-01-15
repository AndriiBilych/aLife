using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using TMPro;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite redSprite;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform labelPrefab;

    private GameObject CreateCircle(Vector2 anchorePosition, Sprite sprite)
    {
        //TODO: test if this is faster and more effective than instantiation
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = sprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchorePosition;
        rectTransform.sizeDelta = new Vector2(20, 20);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private void CreateConnection(Vector2 a, Vector2 b, Color color)
    {
        GameObject gameObject = new GameObject("Connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = color;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (b - a).normalized;
        float dis = Vector2.Distance(a, b);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(dis, 3f);
        rectTransform.anchoredPosition = a + dir * (dis * 0.5f);
        
        // TODO: Calculate angle yourself
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }
    
    public void ShowGraph(List<int> bacteriaList, List<int> creeperList)
    {
        //TODO: improve graph, add export
        
        /*Destroys all children of a gameobject except background*/
        foreach (Transform child in graphContainer.transform)
        {
            if (child.name != "Background")
                Destroy(child.gameObject);
        }
        
        /*The height and width of the container*/
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        
        /*Limits for the graph values*/
        float yMaximum = 0f;
        float xMaximum = bacteriaList.Count;
        
        /*Distance between circles*/
        float xOffset = graphWidth / xMaximum;

        bool shouldUseLabel = true;
        bool shouldMakeCircle = true;

        GameObject lastCircle = null;

        int verticalSelectorAmount = 10;

        /*Look for the largest number of a list*/
        for (int i = 0; i < bacteriaList.Count; i++)
        {
            if (bacteriaList[i] > yMaximum) yMaximum = bacteriaList[i];
        }
        
        /*Bacteria graph loop*/
        for (int i = 0; i < bacteriaList.Count; i++)
        {
            float x = (xOffset / 2f) + i * xOffset;
            float y = (bacteriaList[i] / yMaximum) * graphHeight;
            
            // TODO: put less circles when too much tacts
            GameObject currentCircle = CreateCircle(new Vector2(x, y), greenSprite);
            
            if ( lastCircle != null) CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition, currentCircle.GetComponent<RectTransform>().anchoredPosition, Color.green);
            
            lastCircle = currentCircle;

            /*Put less labels if there are too many items in a graph*/
            // if (bacteriaList.Count > 50 && i % 10 == 0) shouldUseLabel = true;
             if (bacteriaList.Count < 25 || bacteriaList.Count > 25 && i % 10 == 0) shouldUseLabel = true;
            else shouldUseLabel = false;
            
            if (shouldUseLabel)
            {
                RectTransform hLabel = Instantiate(labelPrefab, graphContainer, false);
                hLabel.anchoredPosition = new Vector2(x, -20f);
                hLabel.GetComponent<TextMeshProUGUI>().text = i.ToString();
            }
        }

        lastCircle = null;
        
        /*Creeper graph loop*/
        for (int i = 0; i < bacteriaList.Count; i++)
        {
            float x = (xOffset / 2f) + i * xOffset;
            float y = (creeperList[i] / yMaximum) * graphHeight;
            
            GameObject currentCircle = CreateCircle(new Vector2(x, y), redSprite);
            
            if ( lastCircle != null) CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition, currentCircle.GetComponent<RectTransform>().anchoredPosition, Color.red);
            
            lastCircle = currentCircle;
            
        }
        
        /*Vertical labels*/
        for (int j = 0; j <= verticalSelectorAmount; j++)
        {
            float ySelector = graphHeight / verticalSelectorAmount;
            
            RectTransform vLabel = Instantiate(labelPrefab, graphContainer, false);
            vLabel.anchoredPosition = new Vector2(-35f, j * ySelector);
            double text = Math.Floor(j * (yMaximum / verticalSelectorAmount));

            // Debug.Log(text);

            if (text >= 1000)
            {
                text /= 1000;
                vLabel.GetComponent<TextMeshProUGUI>().text = Math.Floor(text) + "k";
            }
            else
                vLabel.GetComponent<TextMeshProUGUI>().text = text.ToString();
        }
    }
}
