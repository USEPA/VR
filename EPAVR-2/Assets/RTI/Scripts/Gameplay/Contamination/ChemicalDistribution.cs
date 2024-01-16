using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [System.Serializable]
    public class ChemicalDistributionItem : DistributionItem<ChemicalAgent> { }
    [System.Serializable]
    public class ChemicalDistribution : Distribution<ChemicalAgent, ChemicalDistributionItem>
    {
        public ChemicalDistribution(ChemicalDistribution other)
        {
            // Create new list
            items = new List<ChemicalDistributionItem>();
            // Loop through each item in the other distribution and add it
            foreach (ChemicalDistributionItem item in other.Items)
            {
                Add(item.Value, item.Weight);
            }
        }
    }
}

