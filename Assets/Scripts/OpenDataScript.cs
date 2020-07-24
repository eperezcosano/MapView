using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Xml;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class OpenDataScript : MonoBehaviour
{
    [SerializeField]
    GameObject prefabPoint;
    [SerializeField]
    GameObject pubPrefab;
    [SerializeField]
    GameObject toiletPrefab;
    [SerializeField]
    GameObject libPrefab;
    [SerializeField]
    GameObject directionalLight;
    [SerializeField]
    Toggle toggleToilets;
    [SerializeField]
    Toggle toggleParkings;
    [SerializeField]
    Toggle toggleNightMode;
    [SerializeField]
    RawImage arrow;

    double latUser;
    double lonUser;
    double offset = 0.02f;

    public void Start()
    {

        if (SceneManager.GetActiveScene().name.Contains("MapView"))
        {
            toggleParkings.onValueChanged.AddListener(delegate {
                TogglePois(toggleParkings.isOn, "poi_parkings", true);
            });
            StartCoroutine(DownloadParkingBcn());
        }

        toggleToilets.onValueChanged.AddListener(delegate {
            TogglePois(toggleToilets.isOn, "poi_toilets", false);
        });
        toggleNightMode.onValueChanged.AddListener(delegate {
            TogglePois(!toggleNightMode.isOn, "poi_libs", false);
            TogglePois(toggleNightMode.isOn, "poi_pubs", false);
            ToggleLight(toggleNightMode);
        });

        StartCoroutine(DownloadToiletsBcn());
        StartCoroutine(SunriseAndSunsetTimes());
    }

    public void ToggleLight(Toggle toggle)
    {
        if (toggle.isOn)
        {
            directionalLight.GetComponent<Light>().color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            directionalLight.GetComponent<Light>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void TogglePois(bool toggle, string tag, bool lines)
    {
        GameObject[] pois = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject poi in pois)
        {
            if (lines)
            {
                poi.GetComponent<LineRenderer>().enabled = toggle;
            }
            else
            {
                poi.GetComponent<Renderer>().enabled = toggle;
            }
        }
    }

    IEnumerator SunriseAndSunsetTimes()
    {
        WWW www = new WWW("https://api.sunrise-sunset.org/json?lat=41.3851&lng=2.1734");
        yield return www;

        JObject obj = JObject.Parse(www.text);

        //Debug.Log("sunrise: " + obj["results"]["sunrise"] + ", sunset: " + obj["results"]["sunset"]);

        DateTime sunrise = DateTime.SpecifyKind(
            DateTime.Parse(obj["results"]["sunrise"].ToString()),
            DateTimeKind.Utc).ToLocalTime();
        //Debug.Log(sunrise);

        DateTime sunset = DateTime.SpecifyKind(
            DateTime.Parse(obj["results"]["sunset"].ToString()),
            DateTimeKind.Utc).ToLocalTime();
        //Debug.Log(sunset);

        GetComponent<Notifications>().sunrise = sunrise.ToShortTimeString();
        GetComponent<Notifications>().sunset = sunset.ToShortTimeString();

        DateTime now = DateTime.Now;

        if ((now > sunrise) && (now < sunset))
        {
            //daylight
            Debug.Log("Daylight");
            toggleNightMode.isOn = false;
        }
        else
        {
            //overnight
            Debug.Log("Overnight");
            toggleNightMode.isOn = true;
        }

        StartCoroutine(DownloadLibrariesBcn());
    }

    IEnumerator DownloadToiletsBcn()
    {
        string URLString = "http://w10.bcn.es/APPS/asiasiacache/peticioXmlAsia?id=204";
        XmlTextReader reader = new XmlTextReader(URLString);
        yield return reader;
        bool save = false;
        string name = "";
        float lat = 0f;
        float lon = 0f;
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element: // The node is an element.
                    //Debug.Log("XmlNodeType.Element: " + reader.Name);

                    if (reader.Name == "nom")
                    {
                        save = true;
                    }

                    while (reader.MoveToNextAttribute()) // Read the attributes.
                    {
                        //Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
                        if (reader.Name == "lat")
                        {
                            lat = float.Parse(reader.Value);
                        }
                        else if (reader.Name == "lon")
                        {
                            lon = float.Parse(reader.Value);
                        }
                    }
                    break;
                case XmlNodeType.Text: //Display the text in each element.
                    //Debug.Log("XmlNodeType.Text:" + reader.Value);

                    if (save == true)
                    {
                        name = reader.Value;
                        save = false;
                    }

                    break;
                case XmlNodeType.EndElement: //Display the end of the element.
                    //Debug.Log("XmlNodeType.EndElement: </" + reader.Name + ">");
                    break;
            }
            if (name != "" && lat != 0f && lon != 0f)
            {
                GameObject poi = Instantiate(toiletPrefab);
                poi.GetComponent<PoiScript>().latObject = lat;
                poi.GetComponent<PoiScript>().lonObject = lon;
                poi.GetComponent<PoiScript>().textDescription = name;
                //poi.GetComponent<MeshRenderer>().material.color = Color.red;
                poi.GetComponent<Renderer>().enabled = false;
                poi.tag = "poi_toilets";
                poi.SendMessage("MapLocation");

                name = "";
                lat = 0f;
                lon = 0f;
            }
        }
        Debug.Log("Toilet loaded");
        

    }

    IEnumerator DownloadParkingBcn()
    {

#if !UNITY_EDITOR
        if (Input.location.status == LocationServiceStatus.Running)
        {
            lonUser = Input.location.lastData.longitude;
            latUser = Input.location.lastData.latitude;
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
        string query = "SELECT * from $26f6ecdd-b0ac-46c4-84f0-a312640913ca$ " +
            "WHERE $LATITUD_I$ < " + (latUser + offset/2) + " AND $LATITUD_I$ > " + (latUser - offset/2) +
            " AND $LONGITUD_I$ < " + (lonUser + offset) + " AND $LONGITUD_I$ > " + (lonUser - offset);
        query = query.Replace('$', '"');
        WWW www = new WWW("https://opendata-ajuntament.barcelona.cat/data/api/action/datastore_search_sql?sql=" + query); //14000
        yield return www;

        JObject obj = JObject.Parse(www.text);
        JArray parkings = (JArray)obj["result"]["records"];

        for (int i = 0; i < parkings.Count; i++)
        {
            JObject parking = (JObject)parkings.GetItem(i);
            float lat_i = (float)parking["LATITUD_I"];
            float lon_i = (float)parking["LONGITUD_I"];
            float lat_f = (float)parking["LATITUD_F"];
            float lon_f = (float)parking["LONGITUD_F"];
            string color = (string)parking["TIPUS_TRAM"];

            GameObject poi = Instantiate(prefabPoint);
            poi.GetComponent<PoiScript>().drawLine = true;
            poi.GetComponent<PoiScript>().latObject = lat_i;
            poi.GetComponent<PoiScript>().lonObject = lon_i;
            poi.GetComponent<PoiScript>().latObject_2 = lat_f;
            poi.GetComponent<PoiScript>().lonObject_2 = lon_f;
            poi.GetComponent<PoiScript>().colorLine = color;
            poi.GetComponent<Renderer>().enabled = false;
            poi.GetComponent<LineRenderer>().enabled = false;
            poi.tag = "poi_parkings";
            poi.SendMessage("MapLocation");

        }

        Debug.Log("Parkings loaded");
        //GameObject.Find("GPS Info").SetActive(true);
        toggleToilets.gameObject.SetActive(true);
        arrow.gameObject.SetActive(true);
        toggleParkings.gameObject.SetActive(true);
        toggleNightMode.gameObject.SetActive(true);
        GameObject.Find("Panel").SetActive(false);
        GameObject.Find("LoadingText").SetActive(false);
        
        

    }

    IEnumerator DownloadLibrariesBcn()
    {
        string query = "SELECT DISTINCT $EQUIPAMENT$, $HORARI_PERIODE_INICI$, $HORARI_PERIODE_FI$, $LONGITUD$, $LATITUD$ " +
            "FROM $cd0220bb-f75d-4737-ac27-339db8650ef6$ WHERE $LATITUD$ IS NOT NULL AND $LONGITUD$ IS NOT NULL";
        query = query.Replace('$', '"');
        WWW www = new WWW("https://opendata-ajuntament.barcelona.cat/data/api/action/datastore_search_sql?sql=" + query);
        yield return www;

        JObject obj = JObject.Parse(www.text);
        JArray libs = (JArray)obj["result"]["records"];

        List<string> libsList = new List<string>();
        for (int i = 0; i < libs.Count; i++)
        {
            try
            {
                JObject lib = (JObject)libs.GetItem(i);
                float lat = (float)lib["LATITUD"];
                float lon = (float)lib["LONGITUD"];
                string name = (string)lib["EQUIPAMENT"];

                if (!libsList.Contains(name))
                {
                    libsList.Add(name);
                    GameObject poi = Instantiate(libPrefab);
                    poi.GetComponent<PoiScript>().latObject = lat;
                    poi.GetComponent<PoiScript>().lonObject = lon;
                    poi.GetComponent<PoiScript>().textDescription = name;
                    poi.tag = "poi_libs";
                    poi.GetComponent<Renderer>().enabled = !toggleNightMode.isOn;
                    //poi.GetComponent<MeshRenderer>().material.color = Color.red;
                    poi.SendMessage("MapLocation");
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Debug.Log("Exception Lib at " + i + "name: " + libs.GetItem(i));
            }


        }

        StartCoroutine(DownloadMusicAndPubsBcn());

    }

    IEnumerator DownloadMusicAndPubsBcn()
    {
        string query = "SELECT DISTINCT $EQUIPAMENT$, $LATITUD$, $LONGITUD$" +
            " FROM $55fa8243-c4f5-4b0d-9fa1-a731a651166e$ WHERE $LATITUD$ IS NOT NULL AND $LONGITUD$ IS NOT NULL";
        query = query.Replace('$', '"');
        WWW www = new WWW("https://opendata-ajuntament.barcelona.cat/data/api/action/datastore_search_sql?sql=" + query);
        yield return www;

        JObject obj = JObject.Parse(www.text);
        JArray pubs = (JArray)obj["result"]["records"];

        List<string> pubsList = new List<string>();
        for (int i = 0; i < pubs.Count; i++)
        {
            try
            {
                JObject pub = (JObject)pubs.GetItem(i);
                float lat = (float)pub["LATITUD"];
                float lon = (float)pub["LONGITUD"];
                string name = (string)pub["EQUIPAMENT"];

                if (!pubsList.Contains(name))
                {
                    pubsList.Add(name);
                    GameObject poi = Instantiate(pubPrefab);
                    poi.GetComponent<PoiScript>().latObject = lat;
                    poi.GetComponent<PoiScript>().lonObject = lon;
                    poi.GetComponent<PoiScript>().textDescription = name;
                    poi.tag = "poi_pubs";
                    poi.GetComponent<Renderer>().enabled = toggleNightMode.isOn;
                    //poi.GetComponent<MeshRenderer>().material.color = Color.red;
                    poi.SendMessage("MapLocation");
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Debug.Log("Exception Pub at " + i);
            }

        }

    }

}
