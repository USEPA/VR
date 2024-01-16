using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemDistributionTest : MonoBehaviour
    {
        [SerializeField] ChemicalDistribution chemDistribution;
        public ChemicalAgent selectedChemical;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnValidate()
        {
            if (chemDistribution == null) return;
            chemDistribution.OnItemsChange();
        }


        #if UNITY_EDITOR
        [ContextMenu("Draw a random chemical")]
        public void GetRandomChemical()
        {
            selectedChemical = chemDistribution.Draw();
        }
        #endif
    }
}

