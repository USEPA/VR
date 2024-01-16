using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MainMenu : Menu
    {
        #region Helper Methods
        public void QuitApplication()
        {
            if (!CoreGameManager.Instance) return;
            CoreGameManager.Instance.ExitToDesktop();
        }
        #endregion
    }
}

