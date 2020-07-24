using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserScript : MonoBehaviour
{
    [SerializeField]
    Text gpsDebug;
    [SerializeField]
    RawImage arrow;
    [SerializeField]
    Canvas canvas;

    double latUser;
    double lonUser;
    float compass;

    public void MapLocation()
    {
        StartCoroutine(Relocation());
    }

#if !UNITY_EDITOR
    private void Update()
    {
        if (Input.compass.enabled)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 180 - Input.compass.trueHeading);
        }
    }
#endif

    IEnumerator Relocation()
    {
        for (; ; )
        {
            int x = MapHandlerScript.centerTileX;
            int y = MapHandlerScript.centerTileY;
            int zoom = MapHandlerScript.zoom;

#if !UNITY_EDITOR
            if (Input.location.status == LocationServiceStatus.Running)
            {
                lonUser = Input.location.lastData.longitude;
                latUser = Input.location.lastData.latitude;
                compass = Input.compass.trueHeading;

            }
            else
            {
                Debug.Log("GPS Not Activated");
                //break;
            }
#else
            //lonUser = 1.987563f;  //EETAC
            //latUser = 41.275f;
            lonUser = 2.186369f;  //Casa
            latUser = 41.392957f;
#endif

            double a = Draw(lonUser, TileToWorldPos(x, y, zoom).X, TileToWorldPos(x + 1, y, zoom).X);
            double b = Draw(latUser, TileToWorldPos(x, y + 1, zoom).Y, TileToWorldPos(x, y, zoom).Y);

            if (SceneManager.GetActiveScene().name.Contains("CameraScene"))
            {
                Vector3 userPoint = new Vector3((float)a - 0.5f, 0.0f, (float)b - 0.5f);
                transform.position = userPoint;
                Camera.main.transform.position = new Vector3((float)a - 0.5f, 0.25f, 0f);
            }
            else
            {
                Vector3 userPoint = new Vector3((float)a - 0.5f, (float)b - 0.5f, 0.0f);
                transform.position = userPoint;

                Vector2 arrowPoint;
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(userPoint);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, null, out arrowPoint);
                arrow.transform.localPosition = arrowPoint;

                gpsDebug.GetComponent<Text>().text =
                      "Lat: " + latUser.ToString()
                    + "\n Lon: " + lonUser.ToString()
                    + "\nCompass: " + compass.ToString() + " Z: " + arrow.transform.rotation.eulerAngles.z;
            }

            if (a < 0f || a > 1f || b < 0f || b > 1f)
            {
                Debug.Log("Center Map");
                GameObject.Find("MapHandler").GetComponent<MapHandlerScript>().DownloadMap();
            }

            yield return new WaitForSeconds(3);

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
}
