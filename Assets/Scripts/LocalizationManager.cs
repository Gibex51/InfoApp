using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SmartLocalization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Globalization;
using System.Text;
using UnityEngine.Analytics;


public interface uItem {}

public class callInfoItem : uItem {
	public string Text;
	public string callNumber;
	public float Height;

	public callInfoItem(string iText, string iCallNumber, float iHeight){
		Text = iText;
		callNumber = iCallNumber;
		Height = iHeight;
	}
}

public class inetLinkItem : uItem {
	public string Text;
	public string Link;
	public float Height;
	
	public inetLinkItem(string iText, string iLink, float iHeight){
		Text = iText;
		Link = iLink;
		Height = iHeight;
	}
}

public class textInfoItem : uItem {
	public string Text;
	public float Height;

	public textInfoItem(string iText, float iHeight){
		Text = iText;
		Height = iHeight;
	}
}

public class calendarItem : uItem {
	public string MonthName;
	public int FirstDayPos;
	public int DaysInMonth;
	public List<int> SelectedDays;
	public float Height;
	
	public calendarItem(string iMonthName, int iFirstDayPos, int iDayInMonth, List<int> iSelectedDays, float iHeight){
		MonthName = iMonthName;
		FirstDayPos = iFirstDayPos;
		DaysInMonth = iDayInMonth;
		SelectedDays = iSelectedDays;
		Height = iHeight;
	}
}

public class tableCell{
	public string value;
	public Color color;
}

public class tableItem : uItem {
	public List<List<tableCell>> Table;
	public float rowHeight;

	public tableItem(List<List<tableCell>> iTable, float iRowHeight){
		Table = iTable;
		rowHeight = iRowHeight;
	}
}

public class subMenuItemExtra : uItem {
	public string Text;
	public string ExtraText;
	public float Height;
	public List <uItem> Items;

	public subMenuItemExtra(string iText, string iExtraText, float iHeight){
		Text = iText;
		ExtraText = iExtraText;
		Height = iHeight;
	}
}

public class subMenuItemSingle : uItem {
	public string Text;
	public float Height;
	public List <uItem> Items;
	
	public subMenuItemSingle(string iText, float iHeight){
		Text = iText;
		Height = iHeight;
	}
}

public class mainMenuItem : uItem {
	public string Text;
	public List <uItem> Items;
	public float Height;
	public int IconId;

	public mainMenuItem(string iText, float iHeight, int iIconId){
		Text = iText;
		Height = iHeight;
		IconId = iIconId;
	}
}

public class mapItem : uItem {
	public string CenterCoord;
	public string MarkCoord;
	public float Height;
	
	public mapItem(string iCenterCoord, string iMarkCoord, float iHeight){
		CenterCoord = iCenterCoord;
		MarkCoord = iMarkCoord;
		Height = iHeight;
	}
}

public class MenuData {
	public List <uItem> Items;
}

public class LocalizationManager : MonoBehaviour {

	private LanguageManager langManager;

	public ThemeManager themeManager;
	public FileManager fileManager;
	public MenuBuilder menuBuilder;

	public Text mainTitle;
	public GameObject updateText;
	public GameObject syncText;
	public Text infoText;

	public GameObject syncWindow;

	public MenuData menuData;
	public AppData appData;
	
	private float itemHeightMainMenu = 0.15f;
	private float itemHeightCall = 0.14f;
	private float itemHeightSingle = 0.20f;
	private float itemHeightExtra = 0.20f;
	private float itemHeightCalendar = 0.5f;
	private float itemHeightText = 0.14f;
	private float itemHeightMap = 0.5f;
	private float itemHeightLink = 0.08f;
	private float rowHeightTable = 0.08f;
	
	private string[] month_names = {"<Ошибка>", "Январь","Февраль","Март","Апрель","Май","Июнь","Июль","Август","Сентябрь","Октябрь","Ноябрь","Декабрь"};

	private XmlDocument downloadedXML = new XmlDocument();
	private List<AppRes> downloadedResList = new List<AppRes>();

