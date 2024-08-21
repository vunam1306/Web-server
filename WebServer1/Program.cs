using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

public class SimpleHttpServer
{
    private static bool isRunning = true;
    private static Dictionary<string, string> sessions = new Dictionary<string, string>();

    public static void Main()
    {
        string prefix = "http://localhost:80/";
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(prefix);

        Console.WriteLine("Listening for requests on " + prefix);

        listener.Start();

        while (isRunning)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        listener.Stop();
    }

    
    private static void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string sessionId = GetSessionId(request);
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            SetSessionCookie(response, sessionId);
        }

        string url = request.Url.LocalPath.TrimStart('/');
        string sourceDirectory = "D:\\VISUALCODE\\Project\\WebServer1\\src";
        string filePath = Path.Combine(sourceDirectory, url);

        if (Directory.Exists(filePath))
        {
            // folder display
            DisplayDirectoryContents(response, filePath, sessionId);
        }
        else if (File.Exists(filePath))
        {
            // file 
            if (Path.GetExtension(filePath).Equals(".html", StringComparison.OrdinalIgnoreCase))
            {
                string htmlContent = File.ReadAllText(filePath);
                byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                // file khac
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
        else
        {
            string responseString = $"<html><body><h1>File Not Found</h1><p>Session ID: {sessionId}</p></body></html>";
            byte[] notFoundBuffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ContentLength64 = notFoundBuffer.Length;
            response.OutputStream.Write(notFoundBuffer, 0, notFoundBuffer.Length);
        }

        response.Close();
    }

    private static void DisplayDirectoryContents(HttpListenerResponse response, string directoryPath, string sessionId)
    {
        try
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<html><body>");
            htmlBuilder.AppendLine($"<h1>Directory Contents</h1><p>Session ID: {sessionId}</p>");
            htmlBuilder.AppendLine("<ul>");

            string[] files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                htmlBuilder.AppendLine($"<li><a href=\"/{fileName}\">{fileName}</a></li>");
            }

            htmlBuilder.AppendLine("</ul>");
            htmlBuilder.AppendLine("</body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(htmlBuilder.ToString());
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            string errorResponse = $"<html><body><h1>Error: {ex.Message}</h1></body></html>";
            byte[] errorBuffer = Encoding.UTF8.GetBytes(errorResponse);
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentLength64 = errorBuffer.Length;
            response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
        }
    }
    private static string GetSessionId(HttpListenerRequest request)
    {
        string sessionId = null;

        if (request.Cookies != null)
        {
            Cookie sessionCookie = request.Cookies["sessionId"];
            if (sessionCookie != null)
            {
                sessionId = sessionCookie.Value;
            }
        }

        return sessionId;
    }

    private static void SetSessionCookie(HttpListenerResponse response, string sessionId)
    {
        Cookie sessionCookie = new Cookie("sessionId", sessionId);
        response.SetCookie(sessionCookie);
    }

}