using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TableFill : MonoBehaviour {

	public GameObject cellObj;

	public void ClearTable(){
		for (int child_ind = 0; child_ind < transform.childCount; child_ind++)
			DestroyImmediate(transform.GetChild(child_ind).gameObject);
	}

	public void FillTable(List<List<tableCell>> Table){
		float rowHeight = 1.0f / Table.Count;
		for (int row = 0; row < Table.Count; row++) {
			float colWidth = 1.0f / Table[row].Count;
			for (int col = 0; col< Table[row].Count; col++) {
				GameObject currCell = Instantiate (cellObj, new Vector3 (), new Quaternion ()) as GameObject;
				RectTransform cellRT = currCell.GetComponent<RectTransform> ();
				Image cellImage = currCell.GetComponent<Image> ();
				Text CellText = currCell.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
				CellText.text = Table[row][col].value;
				cellImage.color = Table[row][col].color;

				if ((CellText.text.Length > 5) && (Table[row].Count > 1))
					CellText.fontSize -= 3;

				cellRT.SetParent(transform);
				cellRT.localScale = new Vector3 (1, 1, 1);
				cellRT.anchorMin = new Vector2 (1.0f - colWidth * (col + 1), 1.0f - rowHeight * (row + 1));
				cellRT.anchorMax = new Vector2 (1.0f - colWidth * col, 1.0f - rowHeight * row);
				cellRT.offsetMax = new Vector2 (0, 0);
				cellRT.offsetMin = new Vector2 (0, 0);
			}
		}
	}
}
