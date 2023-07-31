using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.samples
{
    public class MessageBox : MonoBehaviour
    {
        public const string Filename = "MessageBox.prefab";
        private static readonly Queue<MessageBox> Unused = new Queue<MessageBox>();

        private static AssetRequest request;
        public Text titleText;
        public Text contentText;
        public Text yesText;
        public Text noText;

        private readonly Request _request = new Request();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            request?.Release();
            request = null;
        }

        public static Request LoadAsync()
        {
            if (request == null)
                request = Asset.LoadAsync(Filename, typeof(GameObject));
            return request;
        }

        public void OnClickYes()
        {
            _request.SetResult(Request.Result.Success);
            Complete();
        }

        private void Complete()
        {
            gameObject.SetActive(false);
            Unused.Enqueue(this);
        }

        public void OnClickNo()
        {
            _request.Cancel();
            Complete();
        }

        private Request SendRequest(string title, string content, string yes, string no)
        {
            if (string.IsNullOrEmpty(yes)) yes = Constants.Text.Ok;
            if (string.IsNullOrEmpty(no)) no = Constants.Text.No;

            titleText.text = title;
            contentText.text = content;
            yesText.text = yes;
            noText.text = no;
            gameObject.SetActive(true);
            _request.Reset();
            _request.SendRequest();
            return _request;
        }

        public static Request Show(string title, string content, string yes = null, string no = null)
        {
            if (Unused.Count > 0)
            {
                var item = Unused.Dequeue();
                return item.SendRequest(title, content, yes, no);
            }

            var prefab = Asset.Get<GameObject>(Filename);
            var go = Instantiate(prefab);
            go.name = prefab.name;
            var messageBox = go.GetComponent<MessageBox>();
            return messageBox.SendRequest(title, content, yes, no);
        }
    }
}