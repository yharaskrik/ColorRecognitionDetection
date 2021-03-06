using UnityEngine;
using System.Collections;

public class Webcam : MonoBehaviour 
{
	public WebCamDevice     myDevice;       // the webcam
	public WebCamTexture    camFeed;        // texture to process the webcam feed
	public Texture2D        camWindow;      // texture to display webcam feed
	public Texture2D        pixelWindow;    // texture to display color objects

	// target colours to recognize
	public Color            target1;
	public Color            target2;

	public int previousX = -1;
	public float previousDistance = -1f;

	// thresholds for color variance between target color and display color in range [0.0-1.0]
	// recommend 0.1 as starting point (higher value captures more info)
	public float            threshold1 = 0.05f;
	public float            threshold2;

	public Color32[]        data;           // array that stores colour data from the webcam upon update

	// Use this for initialization
	void Start () 
	{
		// in case the machine has multiple devices, this loop searches for available webcam devices and prints out name of webcam being used
		WebCamDevice[] devices = WebCamTexture.devices;
		for (int i = 0; i < devices.Length; i++) 
		{
			Debug.Log (devices [i].name);
			myDevice = devices [i];         // currently set to the last device
		}

		// sets up camFeed as input feed from webcam and displays feed onto camWindo
		camFeed = new WebCamTexture (myDevice.name);
		camFeed.Play ();
		camWindow = new Texture2D (camFeed.width, camFeed.height);
		camWindow.SetPixels32 (camFeed.GetPixels32 ());
		camWindow.Apply();

		// creates pixelWindow to display colour tracking
		// note that actual feed is much bigger than what we display (saves processing)
		pixelWindow = new Texture2D (camFeed.width/8, camFeed.height/8);    // sets the size for pixelWindow

//		target1 = new Color (0.765f, 0.298f, 0.498f, 0.000f);
//		target1 = new Color(0.847f, 0.322f, 0.322f, 0.000f);
//		target1 = new Color (0.259f, 0.978f, 0.978f, 0.000f);
//		target2 = new Color (0.910f, 0.922f, 0.453f, 0.000f);

		//Sleeve colors
		target1 = new Color (0.188f, 0.608f, 0.263f, 0.000f);
		target2 = new Color (0.047f, 0.490f, 0.514f, 0.000f);
		threshold2 = 0.01f;
		threshold1 = 0.04f;
		Debug.Log (target1);

		Debug.Log ("size of camFeed = "     + camFeed.width     + ", " + camFeed.height);
		Debug.Log ("size of camWindow = "   + camWindow.width   + ", " + camWindow.height);
		Debug.Log ("size of pixelWindow = " + pixelWindow.width + ", " + pixelWindow.height);
	}

