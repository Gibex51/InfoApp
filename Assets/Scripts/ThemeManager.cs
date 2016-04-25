using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour {

	public Color titleColor;
	public Color menuColor;
	public Color menuIconColor;

	public Color titleFontColor;
	public Color menuFontColor;
	public Color linkFontColor;

	public Color calendarSelectionColor;

	public Font globalFont;
	public Font linkFont;
	public int normalFontSize;
	public int miniFontSize;
	public int smallFontSize;

	public Image[] Objects_TitleColor;
	public Image[] Objects_MenuBackColor;
	public Image[] Objects_MenuIconColor;

	public Text[] Objects_TitleFontColor;
	public Text[] Objects_MenuNormalFont;
	public Text[] Objects_MenuMiniFont;
	public Text[] Objects_MenuSmallFont;

	public Text[] Objects_LinkSmallFont;
	
	public Camera mainCamera;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < Objects_TitleColor.Length; i++) {
			Objects_TitleColor[i].color = titleColor;
		}

		for (int i = 0; i < Objects_MenuBackColor.Length; i++) {
			Objects_MenuBackColor[i].color = menuColor;
		}

		for (int i = 0; i < Objects_MenuIconColor.Length; i++) {
			Objects_MenuIconColor[i].color = menuIconColor;
		}

		for (int i = 0; i < Objects_TitleFontColor.Length; i++) {
			Objects_TitleFontColor[i].color = titleFontColor;
			Objects_TitleFontColor[i].fontSize = normalFontSize;
			Objects_TitleFontColor[i].font = globalFont;			 
		}
		
		for (int i = 0; i < Objects_MenuNormalFont.Length; i++) {
			Objects_MenuNormalFont[i].color = menuFontColor;
			Objects_MenuNormalFont[i].font = globalFont;
			Objects_MenuNormalFont[i].fontSize = normalFontSize; 
		}

		for (int i = 0; i < Objects_MenuMiniFont.Length; i++) {
			Objects_MenuMiniFont[i].color = menuFontColor;
			Objects_MenuMiniFont[i].font = globalFont;
			Objects_MenuMiniFont[i].fontSize = miniFontSize; 
		}

		for (int i = 0; i < Objects_MenuSmallFont.Length; i++) {
			Objects_MenuSmallFont[i].color = menuFontColor;
			Objects_MenuSmallFont[i].font = globalFont;
			Objects_MenuSmallFont[i].fontSize = smallFontSize; 
		}

		for (int i = 0; i < Objects_LinkSmallFont.Length; i++) {
			Objects_LinkSmallFont[i].color = linkFontColor;
			Objects_LinkSmallFont[i].font = linkFont;
			Objects_LinkSmallFont[i].fontSize = smallFontSize; 
		}

		mainCamera.backgroundColor = titleColor;
	}
}
