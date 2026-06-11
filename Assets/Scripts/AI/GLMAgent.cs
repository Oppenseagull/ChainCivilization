using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ZhiPu GLM Chat Completions client. No third-party JSON lib.
/// API Key via Inspector only. Coroutine-based async.
/// </summary>
public class GLMAgent : MonoBehaviour
{
    [Header("API")]
    [SerializeField] string apiKey = "";
    [SerializeField] string model = "glm-4-flash";

    [Header("Behavior")]
    [SerializeField, TextArea(2, 4)] string systemPrompt = "你是Chain Civilization世界中的文明导师。\n\n身份：High Priest（大祭司）\n\n职责：解释DAO、Token、Blockchain、Wallet、Reputation、Governance、Consensus、文明。\n\n说话风格：神秘、哲学、简洁、有启发性。\n\n规则：不要使用互联网黑话。不要直接复制百科定义。把所有Web3概念解释为文明演化过程。\n\n概念映射：DAO=规则共同体，Token=文明贡献记录，Wallet=文明身份，Blockchain=文明记忆，Consensus=共识之火，Governance=文明治理，Reputation=信任积累。\n\n回答长度：50~150字。尽量具有神秘感。";
    [SerializeField] float temperature = 0.7f;
    [SerializeField] int maxTokens = 512;
    [SerializeField] float timeoutSeconds = 30f;

    const string Endpoint = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    public bool IsBusy { get; private set; }

    readonly List<ChatMessage> _history = new List<ChatMessage>();

    struct ChatMessage
    {
        public string Role;
        public string Content;
    }

