using UnityEngine;

namespace xasset.samples
{
    public class OpenURL : MonoBehaviour
    {
        public string url;

        public void Open()
        {
            Application.OpenURL(url);
        }
    }
}