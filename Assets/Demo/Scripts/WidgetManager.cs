using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace libx
{
    public class WidgetManager : MonoBehaviour
    {
        private static List<Widget> widgets = new List<Widget>();

        private static WidgetManager instance;

        [RuntimeInitializeOnLoadMethod]
        public static WidgetManager GetInstance()
        {
            if (instance == null)
            {
                instance = new GameObject(typeof(WidgetManager).Name).AddComponent<WidgetManager>();
                DontDestroyOnLoad(GetInstance().gameObject);
            }
            return instance;
        }

        public static void Add(Widget w)
        {
            widgets.Add(w);
        }

        public static bool Remove(Widget w)
        {
            return widgets.Remove(w);
        }

        // Update is called once per frame
        void Update()
        {
            widgets.ForEach(w => w.Update());
        }
    }
}