	// Update is called once per frame
	void Update () 
	{
		pixelWindow = new Texture2D (camFeed.width/8, camFeed.height/8);

		// gets the updated pixel data
		data = camWindow.GetPixels32 ();

		// if the data has been collected
		if(data.Length == 14400)   // use the data size that's shown in inspector
		{
			int totalXTarget1 = 0;
			int totalYTarget1 = 0;
			int totalXTarget2 = 0;
			int totalYTarget2 = 0;

			bool foundRight = false;
			int mostRightYTarget1 = camWindow.height;
			int mostRightXTarget1 = -1;
			int mostRightXYTarget1 = -1;

			bool foundLeft = false;
			int mostLeftYTarget1 = camWindow.height;
			int mostLeftXTarget1 = -1;
			int mostLeftXYTarget1 = -1;

			bool foundRight2 = false;
			int mostRightYTarget2 = -1;
			int mostRightXTarget2 = -1;
			int mostRightXYTarget2 = -1;

			bool foundLeft2 = false;
			int mostLeftYTarget2 = -1;
			int mostLeftXTarget2 = -1;
			int mostLeftXYTarget2 = -1;

			int countTarget1 = 0;
			int countTarget2 = 0;
			// goes through the data array
			for (int xy = 0; xy < data.Length; xy++) 
			{
				
				// checks if pixel colour matches the target colours within the selected threshold
				// if match, do something, else, change the pixel colour to white
				// note: to use one target set the target color to white and threshold to 0
				if (ColorSqrDistance(target1, data[xy]) < threshold1)
				{
					int xValueTarget1 = xy % camWindow.width;
					int yValueTarget1 = xy / camWindow.width;

					totalXTarget1 += xValueTarget1;
					totalYTarget1 += yValueTarget1;
					countTarget1++;

					if (xValueTarget1 > mostRightXTarget1) {
						foundLeft = true;
						mostRightXTarget1 = xValueTarget1;
						mostRightYTarget1 = yValueTarget1;
						mostRightXYTarget1 = xy;
					} else if (xValueTarget1 > mostLeftXTarget1 | yValueTarget1 < mostLeftYTarget1) {
						mostLeftXTarget1 = xValueTarget1;
						mostLeftYTarget1 = mostLeftYTarget1;
						mostLeftXYTarget1 = xy;
						foundRight = true;
					}
						
				}
				else if(ColorSqrDistance(target2, data[xy]) < threshold2)
				{
					// keep track of target2
					int xValueTarget2 = xy % camWindow.width;
					int yValueTarget2 = xy / camWindow.width;

					totalXTarget2 += xValueTarget2;
					totalYTarget2 += yValueTarget2;
					countTarget2++;

					if (xValueTarget2 > mostRightXTarget2 | yValueTarget2 < mostRightYTarget2) {
						mostRightXTarget2 = xValueTarget2;
						mostRightYTarget2 = yValueTarget2;
						mostRightXYTarget2 = xy;
						foundRight2 = true;
					} else if (yValueTarget2 > mostLeftYTarget2) {
						mostLeftXTarget2 = xValueTarget2;
						mostLeftYTarget2 = yValueTarget2;
						mostLeftXYTarget2 = xy;
						foundLeft2 = true;
					}

				}
				else 
				{
					data [xy] = Color.white;
				}
			}

			// Showing the points being used
			if (foundLeft & foundRight) {
				data [mostRightXYTarget1] = Color.black;
				data [mostLeftXYTarget1] = Color.magenta;
			}

			if (foundLeft2 & foundRight2) {
				data [mostRightXYTarget2] = Color.black;
				data [mostLeftXYTarget2] = Color.magenta;
			}

			data [0] = Color.black;
			data [1] = Color.black;
			// sets pixelWindow with the new color specific color data
			pixelWindow.SetPixels32 (data);

			// apply color changes to texture
			pixelWindow.Apply ();

			if (foundLeft & foundLeft2 & mostLeftXTarget1 < mostRightXTarget1 & mostLeftYTarget1 > mostRightYTarget1 & mostLeftXTarget2 < mostRightXTarget2 & mostLeftYTarget2 > mostRightYTarget2) {
				Debug.Log ("Picture frame is shown.");
			}

			if (countTarget1 > 0 & countTarget2 > 0) {
				// tracking pinch

				int averageXTarget1 = totalXTarget1 / countTarget1;
				int averageYTarget1 = totalYTarget1 / countTarget1;
				int averageXTarget2 = totalXTarget2 / countTarget2;
				int averageYTarget2 = totalYTarget2 / countTarget2;

				float distance = Mathf.Sqrt (Mathf.Pow (averageXTarget1 - averageXTarget2, 2) + Mathf.Pow (averageYTarget1 - averageYTarget2, 2));
				if (previousDistance == -1)
					previousDistance = distance;
				else if (distance > previousDistance)
					Debug.Log ("Moving Away:  Zooming out");
				else if (distance < previousDistance)
					Debug.Log ("Moving Towards: Zooming in");
				previousDistance = distance;
				Debug.Log ("Distance: " + previousDistance);
			} else if (countTarget1 > 0) {
				
				int averageXTarget1 = totalXTarget1 / countTarget1;
				if (previousX == -1)
					previousX = averageXTarget1;
				else if (averageXTarget1 < previousX)
					Debug.Log ("Moving Right");
				else if (averageXTarget1 > previousX)
					Debug.Log ("Moving left");
				previousX = averageXTarget1;
				Debug.Log ("X: " + previousX); 
			}
		}

		// update the texture showing webcam feed
		camWindow = new Texture2D (camFeed.width, camFeed.height);
		camWindow.SetPixels32 (camFeed.GetPixels32 ());
		camWindow.Apply ();
		TextureScale.Bilinear (camWindow, camWindow.width/8, camWindow.height/8);   // rescales texture
	}

	void OnGUI()
	{
		// draws textures onto the screen
		GUI.DrawTexture (new Rect (0,   0, (camWindow.width*2), (camWindow.height*2)), camWindow);     
		GUI.DrawTexture (new Rect (400, 0, (camWindow.width*4), (camWindow.height*4)), pixelWindow);


//		Debug.Log (target1);
//		Debug.Log (target2);
	}

	// compares two colours' squared distance
	float ColorSqrDistance(Color c1, Color c2) 
	{
		return ((c2.r - c1.r) * (c2.r - c1.r) + (c2.b - c1.b) * (c2.b - c1.b) + (c2.g - c1.g) * (c2.g - c1.g));
	}
}