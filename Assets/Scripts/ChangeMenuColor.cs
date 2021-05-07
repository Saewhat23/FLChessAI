using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMenuColor : MonoBehaviour
{

    public Color startColor;
    public Color mouseOverColor;
    bool mouseOver = false;
    // Start is called before the first frame update

    public void OnMouseEnter()
    {
        mouseOver = true;
        GetComponent<Renderer>().material.SetColor("_Color", mouseOverColor);
    }
    public void OnMouseExit()
    {
        mouseOver = false;
        GetComponent<Renderer>().material.SetColor("_Color", startColor);

    }

}
