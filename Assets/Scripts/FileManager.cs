using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

public class AppRes {
	public string Dir;
	public string FileName;

	public AppRes(string iDir, string iFileName){
		Dir = iDir;
		FileName = iFileName;
	}
}

public class FileManager : MonoBehaviour {

	private string savePath = "";
	private string cachePath = "";
	private bool dataUpdated = false;
	private bool resourcesUpdated = false;
	private int filesDownloading = 0;
	private int filesDownloaded = 0;

	public AppData appData;

	void Start(){
		string prepath = Application.persistentDataPath + appData.dataSubDir;
		if (!Directory.Exists (prepath))
			Directory.CreateDirectory (prepath);
		savePath = Application.persistentDataPath + appData.dataSubDir;
		cachePath = Application.persistentDataPath + appData.cacheSubDir;
		if (!Directory.Exists (cachePath)) {
			Directory.CreateDirectory (cachePath);
		}
		StartCoroutine(downloadAppData ());
	}

	public bool IsDataUpdated(){
		return dataUpdated;
	}

	public bool IsResourcesUpdated(){
		return resourcesUpdated;
	}

	public void ClearCacheDirectory(){
		string[] fileList = Directory.GetFiles (cachePath);
		for (int i = 0; i < fileList.Length; i++)
			File.Delete (fileList [i]);
	}

	public Sprite LoadSpriteFromRes(AppRes appRes){
		string resFileName = savePath + appRes.Dir + "/" + appRes.FileName;
		if (!File.Exists (resFileName))
			return null;
		Texture2D tex = new Texture2D (2,2, TextureFormat.RGBA32, true);
		tex.filterMode = FilterMode.Trilinear;
		tex.anisoLevel = 16;
		tex.Apply ();
		tex.LoadImage (File.ReadAllBytes (resFileName));
		return Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (tex.width / 2, tex.height / 2));
	}

	public string ReadXML(){
		if (System.IO.File.Exists (savePath + appData.dataFileName)) {
			return System.IO.File.ReadAllText (savePath + appData.dataFileName);
		}
		PlayerPrefs.SetString ("SyncDate", "-");
		TextAsset xmlDataAsset = Resources.Load (appData.dataFileName) as TextAsset;
		return xmlDataAsset.text;
	}

	public Texture2D GetCachedTexture2D(string fileName){
		if (!File.Exists (cachePath + fileName)) return null;
		Texture2D tex = new Texture2D (2,2, TextureFormat.RGBA32, false);
		tex.LoadImage (File.ReadAllBytes (cachePath + fileName));
		return tex;
	}

	public void WriteFileToCache(string fileName, byte[] buffer){
		BinaryWriter dataWriter = new BinaryWriter (File.Open (cachePath + fileName, FileMode.OpenOrCreate));
		dataWriter.Write (buffer);
		dataWriter.Close();
	}

	private IEnumerator downloadFile(string url, string outFileName){
		float startDownloadTime = Time.time;
		bool timeOut = false;
		WWW fileData = new WWW (url);
		while (!fileData.isDone) {
			Debug.Log (Time.time - startDownloadTime);
			if (Time.time - startDownloadTime > 5) {
				timeOut = true;
				break;
			}
			yield return fileData;
		}
		if (string.IsNullOrEmpty (fileData.error) && (!timeOut)) {
			string saveFile = savePath + outFileName;
			if (File.Exists (saveFile))
				File.Delete (saveFile);
			BinaryWriter dataWriter = new BinaryWriter (File.Open (saveFile, FileMode.OpenOrCreate));
			dataWriter.Write (fileData.bytes);
			dataWriter.Close ();
			Debug.Log ("File Downloaded: "+saveFile);
			filesDownloaded++;
		} else if (timeOut) {
			Debug.Log ("Error: Timeout" );
		} else {
			Debug.Log ("Error: " + fileData.error);
		}
		filesDownloading--;
	}

	public void skipDownloadResources(){
		resourcesUpdated = true;
	}

	public IEnumerator downloadResources(List<AppRes> resList, bool skip){
		resourcesUpdated = false;
		if (savePath == "") yield break;

		filesDownloading = resList.Count;
		filesDownloaded = 0;

		for (int i = 0; i < resList.Count; i++) {
			string currPath = savePath + resList [i].Dir;
			if (!Directory.Exists (currPath))
				Directory.CreateDirectory (currPath);
			if ((!skip) || (!File.Exists (currPath + "/" + resList [i].FileName))){
				StartCoroutine (downloadFile (appData.downloadServer + resList [i].Dir + "/" + resList [i].FileName, resList [i].Dir + "/" + resList [i].FileName));
			} else {
				filesDownloading--;
			    Debug.Log ("Skip File: " + currPath + "/" + resList [i].FileName);
			}
		}

		while (filesDownloading > 0) {
			yield return new WaitForSeconds (0.1f); 
		}
		resourcesUpdated = true;
	}

	public IEnumerator downloadAppData(){
		dataUpdated = false;
		if (savePath == "") yield break;

		filesDownloading = 1;
		filesDownloaded = 0;
		StartCoroutine(downloadFile (appData.downloadPath, appData.dataFileName));

		while (filesDownloading > 0) {
			yield return new WaitForSeconds (0.1f); 
		}

		if (filesDownloaded == 1) {
			PlayerPrefs.SetString("SyncDate", System.DateTime.Now.ToString("dd.MM.yy")); 
		}

		dataUpdated = true;
	}

}
