using System.Collections.Generic;

using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	[CreateAssetMenu(menuName = "Billboard Generator/Billboard Atlas Asset")]
	public class BillboardAtlasAsset : ScriptableObject
	{
		public Vector3 rendererScale;

		public List<UVCoordinates> uvCoordinates;
	}
}
