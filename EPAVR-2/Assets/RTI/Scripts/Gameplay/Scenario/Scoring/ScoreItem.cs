using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [System.Serializable]
    public class ScoreItem
    {
        #region Public Variables
        public string Title;
        public int Value = 0;
        public int MaxValue = 0;
        #endregion

        #region Constructors
        public ScoreItem(string _title, int _value = 0)
        {
            Title = _title;
            Value = _value;
            MaxValue = 0;
        }

        public ScoreItem(string _title, int _value, int _maxValue)
        {
            Title = _title;
            Value = _value;
            MaxValue = _maxValue;
        }

        public ScoreItem(ScoreItem other)
        {
            Title = other.Title;
            Value = other.Value;
        }
        #endregion
    }
}

