using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Analytics;

[Serializable]
public struct SubMenuSet{
	public GameObject Container;
	public Text Title;
	public RectTransform Screen;
	public RectTransform Scroller;
}

public class MenuBuilder : MonoBehaviour {

	public MenuMover menuMover;
	public LocalizationManager locMgr;
		
	public GameObject submenuScreen;
	public GameObject rootScreen;

	public GameObject menuItem;
	public GameObject submenuItem;
	public GameObject submenuItem2;
	public GameObject infoItemPhoneNumber;
	public GameObject infoItemText;
	public GameObject infoItemImage;
	public GameObject infoItemCalendar;
	public GameObject infoItemTable;
	public GameObject infoItemMap;
	public GameObject infoItemURL;

	public Sprite[] menuSprites;
	public SubMenuSet[] menuSets;

	private int currSubMenuSetIndex = 0;

	private float currMenuHeight = 0;

	public void IncSubMenuSetIndex(){
		currSubMenuSetIndex++;
		if (currSubMenuSetIndex >= menuSets.Length)
			currSubMenuSetIndex = menuSets.Length - 1;
	}

	public void DecSubMenuSetIndex(){
		currSubMenuSetIndex--;
		if (currSubMenuSetIndex < 0)
			currSubMenuSetIndex = 0;
	}

	public int GetSubMenuSetIndex(){
		return currSubMenuSetIndex;
	}

	public void Exit(){
		Application.Quit ();
	}

	public void InitializeMenu(){
		currMenuHeight = 0;
		for (int i = 0; i < locMgr.menuData.Items.Count; i++) {
			if (locMgr.menuData.Items[i] is mainMenuItem){
				mainMenuItem mmItem = locMgr.menuData.Items[i] as mainMenuItem;
				addMenu(menuSets[0].Container, mmItem);
			}
		}
	}

	public void clearMenu (GameObject container) {
		int nchilds = container.transform.childCount;
		for (int chInd = nchilds-1; chInd >= 0; chInd--) {
			DestroyImmediate(container.transform.GetChild(chInd).gameObject);
		}
	}

	// Click procs

	public void callNumber(string Number){
		if (menuMover.IsCancelNextMenu()) return;
		#if !UNITY_EDITOR
		AnalyticsResult res = Analytics.CustomEvent("userCall", new Dictionary<string, object> {
			{ "callOrg", menuSets[currSubMenuSetIndex].Title.text }
		});
		Debug.Log("Event: userCall: " + res.ToString());
		Application.OpenURL ("tel:" + Number);
		#endif
	}

	public void gotoURL(string Link){
		if (menuMover.IsCancelNextMenu()) return;
		Application.OpenURL (Link);
	}

	private void fillNextContainer(List<uItem> Items, GameObject NextContainer){
		for (int itemIndex = 0; itemIndex < Items.Count; itemIndex++) {
			if (Items[itemIndex] is subMenuItemExtra){
				subMenuItemExtra smItem = Items[itemIndex] as subMenuItemExtra;
				addSubMenu (NextContainer, smItem);
			}
			if (Items[itemIndex] is subMenuItemSingle){
				subMenuItemSingle smItem = Items[itemIndex] as subMenuItemSingle;
				addSubMenu2 (NextContainer, smItem);
			}
			if (Items[itemIndex] is callInfoItem){
				callInfoItem iItem = Items[itemIndex] as callInfoItem;
				addInfoPhoneNumber (NextContainer, iItem);
			}
			if (Items[itemIndex] is inetLinkItem){
				inetLinkItem iItem = Items[itemIndex] as inetLinkItem;
				addUrlItem(NextContainer, iItem);
			}
			if (Items[itemIndex] is calendarItem){
				calendarItem iItem = Items[itemIndex] as calendarItem;
				addCalendarItem(NextContainer, iItem);
			}	
			if (Items[itemIndex] is tableItem){
				tableItem iItem = Items[itemIndex] as tableItem;
				addTableItem(NextContainer, iItem);
			}	
			if (Items[itemIndex] is textInfoItem){
				textInfoItem iItem = Items[itemIndex] as textInfoItem;
				addTextInfoItem(NextContainer, iItem);
			}
			if (Items[itemIndex] is mapItem){
				mapItem iItem = Items[itemIndex] as mapItem;
				addMapItem(NextContainer, iItem);
			}
		}
	}

