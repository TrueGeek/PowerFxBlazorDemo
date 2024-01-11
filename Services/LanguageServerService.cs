using System;
using Microsoft.PowerFx.LanguageServerProtocol;
using PowerFxBlazorDemo.Factories;
using System.Net;
using Microsoft.JSInterop;
using System.Text.Json;

namespace PowerFxBlazorDemo.Services;

//public static class LanguageServerService
//{

//    [JSInvokable]
//    public static async Task<string> LSP(string data)
//    {

//        // The scope factory.
//        // This lets the LanguageServer object get the symbols in the RecalcEngine to use for intellisense.
//        var scopeFactory = new PowerFxScopeFactory();

//        var sendToClientData = new List<string>();
//        var languageServer = new LanguageServer((string data) => sendToClientData.Add(data), scopeFactory);

//        try
//        {

//            languageServer.OnDataReceived(data);

//            return JsonSerializer.Serialize(sendToClientData.ToArray());

//        }
//        catch (Exception ex)
//        {

//            await Task.Delay(0);
//            return ex.Message;

//        }

//    }

//}
