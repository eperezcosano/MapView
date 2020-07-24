using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapManager : MonoBehaviour
{

    Vector2 curDist;
    Vector2 prevDist;
    float touchDelta = 0.0f;
    float speedTouch0 = 0.0f;
    float speedTouch1 = 0.0f;

    public int speed = 4;

    public float MINSCALE = 2.0F;
    public float MAXSCALE = 5.0F;
    public float minPinchSpeed = 5.0F;
    public float varianceInDistances = 5.0F;

    //https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input

    private void Start()
    {
        StartCoroutine(TapHandler());
    }

    IEnumerator TapHandler()
    {
        for (; ; )
        {
#if UNITY_EDITOR
            if (Input.mouseScrollDelta.y < 0)
            {
                GameObject objectm = GameObject.Find("MapHandler");
                objectm.SendMessage("Zoom", 1);
                yield return new WaitForSeconds(2);
            }
            else if (Input.mouseScrollDelta.y > 0)
            {
                GameObject objectm = GameObject.Find("MapHandler");
                objectm.SendMessage("Zoom", -1);
                yield return new WaitForSeconds(2);
            }
#else
            if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                Debug.Log("Touch screen");
                curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
                prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
                touchDelta = curDist.magnitude - prevDist.magnitude;
                speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
                speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;


                if ((touchDelta + varianceInDistances <= 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                {
                    GameObject objectm = GameObject.Find("MapHandler");
                    objectm.SendMessage("Zoom", -1);
                }

                if ((touchDelta + varianceInDistances > 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                {
                    GameObject objectm = GameObject.Find("MapHandler");
                    objectm.SendMessage("Zoom", 1);                }

            }
#endif
            yield return new WaitForSeconds(1);
        }
    }
}