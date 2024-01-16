using HurricaneVR.Framework.Shared;

namespace HurricaneVR.Framework.Core.Sockets
{
    public class HVRTagSocketable : HVRSocketable
    {
        public HVRSocketableTag Tags;


        public void Reset()
        {
            if (Tags == null)
            {
                Tags = new HVRSocketableTag();

#if UNITY_EDITOR

                UnityEditor.EditorUtility.SetDirty(this);

#endif
            }
            
            Tags.Tags = HVRSettings.Instance.DefaultSocketableTags;
        }
    }
}