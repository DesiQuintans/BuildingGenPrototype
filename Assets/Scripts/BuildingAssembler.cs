using UnityEngine;
using System.Collections;
using System.Linq;

public class BuildingAssembler : MonoBehaviour {
	
	public TextAsset[] bathroomModules;
	public TextAsset[] bedroomModules;
	public TextAsset[] kitchenModules;
	public TextAsset[] livingRoomModules;
	
	private static TextAsset[] livingRooms;
	private static TextAsset[] kitchens;
	private static TextAsset[] bathrooms;
	private static TextAsset[] bedrooms;
	
	public static char[,] blueprint;
	
	private static int blueprintCenterCol = 0;
	private static int blueprintCenterRow = 0;
	
	// Use this for initialization
	void Awake () {
		livingRooms = livingRoomModules;
		kitchens = kitchenModules;
		bathrooms = bathroomModules;
		bedrooms = bedroomModules;
	}
	
	/// <summary>
	/// Assembles a building from individual modules.
	/// </summary>
	/// <returns>
	/// The building.
	/// </returns>
	public static char[,] AssembleBuilding ()
	{
		blueprint = new char[100, 100];
		ArrayHelper2D.FloodFill2D (blueprint, ' ');
		blueprintCenterRow = (int) blueprint.GetUpperBound (0) / 2;
		blueprintCenterCol = (int) blueprint.GetUpperBound (1) / 2;
		
		// Living room.
		int[][] livingRoomDoors;
		DrawHubRoom (blueprintCenterCol, blueprintCenterRow, livingRooms, out livingRoomDoors);
		
		// Kitchen.
//		int[][] kitchenDoors;
		int randomDoor = Random.Range (0, livingRoomDoors.Length);
//		DrawModule (livingRoomDoors[randomDoor][0], livingRoomDoors[randomDoor][1], livingRoomDoors[randomDoor][2], kitchens, out kitchenDoors);
		
		// Bathroom
//		int[][] bathroomDoors;
//		livingRoomDoors = FindDoors (blueprint);
//		randomDoor = Random.Range (0, livingRoomDoors.Length);
//		DrawModule (livingRoomDoors[randomDoor][0], livingRoomDoors[randomDoor][1], livingRoomDoors[randomDoor][2], bathrooms, out bathroomDoors);
		
		/*
			Hallways
			Bathrooms
			Bedrooms
			Laundry
		*/
		
		blueprint = ArrayHelper2D.SearchReplaceChars (blueprint, new char[] {'d'}, '#');
		ArrayHelper2D.Print2D (blueprint, false);
		
		return blueprint;
	}
	
	/// <summary>
	/// Draws the hub room. It is centered in the middle of the blueprint array, and other rooms rotate themselves to connect to this one.
	/// </summary>
	/// <param name='focusCol'>
	/// Focus col.
	/// </param>
	/// <param name='focusRow'>
	/// Focus row.
	/// </param>
	/// <param name='moduleArray'>
	/// Module array.
	/// </param>
	/// <param name='doorLocations'>
	/// Door locations.
	/// </param>
	public static void DrawHubRoom (int focusCol, int focusRow, TextAsset[] moduleArray, out int[][] doorLocations)
	{
		int moduleWidth = 0;
		int moduleHeight = 0;
		char [,] module = RandomModule (moduleArray, out moduleWidth, out moduleHeight, out doorLocations);
		
		int startCol = (int) focusCol - (moduleWidth / 2);
		int startRow = (int) focusRow - (moduleHeight / 2);
		
		print ("Hub room origin: " + startRow + ", " + startCol);
		
		for (int row = 0; row < moduleHeight; row++)
		{
			for (int col = 0; col < moduleWidth; col++)
			{
				char moduleTile = module[row, col];
				blueprint[startRow + row, startCol + col] = moduleTile;
			}
		}
		
		foreach (int[] inner in doorLocations)
		{
			inner[0] += startRow;
			inner[1] += startCol;
		}
	}
	
