using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class AgentInfoDisplayItem : MonoBehaviour
    {
        public abstract void ParseAgentInfo(ChemicalAgent _agent);
    }
}

