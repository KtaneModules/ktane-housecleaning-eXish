using UnityEngine;

public class HouseCell {

	private GameObject[] walls; //0 = Left, 1 = Top, 2 = Right, 3 = Bottom
	private int itemIndex; //0 = Nothing, 1 = Red Shells, 2 = Bananas, 3 = Green Shells, 4 = Fake Item Boxes
	private bool isPressed;

	public HouseCell(GameObject cellObj, string cellInfo)
    {
		walls = new GameObject[4];
		for (int i = 1; i < 5; i++)
			walls[i - 1] = cellObj.transform.GetChild(i).gameObject;
		string[] wallTypes = { "L", "T", "R", "B" };
		string[] itemTypes = { "1", "2", "3", "4" };
		for (int i = 0; i < wallTypes.Length; i++)
        {
			if (cellInfo.Contains(wallTypes[i]))
				walls[i].SetActive(true);
		}
		for (int i = 0; i < itemTypes.Length; i++)
		{
			if (cellInfo.Contains(itemTypes[i]))
				itemIndex = i + 1;
		}
	}

	public int getItemIndex()
    {
		return itemIndex;
    }

	public bool getIsPressed()
    {
		return isPressed;
    }

	public void setIsPressed()
    {
		isPressed = true;
    }
}
