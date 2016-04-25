using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public struct appMenuItem{
	public string localName;
	public string xmlName;
}

public class AppData : MonoBehaviour {
	
	public string dataFileName;
	public string dataSubDir;
	public string cacheSubDir;
	public string downloadPath;
	public string downloadServer;
	public string submitDataUrl;
	public string oneSignalId;
}
