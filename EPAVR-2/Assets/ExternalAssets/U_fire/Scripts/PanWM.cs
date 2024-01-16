using UnityEngine;

/// <summary>
/// Placing this script on the game object will make that game object pan with mouse movement.
/// </summary>

public class PanWM : MonoBehaviour 
{
	public Vector2 degrees = new Vector2(5f, 3f);

	public float range = 1f;
	//private float t=0f;
	private float shift_k =1f;


	void Start ()
	{

	}

	void Update ()
	{

		if (Input.GetKeyDown(KeyCode.LeftShift))
			shift_k=10f;//movement k - fast
		if (Input.GetKeyUp(KeyCode.LeftShift))
			shift_k=1f;//movement k - slow

		if (Input.GetKey(KeyCode.W)){
			this.transform.position+= (this.transform.forward*shift_k*5f)*Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S)){
			this.transform.position+=(-this.transform.forward*shift_k*5f)*Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D)){
			this.transform.position+=(this.transform.right*shift_k*5f)*Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A)){
			this.transform.position+=(-this.transform.right*shift_k*5f)*Time.deltaTime;
		}
		//if (Input.GetKey(KeyCode.Mouse1))
		//{
			this.transform.Rotate(Vector3.up, (-Screen.width/2 + Input.mousePosition.x)*Time.deltaTime/10f);

			this.transform.Rotate(Vector3.right, (Screen.height/2 - Input.mousePosition.y)*Time.deltaTime/10f);
			transform.rotation = Quaternion.RotateTowards(this.transform.rotation,Quaternion.Euler(new Vector3(this.transform.eulerAngles.x,this.transform.eulerAngles.y,0f)) ,Time.deltaTime*100f);
		//}



	}
}