	public void gotoNextMenu(uItem Item, SubMenuSet subMenuSet){
		if (menuMover.IsCancelNextMenu() || menuMover.IsMenuMoving()) return;
		currMenuHeight = 0;
		clearMenu (subMenuSet.Container);

		List<uItem> subItems = null;

		if (Item is mainMenuItem) {
			mainMenuItem mmItem = Item as mainMenuItem;
			subMenuSet.Title.text = mmItem.Text;
			subItems = mmItem.Items;
		}
		if (Item is subMenuItemExtra) {
			subMenuItemExtra smItem = Item as subMenuItemExtra;
			subMenuSet.Title.text = smItem.Text;
			subItems = smItem.Items;
		}
		if (Item is subMenuItemSingle) {
			subMenuItemSingle smItem = Item as subMenuItemSingle;
			subMenuSet.Title.text = smItem.Text;
			subItems = smItem.Items;
		}

		if (subItems != null)
			fillNextContainer(subItems, subMenuSet.Container);

		Resources.UnloadUnusedAssets ();
		menuMover.nextMenuLevel ();
	}

	// Item Additional Procs

	private bool CheckSubMenuSet(int subMenuSetIndex){
		if ((subMenuSetIndex < 1) || (subMenuSetIndex > menuSets.Length)) {
			Debug.Log ("Invalid SubMenuSetIndex: " + subMenuSetIndex.ToString ());
			return false;
		}
		if (subMenuSetIndex == menuSets.Length) {
			int newSetLen = menuSets.Length + 1;
			SubMenuSet[] newSet = new SubMenuSet[newSetLen];
			for (int setInd = 0; setInd < newSetLen - 1; setInd++){
				newSet[setInd].Container = menuSets[setInd].Container;
				newSet[setInd].Title = menuSets[setInd].Title;
				newSet[setInd].Screen = menuSets[setInd].Screen;
				newSet[setInd].Scroller = menuSets[setInd].Scroller;
			}
			RectTransform newSubMenuTF = (Instantiate(submenuScreen, new Vector3(0,0,0), new Quaternion(0,0,0,0)) as GameObject).GetComponent<RectTransform>();
			newSubMenuTF.SetParent (rootScreen.transform);

			Button backButton = newSubMenuTF.FindChild ("TitleBack").gameObject.GetComponent<Button> ();
			backButton.onClick.AddListener (() => menuMover.prevMenuLevel());

			newSet[newSetLen - 1].Container = newSubMenuTF.FindChild("ItemsContainer").gameObject;
			newSet[newSetLen - 1].Title = newSubMenuTF.FindChild ("TitleText").gameObject.GetComponent<Text>();
			newSet[newSetLen - 1].Screen = newSubMenuTF;
			newSet[newSetLen - 1].Scroller = newSet[newSetLen - 1].Container.GetComponent<RectTransform>();
			menuMover.ResetScreenPos(newSubMenuTF);
			menuSets = newSet; 
		}

		return true;
	}

	private void FillItemRectTransform(RectTransform iRT, float itemHeight, Transform itemParent){
		iRT.SetParent (itemParent);
		iRT.anchorMin = new Vector2 (iRT.anchorMin.x, 1.0f - currMenuHeight - itemHeight);
		iRT.anchorMax = new Vector2 (iRT.anchorMax.x, 1.0f - currMenuHeight);
		iRT.offsetMax = new Vector2 (0, 0);
		iRT.offsetMin = new Vector2 (0, 0);
		iRT.localScale = new Vector3 (1, 1, 1);
		currMenuHeight += itemHeight;
	}

	private void smprocSetText(GameObject menuObj, string compName, string menuText){
		Text mText = menuObj.transform.FindChild (compName).GetComponent<Text> ();
		mText.text = menuText;
	}

	private void smprocSetNextButton(GameObject menuObj, uItem smItem, int NextIndexInc){
		Button mButton = menuObj.GetComponent<Button> ();
		CheckSubMenuSet (currSubMenuSetIndex + NextIndexInc);
		SubMenuSet smSet = menuSets [currSubMenuSetIndex + NextIndexInc];
		mButton.onClick.AddListener (() => gotoNextMenu (smItem, smSet));
	}

