using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CalendarFill : MonoBehaviour {

	public Text headerText;
	public GameObject dayObj;

	public void ClearCalendar(){
		for (int child_ind = 0; child_ind < transform.childCount; child_ind++) {
			GameObject childObj = transform.GetChild(child_ind).gameObject;
			if (childObj.name != "Header")
				DestroyImmediate(childObj);
		}
	}

	public void FillCalendar(string MonthName, int FirstDayPos, int DaysCount, List<int> SelectedDays){
		headerText.text = MonthName;
		if (FirstDayPos < 0) FirstDayPos = 0;
		if (FirstDayPos > 6) FirstDayPos = 6;
		if (DaysCount < 1) DaysCount = 1;
		if (DaysCount > 31) DaysCount = 31;
		int nlines = Mathf.CeilToInt((DaysCount + FirstDayPos) / 7.0f);
		int currday = -FirstDayPos;
		Image[] daysImg = new Image[DaysCount];
		for (int currline = 0; currline < nlines; currline++) {
			for (int dayinline = 0; dayinline < 7; dayinline++) {
				currday++;
				GameObject currDayObj = Instantiate (dayObj, new Vector3 (), new Quaternion ()) as GameObject;
				RectTransform DayRT = currDayObj.GetComponent<RectTransform> ();
				Text DayText = currDayObj.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
				if ((currday > 0) && (currday <= DaysCount)){
					DayText.text = currday.ToString ();
					daysImg[currday-1] = currDayObj.GetComponent<Image>();
				}else
					DayText.text = "";

				DayRT.SetParent(transform);
				DayRT.localScale = new Vector3 (1, 1, 1);
				DayRT.anchorMin = new Vector2 (dayinline/7.0f, 0.84f - ((currline+1) * 0.84f)/nlines);
				DayRT.anchorMax = new Vector2 ((dayinline + 1)/7.0f, 0.84f - (currline * 0.84f)/nlines);
				DayRT.offsetMax = new Vector2 (0, 0);
				DayRT.offsetMin = new Vector2 (0, 0);
			}
		}

		ThemeManager tm = GameObject.FindGameObjectWithTag ("GameController").GetComponent<ThemeManager> ();
		for (int currSelDay = 0; currSelDay < SelectedDays.Count; currSelDay++)
			daysImg [SelectedDays [currSelDay] - 1].color = tm.calendarSelectionColor;
	}

}
