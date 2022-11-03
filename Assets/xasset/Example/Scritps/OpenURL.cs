using UnityEngine;

namespace xasset.example
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