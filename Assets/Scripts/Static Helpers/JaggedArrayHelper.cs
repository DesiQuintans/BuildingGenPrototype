using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class JaggedArrayHelper
{
	/// <summary>
	/// Converts a string to a jagged int. Separate outer indices with vertical bars and inner indices with commas.
	/// </summary>
	/// <returns>
	/// int[][] with as many subindices as specified in the arguments.
	/// </returns>
	/// <param name='myString'>
	/// Comma- and pipe-separated string of numbers.
	/// </param>
	/// <param name='numberOfSubindices'>
	/// Number of subindices in the inner array.
	/// </param>
	public static int[][] StringToJaggedInt (string myString, int numberOfSubindices)
	{
		myString = myString.TrimEnd ('|');
		string[][] stringArray = myString.Split ('|').Select (t => t.Split(',')).ToArray ();
		int[][] intArray = new int[stringArray.Length][];
		
		for (int i = 0; i < stringArray.Length; i++)
		{
			intArray[i] = new int[numberOfSubindices];
			for (int j = 0; j < numberOfSubindices; j++)
			{
				intArray[i][j] = int.Parse (stringArray[i][j]);
			}
		}
		
		return intArray;
	}
	
	/// <summary>
	/// Prints the jagged array.
	/// </summary>
	/// <param name='jagged'>
	/// Jagged array.
	/// </param>
	/// <typeparam name='T'>
	/// The 1st type parameter.
	/// </typeparam>
	public static void PrintJagged<T> (T[][] jagged)
	{
		for (int i = 0; i < jagged.Length; i++)
		{
			for (int j = 0; j < jagged[i].Length; j++)
			{
				Debug.Log (jagged[i][j]);
			}
		}
	}
}