	/// <summary>
	/// Draws the module.
	/// </summary>
	/// <param name='receivingColumn'>
	/// Receiving column.
	/// </param>
	/// <param name='receivingRow'>
	/// Receiving row.
	/// </param>
	/// <param name='receivingRotation'>
	/// Receiving rotation.
	/// </param>
	/// <param name='moduleArray'>
	/// Module array.
	/// </param>
	/// <param name='doorLocations'>
	/// Door locations.
	/// </param>
	public static void DrawModule (int receivingRow, int receivingColumn, int receivingRotation, TextAsset[] moduleArray, out int[][] doorLocations)
	{ 
		int moduleWidth = 0;
		int moduleHeight = 0;
		char [,] module = RandomModule (moduleArray, out moduleWidth, out moduleHeight, out doorLocations);
		
		// Pick a door in the module randomly, store its position, and mark it so that it's easy to find again.
		int randomDoor = Random.Range (0, doorLocations.Length);
		int doorRow = doorLocations[randomDoor][0];
		int doorColumn = doorLocations[randomDoor][1];
		int roomRotation = doorLocations[randomDoor][2];
		module[doorRow, doorColumn] = 'D';
		
		Debug.Log ("Before rotation: " + doorRow + " " + doorColumn + " " + roomRotation + " receiving: " + receivingRow + " " + receivingColumn + " " + receivingRotation);
		
		// Need to rotate a room in one hit, based on its rotation and the rotation of the receiving wall.
		module = ArrayHelper2D.Rotate2D (module, RotateModuleToHub (receivingRotation, roomRotation));
		ArrayHelper2D.FindFirstInstance (module, 'D', out doorRow, out doorColumn);
		GetModuleSize (module, out moduleWidth, out moduleHeight);
		
		int startCol = 0;
		int startRow = 0;
		
		switch (receivingRotation)
		{
			case 0: // N wall
				startRow = receivingRow - doorRow;
				startCol = receivingColumn - doorColumn;
				break;
			case 1: // E wall
				startCol = receivingColumn;
				startRow = receivingRow - doorRow;
				break;
			case 2: // S wall
				startRow = receivingRow;
				startCol = receivingColumn - doorColumn;
				break;
			case 3: // W wall
				startCol = receivingColumn - doorColumn;
				startRow = receivingRow - doorRow;
				break;
			default:
				Debug.LogError ("DrawModule(): Couldn't determine the rotation of the receiving room, so the module was printed to position 0,0.");
				break;
		}
		
		print ("Module origin: " + startRow + ", " + startCol);
		
		Debug.Log ("After rotation: " + doorRow + " " + doorColumn + " " + roomRotation + " receiving: " + receivingRow + " " + receivingColumn + " " + receivingRotation);
		
		for (int row = 0; row < moduleHeight; row++)
		{
			for (int col = 0; col < moduleWidth; col++)
			{
				char moduleTile = module[row, col];
				if (moduleTile == ' ') continue;
				blueprint[startRow + row, startCol + col] = moduleTile;
			}
		}
		
		foreach (int[] inner in doorLocations)
		{
			inner[0] += startRow;
			inner[1] += startCol;
		}
	}
	
	/// <summary>
	/// Selects a random module from the provided array of modules.
	/// </summary>
	/// <returns>
	/// The module.
	/// </returns>
	/// <param name='array'>
	/// Array.
	/// </param>
	/// <param name='moduleWidth'>
	/// Module width.
	/// </param>
	/// <param name='moduleHeight'>
	/// Module height.
	/// </param>
	/// <param name='doorLocations'>
	/// Locations of all doors (as 'd') in the module.
	/// </param>
	public static char[,] RandomModule (TextAsset[] array, out int moduleWidth, out int moduleHeight, out int[][] doorLocations)
	{
		char [,] module = ArrayHelper2D.ConvertJaggedTo2D (array[Random.Range (0, array.Length)].text.Split('\n'), ' ');
		
		GetModuleSize (module, out moduleWidth, out moduleHeight);
		doorLocations = FindDoors (module);
		
		return module;
	}
	
	/// <summary>
	/// Gets the size of the module.
	/// </summary>
	/// <param name='module'>
	/// Module.
	/// </param>
	/// <param name='moduleWidth'>
	/// Module width.
	/// </param>
	/// <param name='moduleHeight'>
	/// Module height.
	/// </param>
	public static void GetModuleSize (char[,] module, out int moduleWidth, out int moduleHeight)
	{
		moduleHeight = module.GetUpperBound (0) + 1; // 1 added to bounds to get iterators looping through entire thing.
		moduleWidth = module.GetUpperBound (1) + 1;
	}
	
	/// <summary>
	/// Finds all doors (as 'd') in the module.
	/// </summary>
	/// <returns>
	/// Door locations in a jagged array, where the outer array length is the number of doors, and the inner arrays hold row, column, rotation.
	/// </returns>
	/// <param name='haystack'>
	/// Haystack.
	/// </param>
	public static int[][] FindDoors (char[,] haystack)
	{
		// 2D array lengths increased by 1 to ensure that the whole array is iterated through.
		// For a 2D with 15 rows, for example, GetUpperBound(0) will return 15, and a for(row < GetUpperBound(0)) will stop at row = 14.
		int maxRows = haystack.GetUpperBound (0) + 1;
		int maxColumns = haystack.GetUpperBound (1) + 1;
		
		string foundString = "";
		for (int row = 0; row < maxRows; ++row)
		{ 
			for (int column = 0; column < maxColumns; ++column)
			{
				char straw = haystack[row, column];
				if (straw == 'd')
				{
					foundString += row + "," + column + "," + GetWallDirection (haystack, row, column) + "|";
					continue;
				}
			}
		}
		
		return JaggedArrayHelper.StringToJaggedInt (foundString, 3);
	}
	
