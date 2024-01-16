using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Utilities
{
	/// <summary>
	/// A utility class that provides methods for packing textures into atlases.
	/// </summary>
	public static class PackingUtilities
	{
		/// <summary>
		/// Tightly packs the supplied <paramref name="textures"/> into a texture atlas and throws an exception if it cannot pack one of them.
		/// </summary>
		/// <param name="textures">The textures to pack.</param>
		/// <param name="freeRectChoiceHeuristic">TBA.</param>
		/// <param name="initialBinWidth">When greater than 0, this value is used as the initial bin width instead of automatically calculating it. Sometimes this might reduce generation time and yields better results.</param>
		/// <param name="initialBinHeight">When greater than 0, this value is used as the initial bin height instead of automatically calculating it. Sometimes this might reduce generation time and yields better results.</param>
		/// <returns>A texture atlas consisting of tightly-packed textures from <paramref name="textures"/>.</returns>
		public static (Texture2D, List<UVCoordinates>) PackTight(IList<Texture2D> textures, MaxRectsBin.FreeRectChoiceHeuristic freeRectChoiceHeuristic = MaxRectsBin.FreeRectChoiceHeuristic.RectBestAreaFit, int initialBinWidth = -1, int initialBinHeight = -1)
		{
			for (int i = 0; i < textures.Count; i++)
			{
				textures[i] = TextureUtilities.Pad(TextureUtilities.Trim(textures[i]), 2, 2);
			}

			Vector2Int binSize = initialBinWidth > 0 && initialBinHeight > 0 ? new Vector2Int(initialBinWidth, initialBinHeight) : new Vector2Int(textures.Max(texture => texture.width), textures.Max(texture => texture.height)) * (int)Math.Ceiling(Math.Sqrt(textures.Count));

			List<(Rect Rect, Texture2D Texture)> pairs = new List<(Rect Rect, Texture2D Texture)>();

			MaxRectsBin bin = new MaxRectsBin(binSize.x, binSize.y, false);

			foreach (Texture2D texture in textures)
			{
				pairs.Add((bin.Insert(texture.width, texture.height, freeRectChoiceHeuristic), texture));
			}

			Vector2Int binExtents = bin.Extents;

			Texture2D atlas = new Texture2D(binExtents.x, binExtents.y, TextureFormat.ARGB32, false, false);

			for (int x = 0; x < binExtents.x; x++)
			{
				for (int y = 0; y < binExtents.y; y++)
				{
					atlas.SetPixel(x, y, Color.clear);
				}
			}

			for (int i = 0; i < pairs.Count; i++)
			{
				if (pairs[i].Rect == Rect.zero) throw new Exception($"Unable to pack texture number {i + 1}. Consider increasing the '{nameof(GeneratorSettings.initialBinWidth)}' and '{nameof(GeneratorSettings.initialBinHeight)}' then re-generating.");

				atlas.SetPixels32((int)pairs[i].Rect.x, (int)pairs[i].Rect.y, (int)pairs[i].Rect.width, (int)pairs[i].Rect.height, pairs[i].Texture.GetPixels32());
			}

			atlas.Apply();

			List<UVCoordinates> uvCoordinates = new List<UVCoordinates>();

			for (int i = 0; i < pairs.Count; i++)
			{
				uvCoordinates.Add(new UVCoordinates(pairs[i].Rect.x, pairs[i].Rect.y, pairs[i].Rect.width, pairs[i].Rect.height, atlas.width, atlas.height));
			}

			return (atlas, uvCoordinates);
		}
	}
}
