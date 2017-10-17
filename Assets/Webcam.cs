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
					// keep track of target1
					int xValueTarget1 = xy % camWindow.width;
					int yValueTarget1 = xy / camWindow.width;

					totalXTarget1 += xValueTarget1;
					totalYTarget1 += yValueTarget1;
					countTarget1++;
				}
				else if(ColorSqrDistance(target2, data[xy]) < threshold2)
				{
					// keep track of target2
					int xValueTarget2 = xy % camWindow.width;
					int yValueTarget2 = xy / camWindow.width;

					totalXTarget2 += xValueTarget2;
					totalYTarget2 += yValueTarget2;
					countTarget2++;
				}
				else 
				{
					data [xy] = Color.white;
				}
			}
			// sets pixelWindow with the new color specific color data
			pixelWindow.SetPixels32 (data);

			// apply color changes to texture
			pixelWindow.Apply ();

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
			} else if (countTarget1 > 0) {
				
				int averageXTarget1 = totalXTarget1 / countTarget1;
				if (previousX == -1)
					previousX = averageXTarget1;
				else if (averageXTarget1 < previousX)
					Debug.Log ("Moving Right");
				else if (averageXTarget1 > previousX)
					Debug.Log ("Moving left");
				previousX = averageXTarget1;
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
		target1 = new Color (0.075f, 0.745f, 0.631f, 0.000f);
		target2 = new Color (0.910f, 0.922f, 0.453f, 0.000f);
		threshold2 = 0.05f;
		threshold1 = 0.05f;
//		Debug.Log (target1);
//		Debug.Log (target2);
	}

	// compares two colours' squared distance
	float ColorSqrDistance(Color c1, Color c2) 
	{
		return ((c2.r - c1.r) * (c2.r - c1.r) + (c2.b - c1.b) * (c2.b - c1.b) + (c2.g - c1.g) * (c2.g - c1.g));
	}
}