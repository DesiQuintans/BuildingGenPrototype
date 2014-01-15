using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileIDs
{
	// Tile symbol lists.
	#region General lists
		public static readonly List<char> any = new List<char>() {};
		public static readonly List<char> ignored = new List<char>() {' ', '_'};
		public static readonly List<char> foot = new List<char>() {'+'};
	#endregion
	
	#region Structural layer
		public static readonly List<char> floor = new List<char>()
		{
			'.', 'L'
		};
		
		public static readonly List<char> passable = new List<char>()
		{
			'.', 'D', 'L'
		};
		
		public static readonly List<char> wall = new List<char>()
		{
			'#', 'D', 'W'
		};
		
		public static readonly List<char> opening = new List<char>() {'D', 'W'};
		
		public static readonly List<char> structural = CombineLists (new List<char>[]
		{
			floor, passable, wall, opening
		});
	#endregion
	
	#region Furniture layer
		public static readonly List<char> kitchen = new List<char>()
		{
			'K', 'f', 's', 'S'
		};
		public static readonly List<char> bedroom = new List<char>()
		{
			'B', 'w'
		};
		public static readonly List<char> general = new List<char>()
		{
			'h', 't', 'd', 'c', 'l', 'v'
		};
		public static readonly List<char> bathroom = new List<char>()
		{
			's'
		};
		
		public static readonly List<char> table = new List<char>()
		{
			't', 'd'
		};
		
		public static readonly List<char> twopiece = new List<char>()
		{
			't', 'd'
		};
		
		public static readonly List<char> seating = new List<char>()
		{
			'h'
		};
	
		public static readonly List<char> furniture = CombineLists (new List<char>[]
		{
			kitchen, bedroom, general, bathroom
		});
	#endregion
	
	#region Roof layer
		public static readonly List<char> roofNum0 = new List<char>() {'0'};
	#endregion
	
	static List<char> CombineLists (List<char>[] lists)
	{
		List<char> tempList = new List<char>();
		foreach (List<char> list in lists)
		{
			tempList.AddRange (list);
		}
		return tempList.Distinct().ToList();
	}
	
	
	// Each of the following jagged arrays are filled in order of rotation. I should be able to use the index of the outermost array
	// to give rotation to the final model, as follows:
	//		jagged[0] -> ROTATE_NONE
	//		jagged[1] -> ROTATE_RIGHT
	//		jagged[2] -> ROTATE_FLIP
	//		jagged[3] -> ROTATE_LEFT
	
	public static List<char>[][] outerWallStraightIntersection = MapHelper.DefineTilemask
	(
		any,			ignored,			any,
		wall,			any,				wall,
		any,			wall,				any
	);
	
	public static List<char>[][] outerWallInnerCornerIntersection = MapHelper.DefineTilemask
	(
		any, 			wall, 				ignored,
		any, 			any,				wall,
		any, 			wall,				any
	);
	
	public static List<char>[][] outerWallInnerCornerIntersectionEnantiomer = MapHelper.DefineTilemask
	(
		ignored, 		wall, 				any,
		wall, 			any,				any,
		any,			wall, 				any
	);
	
	public static List<char>[][] outerWallStraight = MapHelper.DefineTilemask
	(
		any,			ignored,			any,
		wall,			any,				wall,
		any,			any,				any
	);
	
	public static List<char>[][] outerWallInnerCorner = MapHelper.DefineTilemask
	(
		any, 			wall, 				ignored,
		any, 			any,				wall,
		any, 			any,				any
	);
	
	public static List<char>[][] outerWallOuterCorner = MapHelper.DefineTilemask
	(
		any,			wall,				any,
		any,			any,				wall,
		ignored,		any,				any
	);
	
	public static List<char>[][] innerWall4Way = MapHelper.DefineTilemask
	(
		any,			wall,				any,
		wall,			any,				wall,
		any,			wall,				any
	);
	
	public static List<char>[][] innerWall3Way = MapHelper.DefineTilemask
	(
		any,			wall,				any,
		wall,			any,				wall,
		any,			any,				any
	);
	
	public static List<char>[][] innerWallCorner = MapHelper.DefineTilemask
	(
		any,			wall,				any,
		any,			any,				wall,
		floor,			any,				any
	);
	
	public static List<char>[][] innerWallStraight = MapHelper.DefineTilemask
	(
		any,			floor,				any,
		wall,			any,				wall,
		any,			floor,				any
	);
	
	public static List<char>[][] outerWallDoor = MapHelper.DefineTilemask
	(
		any,			any,				any,
		wall,			any,				wall,
		any,			floor,				any
	);
	
	public static List<char>[][] innerWallDoor = MapHelper.DefineTilemask
	(
		any,			floor,				any,
		wall,			any,				wall,
		any,			floor,				any
	);
	
	public static List<char>[][] outerWallWindow = MapHelper.DefineTilemask
	(
		any,			ignored,			any,
		wall,			any,				wall,
		any,			any,				any
	);
	
	public static List<char>[][] roofEdgeInnerCorner = MapHelper.DefineTilemask
	(
		any,			roofNum0,			ignored,
		any,			any,				roofNum0,
		any,			any,				any
	);
	
	public static List<char>[][] roofEdgeOuterCorner = MapHelper.DefineTilemask
	(
		any,			roofNum0,			any,
		any,			any,				roofNum0,
		ignored,		any,				any
	);
	
	public static List<char>[][] roofEdgeStraight = MapHelper.DefineTilemask
	(
		any,			ignored,			any,
		roofNum0,		any,				roofNum0,
		any,			any,				any
	);
	
	public static List<char>[][] stairs = MapHelper.DefineTilemask
	(
		any,			any,				any,
		foot,			any,				any,
		any,			any,				any
	);
	
	// Furniture
	
	public static List<char>[][] furnitureModularCorner = MapHelper.DefineTilemask
	(
		any,			any,				any,
		any,			any,				furniture,
		any,			furniture,			passable
	);
	
	public static List<char>[][] furnitureModularStraight = MapHelper.DefineTilemask
	(
		any,			any,				any,
		any,			any,				any,
		any,			passable,			any
	);
	
	public static List<char>[][] furnitureModularAtWindow = MapHelper.DefineTilemask
	(
		any,			opening,			any,
		any,			any,				any,
		any,			passable,			any
	);
	
	public static List<char>[][] furnitureTwoPieceAsymmetrical = MapHelper.DefineTilemask
	(
		any,			foot,				any,
		any,			any,				any,
		any,			any,				any
	);
	
	public static List<char>[][] furnitureTwoPieceSymmetrical = MapHelper.DefineTilemask
	(
		any,			any,				any,
		twopiece,		any,				any,				
		any,			any,				any
	);
	
	public static List<char>[][] furnitureChairAlignment = MapHelper.DefineTilemask
	(
		any,			table,				any,
		any,			any,				any,
		any,			any,				any
	);
	
	public static List<char>[][] furnitureSofaSingle = MapHelper.DefineTilemask
	(
		seating,		any,				any,
		passable,		any,				any,
		any,			any,				any
	);
	
	public static List<char>[][] furnitureSofaSingleEnantiomer = MapHelper.DefineTilemask
	(
		any,			any,				any,
		passable,		any,				any,
		seating,		any,				any
	);
	
	public static List<char>[][] furnitureSofaModularCorner = MapHelper.DefineTilemask
	(
		any,			any,				any,
		any,			any,				seating,
		any,			seating,			any
	);
	
	public static List<char>[][] furnitureSofaModularStraight = MapHelper.DefineTilemask
	(
		any,			any,				any,
		seating,		any,				seating,
		any,			passable,			any
	);
	
	public static List<char>[][] furnitureSofaModularEnd = MapHelper.DefineTilemask
	(
		any,			any,				any,
		any,			any,				seating,
		any,			passable,			any
	);
	
	public static List<char>[][] furnitureSofaModularEndEnantiomer = MapHelper.DefineTilemask
	(
		any,			any,				any,
		any,			any,				passable,
		any,			seating,			any
	);
}
