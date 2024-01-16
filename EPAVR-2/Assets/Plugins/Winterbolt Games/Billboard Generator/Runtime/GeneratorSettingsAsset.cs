using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	/// <summary>
	/// An asset that contains a <see cref="GeneratorSettings"/>.
	/// </summary>
	[CreateAssetMenu(menuName = "Billboard Generator/Generator Settings Asset")]
	public sealed class GeneratorSettingsAsset : ScriptableObject
	{
		public GeneratorSettings settings;
	}
}