	private void smprocSetCustomButton(GameObject menuObj, UnityEngine.Events.UnityAction actionProc){
		Button mButton = menuObj.GetComponent<Button> ();
		mButton.onClick.AddListener (actionProc);
	}

    // Items Procs

	public void addMapItem(GameObject Container, mapItem infoItem){
		GameObject newMenu = Instantiate (infoItemMap, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		MapFill mapScript = newMenu.GetComponent<MapFill> ();
		smprocSetCustomButton(newMenu.transform.FindChild ("update").gameObject, () => mapScript.UpdateMap());
		Image mapImg = newMenu.transform.FindChild ("mapImage").gameObject.GetComponent<Image> ();
		RectTransform menuRT = newMenu.GetComponent<RectTransform> ();
		FillItemRectTransform (menuRT, infoItem.Height, Container.transform);
		float aspect = menuRT.rect.height/menuRT.rect.width; 
		float width = 600;
		float height = width * aspect;
		if (height > 450) {
			height = 450;
			width = height / aspect;
		}
		StartCoroutine(mapScript.LoadMap (infoItem.CenterCoord, infoItem.MarkCoord, Mathf.RoundToInt(width), Mathf.RoundToInt(height), mapImg));
	}
	
	public void addTableItem(GameObject Container, tableItem infoItem){
		GameObject newMenu = Instantiate (infoItemTable, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		TableFill fillScript = newMenu.GetComponent<TableFill> ();
		fillScript.FillTable (infoItem.Table);
		float itemHeight = infoItem.rowHeight * infoItem.Table.Count;
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), itemHeight, Container.transform);
	}

	public void addCalendarItem(GameObject Container, calendarItem infoItem){
		GameObject newMenu = Instantiate (infoItemCalendar, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		CalendarFill fillScript = newMenu.GetComponent<CalendarFill> ();
		fillScript.FillCalendar (infoItem.MonthName, infoItem.FirstDayPos, infoItem.DaysInMonth, infoItem.SelectedDays);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), infoItem.Height, Container.transform);
	}

	public void addInfoPhoneNumber(GameObject Container, callInfoItem infoItem){
		GameObject newMenu = Instantiate (infoItemPhoneNumber, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		smprocSetCustomButton (newMenu, () => callNumber (infoItem.callNumber));
		smprocSetText (newMenu, "Text", infoItem.Text);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), infoItem.Height, Container.transform);
	}

	public void addUrlItem(GameObject Container, inetLinkItem infoItem){
		GameObject newMenu = Instantiate (infoItemURL, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		smprocSetCustomButton (newMenu, () => gotoURL (infoItem.Link));
		smprocSetText (newMenu, "Text", infoItem.Text);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), infoItem.Height, Container.transform);
	}

	public void addTextInfoItem(GameObject Container, textInfoItem infoItem){
		GameObject newMenu = Instantiate (infoItemText, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		smprocSetText (newMenu, "Text", infoItem.Text);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), infoItem.Height, Container.transform);
	}

	public void addSubMenu(GameObject Container, subMenuItemExtra smItem){
		GameObject newMenu = Instantiate (submenuItem, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		smprocSetNextButton (newMenu, smItem, 2);
		smprocSetText (newMenu, "Text", smItem.Text);
		smprocSetText (newMenu, "ExtraText", smItem.ExtraText);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), smItem.Height, Container.transform);
	}

	public void addSubMenu2(GameObject Container, subMenuItemSingle smItem){
		GameObject newMenu = Instantiate (submenuItem2, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
		smprocSetNextButton (newMenu, smItem, 2);
		smprocSetText (newMenu, "Text", smItem.Text);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), smItem.Height, Container.transform);
	}

	public void addMenu(GameObject Container, mainMenuItem MainMenuItem){
		GameObject newMenu = Instantiate (menuItem, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0)) as GameObject;

		if ((MainMenuItem.IconId > menuSprites.Length - 1) || 
		    (MainMenuItem.IconId < 0)) MainMenuItem.IconId = 0;
		newMenu.transform.FindChild ("Image").gameObject.GetComponent<Image> ().sprite = menuSprites[MainMenuItem.IconId];

		smprocSetNextButton (newMenu, MainMenuItem, 1);
		smprocSetText (newMenu, "Text", MainMenuItem.Text);
		FillItemRectTransform (newMenu.GetComponent<RectTransform> (), MainMenuItem.Height, Container.transform);
	}
}
