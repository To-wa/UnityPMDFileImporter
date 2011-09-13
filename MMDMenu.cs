using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

using MMD;

public class MMDMenu : EditorWindow
{
	private string filePath = "";
	private string CreateFileName = "";
	
	[MenuItem("Custom/MMD Import")]
	static void Init(){
		MMDMenu window = (MMDMenu)EditorWindow.GetWindow(typeof(MMDMenu));
		window.position = new Rect(Screen.width/2, Screen.height / 2, 400, 500);
	}
	
	void OnGUI()
	{
		GUILayout.Label("Import File Path");
		GUILayout.TextArea(filePath);
		
		
		GUILayout.BeginHorizontal();
		
		/*if(GUILayout.Button("Clear"))
		{
			filePath = "";
		}*/
		if(GUILayout.Button("Select MMD File"))
		{
			filePath = EditorUtility.OpenFilePanel("Import MMD", "", "pmd");
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Label("CreateFileName");
		CreateFileName = GUILayout.TextField(CreateFileName);

		GUILayout.BeginHorizontal();
		
		if(GUILayout.Button("Not Skinned"))
		{
			if(filePath.Length > 0)
			{
				if(CreateFileName.Length <= 0)		CreateFileName = "MMD";
				
				//ReadMMDDataNotSkinned();
				MMD.MMDReader mmd = new MMD.MMDReader();
				mmd.ReadMMDDataNotSkinned(filePath, CreateFileName);
			}
		}
		if(GUILayout.Button("Skinned"))
		{
			if(filePath.Length > 0)
			{
				if(CreateFileName.Length <= 0)		CreateFileName = "MMD";
				
				MMD.MMDReader mmd = new MMD.MMDReader();
				mmd.ReadMMDDataSkinned(filePath, CreateFileName);
				//ReadMMDDataSkinned();
			}
		}
		
		GUILayout.EndHorizontal();
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