	/// <summary>
	/// Determines what side of a room a wall is on by looking for adjacent non-room spaces.
	/// The method assumes that row 0 is North, and column 0 is West.
	/// </summary>
	/// <returns>
	/// The number of rotations (as int).
	/// </returns>
	/// <param name='module'>
	/// Module.
	/// </param>
	/// <param name='row'>
	/// Row of selected door.
	/// </param>
	/// <param name='column'>
	/// Column of selected door.
	/// </param> 
	public static int GetWallDirection (char[,] module, int row, int column)
	{
		char[] neighbour = ArrayHelper2D.ListNeighbouringIndices (module, row, column);
		
		if (neighbour[1] == '_' || neighbour[1] == ' ')
		{
			// N wall
			return 0;
		}
		else if (neighbour[5] == '_' || neighbour[5] == ' ')
		{ 
			// E wall
			return 1;
		}
		else if (neighbour[7] == '_' || neighbour[7] == ' ')
		{
			// S wall
			return 2;
		}
		else if (neighbour[3] == '_' || neighbour[3] == ' ')
		{
			// W wall
			return 3;
		}
		else
		{
			// Couldn't find an orientation.
			Debug.LogError ("GetWallDirection(): Rotational data couldn't be obtained for the following module. Returned 0.\n");
			ArrayHelper2D.Print2D (module, false);
			return 0;
		}
	}
	
	/// <summary>
	/// Returns the number of rotations necessary to make the module mate properly with the receiving wall.
	/// </summary>
	/// <returns>
	/// The module to hub.
	/// </returns>
	/// <param name='receivingRotation'>
	/// Receiving rotation.
	/// </param>
	/// <param name='roomRotation'>
	/// Room rotation.
	/// </param>
	public static int RotateModuleToHub (int receivingRotation, int roomRotation)
	{
		switch (receivingRotation)
		{
			case 0: // North wall
				switch (roomRotation)
				{
					case 0: // Module's door is on North wall.
						return 2;
					case 1: // Module's door is on East wall.
						return 1;
					case 2: // Module's door is on South wall.
						return 0;
					case 3: // Module's door is on West wall.
						return 3;
					default:
						Debug.LogError ("RotateModuleToHub(): The receiving wall is North-facing, but the rotation of the module couldn't be determined. No rotations have been performed.");
						return 0;
				}
			case 1: // East wall
				switch (roomRotation)
				{
					case 0: // Module's door is on North wall.
						return 3;
					case 1: // Module's door is on East wall.
						return 2;
					case 2: // Module's door is on South wall.
						return 1;
					case 3: // Module's door is on West wall.
						return 0;
					default:
						Debug.LogError ("RotateModuleToHub(): The receiving wall is East-facing, but the rotation of the module couldn't be determined. No rotations have been performed.");
						return 0;
				}
			case 2: // South wall
				switch (roomRotation)
				{
					case 0: // Module's door is on North wall.
						return 0;
					case 1: // Module's door is on East wall.
						return 3;
					case 2: // Module's door is on South wall.
						return 2;
					case 3: // Module's door is on West wall.
						return 1;
					default:
						Debug.LogError ("RotateModuleToHub(): The receiving wall is South-facing, but the rotation of the module couldn't be determined. No rotations have been performed.");
						return 0;
				}
			case 3: // West wall
				switch (roomRotation)
				{
					case 0: // Module's door is on North wall.
						return 1;
					case 1: // Module's door is on East wall.
						return 0;
					case 2: // Module's door is on South wall.
						return 3;
					case 3: // Module's door is on West wall.
						return 2;
					default:
						Debug.LogError ("RotateModuleToHub(): The receiving wall is West-facing, but the rotation of the module couldn't be determined. No rotations have been performed.");
						return 0;
				}
			default:
				Debug.LogError ("RotateModuleToHub(): The rotation of the receiving wall couldn't be determined. No rotations have been performed.");
				return 0;
		}
	}
	
	// Note: This system uses 'd' for possible doors, but 'd' is already being used for desks. Remove desks?
	// Todo: Trim empty lines and columns from blueprint array before passing to generator.
}