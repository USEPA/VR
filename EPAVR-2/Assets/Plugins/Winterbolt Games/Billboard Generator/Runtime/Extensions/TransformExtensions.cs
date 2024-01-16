using System.Collections.Generic;
using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Extensions
{
	/// <summary>
	/// Extensions for <see cref="Transform"/>.
	/// </summary>
	public static class TransformExtensions
	{
		/// <summary>
		/// Iterates over the children of the specified <paramref name="transform"/> without allocating.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnumerable<Transform> ChildrenIterator(this Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				yield return transform.GetChild(i);
			}
		}

		/// <summary>
		/// Get the an array containing the specified <paramref name="transform"/> and all of its children.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns>
		/// An array containing <paramref name="transform"/> and all of its children.
		/// </returns>
		public static IReadOnlyList<Transform> GetTree(this Transform transform)
		{
			List<Transform> tree = new List<Transform>
			{
				transform,
			};

			foreach (Transform child in ChildrenIterator(transform))
			{
				if (child.childCount > 0)
				{
					tree.AddRange(GetTree(child));
				}
				else
				{
					tree.Add(child);
				}
			}

			return tree.ToArray();
		}
	}
}
