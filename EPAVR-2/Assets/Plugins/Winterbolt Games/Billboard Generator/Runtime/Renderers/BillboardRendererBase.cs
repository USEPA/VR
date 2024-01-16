using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Renderers
{
	/// <summary>
	/// The base class of all billboard types included in Billboard Generator.
	/// </summary>
	public abstract class BillboardRendererBase : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The target of the billboard renderer.")]
		protected Transform target;

		[SerializeField]
		[Tooltip("When true, the billboard will pick the current active camera as it's target.")]
		protected bool useBillboardTarget;

		[Tooltip("If true, the billboard will need a MeshFilter and a MeshRenderer otherwise it will use Graphics.DrawMesh.")]
		public bool useAlternativeWorkflow;

		/// <summary>
		/// The mesh drawn by the billboard renderer when not using the alternative workflow.
		/// </summary>
		protected Mesh mesh;

		[SerializeField]
		[Tooltip("The material used by the billboard renderer when NOT using the alternative workflow.")]
		protected Material material;

		/// <summary>
		/// Cached reference to <see cref="Component.transform"/>.
		/// </summary>
		protected Transform cachedTransform;

		[SerializeField]
		[Tooltip("The mesh filter used by the billboard renderer when using the alternative workflow.")]
		protected MeshFilter meshFilter;

		[SerializeField]
		[Tooltip("The mesh renderer used to the billboard renderer when using the alternative workflow.")]
		protected MeshRenderer meshRenderer;

		[Tooltip("Disable this if the billboard is included in a batch.")]
		public bool update;

		[Tooltip("Disable this if the billboard is using the alternative workflow.")]
		public bool render;

		/// <summary>
		/// The last recorded target position.
		/// </summary>
		protected Vector3 lastTargetPosition = new Vector3(float.NaN, float.NaN, float.NaN);

		/// <summary>
		/// An array that holds the current UV coordinates of the billboard texture that represents the current angle in the billboard texture atlas.
		/// </summary>
		protected readonly Vector2[] uvs = new Vector2[4];

		protected static readonly Vector3[] QuadVertices = { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 0) };
		
		protected static readonly Vector2[] QuadUVs = { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), };
		
		protected static readonly int[] QuadTriangles = { 2, 1, 0, 3, 2, 0, };

		protected virtual void Start() => PrepareBillboard();

		protected virtual void LateUpdate()
		{
			if (useBillboardTarget && target != BillboardTarget.TargetTransform)
			{
				target = BillboardTarget.TargetTransform;
			}

			if (target == null) return;

			if (update) UpdateBillboard();

			if (render && !useAlternativeWorkflow) RenderBillboard();
		}

		/// <summary>
		/// Checks if this billboard is properly setup or not.
		/// </summary>
		/// <param name="warn">Whether to warn about errors by logging to the console or not.</param>
		/// <returns>Whether this billboard is properly setup or not.</returns>
		public virtual bool IsValid(bool warn = true)
		{
			return true;
		}

		/// <summary>
		/// Creates a new quad mesh that can be utilized by the <see cref="RuntimeBillboardRenderer"/>
		/// </summary>
		/// 
		/// <returns>
		/// A quad mesh with whose UV coordinates are in the following order: Bottom-Left > Bottom-Right > Top-Right > Top-Left.
		/// </returns>
		protected virtual Mesh MakeQuad()
		{
			Mesh mesh = new Mesh
			{
				name = "Billboard",

				vertices = QuadVertices,
				
				uv = QuadUVs,
				
				triangles = QuadTriangles,
			};

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			mesh.MarkDynamic();

			return mesh;
		}

		/// <summary>
		/// Prepares the billboard.
		/// </summary>
		public virtual void PrepareBillboard()
		{
			if (useBillboardTarget) target = BillboardTarget.TargetTransform;

			cachedTransform = transform;

			mesh = MakeQuad();

			if (useAlternativeWorkflow) meshFilter.mesh = mesh;
		}

		/// <summary>
		/// Updates the billboard.
		/// </summary>
		public abstract void UpdateBillboard();

		/// <summary>
		/// Renders the billboard.
		/// </summary>
		public abstract void RenderBillboard();
	}
}
