using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public enum ManagerStatus { Shutdown, Initializing, Started, Resetting }
    public interface IManager
    {
        ManagerStatus Status { get; }
        void Startup();
        void ResetToStart();
    }
}

