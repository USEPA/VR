using UnityEngine;
using System;
using System.Collections;

public class Viewer : MonoBehaviour {
	
	GameObject[] objects;
	int activeObjectIdx;
	public float cameraDistance = 15f;
	
	private GameObject activeObject;
	private Quaternion originalRotation;
	public float turnAnglePerSecond = 90.0f;
	private float size;
	
	private int totalVertexCount, totalTriangles, lodCount, meshCount;
	
	public Vector3 cameraOffset = new Vector3(0.0f, 1.0f, 0.0f);
	
	// Use this for initialization
	void Start () {
		objects = GameObject.FindGameObjectsWithTag("ObjectOfInterest");
		
		Array.Sort(objects, delegate(GameObject go1, GameObject go2) {
                    return go1.transform.position.x.CompareTo(go2.transform.position.x);
                  });
		
		SetActiveObject(objects[activeObjectIdx]);
	}
	
	// Update is called once per frame
	void Update () {		
		activeObject.transform.rotation *= Quaternion.AngleAxis(turnAnglePerSecond * Time.deltaTime, activeObject.transform.up);
		
		Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, objects[activeObjectIdx].transform.position + (-Camera.main.transform.forward * size) + cameraOffset, Time.deltaTime * 5f);
	}
	
	void OnGUI()
	{
		float margin = 0.025f * Screen.height;
		float smallMargin = 0.0125f * Screen.height;
		
		float bottomMargin = 0.05f * Screen.height;
		Vector2 buttonSize = new Vector2(0.1f*Screen.width, 0.05f*Screen.height);
		
		// Next
		if( GUI.Button(new Rect(0.5f * Screen.width + margin, Screen.height-bottomMargin-buttonSize.y, buttonSize.x, buttonSize.y), "Next") )
		{
			activeObjectIdx++;
			if( activeObjectIdx >= objects.Length )
				activeObjectIdx = 0;
			
			SetActiveObject(objects[activeObjectIdx]);
		}	
		
		// Previous
		if( GUI.Button(new Rect(0.5f*Screen.width-margin-buttonSize.x, Screen.height-bottomMargin-buttonSize.y, buttonSize.x, buttonSize.y), "Previous") )
		{
			activeObjectIdx--;
			if( activeObjectIdx < 0 )
				activeObjectIdx = objects.Length - 1;
			
			SetActiveObject(objects[activeObjectIdx]);
		}
		
		UpdateMeshInfo();
		
		GUIStyle style = GUI.skin.label;
				
		float height = style.CalcHeight(new GUIContent("ABC"), Screen.width);
		Rect rect = new Rect(0.0f, 0.0f, Screen.width, height);
		style.fontStyle = FontStyle.Bold;
		GUI.Label(rect, new GUIContent(activeObject.name.ToString()));
		style.fontStyle = FontStyle.Normal;
		rect.y = rect.yMax + smallMargin;
		GUI.Label(rect, new GUIContent("LOD Count: " + lodCount.ToString()));
		rect.y = rect.yMax + smallMargin;
		GUI.Label(rect, new GUIContent("Mesh Count (LOD 0): " + meshCount.ToString()));
		rect.y = rect.yMax + smallMargin;
		//GUI.Label(rect, new GUIContent("Total Vertex Count: " + totalVertexCount.ToString()));
		//rect.y = rect.yMax + smallMargin;
		GUI.Label(rect, new GUIContent("Total Tris: " + (totalTriangles / 3)));
	}
	
	void SetActiveObject(GameObject go) {
		if( activeObject )
			activeObject.transform.rotation = originalRotation;
		
		activeObject = go;
		originalRotation = activeObject.transform.rotation;
		
		size = cameraDistance;
		
		Renderer[] r = objects[activeObjectIdx].GetComponentsInChildren<Renderer>();
		if( r.Length > 0 )
		{
			Array.Sort(r, delegate(Renderer r1, Renderer r2) {
                    return r2.bounds.size.magnitude.CompareTo(r1.bounds.size.magnitude);
                  });
			size = r[0].bounds.size.magnitude;
		}
	}
	
	void UpdateMeshInfo() {
		totalTriangles = 0;
		totalVertexCount = 0;
		meshCount = 0;
		
		LODGroup lodGroup = activeObject.GetComponent<LODGroup>();
		lodCount = lodGroup != null ? lodGroup.lodCount : 1;		
		
		
		MeshFilter[] meshFilters = activeObject.GetComponentsInChildren<MeshFilter>();
		if( meshFilters.Length > 0 ) {
			foreach( MeshFilter mf in meshFilters) {				
				Mesh m = mf.mesh;
				if( m != null && mf.GetComponent<Renderer>().isVisible ) {					
					totalVertexCount += m.vertexCount;
					totalTriangles += m.triangles.Length;
					meshCount += 1;
				}
			}
		}
	}
}
