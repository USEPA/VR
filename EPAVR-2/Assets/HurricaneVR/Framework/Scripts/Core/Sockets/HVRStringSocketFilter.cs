namespace HurricaneVR.Framework.Core.Sockets
{
    public class HVRStringSocketFilter : HVRSocketFilter
    {
        public string SocketType;

        public override bool IsValid(HVRSocketable socketable)
        {
            if (string.IsNullOrWhiteSpace(SocketType)) return false;
            if (!socketable) return false;
            var stringFilter = socketable as HVRStringSocketable;
            if (stringFilter == null) return false;
            if (string.IsNullOrWhiteSpace(stringFilter.SocketType)) return false;
            return SocketType.ToLowerInvariant().Equals(stringFilter.SocketType.ToLowerInvariant());
        }
    }
}