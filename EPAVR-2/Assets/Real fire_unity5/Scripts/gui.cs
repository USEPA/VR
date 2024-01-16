using UnityEngine;
using System.Collections;

public class gui : MonoBehaviour
{
	public Material mat;
	public	Material mat1;
	string namestring = "DAY";

	 void OnGUI()
	{
        if (GUI.Button(new Rect(10, 10, 150, 100), name)){
           if (namestring == "DAY"){
			RenderSettings.skybox = new Material(mat);
				namestring = "NIGHT";
				GameObject.Find("sun").GetComponent<Light>().intensity=0.1f;
			}
			else { 
			RenderSettings.skybox = new Material(mat1);
				namestring = "DAY";
				GameObject.Find("sun").GetComponent<Light>().intensity=0f;
			}
        print (mat);
		}
    }
}
