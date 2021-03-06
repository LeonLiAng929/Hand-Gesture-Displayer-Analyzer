﻿/*
 Leon (Ang) Li, Bachelor of Computer Science(Advanced).
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    alii0017@student.monash.edu
 Developed for FIT2082 project 'AR Hand Gesture Capture for Interactive Data Analytics' 
 Supervised by:
 Barrett Ens  
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    barrett.ens@monash.edu
 Max Cordeil
    Monash University Australia
    Wellington Rd, Clayton VIC 3800, Australia
    max.cordeil@monash.edu
 
 This class handles all instances of Gesture, which correspond to .csv files recorded by the web app 
 and playbacks the gesture data during runtime.
 User can manage these gesture data with different categories.
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using System;

public class GestureDisplayer : MonoBehaviour
{
    [System.Serializable]
    public class GesturePool
    {
        /*
         * A gesture pool is used to categorize gestures elicited.
         * User will need to populate the .csv files recorded by the webapp before running the unity app.
         */ 
        public string tag;
        public GameObject handModel;
        public TextAsset[] CsvData;  // an array of csv files recorded from people for one particular gesutrue.
        //public Vector3[] PositionFactor;
        public int Length()
        {
            return CsvData.Length;
        }
    }

    public Transform handModelsContainer;
    //public Transform gestureTagsContainer;
    public Text userInputTag;
    public TextMeshProUGUI showAllButtonText;
    public TextMeshProUGUI switchModeButton;
    public Slider slider;

    //public TMP_Text gestureTagDisplayWindow;
    private bool shown = false;
    private string previousInput = null;

    private int gestureCount = 0;
    private float rescaleReference = 300;
    private float boundingLength;
    private float rescaleFactor;

    private bool animationMode = true;
   
    #region Singleton
    public static GestureDisplayer instance;
    #endregion

    /*#region Configuration Mngmnt
    private void InitializePoolsByConfig()
    {
        XmlDocument configDocument = new XmlDocument();
        configDocument.Load(@"Assets/preconfig.xml");
        GameObject handModelObj = GameObject.Find(configDocument.DocumentElement.SelectSingleNode("handmodelname").InnerText);
        List<string> dataDirectories = new List<string>();

        XmlNode csvDirectoriesNode = configDocument.DocumentElement.SelectSingleNode("pathtocsvdirectories");
        if (csvDirectoriesNode.Attributes.Count > 0 && csvDirectoriesNode.Attributes[0].Value == "array")
        {
            foreach (XmlNode dirNode in csvDirectoriesNode.ChildNodes)
                dataDirectories.Add(dirNode.InnerText);
        }

        pools = new List<GesturePool>();
        Dictionary<string, Tuple<List<string>, List<Vector3>>> poolFilesData = new Dictionary<string, Tuple<List<string>, List<Vector3>>>();
        int distanceSize = 300;

        for (int i = 0; i < dataDirectories.Count; i++)
        {
            List<string> files = Directory.GetFiles(dataDirectories[i]).ToList();
            if (files.Count > 1)
            {
                files = files.OrderBy(item => item.Substring(item.LastIndexOf("#"))).ToList();
                for (int j = 0; j < files.Count; j += 2)
                {
                    if (File.Exists(files[j]) && File.Exists(files[j + 1]))
                    {
                        FileInfo f1 = new FileInfo(files[j]);
                        FileInfo f2 = new FileInfo(files[j + 1]);
                        if (f1.Length > f2.Length)
                        {
                            string key = f1.Name.Substring(f1.Name.IndexOf("_") + 1, +f1.Name.IndexOf("#") - (f1.Name.IndexOf("_") + 1));
                            if (poolFilesData.ContainsKey(key))
                            {
                                poolFilesData[key].Item1.Add(File.ReadAllText(files[j]));
                                // i = row and j = col
                                poolFilesData[key].Item2.Add(new Vector3(j / 2 * distanceSize, i * distanceSize, 0));
                            }
                            else
                                poolFilesData.Add(
                                    key,
                                    new Tuple<List<string>, List<Vector3>>(
                                        new List<string>() { File.ReadAllText(files[j]) },
                                        new List<Vector3>() { new Vector3(j / 2 * distanceSize, i * distanceSize, 0) }));
                        }
                        else
                        {
                            string key = f2.Name.Substring(f2.Name.IndexOf("_") + 1, +f2.Name.IndexOf("#") - (f2.Name.IndexOf("_") + 1));
                            if (poolFilesData.ContainsKey(key))
                            {
                                poolFilesData[key].Item1.Add(File.ReadAllText(files[j + 1]));
                                // i = row and j = col
                                poolFilesData[key].Item2.Add(new Vector3(j / 2 * distanceSize, i * distanceSize, 0));
                            }
                            else
                                poolFilesData.Add(
                                    key,
                                    new Tuple<List<string>, List<Vector3>>(
                                        new List<string>() { File.ReadAllText(files[j + 1]) },
                                        new List<Vector3>() { new Vector3(j / 2 * distanceSize, i * distanceSize, 0) }));
                        }
                    }
                }
            }
        }

        for (int k = 0; k < poolFilesData.Count; k++)
        {
            pools.Add(new GesturePool());
            pools[k].handModel = handModelObj;
            pools[k].tag = poolFilesData.ElementAt(k).Key;

            pools[k].CsvData = new TextAsset[poolFilesData.ElementAt(k).Value.Item1.Count];
            for (int j = 0; j < poolFilesData.ElementAt(k).Value.Item1.Count; j++)
                pools[k].CsvData[j] = new TextAsset(poolFilesData.ElementAt(k).Value.Item1[j]);

            //pools[k].PositionFactor = new Vector3[poolFilesData.ElementAt(k).Value.Item2.Count];
            //poolFilesData.ElementAt(k).Value.Item2.CopyTo(pools[k].PositionFactor);
        }
    }
    #endregion*/

    private void Awake()
    {
        instance = this;
        poolDic = new Dictionary<string, List<Gesture>>();

        foreach (GesturePool pool in pools)
        {
            gestureCount += pool.Length();
        }


        boundingLength = float.Parse(Math.Sqrt(Screen.width * Screen.height / gestureCount).ToString());
        int row = Convert.ToInt32(Screen.width / boundingLength);
        int col = Convert.ToInt32(Screen.height / boundingLength);
        int tempCount = gestureCount;
        List<Vector3> positionFactors = new List<Vector3>();
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (tempCount == 0)
                {
                    break;
                }
                else
                {
                    tempCount -= 1;
                    float maxX = rescaleReference * row / 2;
                    float maxY = rescaleReference * col / 2;
                    positionFactors.Add(new Vector3(maxX - i * rescaleReference, maxY - j * rescaleReference, 0));
                }
            }
        }
        
        for (int i =0; i < pools.ToArray().Length; i++)
        {
            List<Gesture> geturePool = new List<Gesture>();
            for (int j = 0; j < pools[i].CsvData.Length; j++)
            {
                GameObject hand = Instantiate(pools[i].handModel, handModelsContainer);
                //GameObject gestureTag = Instantiate(gestureTagTemplate, gestureTagsContainer);
                //gestureTag.SetActive(false);
                hand.SetActive(false);
                string tag = pools[i].tag + j.ToString();
                Gesture gesture = new Gesture();
                gesture.SetHand(hand);
                gesture.SetTag(tag);
                gesture.SetCSV(pools[i].CsvData[j]);
                gesture.PositionUpdate(positionFactors[tempCount]);
                hand.GetComponent<TagDisplayer>().gesture = gesture;
                tempCount += 1;
                geturePool.Add(gesture);
            }
            poolDic.Add(pools[i].tag, geturePool);
        }

       
        rescaleFactor = boundingLength / rescaleReference;

        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {   
                gesture.prepare();

                // rescale the size of gesture and its tag according to the size of screen. So that all gestures could be displayed on the screen at once.
                gesture.GetHand().transform.localScale = gesture.GetHand().transform.localScale * rescaleFactor;
                //gesture.getTag().transform.localScale = gesture.getTag().transform.localScale * rescaleFactor;
            }
        }
    }
    
    public List<GesturePool> pools;
    public Dictionary<string, List<Gesture>> poolDic;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        //Debug.Log("Before scene loaded");
        //Debug.Log("GestureDisplayer OnLoad called");
        //if (instance.pools == null)
        //    Debug.Log("[L]Pool is unassigned");
        //else
        //    Debug.Log("[L]Pool is assigned");
        //if (instance.poolDic == null)
        //    Debug.Log("[L]PoolDic is unassigned");
        //else
        //    Debug.Log("[L]PoolDic is assigned");
    }

    // Update is called once per frame
    void Update()
    {
        if (animationMode)
        {
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.ConditionCheck();
                }

            }
        }
    }
    
    void FixedUpdate()
    {
        if (animationMode)
        {
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.AnimateInAnimationMode();
                }
            }
        }
    }

    /// <summary>
    /// Draws the bounding box for a gesture
    /// </summary>
    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                Gizmos.color = Color.red;
                if (gesture.GetHand().activeSelf)
                {
                    Gizmos.DrawWireCube(gesture.GetCentroid() + gesture.GetPositionFactor() * rescaleFactor, gesture.GetBoundingBoxSize() * rescaleFactor);
                }
            }
        }
    }
    
    /// <summary>
    /// Displays a category of gestures and their tag.
    /// </summary>
    public void DisplayGesture()
    {
        previousInput = userInputTag.text;
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                gesture.GetHand().SetActive(false);
            }
        }
        foreach (Gesture gesture in poolDic[userInputTag.text])
        {
            gesture.GetHand().SetActive(true);
        }
    }

    /// <summary>
    /// Hides a category of gestures and their tag.
    /// </summary>
    public void HideGesture()
    {
        if(previousInput != null) { 
            foreach (Gesture gesture in poolDic[previousInput])
            {
                gesture.GetHand().SetActive(false);
                //gesture.getTag().SetActive(false);
                //gestureTagDisplayWindow.text = "";
            }
        }
    }

    public void ShowAll()
    {
        if (shown == false)
        {
            showAllButtonText.text = "Hide All";
            shown = true;
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.GetHand().SetActive(true);
                    //gesture.getTag().SetActive(true);
                    //gestureTagDisplayWindow.text = gesture.getTag();
                }
            }
        }
        else
        {
            showAllButtonText.text = "Show All";
            shown = false;
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.GetHand().SetActive(false);
                    //gesture.getTag().SetActive(false);
                    //gestureTagDisplayWindow.text = gesture.getTag();
                }
            }
        }
    }

    public void SwitchMode()
    {
        if (animationMode)
        {
            animationMode = false;
            switchModeButton.text = "Animation Mode";
            slider.gameObject.SetActive(true);
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.Reset();
                    
                }
            }
        }
        else
        {
            animationMode = true;
            switchModeButton.text = "Slider Mode";
            slider.gameObject.SetActive(false);
            foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
            {
                foreach (Gesture gesture in entry.Value)
                {
                    gesture.Reset();
                    
                }
            }
        }
    }
    public void OnValueChanged()
    {
        //Debug.Log(slider.value);
        foreach (KeyValuePair<string, List<Gesture>> entry in poolDic)
        {
            foreach (Gesture gesture in entry.Value)
            {
                gesture.AnimateInSliderMode(slider.maxValue, slider.value);
            }
        }
    }
}
