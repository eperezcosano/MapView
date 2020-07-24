using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PoiScript: MonoBehaviour
{
    [SerializeField]
    public bool drawLine = false;
    [SerializeField]
    public double latObject;
    [SerializeField]
    public double lonObject;
    [SerializeField]
    public double latObject_2;
    [SerializeField]
    public double lonObject_2;
    [SerializeField]
    public string textDescription;
    [SerializeField]
    public string colorLine;
    [SerializeField]
    Toggle toggleNight;


    public bool rotate = false;
    GameObject description;

    void Update()
    {

        if (rotate)
        {
            transform.Rotate(Vector3.forward * (200 * Time.deltaTime));
        }
        else if (SceneManager.GetActiveScene().name.Contains("CameraScene"))
        {
            transform.LookAt(Camera.main.transform.position);
            transform.rotation *= Quaternion.Euler(90, 0, 0);
        }
    }

    void Start()
    {
        description = GameObject.Find("Description");
    }

    public void MapLocation()
    {
        int x = MapHandlerScript.centerTileX;
        int y = MapHandlerScript.centerTileY;
        int zoom = MapHandlerScript.zoom;

        double a = Draw(lonObject, TileToWorldPos(x, y, zoom).X, TileToWorldPos(x + 1, y, zoom).X);
        double b = Draw(latObject, TileToWorldPos(x, y + 1, zoom).Y, TileToWorldPos(x, y, zoom).Y);

        if (drawLine == true)
        {
            double c = Draw(lonObject_2, TileToWorldPos(x, y, zoom).X, TileToWorldPos(x + 1, y, zoom).X);
            double d = Draw(latObject_2, TileToWorldPos(x, y + 1, zoom).Y, TileToWorldPos(x, y, zoom).Y);

            Color newColor;
            switch (colorLine)
            {
                case "DUM":
                    newColor = Color.yellow;
                    break;
                case "AZL":
                    newColor = Color.blue;
                    break;
                case "VR":
                    newColor = Color.red;
                    break;
                case "VM":
                    newColor = Color.green;
                    break;
                case "BUS":
                    newColor = Color.cyan;
                    break;
                default:
                    newColor = Color.white;
                    break;
            }

            LineRenderer MylineRenderer = this.GetComponent<LineRenderer>();
            MylineRenderer.startColor = newColor;
            MylineRenderer.endColor = newColor;
            MylineRenderer.SetPosition(0, new Vector3((float)a - 0.5f, (float)b - 0.5f, -0.1f));
            MylineRenderer.SetPosition(1, new Vector3((float)c - 0.5f, (float)d - 0.5f, -0.1f));
            //MylineRenderer.enabled = true;

        }
        else
        {
            if (SceneManager.GetActiveScene().name.Contains("CameraScene"))
            {

                if (a > -1f && a < 2f && b > -1f && b < 2f)
                {
                    transform.position = new Vector3((float)a - 0.5f, 0.1f, (float)b - 0.5f);

                } else {
                    GetComponent<Renderer>().enabled = false;
                }

                
            }
            else
            {
                transform.position = new Vector3((float)a - 0.5f, (float)b - 0.5f, -0.05f);
            }
        } 
    }

    public struct Point
    {
        public double X;
        public double Y;
    }

    public Point TileToWorldPos(double tile_x, double tile_y, int zoom)
    {
        Point p = new Point();
        double n = System.Math.PI - (2.0 * System.Math.PI * tile_y / System.Math.Pow(2.0, zoom));

        p.X = (tile_x / System.Math.Pow(2.0, zoom) * 360.0) - 180.0;
        p.Y = 180.0 / System.Math.PI * System.Math.Atan(System.Math.Sinh(n));

        return p;
    }

    public double Draw(double targetPos, double minPos, double maxPos)
    {
        return (targetPos - minPos) / (maxPos - minPos);
    }

    public void SetUnpressedColor()
    {
        if (drawLine == false)
        {
            //GetComponent<MeshRenderer>().material.color = Color.red;
            rotate = false;
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    public void OnMouseDown()
    {
        if (drawLine == false)
        {
            string[] tagsLocate =
            {
                "poi",
                "poi_toilets",
                "poi_pubs",
                "poi_libs"
            };
            foreach (string tag in tagsLocate)
            {
                GameObject[] poiList = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject o in poiList)
                {
                    o.SendMessage("SetUnpressedColor");
                }
            }

            //GetComponent<MeshRenderer>().material.color = Color.green;
            rotate = true;
            description.GetComponent<Text>().text = textDescription;
        }
    }
}
