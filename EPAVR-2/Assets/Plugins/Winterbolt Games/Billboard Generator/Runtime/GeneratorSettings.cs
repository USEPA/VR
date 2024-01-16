using System;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime.Utilities;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	/// <summary>
	/// A structure that represents the settings used by <see cref="Generator"/>.
	/// </summary>
	[Serializable]
	public struct GeneratorSettings
	{
		[Range(1, 360)]
		[Tooltip("The number of angles to capture the object from.")]
		public int directions;

		[Tooltip("The width of the generated billboard texture.")]
		public int textureWidth;

		[Tooltip("The height of the generated billboard texture.")]
		public int textureHeight;

		[Tooltip("When true, uses a copy of the main camera in the scene to generate billboards otherwise, creates a new camera with the settings specified here.")]
		public bool useMainCamera;

		[Tooltip("The color of the billboard texture's background.")]
		public Color textureBackgroundColor;

		[Tooltip("When true, uses orthographic projection when generating billboards otherwise, uses perspective projection.")]
		public bool useOrthographicCamera;

		[Tooltip("When 'UseOrthographicCamera' is set to true, this field means how big the size of the orthographic projection is otherwise, it means how wide the field of view of the perspective projection is.")]
		public float cameraSizeOrFieldOfView;

		[Tooltip("The clear flags of the camera used to generate billboards.")]
		public CameraClearFlags cameraClearFlags;

		[Tooltip("The camera position offset from the origin of the object to generate billboards for.")]
		public Vector3 cameraOffset;

		[Tooltip("When packing generated billboard textures, which choice heuristic function should be used? (RECOMMENDED: Left Bottom Rule or Contact Point Rule)")]
		public MaxRectsBin.FreeRectChoiceHeuristic freeRectChoiceHeuristic;

		[Tooltip("Instead of automatically determining the initial atlas width, the generator uses this value which might improve the generation speed and sometimes yield better results. Ignored when <= 0.")]
		public int initialBinWidth;

		[Tooltip("Instead of automatically determining the initial atlas height, the generator uses this value which might improve the generation speed and sometimes yield better results. Ignored when <= 0.")]
		public int initialBinHeight;

		[Tooltip("Should only the target game object be rendered?")]
		public bool isolateTarget;
	}
}
