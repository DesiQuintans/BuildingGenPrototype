using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArrayHelper2D {

	public static bool IsIndexValid (char[,] thisArray, int row, int column)
	{
		// Must check the number of rows at this point, or else an OutOfRange exception gets thrown when checking number of columns.
		if (row < thisArray.GetLowerBound (0) || row > (thisArray.GetUpperBound (0))) return false;
		
		if (column < thisArray.GetLowerBound (1) || column > (thisArray.GetUpperBound (1))) return false;
		else return true;
	}
	
	public static bool AreSurroundingsValid (char[,] thisArray, int row, int column)
	{
			if (IsIndexValid (thisArray, (row-1), column) == false) return false;
			else if (IsIndexValid (thisArray, (row+1), column) == false) return false;
			else if (IsIndexValid (thisArray, row, (column-1)) == false) return false;
			else if (IsIndexValid (thisArray, row, (column+1)) == false) return false;
			else return true;
	}
	
	public static Quaternion CheckLocalRotation (char[,] thisArray, int row, int column, List<char> facingTileList)
	{
		// First check for extremities. Being at an edge of the array automatically makes certain positions
		// and rotations impossible (e.g. being at the top row means it must be unrotated).
		if (IsIndexValid (thisArray, row, (column-1)) == false) // Leftmost column
		{
			return Quaternion.Euler(-90, -90, 0);
		}
		else if (IsIndexValid (thisArray, row, (column+1)) == false) // Rightmost column
		{
			return Quaternion.Euler(-90, 90, 0);
		}
		else if (IsIndexValid (thisArray, (row-1), column) == false) // Top row
		{
			return Quaternion.Euler(-90, 0, 0);
		}
		else if (IsIndexValid (thisArray, (row+1), column) == false) // Bottom row
		{
			return Quaternion.Euler(-90, 180, 0);
		}
		
		// The given index isn't at the edges, so we know that all the array indices around it can be pulled without error.
		// I originally checked the surrounding walls to correctly place outer doors and windows, but checking for an adjacent floor tile
		// is much simpler and more reliable.
		
		if (IsCharInList (thisArray, row, column, 'E', facingTileList))
		{
			return Quaternion.Euler(-90, -90, 0); // Left wall
		}
		else if (IsCharInList (thisArray, row, column, 'W', facingTileList))
		{
			return Quaternion.Euler(-90, 90, 0); // Right wall
		}
		else if (IsCharInList (thisArray, row, column, 'N', facingTileList))
		{
			return Quaternion.Euler(-90, 180, 0); // Bottom wall
		}
		else if (IsCharInList (thisArray, row, column, 'S', facingTileList))
		{
			return Quaternion.Euler(-90, 0, 0); // Top wall (neutral rotation)
		}
		else
		{
			// Error case. Use neutral rotation.
			return Quaternion.Euler(-90, 0, 0);
		}
	}
	
	public static bool IsCharInList (char[,] thisArray, int row, int column, char direction, List<char> charList)
	{
		int tempRow = row;
		int tempColumn = column;
		
		switch (direction)
		{
			case 'N':
				tempRow = row - 1;
				tempColumn = column;
				break;
			case 'E':
				tempRow = row;
				tempColumn = column + 1;
				break;
			case 'S':
				tempRow = row + 1;
				tempColumn = column;
				break;
			case 'W':
				tempRow = row;
				tempColumn = column - 1;
				break;
			case '7':
				tempRow = row - 1;
				tempColumn = column - 1;
				break;
			case '9':
				tempRow = row - 1;
				tempColumn = column + 1;
				break;
			case '1':
				tempRow = row + 1;
				tempColumn = column - 1;
				break;
			case '3':
				tempRow = row + 1;
				tempColumn = column + 1;
				break;
			default:
				Debug.LogError ("Invalid direction: Only characters N/S/E/W/7/9/1/3 accepted. (ArrayHelper.IsCharInList)");
				break;
		}
		
		if (IsIndexValid (thisArray, tempRow, tempColumn) == true)
		{
			if (charList.Contains(thisArray[tempRow, tempColumn]))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}
	
	public static void FloodFill2D (char[,] thisArray, char filler)
	{
		for (int row = 0; row < (thisArray.GetUpperBound (0) + 1); row++)
		{
			for (int column = 0; column < (thisArray.GetUpperBound (1) + 1); column++)
			{
				thisArray[row, column] = filler;
			}
		}
	}
	
	public static void Print2D (char[,] thisArray, bool printLineNumbers)
	{
		// 2D array lengths increased by 1 to ensure that the whole array is iterated through.
		// For a 2D with 15 rows, for example, GetUpperBound(0) will return 15, and a for(row < GetUpperBound(0)) will stop at row = 14.
		int maxRows = thisArray.GetUpperBound (0) + 1;
		int maxColumns = thisArray.GetUpperBound (1) + 1;
		string printout = "Contents of 2D Array (" + maxRows + "x" + maxColumns + "):\n";
		
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				if (IsIndexValid (thisArray, row, column) == true)
				{
					printout += thisArray[row, column];
				}
				else
				{
					printout += '_';
				}
			}
			if (printLineNumbers == true) printout += "\t\t\t\t\t\t\t>>>" + row;
			
			printout += "\n";
		}
		
		Debug.Log (printout);
	}
	
	public static char[,] ConvertJaggedTo2D (string[] jaggedArray, char filler)
	{
		int maxWidth = 0;
		foreach (string line in jaggedArray)
		{
			if (line.Length > maxWidth) maxWidth = line.Length;
		}
		
		
		char[,] array2D = new char[jaggedArray.Length, maxWidth];
		
		FloodFill2D (array2D, filler);
		
		for (int row = 0; row < (jaggedArray.Length); row++)
		{
			for (int column = 0; column < (jaggedArray[row].Length); column++)
			{
				array2D[row, column] = jaggedArray[row][column];
			}
		}

		return array2D;
	}
	
	public static char[,] GetNeighbouringIndices (char[,] thisArray, int row, int column)
	{
		char[,] arr = new char[3,3];
		
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				int tempRow = row + i;
				int tempColumn = column + j;
				
				if (IsIndexValid (thisArray, tempRow, tempColumn) == true)
				{
					arr[(i + 1), (j + 1)] = thisArray[tempRow, tempColumn];
				}
				else
				{
					arr[(i + 1), (j + 1)] = '_';
				}
			}
		}
		
		return arr;
	}
	
	public static char[] ListNeighbouringIndices (char[,] thisArray, int row, int column)
	{
		char[] localRange = new char[9];
		int localRangeCounter = 0;
		
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				int tempRow = row + i;
				int tempColumn = column + j;
				
				if (IsIndexValid (thisArray, tempRow, tempColumn) == true)
				{
					localRange[localRangeCounter] = thisArray[tempRow, tempColumn];
				}
				else
				{
					localRange[localRangeCounter]  = '_';
				}
				
				localRangeCounter++;
			}
		}
		
		return localRange;
	}
	
	public static char[,] SearchReplaceChars (char[,] haystack, char[] needles, char replacement)
	{
		// 2D array lengths increased by 1 to ensure that the whole array is iterated through.
		// For a 2D with 15 rows, for example, GetUpperBound(0) will return 15, and a for(row < GetUpperBound(0)) will stop at row = 14.
		int maxRows = haystack.GetUpperBound (0) + 1;
		int maxColumns = haystack.GetUpperBound (1) + 1;
		
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				char straw = haystack[row, column];
				
				foreach (char needle in needles)
				{
					if (straw == needle)
					{
						haystack[row, column] = replacement;
						continue;
					}
				}
			}
		}
		
		return haystack;
	}
	
	public static int[][] FindCharIntoJagged (char[,] haystack, char needle)
	{
		// 2D array lengths increased by 1 to ensure that the whole array is iterated through.
		// For a 2D with 15 rows, for example, GetUpperBound(0) will return 15, and a for(row < GetUpperBound(0)) will stop at row = 14.
		int maxRows = haystack.GetUpperBound (0) + 1;
		int maxColumns = haystack.GetUpperBound (1) + 1;
		
		string foundString = "";
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				char straw = haystack[row, column];
				if (straw == needle)
				{
					foundString += row + "," + column + "|";
					continue;
				}
			}
		}
		
		return JaggedArrayHelper.StringToJaggedInt (foundString, 2);
	}
	
	public static bool FindFirstInstance (char[,] haystack, char needle, out int foundRow, out int foundColumn)
	{
		// 2D array lengths increased by 1 to ensure that the whole array is iterated through.
		// For a 2D with 15 rows, for example, GetUpperBound(0) will return 15, and a for(row < GetUpperBound(0)) will stop at row = 14.
		int maxRows = haystack.GetUpperBound (0) + 1;
		int maxColumns = haystack.GetUpperBound (1) + 1;
		foundRow = 0;
		foundColumn = 0;
		
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				if (haystack[row, column] == needle)
				{
					foundRow = row;
					foundColumn = column;
					return true;
				}
			}
		}
		
		return false;
	}
	
	public static char[,] Rotate2D (char[,] array2D, int rotations)
	{
		char[,] rotated = array2D;
		
		for (int i = 0; i < rotations; i++)
		{
			int colLength = rotated.GetUpperBound (1) + 1;
			int rowLength = rotated.GetUpperBound (0) + 1;
			char[,] temp = new char[colLength, rowLength];
			FloodFill2D (temp, '_');
			
			
			for (int column = 0; column < colLength; ++column)
			{
				int tempCol = 0;
				for (int row = rowLength - 1; row > -1; --row)
				{
					temp[column, tempCol] = rotated[row, column];
					tempCol++;
				}
			}
			rotated = temp;
		}
		
		return rotated;
	}
}
