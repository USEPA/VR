using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal sealed class GeneratorWindowEntry
	{
		public bool IsExpanded;

		public GameObject GameObject;

		public GeneratorSettingsAsset SettingsAsset;

		public bool AreSettingsExpanded;
	}
}