	// Use this for initialization
	void Start () {
		OneSignal.Init(appData.oneSignalId, "920809726025", HandleNotification);

		menuData = new MenuData ();

		langManager = LanguageManager.Instance;
		LanguageManager.Instance.OnChangeLanguage += OnLanguageChanged;
		switch (Application.systemLanguage) {
			case SystemLanguage.Russian: langManager.ChangeLanguage("ru"); break;
			case SystemLanguage.Finnish: langManager.ChangeLanguage("fi"); break;
			case SystemLanguage.English: langManager.ChangeLanguage("en"); break;
			default: langManager.ChangeLanguage("ru"); break;
		}

		//StartCoroutine (SendDeviceInfo ());

		StartCoroutine (waitForDownloadAppData ());
		StartCoroutine (waitForDownloadResourceData ());
	}

	// Gets called when the player opens the notification.
	private static void HandleNotification(string message, Dictionary<string, object> additionalData, bool isActive) {

	}

	/*private IEnumerator SendDeviceInfo(){
		//float wInch = Screen.width / Screen.dpi;
		//float hInch = Screen.height / Screen.dpi;
		//Debug.Log ("Diagonal: " + Mathf.Sqrt (wInch * wInch + hInch * hInch));

		string infoString = SystemInfo.deviceUniqueIdentifier + ";" + SystemInfo.deviceModel + ";" + SystemInfo.operatingSystem;
		byte[] infoBytes = Encoding.UTF8.GetBytes (infoString);
		string encodedInfo = Convert.ToBase64String (infoBytes);

		WWW sendData = new WWW (appData.downloadServer + appData.submitDataUrl + "?data=" + encodedInfo);

		while (!sendData.isDone)
			yield return sendData;

		yield break;
	}*/

	private IEnumerator waitForDownloadAppData(){
		while (!fileManager.IsDataUpdated())
			yield return 0;
		Debug.Log ("Data Updated");
		downloadedXML.LoadXml (fileManager.ReadXML ());
		UpdateResources ();
	}

	private IEnumerator waitForDownloadResourceData(){
		while (!fileManager.IsResourcesUpdated())
			yield return 0;
		Debug.Log ("Resources Updated");
		UpdateMenuData ();
		menuBuilder.InitializeMenu ();
	}

	//----------------------------------------------------------------
	//----------------------------------------------------------------

	private mainMenuItem AddMainMenuItem(List<uItem> ItemList, string Text, float Height, int IconId) {
		mainMenuItem new_item = new mainMenuItem (Text, Height, IconId);
		ItemList.Add (new_item);
		return new_item;
	}

