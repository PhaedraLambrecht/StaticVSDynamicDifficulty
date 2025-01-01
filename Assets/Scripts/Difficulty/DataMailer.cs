using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class DataMailer : Singleton<DataMailer>
{
    private const string SENDGRID_API_KEY = ""; // Replace with your API key


    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void SendEmail(string filePath)
    {
        StartCoroutine(SendEmailCoroutine(filePath));
    }

    private IEnumerator SendEmailCoroutine(string filePath)
    {
        string url = "https://api.sendgrid.com/v3/mail/send";

        // Read the JSON file content
        string jsonContent = File.ReadAllText(filePath);
        string base64Content = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));

        // Create the email body
        string emailBody = $@"
    {{
        ""personalizations"": [
            {{
                ""to"": [
                    {{
                        ""email"": ""receiver""
                    }}
                ],
                ""subject"": ""Report""
            }}
        ],
        ""from"": {{
            ""email"": ""sender""
        }},
        ""content"": [
            {{
                ""type"": ""text/plain"",
         ""value"": ""report""
                }}
            ],
            ""attachments"": [
                {{
                    ""content"": ""{base64Content}"",
                    ""type"": ""application/json"",
                    ""filename"": ""PlayerReport.json"",
                    ""disposition"": ""attachment""
                }}
            ]
        }}";


        // Create request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(emailBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + SENDGRID_API_KEY);
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Email sent successfully!");
        }
        else
        {
            Debug.LogError("Error sending email: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }
}

