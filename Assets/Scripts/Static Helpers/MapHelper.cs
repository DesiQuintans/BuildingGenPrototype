using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapHelper {
	
	// Constants.
	public static readonly Quaternion ROTATE_NONE = Quaternion.Euler(-90, 0, 0);
	public static readonly Quaternion ROTATE_RIGHT = Quaternion.Euler(-90, 90, 0);
	public static readonly Quaternion ROTATE_FLIP = Quaternion.Euler(-90, 180, 0);
	public static readonly Quaternion ROTATE_LEFT = Quaternion.Euler(-90, -90, 0);
	
	public static bool IsCharInArray (char[] haystack, char needle)
	{
		foreach (char straw in haystack)
		{
			if (straw.Equals(needle)) return true;
		}

		return false;
	}
	
	public static bool IsListInArray (char[] haystack, List<char> needleList)
	{
		foreach (char straw in haystack)
		{
			if (needleList.Contains (straw)) return true;
		}

		return false;
	}
	
	public static char[] RotateLocalRange (char[] localRange, int rotations)
	{
		char[] rotatedList = new char[9] {localRange[0], localRange[1], localRange[2], localRange[3], localRange[4], localRange[5], localRange[6], localRange[7], localRange[8]};
		
		for (int i = 0; i < rotations; i++)
		{
			
			char[] tempList = new char[9] {rotatedList[6], rotatedList[3], rotatedList[0], rotatedList[7], rotatedList[4], rotatedList[1], rotatedList[8], rotatedList[5], rotatedList[2]};
			rotatedList = tempList;
		}
		
		return rotatedList;
	}
	
	public static List<char>[] RotateLocalRange (List<char>[] localRange, int rotations)
	{
		List<char>[] rotatedList = new List<char>[9] {localRange[0], localRange[1], localRange[2], localRange[3], localRange[4], localRange[5], localRange[6], localRange[7], localRange[8]};
		
		for (int i = 0; i < rotations; i++)
		{
			
			List<char>[] tempList = new List<char>[9] {rotatedList[6], rotatedList[3], rotatedList[0], rotatedList[7], rotatedList[4], rotatedList[1], rotatedList[8], rotatedList[5], rotatedList[2]};
			rotatedList = tempList;
		}
		
		return rotatedList;
	}
	
	public static char[][] DefineTilemask (char nW, char n, char nE, char w, char centre, char e, char sW, char s, char sE)
	{
		char[] template = new char[9] {
			nW,	n,		nE,
			w,	centre,	e,
			sW,	s,		sE
		};
		
		return new char[4][] {
			RotateLocalRange (template, 0),
			RotateLocalRange (template, 1),
			RotateLocalRange (template, 2),
			RotateLocalRange (template, 3)
		};
	}
	
	public static List<char>[][] DefineTilemask (List<char> nW, List<char> n, List<char> nE, List<char> w, List<char> centre, List<char> e, List<char> sW, List<char> s, List<char> sE)
	{
		List<char>[] template = new List<char>[9] {
			nW,	n,		nE,
			w,	centre,	e,
			sW,	s,		sE
		};
		
		return new List<char>[4][] {
			RotateLocalRange (template, 0),
			RotateLocalRange (template, 1),
			RotateLocalRange (template, 2),
			RotateLocalRange (template, 3)
		};
	}
}