	private subMenuItemExtra AddSubMenuItemExtra(List<uItem> ItemList, string Text, string SubText, float Height) {
		subMenuItemExtra new_item = new subMenuItemExtra (Text, SubText, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private subMenuItemSingle AddSubMenuItemSingle(List<uItem> ItemList, string Text, float Height) {
		subMenuItemSingle new_item = new subMenuItemSingle (Text, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private mapItem AddMapItem(List<uItem> ItemList, string CenterCoord, string MarkCoord, float Height) {
		mapItem new_item = new mapItem (CenterCoord, MarkCoord, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private calendarItem AddCalendarItem(List<uItem> ItemList, string MonthName, int FirstDayPos, 
	                                     int DayInMonth, List<int> SelectedDays, float Height) {
		calendarItem new_item = new calendarItem (MonthName, FirstDayPos, DayInMonth, SelectedDays, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private callInfoItem AddCallInfoItem(List<uItem> ItemList, string Text, string CallNumber, float Height) {
		callInfoItem new_item = new callInfoItem (Text, CallNumber, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private inetLinkItem AddInetLinkItem(List<uItem> ItemList, string Text, string Link, float Height) {
		inetLinkItem new_item = new inetLinkItem (Text, Link, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private textInfoItem AddTextInfoItem(List<uItem> ItemList, string Text, float Height) {
		textInfoItem new_item = new textInfoItem (Text, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	private tableItem AddTableItem(List<uItem> ItemList, List<List<tableCell>> Table, float Height){
		tableItem new_item = new tableItem (Table, Height);
		ItemList.Add (new_item);
		return new_item;
	}

	//----------------------------------------------------------------
	//----------------------------------------------------------------

	private string GetTextFromAttribute(XmlNode Node, string AttrName){
		XmlNode textNode = Node.Attributes.GetNamedItem(AttrName);
		string itemText = "";
		if (textNode != null) itemText = textNode.Value.Replace("\\n", "\n");
		return itemText;
	}

	private string GetLocalizedTextFromAttribute(XmlNode Node, string AttrName){
		XmlNode textNode = Node.Attributes.GetNamedItem(AttrName);
		string itemText = "";
		if (textNode != null) {
			itemText = textNode.Value.Replace ("\\n", "\n");
		} else {
			textNode = Node.Attributes.GetNamedItem(AttrName + "_" + langManager.LoadedLanguage);
			if (textNode != null)
				itemText = textNode.Value.Replace ("\\n", "\n");
		}
		return itemText;
	}

	private Color GetColorFromAttribute(XmlNode Node, string AttrName, Color defaultColor){
		XmlNode colorNode = Node.Attributes.GetNamedItem(AttrName);
		Color itemColor = defaultColor;
		string itemText = "";
		if (colorNode != null) {
			itemText = colorNode.Value;
			byte[] iColors = BitConverter.GetBytes (Convert.ToInt32 (itemText, 16));
			itemColor.r = iColors [3] / 255.0f;
			itemColor.g = iColors [2] / 255.0f;
			itemColor.b = iColors [1] / 255.0f;
			itemColor.a = iColors [0] / 255.0f;
		}
		return itemColor;
	}

	private float GetFloatFromAttribute(XmlNode Node, string AttrName, float DefaultValue){
		XmlNode floatNode = Node.Attributes.GetNamedItem(AttrName);
		float itemValue = DefaultValue;
		if (floatNode != null) itemValue = Convert.ToSingle (floatNode.Value, CultureInfo.InvariantCulture.NumberFormat);
		return itemValue;
	}

	private int GetIntFromAttribute(XmlNode Node, string AttrName, int DefaultValue){
		XmlNode intNode = Node.Attributes.GetNamedItem(AttrName);
		int itemValue = DefaultValue;
		if (intNode != null) itemValue = Convert.ToInt32 (intNode.Value);
		return itemValue;
	}

	//----------------------------------------------------------------
	//----------------------------------------------------------------

	private void ParseNode(XmlNode Node, uItem ParentItem){
		List<uItem> ParentItems = null;
		if (ParentItem is mainMenuItem) ParentItems = (ParentItem as mainMenuItem).Items;
		if (ParentItem is subMenuItemSingle) ParentItems = (ParentItem as subMenuItemSingle).Items;
		if (ParentItem is subMenuItemExtra) ParentItems = (ParentItem as subMenuItemExtra).Items;
		if ((ParentItems == null) || (Node == null)) return;

		for (int childInd = 0; childInd < Node.ChildNodes.Count; childInd++) {
			XmlNode childNode = Node.ChildNodes[childInd];
			switch (childNode.Name ){
			case "subitem":{
				string itemText = GetLocalizedTextFromAttribute(childNode, "text");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightSingle);
				subMenuItemSingle subItem = AddSubMenuItemSingle (ParentItems, itemText, itemHeight);
				subItem.Items = new List<uItem> ();
				ParseNode(childNode, subItem);
			}break;
			case "subitemextra":{
				string itemText = GetLocalizedTextFromAttribute(childNode, "text");
				string itemTextExtra = GetLocalizedTextFromAttribute(childNode, "extratext");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightExtra);
				subMenuItemExtra subItem = AddSubMenuItemExtra (ParentItems, itemText, itemTextExtra, itemHeight);
				subItem.Items = new List<uItem> ();
				ParseNode(childNode, subItem);
			}break;
			case "textitem":{
				string itemText = GetLocalizedTextFromAttribute(childNode, "text");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightText);
				AddTextInfoItem(ParentItems, itemText, itemHeight);
			}break;
			case "mapitem":{
				string CenterCoord = GetTextFromAttribute(childNode, "center");
				string MarkCoord = GetTextFromAttribute(childNode, "mark");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightMap);
				AddMapItem(ParentItems, CenterCoord, MarkCoord, itemHeight);
			}break;
			case "phone":{
				string itemText = GetLocalizedTextFromAttribute(childNode, "text");
				string callNumber = GetTextFromAttribute(childNode, "callnum");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightCall);
				AddCallInfoItem(ParentItems, itemText, callNumber , itemHeight);
			}break;
			case "link":{
				string itemText = GetLocalizedTextFromAttribute(childNode, "text");
				string inetLink = GetLocalizedTextFromAttribute(childNode, "link");
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightLink);
				AddInetLinkItem(ParentItems, itemText, inetLink , itemHeight);
			}break;
			case "table":{
				float rowHeight = GetFloatFromAttribute(childNode, "rowheight", rowHeightTable);

				List<List<tableCell>> table = new List<List<tableCell>>();
							
				for (int rowInd = 0; rowInd < childNode.ChildNodes.Count; rowInd++) {
					XmlNode rowNode = childNode.ChildNodes[rowInd];
					if (rowNode.Name != "row") continue;
					List<tableCell> tableRow = new List<tableCell>();
					Color rowColor = GetColorFromAttribute (rowNode, "rowcolor", themeManager.menuColor);
					for (int col = rowNode.Attributes.Count - 1; col >= 0 ; col--){
						string coltext = GetLocalizedTextFromAttribute(rowNode, "c" + (col+1).ToString ());
						if (coltext != ""){
							tableCell Cell = new tableCell();
							Cell.value = coltext;
							Cell.color = rowColor;
							tableRow.Add (Cell);
						}
					}
					table.Add (tableRow);
				}
				
				AddTableItem (ParentItems, table, rowHeight);	
			}break;
			case "calendar":{
				float itemHeight = GetFloatFromAttribute(childNode, "height", itemHeightCalendar);
				int monthId = Convert.ToInt32(GetTextFromAttribute (childNode,"num"));
				int monthDays = Convert.ToInt32(GetTextFromAttribute (childNode,"days"));
				int monthDayPos = Convert.ToInt32(GetTextFromAttribute (childNode,"daypos"));
				if ((monthId < 1) || (monthId > 12)) monthId = 0;
				if (monthDays < 28) monthDays = 28;
				if (monthDays > 31) monthDays = 31;
				if (monthDayPos < 0) monthDayPos = 0; 
				if (monthDayPos > 6) monthDayPos = 6;
				
				List<int> days = new List<int>();
				for (int dayInd = 0; dayInd < childNode.ChildNodes.Count; dayInd++){
					XmlNode dayNode = childNode.ChildNodes[dayInd];
					if (dayNode.Name != "day") continue;
					
					int dayId = Convert.ToInt32 (dayNode.InnerText);
					if ((dayId >= 1) && (dayId <= monthDays)){
						days.Add (dayId);
					}
				}
				
				AddCalendarItem (ParentItems, month_names[monthId], monthDayPos, monthDays, days, itemHeight);		
			}break;
			}
		}
	}

	void ReadVersion(XmlNode infoNode){
		if (infoNode == null) return;

		float appVer = GetFloatFromAttribute (infoNode, "app_version", 0);
		float dataVer = GetFloatFromAttribute (infoNode, "data_version", 0);
		float currAppVer = Convert.ToSingle(Application.version, CultureInfo.InvariantCulture.NumberFormat);

		float oldDataVer = PlayerPrefs.GetFloat ("DataVer", 0);
		if (oldDataVer < dataVer) fileManager.ClearCacheDirectory ();
		PlayerPrefs.SetFloat ("DataVer", dataVer);
		PlayerPrefs.Save();

		if (currAppVer < appVer) {
			updateText.SetActive(true);
		}

		infoText.text = "App: " + currAppVer.ToString () + "." + dataVer + "  " + 
						langManager.GetTextValue("info.sync") + " " + PlayerPrefs.GetString ("SyncDate");
	}

	void LoadResources(XmlNode resNode){
		if (resNode == null) {
			fileManager.skipDownloadResources();
			return;
		}

		int resVer = GetIntFromAttribute (resNode, "rev", 0);
		int oldResVer = PlayerPrefs.GetInt ("ResVer", 0);

		for (int childInd = 0; childInd < resNode.ChildNodes.Count; childInd++) {
			XmlNode childNode = resNode.ChildNodes[childInd];
			switch (childNode.Name){
				case "menuicon":{
					AppRes fn = new AppRes("icons", GetTextFromAttribute(childNode, "filename"));
					if (fn.FileName != "")
						downloadedResList.Add (fn);
				}break;
			}
		}

		StartCoroutine (fileManager.downloadResources (downloadedResList, oldResVer >= resVer));
		PlayerPrefs.SetInt ("ResVer", resVer);
	}

	private void UpdateResources(){
		XmlNode rootNode = GetRootNode ();
		ReadVersion (FindNode (rootNode, "fileinfo"));
		LoadResources (FindNode (rootNode, "resources"));
	}

	private void SetResources() {
		int resCount = downloadedResList.Count;
		List<Sprite> sprites = new List<Sprite>();
		for (int resInd = 0; resInd < resCount; resInd++){
			Sprite resSprite = fileManager.LoadSpriteFromRes(downloadedResList[resInd]);
			if (resSprite == null) break;
			sprites.Add (resSprite);
		}
		if ((sprites.Count == resCount) && (resCount > 0)) {
			Debug.Log ("Sprites Replaced");
			menuBuilder.menuSprites = sprites.ToArray ();
		} else {
			Debug.Log ("Original Sprites");
		}
	}

	private XmlNode GetRootNode (){
		for (int ind = 0; ind < downloadedXML.ChildNodes.Count; ind++) {
			if (downloadedXML.ChildNodes[ind].Name == "appdata")
				return downloadedXML.ChildNodes[ind];
		}
		return null;
	}

	private XmlNode FindNode (XmlNode parentNode, string childNodeName){
		for (int ind = 0; ind < parentNode.ChildNodes.Count; ind++) {
			if (parentNode.ChildNodes[ind].Name == childNodeName)
				return parentNode.ChildNodes[ind];
		}
		return null;
	}

	private void UpdateMenuData(){
		SetResources ();

		menuData.Items = new List<uItem> ();

		XmlNode appData = GetRootNode ();
		for (int itemInd = 0; itemInd < appData.ChildNodes.Count; itemInd++) {
			if (appData.ChildNodes[itemInd].Name == "rootitem"){
				string idString = GetTextFromAttribute(appData.ChildNodes[itemInd], "id");
				string menuString = "";
				if (idString == ""){
					menuString = GetLocalizedTextFromAttribute(appData.ChildNodes[itemInd], "text");
				}else{
					menuString = langManager.GetTextValue(idString);
				}
				int IconId = GetIntFromAttribute (appData.ChildNodes[itemInd], "icon", 0);
				mainMenuItem mmItem = AddMainMenuItem (menuData.Items, menuString, itemHeightMainMenu, IconId);
				mmItem.Items = new List<uItem> ();
				ParseNode (appData.ChildNodes[itemInd], mmItem);
			}
		}

		syncWindow.SetActive (false);
	}

	void OnLanguageChanged(LanguageManager thisLanguageManager)
	{
		mainTitle.text = langManager.GetTextValue("main.title");
		updateText.GetComponent<Text>().text = langManager.GetTextValue ("menu.upd");
		syncText.GetComponent<Text>().text = langManager.GetTextValue ("menu.sync");
	}

	void OnDestroy(){
		if (LanguageManager.HasInstance)
			LanguageManager.Instance.OnChangeLanguage -= OnLanguageChanged;
	}
}
