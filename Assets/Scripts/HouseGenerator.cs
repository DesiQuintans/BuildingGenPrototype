using UnityEngine;
using System.Collections;

public class HouseGenerator : MonoBehaviour
{
	public enum roomType {Shared, Private};
	public enum roomFunction {Living, Kitchen, Bathroom, Bedroom, Laundry, Study};
	
	public static char[,] blueprint;
	public static int[][] doorIndices;

	public static char[,] MakeBuilding (int mapWidth, int mapHeight, int numberOfRooms)
	{
		doorIndices = new int[numberOfRooms][];
		
		blueprint = new char[mapWidth, mapHeight];
		ArrayHelper2D.FloodFill2D (blueprint, ' ');
		
		int maxWidth = 20;
		int maxHeight = 20;
		
		int ypos = 0;
		
		for (int i = 0; i < numberOfRooms; i++)
		{
			drawRoom (blueprint, ypos, 10, maxHeight, maxWidth, i, true);
			drawRoom (blueprint, ypos, 10, maxHeight, maxWidth, i, true);
			ypos += 20;
		}
		
//		for (int i = 0; i < numberOfRooms; i++)
//		{
//			Debug.Log(doorIndicesX[i] + " " + doorIndicesY[i]);
//		}
		
		ArrayHelper2D.Print2D (blueprint, false);
		
		return blueprint;
	}
	
	public static void drawRoom (char[,] blueprint, int maxStartRow, int maxStartColumn, int maxHeight, int maxWidth, int roomNumber, bool makeDoor)
	{
		int placingRoomAttempts = 5;
		doorIndices[roomNumber] = new int[2];
		
		while (--placingRoomAttempts > 0)
		{
			int startRow = Random.Range (maxStartRow, maxStartRow);
			int startColumn = Random.Range (0, maxStartColumn);
			
			int Height = Random.Range (3, maxHeight);
			int Width = Random.Range (3, maxWidth);
			
			// Door creation works by finding the number of non-corner wall pieces and randomly selecting one of them (as an int count). The count is reduced
			// for every non-corner wall block that is placed in the loop below, and the selected block is arrived at when the count is 0.
			int randomWall = Random.Range (1, (((Height + Width) - 3) * 2)); // -3 and not -4 because Random.Range's max is exclusive.
			
			int endColumn = (startColumn + Width);
			int endRow = (startRow + Height);
			
			if (ArrayHelper2D.IsIndexValid(blueprint, startRow, startColumn) == false) continue;
			if (ArrayHelper2D.IsIndexValid (blueprint, (startRow + Height), (startColumn + Width)) == false) continue;
			
			for (int row = startRow; row < endRow; row++)
			{
				for (int column = startColumn; column < endColumn; column++)
				{
					if (row == startRow || row == endRow-1)
					{
						if (column == startColumn || column == endColumn - 1)
						{
							// Corner wall
							blueprint[row, column] = '#';
						}
						else
						{
							randomWall--;
							if (randomWall == 0)
							{
								doorIndices[roomNumber][0] = column;
								doorIndices[roomNumber][1] = row;
								blueprint[row, column] = 'D';
							}
							else
							{
								blueprint[row, column] = '#';
							}
						}
					}
					else if (column == startColumn || column == endColumn-1)
					{
						if (row == startRow || row == endRow-1)
						{
							// Corner wall
							blueprint[row, column] = '#';
						}
						else
						{
							randomWall--;
							if (randomWall == 0)
							{
								doorIndices[roomNumber][0] = column;
								doorIndices[roomNumber][1] = row;
								blueprint[row, column] = 'D';
							}
							else
							{
								blueprint[row, column] = '#';
							}
						}
					}
					else
					{
						blueprint[row, column] = '.';
					}
					
				}
			}
			
//			Debug.Log ("Origin (C/R): " + randX + "," + randY);
//			Debug.Log ("Dimensions (WxH): " + randWidth + "," + randHeight);
//			Debug.Log ("End (C/R): " + endX + "," + endY);
			
			placingRoomAttempts = 0;
		}
	}
}