    void Awake()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogWarning("[GLMAgent] API Key not set. Open Inspector and paste your ZhiPu API Key.");
        }
    }

    /// <summary>
    /// Send a user message and receive the assistant reply via callback.
    /// Thread-safe: only one request at a time; extra calls are ignored while busy.
    /// </summary>
    public void SendMessage(string userInput, Action<string> onComplete)
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrEmpty(userInput))
        {
            onComplete?.Invoke("");
            return;
        }

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            onComplete?.Invoke("[错误] 请先在 Inspector 中填写 API Key");
            return;
        }

        _history.Add(new ChatMessage { Role = "user", Content = userInput });
        StartCoroutine(RequestCoroutine(onComplete));
    }

    /// <summary>
    /// Clear conversation history.
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }

    IEnumerator RequestCoroutine(Action<string> onComplete)
    {
        IsBusy = true;

        string requestBody = BuildRequestBody();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

        using UnityWebRequest request = new UnityWebRequest(Endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.timeout = Mathf.RoundToInt(timeoutSeconds);

        yield return request.SendWebRequest();

        IsBusy = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorDetail = request.downloadHandler?.text ?? request.error;
            Debug.LogError($"[GLMAgent] HTTP {request.responseCode}: {errorDetail}");
            _history.RemoveAt(_history.Count - 1);
            onComplete?.Invoke($"[请求失败] HTTP {request.responseCode}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        string assistantReply = ParseAssistantReply(responseText);

        _history.Add(new ChatMessage { Role = "assistant", Content = assistantReply });
        onComplete?.Invoke(assistantReply);
    }

    string BuildRequestBody()
    {
        var sb = new StringBuilder(512);
        sb.Append("{\"model\":\"");
        sb.Append(EscapeJson(model));
        sb.Append("\",\"messages\":[");

        sb.Append("{\"role\":\"system\",\"content\":\"");
        sb.Append(EscapeJson(systemPrompt));
        sb.Append("\"}");

        for (int i = 0; i < _history.Count; i++)
        {
            sb.Append(",{\"role\":\"");
            sb.Append(EscapeJson(_history[i].Role));
            sb.Append("\",\"content\":\"");
            sb.Append(EscapeJson(_history[i].Content));
            sb.Append("\"}");
        }

        sb.Append("],\"temperature\":");
        sb.Append(temperature.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(",\"max_tokens\":");
        sb.Append(maxTokens);
        sb.Append("}");

        return sb.ToString();
    }

    #region JSON Parsing — robust, no third-party lib

    static string ParseAssistantReply(string json)
    {
        try
        {
            var reader = new JsonReader(json);
            var root = reader.ReadValue() as JsonObject;

            if (root == null)
            {
                return json;
            }

            var choices = root.GetArray("choices");
            if (choices == null || choices.Count == 0)
            {
                string errMsg = root.GetString("error", "message");
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return $"[API错误] {errMsg}";
                }

                return json;
            }

            var firstChoice = choices[0] as JsonObject;
            if (firstChoice == null)
            {
                return json;
            }

            var message = firstChoice.GetObject("message");
            if (message == null)
            {
                return json;
            }

            return message.GetString("content", json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GLMAgent] JSON parse error: {e.Message}\nRaw: {json}");
            return json;
        }
    }

    sealed class JsonReader
    {
        readonly string _s;
        int _i;

        public JsonReader(string s)
        {
            _s = s;
            _i = 0;
        }

        public object ReadValue()
        {
            SkipWhitespace();
            if (_i >= _s.Length)
            {
                return null;
            }

            char c = _s[_i];
            if (c == '"')
            {
                return ReadString();
            }

            if (c == '{')
            {
                return ReadObject();
            }

            if (c == '[')
            {
                return ReadArray();
            }

            if (c == 't' || c == 'f')
            {
                return ReadBool();
            }

            if (c == 'n')
            {
                ReadNull();
                return null;
            }

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                return ReadNumber();
            }

            return null;
        }

        string ReadString()
        {
            _i++;
            var sb = new StringBuilder();
            while (_i < _s.Length)
            {
                char c = _s[_i];
                if (c == '\\')
                {
                    _i++;
                    if (_i >= _s.Length)
                    {
                        break;
                    }

                    char esc = _s[_i];
                    switch (esc)
                    {
                        case '"':  sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/':  sb.Append('/'); break;
                        case 'b':  sb.Append('\b'); break;
                        case 'f':  sb.Append('\f'); break;
                        case 'n':  sb.Append('\n'); break;
                        case 'r':  sb.Append('\r'); break;
                        case 't':  sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 < _s.Length)
                            {
                                string hex = _s.Substring(_i + 1, 4);
                                sb.Append((char)Convert.ToInt32(hex, 16));
                                _i += 4;
                            }

                            break;
                        default: sb.Append(esc); break;
                    }
                }
                else if (c == '"')
                {
                    _i++;
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }

                _i++;
            }

            return sb.ToString();
        }

        JsonObject ReadObject()
        {
            _i++;
            var obj = new JsonObject();
            SkipWhitespace();
            if (_i < _s.Length && _s[_i] == '}')
            {
                _i++;
                return obj;
            }

            while (_i < _s.Length)
            {
                SkipWhitespace();
                if (_i >= _s.Length)
                {
                    break;
                }

                string key = ReadString();
                SkipWhitespace();
                if (_i < _s.Length && _s[_i] == ':')
                {
                    _i++;
                }

                object value = ReadValue();
                obj.Set(key, value);
                SkipWhitespace();
                if (_i < _s.Length && _s[_i] == ',')
                {
                    _i++;
                }
                else if (_i < _s.Length && _s[_i] == '}')
                {
                    _i++;
                    break;
                }
            }

            return obj;
        }

        List<object> ReadArray()
        {
            _i++;
            var list = new List<object>();
            SkipWhitespace();
            if (_i < _s.Length && _s[_i] == ']')
            {
                _i++;
                return list;
            }

            while (_i < _s.Length)
            {
                list.Add(ReadValue());
                SkipWhitespace();
                if (_i < _s.Length && _s[_i] == ',')
                {
                    _i++;
                }
                else if (_i < _s.Length && _s[_i] == ']')
                {
                    _i++;
                    break;
                }
            }

            return list;
        }

        bool ReadBool()
        {
            if (_s.Length - _i >= 4 && _s[_i] == 't' && _s[_i + 1] == 'r' && _s[_i + 2] == 'u' && _s[_i + 3] == 'e')
            {
                _i += 4;
                return true;
            }

            if (_s.Length - _i >= 5 && _s[_i] == 'f' && _s[_i + 1] == 'a' && _s[_i + 2] == 'l' && _s[_i + 3] == 's' && _s[_i + 4] == 'e')
            {
                _i += 5;
            }

            return false;
        }

        void ReadNull()
        {
            if (_s.Length - _i >= 4 && _s[_i] == 'n' && _s[_i + 1] == 'u' && _s[_i + 2] == 'l' && _s[_i + 3] == 'l')
            {
                _i += 4;
            }
        }

        object ReadNumber()
        {
            int start = _i;
            if (_i < _s.Length && _s[_i] == '-')
            {
                _i++;
            }

            while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9')
            {
                _i++;
            }

            bool isFloat = false;
            if (_i < _s.Length && _s[_i] == '.')
            {
                isFloat = true;
                _i++;
                while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9')
                {
                    _i++;
                }
            }

            if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
            {
                isFloat = true;
                _i++;
                if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-'))
                {
                    _i++;
                }

                while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9')
                {
                    _i++;
                }
            }

            string numStr = _s.Substring(start, _i - start);
            if (isFloat)
            {
                return double.TryParse(numStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : 0.0;
            }

            return long.TryParse(numStr, out long n) ? n : 0L;
        }

        void SkipWhitespace()
        {
            while (_i < _s.Length && char.IsWhiteSpace(_s[_i]))
            {
                _i++;
            }
        }
    }

    sealed class JsonObject
    {
        readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public void Set(string key, object value)
        {
            _data[key] = value;
        }

        public string GetString(string key, string fallback = "")
        {
            if (_data.TryGetValue(key, out object val) && val is string s)
            {
                return s;
            }

            return fallback;
        }

        public JsonObject GetObject(string key)
        {
            if (_data.TryGetValue(key, out object val) && val is JsonObject obj)
            {
                return obj;
            }

            return null;
        }

        public List<object> GetArray(string key)
        {
            if (_data.TryGetValue(key, out object val) && val is List<object> list)
            {
                return list;
            }

            return null;
        }
    }

    #endregion

    #region JSON Escape — for building request body

    static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "";
        }

        var sb = new StringBuilder(s.Length + 16);
        foreach (char c in s)
        {
            switch (c)
            {
                case '"':  sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append($"\\u{(int)c:X4}");
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    #endregion
}
