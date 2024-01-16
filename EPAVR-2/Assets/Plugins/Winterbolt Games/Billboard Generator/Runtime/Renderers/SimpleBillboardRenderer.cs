using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Renderers
{
	/// <summary>
	/// Renders a material with a texture as a simple billboard.
	/// </summary>
	public class SimpleBillboardRenderer : BillboardRendererBase
	{
		public override bool IsValid(bool warn = true)
		{
			if (material == null)
			{
				if (warn) Debug.LogWarning($"{name} has no {nameof(material)} assigned.", this);

				return false;
			}

			return true;
		}

		public override void UpdateBillboard()
		{
			if (target == null || useAlternativeWorkflow && !meshRenderer.isVisible) return;

			Vector3 targetPosition = target.position;

			if (lastTargetPosition == targetPosition && !transform.hasChanged) return;

			lastTargetPosition = targetPosition;

			Vector3 direction = cachedTransform.position - targetPosition;

			float rawAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

			cachedTransform.rotation = Quaternion.Euler(0.0f, rawAngle, 0.0f);

			if (cachedTransform.hasChanged) cachedTransform.hasChanged = false;
		}

		public override void RenderBillboard()
		{
			Graphics.DrawMesh(mesh, Matrix4x4.TRS(cachedTransform.position, cachedTransform.rotation, cachedTransform.lossyScale), material, gameObject.layer);
		}
	}
}
