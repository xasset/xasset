using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace xasset.example
{
    [RequireComponent(typeof(Text))]
    public class PrintText : MonoBehaviour
    {
        public float stay = 0.5f;
        public float speed = 10; // 10 字/s
        public bool loop;
        public bool playOnAwake = true;
        public UnityEvent completed;
        private string _message;
        private string[] _messages;
        private IEnumerator _print;
        private Text _text;
        private int messageIndex;

        private void Start()
        {
            _text = GetComponent<Text>();
            _message = _text.text;
            _messages = _message.Split('\n');
            if (playOnAwake) PrintMessages();
        }

        private void PrintMessages()
        {
            if (_print != null) StopCoroutine(_print);

            _print = _PrintMessages();
            StartCoroutine(_print);
        }

        public void Replay()
        {
            if (messageIndex >= _messages.Length) messageIndex = 0;

            PrintMessages();
        }

        public void Skip()
        {
            if (NextMessage())
            {
                PrintMessages();
            }
            else
            {
                if (messageIndex >= _messages.Length) _text.text = _messages[_messages.Length - 1];
            }
        }

        private IEnumerator _PrintMessages()
        {
            if (_messages.Length > 0)
                while (messageIndex < _messages.Length)
                {
                    var msg = _messages[messageIndex];
                    yield return _Printing(msg);
                    if (NextMessage()) yield return new WaitForSeconds(stay);
                }

            completed.Invoke();
        }

        private bool NextMessage()
        {
            messageIndex++;
            if (!loop || messageIndex != _messages.Length) return messageIndex < _messages.Length;
            messageIndex = 0;
            return true;
        }

        private IEnumerator _Printing(string s)
        {
            var time = Time.realtimeSinceStartup;
            var index = 0;
            while (index < s.Length)
            {
                var elapsed = Time.realtimeSinceStartup - time;
                index = Mathf.Clamp((int) (elapsed * speed), 0, s.Length);
                _text.text = s.Substring(0, index);
                yield return null;
            }

            _text.text = s;
        }
    }
}