using AsciiFBXExporter;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField]
    GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            button.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        button.SetActive(false);
    }
}