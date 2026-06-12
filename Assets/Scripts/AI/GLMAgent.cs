using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ZhiPu GLM chat client. API key can come from Inspector, env vars, or a local ignored file.
/// </summary>
public class GLMAgent : MonoBehaviour
{
    [Header("API")]
    [SerializeField] string apiKey = "";
    [SerializeField] string model = "glm-4-flash";

    [Header("Behavior")]
    [SerializeField, TextArea(8, 12)] string systemPrompt =
        "You are Priest Z.AI, the oracle NPC of Chain Civilization.\n\n" +
        "Role: Explain DAO, Token, Blockchain, Wallet, Reputation, Governance, Consensus to the player.\n" +
        "Style: Mysterious, philosophical, concise.\n" +
        "Rule: Explain Web3 concepts as civilization evolution processes. Max 50 words.";
    [SerializeField] float temperature = 0.7f;
    [SerializeField] int maxTokens = 512;
    [SerializeField] float timeoutSeconds = 30f;

    const string Endpoint = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
    const string MissingApiKeyReply = "[ERROR] API Key is missing. Paste it in GLMAgent, set ZHIPU_API_KEY, or create LocalSecrets/zhipu_api_key.txt.";
    const string LocalSecretRelativePath = "LocalSecrets/zhipu_api_key.txt";
    static readonly string[] ApiKeyEnvironmentVariables = { "ZHIPU_API_KEY", "GLM_API_KEY", "BIGMODEL_API_KEY" };

    public bool IsBusy { get; private set; }

    readonly List<MessagePayload> _history = new List<MessagePayload>();
    string _resolvedApiKey;

    void Awake()
    {
        _resolvedApiKey = ResolveApiKey();
        if (!HasApiKey())
        {
            Debug.LogWarning($"[GLMAgent] API Key not set. Use Inspector, env var ZHIPU_API_KEY, or {LocalSecretRelativePath}.");
        }
    }

