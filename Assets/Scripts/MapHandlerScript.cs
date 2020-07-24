using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class MapHandlerScript : MonoBehaviour
{
    [SerializeField]
    public GameObject[] tiles;

    public static int centerTileX, centerTileY;
    public static int zoom = 15;

    public Dictionary<string, Texture[]> storedTiles = new Dictionary<string, Texture[]>();

    public void Start()
    {
        HideTiles();
        GameObject.Find("User").SendMessage("MapLocation");
    }

    public void HideTiles()
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<Renderer>().enabled = false;
        }
    }

    //Prints the Map onto de Tiles from de GPS location
    public void DownloadMap() {
#if !UNITY_EDITOR
        if (Input.location.status == LocationServiceStatus.Running)
        {
            WorldToTilePos(Input.location.lastData.longitude, Input.location.lastData.latitude, zoom);
        }
        else
        {
            Debug.Log("GPS Not Activated");
            return;
        }
#else
        //WorldToTilePos(1.9875f, 41.275250f, zoom);    //EETAC
        WorldToTilePos(2.186369f, 41.392957f, zoom);  //Casa
#endif
        StartCoroutine(LoadTiles());

        //Locate each PoI in the map

        string[] tagsLocate =
        {
            "poi",
            "poi_toilets",
            "poi_parkings",
            "poi_pubs",
            "poi_libs"
        };
        foreach (string tag in tagsLocate)
        {
            GameObject[] poiList = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject poi in poiList)
            {
                poi.SendMessage("MapLocation");
            }
        }
    }

    //Calculates the X and Y center coords from Lat and Long coords
    public void WorldToTilePos(float lon, float lat, int zoom)
    {
        double tileX, tileY;
        tileX = (double)((lon + 180.0f) / 360.0f * (1 << zoom));
        tileY = (double)((1.0f - Mathf.Log(Mathf.Tan((float)lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos((float)lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * (1 << zoom));
        centerTileX = Mathf.FloorToInt((float)tileX);
        centerTileY = Mathf.FloorToInt((float)tileY);
    }

    public void Zoom(int deltaZoom) {
        Debug.Log(zoom);
        if ((zoom + deltaZoom) > 19 || (zoom + deltaZoom) < 5)
        {
            return;
        }
        zoom += deltaZoom;
        HideTiles();
        DownloadMap();
    }

    //Download the current map into the tile
    IEnumerator LoadTiles()
    {
        if (storedTiles.TryGetValue(zoom + "_" + centerTileX + "_" + centerTileY, out Texture[] textures))
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<MeshRenderer>().material.mainTexture = textures[i];
                tiles[i].GetComponent<Renderer>().enabled = true;
            }
        }
        else
        {
            Texture[] tmp = new Texture[9];
            //Left to Right, Up to Down strategy.
            for (int i = 0, x = centerTileX - 1, y = centerTileY - 1; i < tiles.Length; i++, x = centerTileX - 1 + (i % 3), y = centerTileY - 1 + (int)Math.Floor((decimal)(i / 3)))
            {
                string uri = "https://a.tile.openstreetmap.org/" + zoom + "/" + x + "/" + y + ".png";
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error + "//" + x + "//" + y + "//" + zoom);
                }
                else
                {
                    Texture texture = DownloadHandlerTexture.GetContent(www);
                    tiles[i].GetComponent<MeshRenderer>().material.mainTexture = texture;
                    tiles[i].GetComponent<Renderer>().enabled = true;
                    tmp[i] = texture;
                }
            }
            try
            {
                storedTiles.Add(zoom + "_" + centerTileX + "_" + centerTileY, tmp);
            }
            catch (Exception)
            {
                
            }
            
        }
    }
}