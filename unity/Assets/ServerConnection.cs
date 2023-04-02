using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class ServerConnection : MonoBehaviour
{
    public string serverURL;

    private void Start()
    {
        try {
        StartCoroutine(GetTerrainData());
        }
        catch (Exception ex)
        {
            Debug.LogError($"SimplexNoise.Generate Error: {ex.Message}");
        }
    }

    private IEnumerator GetTerrainData()
    {
        Debug.Log("Server URL: " + serverURL);
        using (UnityWebRequest www = UnityWebRequest.Get(serverURL))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string terrainDataJSON = www.downloadHandler.text;
                var terrainData = JSON.Parse(terrainDataJSON);

                if (terrainData != null)
                {
                    Debug.Log("Terrain: " + terrainData["terrain"]);

                    if (terrainData["locationTerrain"] != null && terrainData["locationTerrain"]["terrain"] != null)
                    {
                        Debug.Log("trees: " + terrainData["locationTerrain"]["terrain"]["trees"]);
                        Debug.Log("bushes: " + terrainData["locationTerrain"]["terrain"]["bushes"]);
                        Debug.Log("rocks: " + terrainData["locationTerrain"]["terrain"]["rocks"]);

                        var descriptions = terrainData["locationTerrain"]["terrain"]["description"];
                        if (descriptions != null)
                        {
                            for (int i = 0; i < descriptions.Count; i++)
                            {
                                var zoneDescriptions = descriptions[i];
                                for (int j = 0; j < zoneDescriptions.Count; j++)
                                {
                                    var zoneDescription = zoneDescriptions[j];
                                    Debug.Log("LocationDescription: " + zoneDescription["zoneName"]);
                                    Debug.Log("LocationDescription: " + zoneDescription["zoneDescription"]);
                                }
                            }
                        }
                    }

                    // Parse locationMap
                    var locationMap = terrainData["locationMap"];
                    // Assuming locationMap contains the JSON data
                    JSONNode json = JSON.Parse(locationMap.ToString());

                    LocationMapData locationMapData = new LocationMapData();

                    if (json != null)
                    {
                        var grad3 = json["grad3"].AsArray;
                        var p = json["p"].AsArray;
                        var perm = json["perm"].AsArray;
                        var gradP = json["gradP"].AsArray;

                        Debug.Log("grad3.Count: " + grad3.Count);
                        Debug.Log("p.Count: " + p.Count);
                        Debug.Log("perm.Count: " + perm.Count);
                        Debug.Log("gradP.Count: " + gradP.Count);

                        locationMapData.Grad3 = new Vector3[grad3.Count];
                        locationMapData.P = new int[p.Count];
                        locationMapData.Perm = new int[perm.Count];
                        locationMapData.GradP = new Vector3[gradP.Count];

                        for (int i = 0; i < grad3.Count; i++)
                        {
                            float x = grad3[i]["x"].AsFloat;
                            float y = grad3[i]["y"].AsFloat;
                            float z = grad3[i]["z"].AsFloat;
                            locationMapData.Grad3[i] = new Vector3(x, y, z);
                        }

                        for (int i = 0; i < p.Count; i++)
                        {
                            locationMapData.P[i] = p[i].AsInt;
                        }

                        for (int i = 0; i < perm.Count; i++)
                        {
                            locationMapData.Perm[i] = perm[i].AsInt;
                        }

                        for (int i = 0; i < gradP.Count; i++)
                        {
                            float x = gradP[i]["x"].AsFloat;
                            float y = gradP[i]["y"].AsFloat;
                            float z = gradP[i]["z"].AsFloat;
                            locationMapData.GradP[i] = new Vector3(x, y, z);
                        }
                    }

                    // Get the ProceduralMapGenerator component attached to the Terrain game object
                    ProceduralMapGenerator mapGenerator = GameObject.Find("Terrain").GetComponent<ProceduralMapGenerator>();

                    Debug.Log(mapGenerator);

                    // Call GenerateMap method passing in the noise map data
                    Debug.Log("Calling Map Generator");
                    mapGenerator.GenerateMap(locationMapData);

                    // Access the NPCs
                    var npcs = terrainData["npcs"];

                    // Access the Quests
                    var quests = terrainData["quests"];

                    // Iterate through NPCs
                    for (int i = 0; i < npcs.Count; i++)
                    {
                        for (int j = 0; j < npcs[i].Count; j++)
                        {
                            for (int k = 0; k < npcs[i][j].Count; k++)
                            {
                                string npcName = npcs[i][j][k]["name"];
                                int npcCount = npcs[i][j][k]["count"];
                                string npcType = npcs[i][j][k]["type"];
                                string npcRarity = npcs[i][j][k]["rarity"];
                                int npcExp = npcs[i][j][k]["exp"];

                                Debug.Log("NPC Name: " + npcName);
                                Debug.Log("NPC Count: " + npcCount);
                                Debug.Log("NPC Type: " + npcType);
                                Debug.Log("NPC Rarity: " + npcRarity);
                                Debug.Log("NPC Exp: " + npcExp);

                                var dropTable = npcs[i][j][k]["drop_table"];
                                for (int l = 0; l < dropTable.Count; l++)
                                {
                                    foreach (var key in dropTable[l].Keys)
                                    {
                                        var item = dropTable[l][key];
                                        Debug.Log("Item Category: " + key);
                                        foreach (var itemKey in item.Keys)
                                        {
                                            Debug.Log("Item " + itemKey + ": " + item[itemKey]);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Iterate through Quests
                    for (int i = 0; i < quests.Count; i++)
                    {
                    string questName = quests[i]["name"];
                    string questDescription = quests[i]["description"];
                    string questType = quests[i]["type"];
                    string questReward = quests[i]["reward"];
                        Debug.Log("Quest Name: " + questName);
                        Debug.Log("Quest Description: " + questDescription);
                        Debug.Log("Quest Type: " + questType);
                        Debug.Log("Quest Reward: " + questReward);

                        var objectives = quests[i]["objectives"];
                        for (int j = 0; j < objectives.Count; j++)
                        {
                            string objectiveName = objectives[j]["name"];
                            string objectiveDescription = objectives[j]["description"];
                            string objectiveType = objectives[j]["type"];
                            int objectiveCount = objectives[j]["count"];

                            Debug.Log("Objective Name: " + objectiveName);
                            Debug.Log("Objective Description: " + objectiveDescription);
                            Debug.Log("Objective Type: " + objectiveType);
                            Debug.Log("Objective Count: " + objectiveCount);
                        }
                    }
                }
                else
                {
                    Debug.LogError("TerrainData is null");
                }
            }
        }
    }
}

                