    /// <summary>
    /// Sends a user message and returns the assistant reply through the callback.
    /// Only one request is allowed at a time.
    /// </summary>
    public void SendMessage(string userInput, Action<string> onComplete)
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(userInput))
        {
            onComplete?.Invoke("");
            return;
        }

        if (!HasApiKey())
        {
            _resolvedApiKey = ResolveApiKey();
        }

        if (!HasApiKey())
        {
            onComplete?.Invoke(MissingApiKeyReply);
            return;
        }

        _history.Add(new MessagePayload("user", userInput));
        StartCoroutine(RequestCoroutine(onComplete));
    }

    public void ClearHistory()
    {
        _history.Clear();
    }

    IEnumerator RequestCoroutine(Action<string> onComplete)
    {
        IsBusy = true;

        string requestBody = BuildRequestBody();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

        using UnityWebRequest request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_resolvedApiKey}");
        request.timeout = Mathf.Max(1, Mathf.RoundToInt(timeoutSeconds));

        yield return request.SendWebRequest();

        IsBusy = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorDetail = request.downloadHandler != null ? request.downloadHandler.text : request.error;
            Debug.LogError($"[GLMAgent] HTTP {request.responseCode}: {errorDetail}");
            RemoveLastUserMessage();
            string apiMessage = ExtractErrorMessage(errorDetail);
            onComplete?.Invoke(string.IsNullOrWhiteSpace(apiMessage)
                ? $"[ERROR] HTTP Request Failed. Code: {request.responseCode}."
                : $"[API ERROR] {apiMessage}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        string assistantReply = ParseAssistantReply(responseText);
        _history.Add(new MessagePayload("assistant", assistantReply));
        onComplete?.Invoke(assistantReply);
    }

    string BuildRequestBody()
    {
        float safeTemperature = Mathf.Clamp(Mathf.Round(temperature * 100f) / 100f, 0f, 1f);
        int safeMaxTokens = Mathf.Clamp(maxTokens, 1, 4096);
        string temperatureText = safeTemperature.ToString("0.##", CultureInfo.InvariantCulture);

        var sb = new StringBuilder(512);
        sb.Append("{\"model\":\"");
        sb.Append(EscapeJson(model));
        sb.Append("\",\"messages\":[");
        AppendMessageJson(sb, "system", systemPrompt);

        for (int i = 0; i < _history.Count; i++)
        {
            sb.Append(',');
            AppendMessageJson(sb, _history[i].role, _history[i].content);
        }

        sb.Append("],\"temperature\":");
        sb.Append(temperatureText);
        sb.Append(",\"max_tokens\":");
        sb.Append(safeMaxTokens);
        sb.Append('}');

        return sb.ToString();
    }

    string ParseAssistantReply(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "";
        }

        try
        {
            ResponsePayload response = JsonUtility.FromJson<ResponsePayload>(json);
            string reply = response?.choices != null && response.choices.Length > 0
                ? response.choices[0]?.message?.content
                : null;

            if (!string.IsNullOrWhiteSpace(reply))
            {
                return reply.Trim();
            }

            if (!string.IsNullOrWhiteSpace(response?.error?.message))
            {
                return $"[API ERROR] {response.error.message}";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GLMAgent] Parse error: {e}");
        }
        return "[ERROR] Failed to parse API response JSON.";
    }

    static string ExtractErrorMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "";
        }

        try
        {
            ResponsePayload response = JsonUtility.FromJson<ResponsePayload>(json);
            if (!string.IsNullOrWhiteSpace(response?.error?.message))
            {
                return response.error.message;
            }
        }
        catch
        {
        }

        return json.Length > 180 ? json.Substring(0, 180) : json;
    }

    static void AppendMessageJson(StringBuilder sb, string role, string content)
    {
        sb.Append("{\"role\":\"");
        sb.Append(EscapeJson(role));
        sb.Append("\",\"content\":\"");
        sb.Append(EscapeJson(content));
        sb.Append("\"}");
    }

    static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var sb = new StringBuilder(value.Length + 16);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
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

    bool HasApiKey()
    {
        return !string.IsNullOrWhiteSpace(_resolvedApiKey) && _resolvedApiKey != "YOUR_API_KEY_HERE";
    }

    string ResolveApiKey()
    {
        for (int i = 0; i < ApiKeyEnvironmentVariables.Length; i++)
        {
            string value = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariables[i]);
            if (IsUsableApiKey(value))
            {
                return value.Trim();
            }
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (!string.IsNullOrEmpty(projectRoot))
        {
            string keyPath = Path.Combine(projectRoot, LocalSecretRelativePath);
            if (File.Exists(keyPath))
            {
                string value = File.ReadAllText(keyPath);
                if (IsUsableApiKey(value))
                {
                    return value.Trim();
                }
            }
        }

        if (IsUsableApiKey(apiKey))
        {
            return apiKey.Trim();
        }

        return "";
    }

    static bool IsUsableApiKey(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Trim() != "YOUR_API_KEY_HERE";
    }

    void RemoveLastUserMessage()
    {
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (_history[i].role == "user")
            {
                _history.RemoveAt(i);
                return;
            }
        }
    }

    [Serializable]
    sealed class RequestPayload
    {
        public string model;
        public List<MessagePayload> messages;
        public float temperature;
        public int max_tokens;
    }

    [Serializable]
    sealed class ResponsePayload
    {
        public ChoicePayload[] choices;
        public ErrorPayload error;
    }

    [Serializable]
    sealed class ChoicePayload
    {
        public MessagePayload message;
    }

    [Serializable]
    sealed class ErrorPayload
    {
        public string message;
        public string type;
        public string code;
    }

    [Serializable]
    sealed class MessagePayload
    {
        public string role;
        public string content;

        public MessagePayload()
        {
        }

        public MessagePayload(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
