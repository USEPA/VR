using System;

using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	/// <summary>
	/// Represents the UV coordinates of a quad.
	/// </summary>
	[Serializable]
	public struct UVCoordinates
	{
		/// <summary>
		/// The bottom-left UV coordinate of the quad.
		/// </summary>
		public Vector2 bottomLeft;

		/// <summary>
		/// The bottom-right UV coordinate of the quad.
		/// </summary>
		public Vector2 bottomRight;

		/// <summary>
		/// The top-right UV coordinate of the quad.
		/// </summary>
		public Vector2 topRight;

		/// <summary>
		/// The top-left UV coordinate of the quad.
		/// </summary>
		public Vector2 topLeft;

		/// <summary>
		/// The u-scale of these coordinates.
		/// </summary>
		public float UScale => topRight.x - topLeft.x;

		/// <summary>
		/// The v-scale of these coordinates.
		/// </summary>
		public float VScale => topRight.y - bottomRight.y;

		/// <summary>
		/// Creates a new <see cref="UVCoordinates"/>.
		/// </summary>
		/// <param name="x">Position of this UV coordinates on the x-axis in pixels.</param>
		/// <param name="y">Position of this UV coordinates on the y-axis in pixels.</param>
		/// <param name="width">Width of this UV coordinates in the UV map in pixels.</param>
		/// <param name="height">Height of this UV coordinates in the UV map in pixels.</param>
		/// <param name="uvMapWidth">The total width of the UV map in pixels.</param>
		/// <param name="uvMapHeight">The total height of the UV map in pixels.</param>
		public UVCoordinates(float x, float y, float width, float height, float uvMapWidth, float uvMapHeight)
		{
			topLeft = new Vector2(x / uvMapWidth, (y + height) / uvMapHeight);

			topRight = new Vector2((x + width) / uvMapWidth, (y + height) / uvMapHeight);

			bottomLeft = new Vector2(x / uvMapWidth, y / uvMapHeight);

			bottomRight = new Vector2((x + width) / uvMapWidth, y / uvMapHeight);
		}

		public override string ToString()
		{
			return $"{nameof(UVCoordinates)}: {nameof(topLeft)} = {topLeft}, {nameof(topRight)} = {topRight}, {nameof(bottomLeft)} = {bottomLeft}, {nameof(bottomRight)} = {bottomRight}";
		}
	}
}
