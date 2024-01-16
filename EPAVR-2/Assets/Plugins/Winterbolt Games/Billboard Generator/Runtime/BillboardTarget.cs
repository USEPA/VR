using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	/// <summary>
	/// Holds a reference to a transform that will be targeted by all <see cref="RuntimeBillboardRenderer"/> (s) that have
	/// <seealso cref="RuntimeBillboardRenderer._useBillboardTarget"/> field set to true.
	/// </summary>
	public sealed class BillboardTarget : MonoBehaviour
	{
		private static BillboardTarget _instance;

		[SerializeField]
		[Tooltip("The transform that will act as a target for all billboard renderers that have '_useBillboardTarget' set to true.")]
		private Transform targetTransform;

		/// <summary>
		/// The transform that will act as a target for all billboard renderers that have '_useBillboardTarget' set to true.
		/// </summary>
		public static Transform TargetTransform
		{
			get
			{
				if (_instance == null) _instance = FindObjectOfType<BillboardTarget>();

				return _instance.targetTransform;
			}

			set
			{
				if (_instance == null) _instance = FindObjectOfType<BillboardTarget>();

				_instance.targetTransform = value;
			}
		}
	}
}
