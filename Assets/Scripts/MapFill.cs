using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class MapFill : MonoBehaviour {

	private string prevCenterCoord; 
	private string prevMarkCoord; 
	private int prevImgWidth; 
	private int prevImgHeight; 
	private Image prevMapImage;
	private FileManager fileMgr;

	public void UpdateMap(){
		StartCoroutine (UpdateMapCoroutine());
	}

	private IEnumerator UpdateMapCoroutine(){
		DestroyImmediate (prevMapImage.sprite);
		string Request = "http://static-maps.yandex.ru/1.x/?ll=" + prevCenterCoord +
				"&z=17&l=map&size=" + prevImgWidth.ToString () + "," + prevImgHeight.ToString () +
				"&pt=" + prevMarkCoord + ",pm2pnl";
		WWW yandexStatic = new WWW (Request);
		while (!yandexStatic.isDone)
			yield return yandexStatic;

		if (string.IsNullOrEmpty (yandexStatic.error))
			fileMgr.WriteFileToCache (prevCenterCoord, yandexStatic.bytes);
		Texture2D tex = yandexStatic.texture;

		Sprite sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height),new Vector2(tex.width/2, tex.height/2));

		prevMapImage.sprite = sprite;
	}

	public IEnumerator LoadMap(string CenterCoord, string MarkCoord, int ImgWidth, int ImgHeight, Image MapImage){
		fileMgr = GameObject.FindGameObjectWithTag ("GameController").GetComponent<FileManager>();
		Texture2D tex = fileMgr.GetCachedTexture2D (CenterCoord);

		prevCenterCoord = CenterCoord;
		prevMarkCoord = MarkCoord;
		prevImgWidth = ImgWidth;
		prevImgHeight = ImgHeight;
		prevMapImage = MapImage;

		if (tex == null) {
			string Request = "http://static-maps.yandex.ru/1.x/?ll=" + CenterCoord +
					"&z=17&l=map&size=" + ImgWidth.ToString () + "," + ImgHeight.ToString () +
					"&pt=" + MarkCoord + ",pm2pnl";
			WWW yandexStatic = new WWW (Request);
			while (!yandexStatic.isDone)
				yield return yandexStatic;

			if (string.IsNullOrEmpty (yandexStatic.error))
				fileMgr.WriteFileToCache (CenterCoord, yandexStatic.bytes);
			tex = yandexStatic.texture;
		}
		Sprite sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height),new Vector2(tex.width/2, tex.height/2));

		MapImage.sprite = sprite;
	}
}
