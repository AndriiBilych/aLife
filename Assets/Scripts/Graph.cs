using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using TMPro;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform verticalLabelPrefab; //labelTemplateY - vertical label
    [SerializeField] private RectTransform horizontalLabelPrefab; //labelTemplateX - horizontal label

    /*private void Awake()
    {
        List<int> valueList = new List<int> {50, 50, 50, 50, 50, 50, 50, 50, 50, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50};
        
        ShowGraph(valueList);
    }*/

    private GameObject CreateCircle(Vector2 anchorePosition)
    {
        //TODO: test if this is faster and more effective than instantiation
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchorePosition;
        rectTransform.sizeDelta = new Vector2(20, 20);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    public void ShowGraph(List<int> valuesList)
    {
        /*Destroys all children of a gameobject except background*/
        foreach (Transform child in graphContainer.transform)
        {
            if (child.name != "Background")
                Destroy(child.gameObject);
        }
        
        /*The height of the container*/
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        
        /*Limits for the graph*/
        float yMaximum = 0f;
        float xMaximum = valuesList.Count;
        
        /*Distance between circles*/
        float xOffset = graphWidth / xMaximum;

        bool shouldUseLabel = true;

        GameObject lastCircle = null;

        /*Look for the largest number of a list*/
        for (int i = 0; i < valuesList.Count; i++)
        {
            if (valuesList[i] > yMaximum) yMaximum = valuesList[i];
        }
        
        /*Graph loop*/
        for (int i = 0; i < valuesList.Count; i++)
        {
            float x = (xOffset / 2f) + i * xOffset;
            float y = (valuesList[i] / yMaximum) * graphHeight;
            
            GameObject currentCircle = CreateCircle(new Vector2(x, y));
            
            if ( lastCircle != null) CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition, currentCircle.GetComponent<RectTransform>().anchoredPosition);
            
            lastCircle = currentCircle;

            /*Put less labels if there are too many items in a graph*/
            if (valuesList.Count < 25 || valuesList.Count > 25 && i % 5 == 0) shouldUseLabel = true;
            else shouldUseLabel = false;
            
            if (shouldUseLabel)
            {
                RectTransform hLabel = Instantiate(horizontalLabelPrefab, graphContainer, false);
                hLabel.anchoredPosition = new Vector2(x, -20f);
                hLabel.GetComponent<TextMeshProUGUI>().text = i.ToString();
            }
        }
    }

    private void CreateConnection(Vector2 a, Vector2 b)
    {
        GameObject gameObject = new GameObject("Connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = Color.gray;
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
}
