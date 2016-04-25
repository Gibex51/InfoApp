using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuMover : MonoBehaviour {

	public RectTransform rtCanvas;
	public MenuBuilder menuBuilder;

	private float velocity = 0;
	private float scrWidth = 0;

	bool menuMovingNext = false;
	bool menuMovingPrev = false;

	private Vector2 touchStartPos;
	private Vector2 touchPrevDeltaPos;
	private Vector2 scrollerPos;
	private Vector2 lastDirection;
	private float menuVelocity = 0;

	private float currMaxItemsOffset = 0;

	private bool cancelNextMenu = false;

	public bool IsCancelNextMenu(){
		return cancelNextMenu;
	}

	public bool IsMenuMoving(){
		return (menuMovingNext || menuMovingPrev);
	}

	public void nextMenuLevel(){
		int menuLevel = menuBuilder.GetSubMenuSetIndex();
		if ((menuLevel >= menuBuilder.menuSets.Length - 1) || (IsMenuMoving ())) return;
		menuMovingNext = true;
		ResetScrollerPos (menuBuilder.menuSets[menuLevel + 1].Scroller);
		StartCoroutine(MenuMoveNext());
	}

	public void prevMenuLevel(){
		int menuLevel = menuBuilder.GetSubMenuSetIndex();
		if ((menuLevel <= 0) || (IsMenuMoving ()))return;
		menuMovingPrev = true;
		ResetScrollerPos (menuBuilder.menuSets[menuLevel - 1].Scroller);
		StartCoroutine(MenuMovePrev());
	}

	/*public void showDebug() {
		Debug.Log (scrollers [menuLevel].anchoredPosition);
		Debug.Log (scrollers [menuLevel].anchoredPosition3D);
		Debug.Log (scrollers [menuLevel].anchorMax);
		Debug.Log (scrollers [menuLevel].anchorMin);
		Debug.Log (scrollers [menuLevel].localPosition);
		Debug.Log (scrollers [menuLevel].offsetMax);
		Debug.Log (scrollers [menuLevel].offsetMin);
		Debug.Log (scrollers [menuLevel].position);
		Debug.Log (scrollers [menuLevel].rect);
	}*/

	public void ResetScrollerPos(RectTransform Scroller) {
		Vector2 ap = Scroller.anchoredPosition;
		ap.y = 0;
		Scroller.anchoredPosition = ap;

		float scr_height = Scroller.rect.height;
		int childs = Scroller.childCount;
		float sumItemsHeight = 0;
		for (int i = 0; i < childs; i++) {
			RectTransform childRT = Scroller.GetChild(i).GetComponent<RectTransform>();
			sumItemsHeight += (childRT.anchorMax.y - childRT.anchorMin.y) * scr_height;
		}
		currMaxItemsOffset = sumItemsHeight - scr_height;
		if (currMaxItemsOffset < 0)
			currMaxItemsOffset = 0;
	}

	public void ResetAllScreensPos(){
		menuBuilder.menuSets[0].Screen.offsetMin = new Vector2(0,0);
		menuBuilder.menuSets[0].Screen.offsetMax = new Vector2(0,0);
		
		for (int i = 1; i < menuBuilder.menuSets.Length; i++)
			ResetScreenPos(menuBuilder.menuSets[i].Screen);
	}

	public void ResetScreenPos(RectTransform rtScreen){
		rtScreen.offsetMin = new Vector2(scrWidth,0);
		rtScreen.offsetMax = new Vector2(scrWidth,0);
		rtScreen.localScale = new Vector3 (1, 1, 1);
	}

	void Start() {
		velocity = rtCanvas.rect.width / 10.0f;
		scrWidth = rtCanvas.rect.width;
		ResetAllScreensPos ();
	}

	void Update () {
		if ((Application.platform == RuntimePlatform.Android) || 
		    (Application.platform == RuntimePlatform.WP8Player)) {
			if (Input.GetKeyDown (KeyCode.Escape))
				prevMenuLevel();
			//if (Input.GetKeyDown(KeyCode.Menu)) 
		}

		if (Input.touchCount > 0) {
			var touch = Input.GetTouch(0);
			switch (touch.phase) {
			case TouchPhase.Began:
				cancelNextMenu = false;
				touchStartPos = touch.position;
				scrollerPos = menuBuilder.menuSets[menuBuilder.GetSubMenuSetIndex()].Scroller.anchoredPosition;
				break;
			case TouchPhase.Moved:
				lastDirection = (touch.position - touchStartPos) * 0.7f; 
				if ((lastDirection.y < -4) || (lastDirection.y > 4) || 
				    (lastDirection.x < -4) || (lastDirection.x > 4)) cancelNextMenu = true;
				Vector2 newPos = new Vector3(scrollerPos.x, scrollerPos.y + lastDirection.y); 
				if (newPos.y < 0) {
					touchStartPos.y += newPos.y;
					newPos.y = 0;
				}
				if (newPos.y > currMaxItemsOffset) {
					touchStartPos.y += (newPos.y - currMaxItemsOffset);
					newPos.y = currMaxItemsOffset;
				}
				menuBuilder.menuSets[menuBuilder.GetSubMenuSetIndex()].Scroller.anchoredPosition = newPos;
				touchPrevDeltaPos = touch.deltaPosition;
				break;
			case TouchPhase.Ended:
				if (cancelNextMenu)
					menuVelocity = touchPrevDeltaPos.y * 1.5f;
				break;
			}
		}
	}
	
	private IEnumerator MenuMovePrev(){
		while (true) {
			int menuLevel = menuBuilder.GetSubMenuSetIndex();
			if ((menuMovingPrev) && (menuLevel > 0)) {
				bool IsLastMove = false;
				RectTransform currScreen = menuBuilder.menuSets [menuLevel].Screen;
				RectTransform prevScreen = menuBuilder.menuSets [menuLevel - 1].Screen;

				Vector2 offsetMin = currScreen.offsetMin;
				Vector2 offsetMax = currScreen.offsetMax;
				offsetMin.x += velocity;
				offsetMax.x += velocity;
				if (offsetMin.x >= scrWidth) {
					IsLastMove = true;
					offsetMin.x = scrWidth;
					offsetMax.x = scrWidth;
				}
				currScreen.offsetMin = offsetMin;
				currScreen.offsetMax = offsetMax;
			
				offsetMin = prevScreen.offsetMin;
				offsetMax = prevScreen.offsetMax;
				offsetMin.x += velocity;
				offsetMax.x += velocity;
				if (offsetMin.x >= 0) {
					offsetMin.x = 0;
					offsetMax.x = 0;
				}
				prevScreen.offsetMin = offsetMin;
				prevScreen.offsetMax = offsetMax;
				if (IsLastMove) {
					menuMovingPrev = false;
					menuBuilder.DecSubMenuSetIndex();
					yield break;
				}
			} else {
				menuMovingPrev = false;
				yield break;
			}
			yield return null;
		}
	}
	
	private IEnumerator MenuMoveNext(){
		while (true) {
			int menuLevel = menuBuilder.GetSubMenuSetIndex();
			if ((menuMovingNext) && (menuLevel < menuBuilder.menuSets.Length - 1)) {
				bool IsLastMove = false;
				RectTransform currScreen = menuBuilder.menuSets [menuLevel].Screen;
				RectTransform nextScreen = menuBuilder.menuSets [menuLevel + 1].Screen;

				Vector2 offsetMin = currScreen.offsetMin;
				Vector2 offsetMax = currScreen.offsetMax;
				offsetMin.x -= velocity;
				offsetMax.x -= velocity;
				if (offsetMin.x <= -scrWidth) {
					IsLastMove = true;
					offsetMin.x = -scrWidth;
					offsetMax.x = -scrWidth;
				}
				currScreen.offsetMin = offsetMin;
				currScreen.offsetMax = offsetMax;

				offsetMin = nextScreen.offsetMin;
				offsetMax = nextScreen.offsetMax;
				offsetMin.x -= velocity;
				offsetMax.x -= velocity;
				if (offsetMin.x <= 0) {
					offsetMin.x = 0;
					offsetMax.x = 0;
				}
				nextScreen.offsetMin = offsetMin;
				nextScreen.offsetMax = offsetMax;
				if (IsLastMove) {
					menuMovingNext = false;
					menuBuilder.IncSubMenuSetIndex ();
					yield break;
				}
			} else {
				menuMovingNext = false;
				yield break;
			}
			yield return null;
		}
	}

	void FixedUpdate () {
		if (menuVelocity != 0) {
			RectTransform currScroller = menuBuilder.menuSets[menuBuilder.GetSubMenuSetIndex()].Scroller;
			Vector2 currPos = currScroller.anchoredPosition;
			currPos.y += menuVelocity;
			if (currPos.y < 0) {
				currPos.y = 0;
				menuVelocity = 0;
			}
			if (currPos.y > currMaxItemsOffset) {
				currPos.y = currMaxItemsOffset;
				menuVelocity = 0;
			}
			menuVelocity /= 1.2f;
			if ((menuVelocity < 0.1f) && (menuVelocity > -0.1f)) menuVelocity = 0;

			currScroller.anchoredPosition = currPos;
		}
	}
}
