using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoguelikeMapInterpreter : MonoBehaviour
{
	public bool generateRandomBlueprint = false;
	public int numberOfRandomRooms = 2;
	public bool generatePitchedRoof = false;
	public bool generateFlatRoof = true;
	public bool generateLights = true;
	public bool generateFurniture = true;
	
	public TextAsset[] blueprintLayers;
	
	#region Tile references and Blueprint
	public GameObject bedroomBedSingle;
	public GameObject bedroomWardrobe;
	
	public GameObject floor;
	public GameObject floorEdgingFull;
	public GameObject floorEdgingHalf;
	public GameObject floorEdgingQuarter;
	public GameObject floorEdgingThreeQuarter;
	
	public GameObject generalBookshelf;
	public GameObject generalChairDining;
	public GameObject generalChairRocking;
	public GameObject generalChairSofaCorner;
	public GameObject generalChairSofaSingle;
	public GameObject generalChairSofaStraight;
	public GameObject generalTableDesk;
	public GameObject generalTableDining;
	public GameObject generalEndtable;
	public GameObject generalLampCeiling;
	public GameObject generalLampFloor;
	
	public GameObject innerWall3Way;
	public GameObject innerWall4Way;
	public GameObject innerWallCorner;
	public GameObject innerWallDoor;
	public GameObject innerWallMerge;
	public GameObject innerWallWall;
	
	public GameObject kitchenCounterCorner;
	public GameObject kitchenCounterSink;
	public GameObject kitchenCounterStove;
	public GameObject kitchenCounterStraight;
	public GameObject kitchenCounterStraightLow;
	
	
	public GameObject outerWallCornerInner;
	public GameObject outerWallCornerOuter;
	public GameObject outerWallDoor;
	public GameObject outerWallWall;
	public GameObject outerWallWindowSingle;
	
	public GameObject roofCeiling;
	public GameObject roofCeilingHalf;
	public GameObject roofCeilingQuarter;
	public GameObject roofCeilingThreeQuarters;
	
	public GameObject roofFlatCornerConcave;
	public GameObject roofFlatCornerConvex;
	public GameObject roofFlatStraight;
	public GameObject roofFlatTop;
	
	public GameObject roofPitchedCap2Way;
	public GameObject roofPitchedCap3Way;
	public GameObject roofPitchedCap4Way;
	public GameObject roofPitchedCapDouble;
	public GameObject roofPitchedCapDoubleEnd;
	public GameObject roofPitchedCapSingle;
	public GameObject roofPitchedCapSingleEnd;
	public GameObject roofPitchedCapTop;
	public GameObject roofPitchedCornerConcave;
	public GameObject roofPitchedCornerConvex;
	public GameObject roofPitchedStraight;
	
	public GameObject stairs;
	
	public GameObject error;
	
	
	#endregion
	
	#region Internal vars
	// Internal variables.
	private char[,] blueprint;
	
	// Constants.
	public static readonly Quaternion ROTATE_NONE = Quaternion.Euler(-90, 0, 0);
	public static readonly Quaternion ROTATE_RIGHT = Quaternion.Euler(-90, 90, 0);
	public static readonly Quaternion ROTATE_FLIP = Quaternion.Euler(-90, 180, 0);
	public static readonly Quaternion ROTATE_LEFT = Quaternion.Euler(-90, -90, 0);
	#endregion
	
	void Awake ()
	{
		if (generateRandomBlueprint == true)
		{
//			blueprint = HouseGenerator.MakeBuilding(200, 36, numberOfRandomRooms);
			blueprint = BuildingAssembler.AssembleBuilding ();
			
			MakeRoom (blueprint);
			if (generatePitchedRoof == true) MakePitchedRoof (blueprint);
				else if (generateFlatRoof == true) MakeFlatRoof (blueprint);
			
			CombineChildrenOnDemand[] scripts = GetComponentsInChildren <CombineChildrenOnDemand>();
			foreach (CombineChildrenOnDemand script in scripts)
			{
				script.CombineMeshes();
			}
		}
		else
		{
			int layersInThisFloor = blueprintLayers.Length;
			for (int layer = 0; layer < layersInThisFloor; layer++)
			{
				blueprint = ArrayHelper2D.ConvertJaggedTo2D (blueprintLayers[layer].text.Split('\n'), ' ');
			
				if (layer == 0)
				{
					// First layer is always the structural layer.
					MakeRoom (blueprint);
					if (generatePitchedRoof == true) MakePitchedRoof (blueprint);
					if (generateFlatRoof == true) MakeFlatRoof (blueprint);
					
					CombineChildrenOnDemand[] scripts = GetComponentsInChildren <CombineChildrenOnDemand>();
					foreach (CombineChildrenOnDemand script in scripts)
					{
						script.CombineMeshes();
					}
				}
				else
				{
					if (generateFurniture == true) processLayer (blueprint);
				}
			}
			
			Debug.Log ("Time to generate: " + Time.realtimeSinceStartup);
	//		ArrayHelper2D.Print2D (blueprint);
		}
	}
	
	void MakeRoom (char[,] blueprint)
	{
		for (int row = 0; row < (blueprint.GetUpperBound (0) + 1); row++)
		{
			for (int column = 0; column < (blueprint.GetUpperBound (1) + 1); column++)
			{
				switch (blueprint[row, column])
				{
					case ' ':
						break;
					case '.':
						SpawnTile (floor, row, column, ROTATE_NONE);
						break;
					case '#':
						spawnWall (row, column);
						break;
					case 'D':
						spawnDoor (row, column);
						break;
					case 'W':
						spawnWindow (row, column);
						break;
					case 'L':
						SpawnTile (floor, row, column, ROTATE_NONE);
						if (generateLights == true) SpawnTile (generalLampCeiling, row, column, ROTATE_NONE);
						break;
					case '+':
						SpawnTile (floor, row, column, ROTATE_NONE);
						break;
					case '>':
						char[] localRange = ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column);
						TrySpawningTile (localRange, TileIDs.stairs, stairs, floor, row, column);
						break;
					
					// Error indicator
					default:
						Debug.Log ("makeRoom(): No tile is assigned to '" + blueprint[row, column] + "'. (Row " + row + ", Column " + column +")");
						SpawnTile (error, row, column, Quaternion.identity);
						break;
				}
			}
		}
	}
	
	void MakeFlatRoof (char[,] blueprint)
	{
		char[,] roofArray = (char[,]) blueprint.Clone();
		int maxRows = roofArray.GetUpperBound (0) + 1;
		int maxColumns = roofArray.GetUpperBound (1) + 1;
		
		// 1. Replace all outer walls with 0s. 2. Replace all other tiles with 1s.
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				if (TileIDs.ignored.Contains (roofArray[row, column]) == true)
				{
					roofArray[row, column] = ' ';
				}
				else if (TileIDs.wall.Contains (roofArray[row, column]) == true)
				{
					if (MapHelper.IsListInArray (ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column), TileIDs.ignored))
					{
						roofArray[row, column] = '0';
					}
					else
					{
						roofArray[row, column] = '1';
					}
				}
			}
		}
		
		// 4. Spawn roof edge tiles on 0s. 5. Spawn flat rooftop on all other numbers.
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				char[] localRange = ArrayHelper2D.ListNeighbouringIndices (roofArray, row, column);
				
				if (roofArray[row, column] == '0')
				{
					// Ignored tile in range, therefore roof edge.
					if (TrySpawningTile (localRange, TileIDs.roofEdgeStraight, roofFlatStraight, roofCeilingHalf, row, column)) continue;
					if (TrySpawningTile (localRange, TileIDs.roofEdgeInnerCorner, roofFlatCornerConcave, roofCeilingThreeQuarters, row, column)) continue;
					if (TrySpawningTile (localRange, TileIDs.roofEdgeOuterCorner, roofFlatCornerConvex, roofCeilingQuarter, row, column)) continue;
				}
				else if (roofArray[row, column] != ' ')
				{
					SpawnTile (roofCeiling, row, column, ROTATE_NONE);
					SpawnTile (roofFlatTop, row, column, ROTATE_NONE);
				}
			}
		}
		
		ArrayHelper2D.Print2D (roofArray, true);
	}
	
	void MakePitchedRoof (char[,] blueprint)
	{
		char[,] roofArray = (char[,]) blueprint.Clone();
		int maxRows = roofArray.GetUpperBound (0) + 1;
		int maxColumns = roofArray.GetUpperBound (1) + 1;
		
		// 1. Replace all outer walls with 0s. 2. Replace all other tiles with '?' placeholder.
		for (int row = 0; row < maxRows; row++)
		{
			for (int column = 0; column < maxColumns; column++)
			{
				if (TileIDs.wall.Contains (roofArray[row, column]) == true)
				{
					if (MapHelper.IsListInArray (ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column), TileIDs.ignored))
					{
						roofArray[row, column] = '0';
					}
					else
					{
						roofArray[row, column] = '?';
					}
				}
				else if (TileIDs.ignored.Contains (roofArray[row, column]) == false)
				{
					roofArray[row, column] = '?';
				}
			}
		}
		
		// 3. Replace all tiles adjacent to 0s with 1s, adjacent to 1s with 2s, etc.
		
		char[] heightValues = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
		
		for (int heightVal = 0; heightVal < 9; heightVal++)
		{
			for (int row = 0; row < maxRows; row++)
			{
				for (int column = 0; column < maxColumns; column++)
				{
					if (roofArray[row, column] != '?') continue; // Ignore transformed tiles.
					
					if (MapHelper.IsCharInArray (ArrayHelper2D.ListNeighbouringIndices (roofArray, row, column), heightValues[heightVal]))
					{
						roofArray[row, column] = heightValues[heightVal + 1];
					}
				}
			}
		}
		
		// 4. Spawn roof edge tiles on 0s. 5. Spawn inner slope tiles on all other numbers, at a height equal to their number.
		for (int level = 0; level < 10; level++)
		{
			// For each level of roof
			char down = '?';
			char same = '?';
//			char high = '?';
			char skip = '?';
			float elevation = level * 0.5f;
			
			switch (level)
			{
//				case 1:	down = '0'; same = '1'; high = '2'; break;
//				case 2:	down = '1'; same = '2'; high = '3'; break;
//				case 3:	down = '2'; same = '3'; high = '4'; break;
//				case 4:	down = '3'; same = '4'; high = '5'; break;
//				case 5:	down = '4'; same = '5'; high = '6'; break;
//				case 6:	down = '5'; same = '6'; high = '7'; break;
//				case 7:	down = '6'; same = '7'; high = '8'; break;
//				case 8:	down = '7'; same = '8'; high = '9'; break;
//				case 9:	down = '8'; same = '9'; high = '9'; break;
//				default:down = '9'; same = '9'; high = '9'; break;

				case 1:	down = '0'; same = '1'; break;
				case 2:	down = '1'; same = '2'; break;
				case 3:	down = '2'; same = '3'; break;
				case 4:	down = '3'; same = '4'; break;
				case 5:	down = '4'; same = '5'; break;
				case 6:	down = '5'; same = '6'; break;
				case 7:	down = '6'; same = '7'; break;
				case 8:	down = '7'; same = '8'; break;
				case 9:	down = '8'; same = '9'; break;
				default:down = '9'; same = '9'; break;
			}
			
			// These ID arrays get rebuilt for every iteration of the for(level) loop. I couldn't think of any other method which doesn't involve
			// maintaining 9 redundant copies of the same 11 or more ID arrays, with just two of the values changed between each set.
			
			#region Roof Templates

			char[][] roofStraightHallwayID = MapHelper.DefineTilemask (
				down,	same,	down,
				same,	skip,	same,
				skip,	skip,	skip
			);

			char[][] roofStraightID = MapHelper.DefineTilemask (
				skip,	down,	skip,
				same,	skip,	same,
				skip,	skip,	skip
			);
			
			char[][] roofCornerConvexID = MapHelper.DefineTilemask (
				skip,	same,	skip,
				down,	skip,	same,
				skip,	skip,	down
			);
			
			char[][] roofCornerConvexSpecialID = MapHelper.DefineTilemask (
				skip,	same,	skip,
				down,	skip,	same,
				down,	down,	skip
			);
		
			char[][] roofCornerConvexEnantiomerID = MapHelper.DefineTilemask (
				down,	same,	skip,
				skip,	skip,	same,
				skip,	down,	skip
			);
			
			char[][] roofCornerConcaveID = MapHelper.DefineTilemask (
				skip,	same,	down,
				skip,	skip,	same,
				skip,	skip,	skip
			);
			
			char[][] roofCapSingleID = MapHelper.DefineTilemask (
				skip,	down,	skip,
				same,	skip,	same,
				skip,	down,	skip
			);
			
			char[][] roofCapDoubleID = MapHelper.DefineTilemask (
				skip,	same,	skip,
				same,	skip,	same,
				skip,	down,	down
			);
			
			char[][] roofCapSingleEndID = MapHelper.DefineTilemask (
				skip,	down,	skip,
				same,	skip,	down,
				skip,	down,	down
			);
			
			char[][] roofCapDoubleEndID = MapHelper.DefineTilemask (
				same,	same,	down,
				same,	skip,	down,
				down,	down,	down
			);
			
			char[][] roofCapTopID = MapHelper.DefineTilemask (
				down,	down,	down,
				down,	skip,	down,
				down,	down,	down
			);
			
			char[][] roofCap2WayID = MapHelper.DefineTilemask (
				skip,	same,	down,
				down,	skip,	same,
				down,	down,	down
			);
			
			char[][] roofCap3WayID = MapHelper.DefineTilemask (
				down,	down,	down,
				same,	skip,	same,
				down,	same,	down
			);
			
			char[][] roofCap4WayID = MapHelper.DefineTilemask (
				down,	same,	down,
				same,	skip,	same,
				down,	same,	down
			);
			
			#endregion
			
			for (int row = 0; row < maxRows; row++)
			{
				for (int column = 0; column < maxColumns; column++)
				{
					char[] localRange = ArrayHelper2D.ListNeighbouringIndices (roofArray, row, column);
					
					if (level == 0 && roofArray[row, column] == '0')
					{
						// Ignored tile in range, therefore roof edge.
						if (TrySpawningTile (localRange, TileIDs.roofEdgeStraight, roofPitchedStraight, roofCeilingHalf, row, column)) continue;
						if (TrySpawningTile (localRange, TileIDs.roofEdgeInnerCorner, roofPitchedCornerConcave, roofCeilingThreeQuarters, row, column)) continue;
						if (TrySpawningTile (localRange, TileIDs.roofEdgeOuterCorner, roofPitchedCornerConvex, roofCeilingQuarter, row, column)) continue;
					}
					
					if (level == (roofArray[row, column] - '0')) // Subtracting the value of char0 from a char number gets you the integer version.
					{
						SpawnTile (roofCeiling, row, column, ROTATE_NONE);
						
						if (TrySpawningRoof (localRange, roofCap2WayID, roofPitchedCap2Way, row, column, elevation) == true) continue;
						if (TrySpawningRoof (localRange, roofCap3WayID, roofPitchedCap3Way, row, column, elevation) == true) continue;
						if (TrySpawningRoof (localRange, roofCap4WayID, roofPitchedCap4Way, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCapSingleID, roofPitchedCapSingle, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCapSingleEndID, roofPitchedCapSingleEnd, row, column, elevation) == true) continue;
						if (TrySpawningRoof (localRange, roofCapDoubleEndID, roofPitchedCapDoubleEnd, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCapTopID, roofPitchedCapTop, row, column, elevation) == true) continue;
					
						if (TrySpawningRoof (localRange, roofStraightHallwayID, roofPitchedStraight, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCornerConvexID, roofPitchedCornerConvex, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCornerConvexSpecialID, roofPitchedCornerConvex, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCornerConvexEnantiomerID, roofPitchedCornerConvex, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCapDoubleID, roofPitchedCapDouble, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofCornerConcaveID, roofPitchedCornerConcave, row, column, elevation) == true) continue;
						
						if (TrySpawningRoof (localRange, roofStraightID, roofPitchedStraight, row, column, elevation) == true) continue;
					}
				}
			}
		}

		
		
		ArrayHelper2D.Print2D (roofArray, true);
	}
	
	void spawnWall (int row, int column)
	{
		char[] localRange = ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column);
	
		if (MapHelper.IsListInArray (localRange, TileIDs.ignored))
		{
			// Ignored tile in range, therefore outer wall.
			
			// Inner corners
			if (TrySpawningTile (localRange, TileIDs.outerWallInnerCorner, outerWallCornerInner, floorEdgingThreeQuarter, row, column))
			{
				// Check if this intersects with an inner wall. Same check happens in Straight sections, but not with Outer sections.
				// This is not an if->return call because it's possible for an inner corner to have two interior walls intersecting it.
				// Having no return saves me from defining another tilemask for this special case.
				TrySpawningTile (localRange, TileIDs.outerWallInnerCornerIntersection, innerWallMerge, row, column);
				TrySpawningTile (localRange, TileIDs.outerWallInnerCornerIntersectionEnantiomer, innerWallMerge, row, column);
				return;
			}
			
			// Straight wall
			if (TrySpawningTile (localRange, TileIDs.outerWallStraight, outerWallWall, floorEdgingHalf, row, column))
			{
				TrySpawningTile (localRange, TileIDs.outerWallStraightIntersection, innerWallMerge, row, column);
				return;
			}
			
			// Outer corners
			if (TrySpawningTile (localRange, TileIDs.outerWallOuterCorner, outerWallCornerOuter, floorEdgingQuarter, row, column)) return;
		}
		else
		{
			// Inner wall.
			SpawnTile (floorEdgingFull, row, column, ROTATE_NONE);
			
			// 4-way intersection
			if (TrySpawningTile (localRange, TileIDs.innerWall4Way, innerWall4Way, row, column)) return;
			
			// 3-way intersection
			if (TrySpawningTile (localRange, TileIDs.innerWall3Way, innerWall3Way, row, column)) return;
			
			// Straight wall
			if (TrySpawningTile (localRange, TileIDs.innerWallStraight, innerWallWall, row, column)) return;
			
			// Corner
			if (TrySpawningTile (localRange, TileIDs.innerWallCorner, innerWallCorner, row, column)) return;
		}
	}
	
	void spawnDoor (int row, int column)
	{
		char[] localRange = ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column);
		
		if (MapHelper.IsListInArray (localRange, TileIDs.ignored))
		{
			// Ignored tile in range, therefore outer door.
			if (TrySpawningTile (localRange, TileIDs.outerWallDoor, outerWallDoor, floorEdgingHalf, row, column)) return;
		}
		else
		{
			// Inner door.
			SpawnTile (floorEdgingFull, row, column, ROTATE_NONE);
			
			if (TrySpawningTile (localRange, TileIDs.innerWallDoor, innerWallDoor, row, column)) return;
		}
	}
	
	void spawnWindow (int row, int column)
	{
		char[] localRange = ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column);
		
		if (TrySpawningTile (localRange, TileIDs.outerWallWindow, outerWallWindowSingle, floorEdgingHalf, row, column)) return;
	}
	
	void processLayer (char[,] blueprint)
	{
		/*
			f
			v
		*/
		
		for (int row = 0; row < (blueprint.GetUpperBound (0) + 1); row++)
		{
			for (int column = 0; column < (blueprint.GetUpperBound (1) + 1); column++)
			{
				char[] localRange = ArrayHelper2D.ListNeighbouringIndices (blueprint, row, column);
				switch (blueprint[row, column])
				{
					case 'K':
						if (TrySpawningTile (localRange, TileIDs.furnitureModularCorner, kitchenCounterCorner, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureModularAtWindow, kitchenCounterStraightLow, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureModularStraight, kitchenCounterStraight, row, column)) break;
						break;
					case 'S':
						if (TrySpawningTile (localRange, TileIDs.furnitureModularStraight, kitchenCounterStove, row, column)) break;
						break;
					case 's':
						if (TrySpawningTile (localRange, TileIDs.furnitureModularAtWindow, kitchenCounterSink, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureModularStraight, kitchenCounterSink, row, column)) break;
						break;
					case 'B':
						if (TrySpawningTile (localRange, TileIDs.furnitureTwoPieceAsymmetrical, bedroomBedSingle, row, column)) break;
						break;
					case 'c':
						if (TrySpawningTile (localRange, TileIDs.furnitureModularStraight, generalBookshelf, row, column)) break;
						break;
					case 't':
						if (TrySpawningTile (localRange, TileIDs.furnitureTwoPieceSymmetrical, generalTableDining, row, column)) break;
						SpawnTile (generalEndtable, row, column, ROTATE_NONE);
						break;
					case 'd':
						if (TrySpawningTile (localRange, TileIDs.furnitureTwoPieceAsymmetrical, generalTableDesk, row, column)) break;
						break;
					case 'h':
						if (TrySpawningTile (localRange, TileIDs.furnitureChairAlignment, generalChairDining, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaModularCorner, generalChairSofaCorner, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaModularStraight, generalChairSofaStraight, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaModularEnd, generalChairSofaCorner, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaModularEndEnantiomer, generalChairSofaCorner, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaSingle, generalChairSofaSingle, row, column)) break;
						if (TrySpawningTile (localRange, TileIDs.furnitureSofaSingleEnantiomer, generalChairRocking, row, column)) break;
						break;
					case 'l':
						if (generateLights == true) SpawnTile (generalLampFloor, row, column, ROTATE_NONE);
						break;
					case 'w':
						if (TrySpawningTile (localRange, TileIDs.furnitureModularStraight, bedroomWardrobe, row, column)) break;
						break;
					
					// If not furniture, do nothing.
					default:
						break;
				}
			}
		}
	}
	
	bool TrySpawningTile (char[] needleArray, List<char>[][] templateArray, GameObject tilePrefab, int row, int column)
	{
		Quaternion horizontalRotation;
		
		if (TileMatchesTemplate (needleArray, templateArray, out horizontalRotation) == true)
		{
			SpawnTile (tilePrefab, row, column, horizontalRotation);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	bool TrySpawningTile (char[] needleArray, List<char>[][] templateArray, GameObject tilePrefab, GameObject tilePrefab2, int row, int column)
	{
		Quaternion horizontalRotation;
		
		if (TileMatchesTemplate (needleArray, templateArray, out horizontalRotation) == true)
		{
			SpawnTile (tilePrefab, row, column, horizontalRotation);
			SpawnTile (tilePrefab2, row, column, horizontalRotation);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	bool TrySpawningRoof (char[] needleArray, char[][] templateArray, GameObject tilePrefab, int row, int column, float height)
	{
		Quaternion horizontalRotation;
		
		if (TileMatchesTemplate (needleArray, templateArray, out horizontalRotation) == true)
		{
			SpawnTile (tilePrefab, row, column, height, horizontalRotation);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	#region tileMatchesTemplate() overloads
	bool TileMatchesTemplate (char[] needleArray, char[][] templateArray, out Quaternion horizontalRotation)
	{
		horizontalRotation = ROTATE_NONE;
		
		for (int i = 0; i < (templateArray.Length); i++)
		{
			for (int j = 0; j < 9; j++)
			{
				if (j == 4) continue; // Skip checking the centre position (no need to ascertain that a block is what it says it is).
				
				if (templateArray[i][j] != '?')
				{
					if (templateArray[i][j] != needleArray[j]) break;
				}
				
				if (j == 8) // The loop has iterated nine times without stopping, so all tiles must match.
				{
					switch (i)
					{
						case 0:
							horizontalRotation = ROTATE_NONE;
							break;
						case 1:
							horizontalRotation = ROTATE_RIGHT;
							break;
						case 2:
							horizontalRotation = ROTATE_FLIP;
							break;
						case 3:
							horizontalRotation = ROTATE_LEFT;
							break;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static bool TileMatchesTemplate (char[] needleArray, List<char>[][] tileMaskJaggedArray, out Quaternion horizontalRotation)
	{
		horizontalRotation = ROTATE_NONE;
		
		for (int i = 0; i < (tileMaskJaggedArray.Length); i++)
		{
			for (int j = 0; j < 9; j++)
			{
				if (j == 4) continue; // Skip checking the centre position (no need to ascertain that a block is what it says it is).
				
				if (tileMaskJaggedArray[i][j].Count != 0)
				{
					if (tileMaskJaggedArray[i][j].Contains (needleArray[j]) == false) break;
				}
				
				if (j == 8) // The loop has iterated nine times without stopping, so all tiles must match.
				{
					switch (i)
					{
						case 0:
							horizontalRotation = ROTATE_NONE;
							break;
						case 1:
							horizontalRotation = ROTATE_RIGHT;
							break;
						case 2:
							horizontalRotation = ROTATE_FLIP;
							break;
						case 3:
							horizontalRotation = ROTATE_LEFT;
							break;
					}
					return true;
				}
			}
		}
		return false;
	}
	#endregion
	
	#region spawnTile() overloads
	void SpawnTile (GameObject tilePrefab, int row, int column, Quaternion horizontalRotation)
	{
		Instantiate (tilePrefab, new Vector3 (column, 0, -row), horizontalRotation);
	}
	
	void SpawnTile (GameObject tilePrefab, int row, int column, float height, Quaternion horizontalRotation)
	{
		Instantiate (tilePrefab, new Vector3 (column, height, -row), horizontalRotation);
	}
	#endregion
}