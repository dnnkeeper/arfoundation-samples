using UnityEngine;
using System.Collections;

namespace Utility.UI
{
    public class ApplicationManager : MonoBehaviour
    {


        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }
    }
}
