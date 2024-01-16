using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor.Utilities
{
	internal static class AtlasUtilities
	{
		public static void CreateAndSave(Texture2D texture, List<UVCoordinates> uvs)
		{
			string texturePath = EditorUtility.SaveFilePanel("Save As...", Application.dataPath, "Texture", "png");

			if (string.IsNullOrWhiteSpace(texturePath)) return;

			File.WriteAllBytes(texturePath, texture.EncodeToPNG());

			BillboardAtlasAsset atlasAsset = ScriptableObject.CreateInstance<BillboardAtlasAsset>();

			int atlasWidth = texture.width;
			int atlasHeight = texture.height;

			float averageWidthToHeightRatio = uvs.Average(uv => (uv.UScale * atlasWidth) / (uv.VScale * atlasHeight));
			float averageHeightToWidthRatio = uvs.Average(uv => (uv.VScale * atlasHeight) / (uv.UScale * atlasWidth));

			atlasAsset.rendererScale = new Vector3(averageWidthToHeightRatio, averageHeightToWidthRatio, 1.0f);

			atlasAsset.uvCoordinates = uvs;

			string atlasPath = Path.Combine(Path.GetDirectoryName(PathUtilities.ToProjectPath(texturePath)), $"{Path.GetFileNameWithoutExtension(texturePath)}.asset");

			AssetDatabase.CreateAsset(atlasAsset, atlasPath);

			AssetDatabase.Refresh();
		}
	}
}
