using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PowerFxBlazorDemo.Services;

namespace PowerFxBlazorDemo.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{

    [Inject]
    public PowerFxService PowerFxService { get; set; }

    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    private static Home? StaticSelfReference;

    private int selectedTab = 0;

    private string contextObject = "{\"A\":\"ABC\",\"B\":{\"Inner\":123}}";

    private string formula = "";
    private string exampleFormula = "Left(B.Inner * 2, 2)";

    private PowerFxService.EvaluationResult evaluation = new PowerFxService.EvaluationResult
    {
        Result = string.Empty,
        Tokens = new List<Microsoft.PowerFx.Syntax.Token>(),
    };

    private string tokens = string.Empty;
    private string parse = string.Empty;

    private string buildTimeString = string.Empty;
    private string powerFxVersion = string.Empty;
    private string powerFxFormulaBarVersion = string.Empty;

    private DotNetObjectReference<object> objectReference;

    protected override void OnInitialized()
    {
        StaticSelfReference = this;
        base.OnInitialized();
    }

    public void Dispose()
    {
        if (StaticSelfReference != null) StaticSelfReference = null;
    }

    protected override async void OnParametersSet()
    {

        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var fi = new FileInfo(asm.Location);
        buildTimeString = fi.CreationTimeUtc.ToString() + " UTC";

        powerFxVersion = typeof(Microsoft.PowerFx.RecalcEngine).Assembly.GetName().Version.ToString();

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        if (firstRender)
        {

            var formula = GetExampleFormula();

            await JsRuntime.InvokeVoidAsync("MyLib.showEditor", formula);

            await Evaluate(formula);

        }

    }

    private string GetExampleFormula()
    {

        return new Random().Next(0, 4) switch
        {
            0 => "// Return two times the value of B.Inner" + Environment.NewLine
                                + "B.Inner * 2",
            1 => "// Add (the value of B.Inner) days to today and show it as text in the specified format" + Environment.NewLine +
                                "Text( DateAdd( Now(), B.Inner ), \"dd-mm-yyyy hh:mm\" )",
            2 => "// Return the left 2 characters of the string in A" + Environment.NewLine
                                + "Left(A, 2)",
            _ => "// Return the value of B" + Environment.NewLine
                                + "B",
        };
        
    }

    private async Task Evaluate(string formula)
    {
        evaluation = await PowerFxService.Evaluate(contextObject, formula);            
        StateHasChanged();
    }

    [JSInvokable]
    public static async Task<string> LanguageServerService(string data)
    {

        // The scope factory.
        // This lets the LanguageServer object get the symbols in the RecalcEngine to use for intellisense.
        var scopeFactory = new Factories.PowerFxScopeFactory();

        var sendToClientData = new List<string>();
        var languageServer = new Microsoft.PowerFx.LanguageServerProtocol.LanguageServer((string data) => sendToClientData.Add(data), scopeFactory);

        try
        {

            languageServer.OnDataReceived(data);

            return System.Text.Json.JsonSerializer.Serialize(sendToClientData.ToArray());

        }
        catch (Exception ex)
        {

            await Task.Delay(0);
            return ex.Message;

        }

    }

    [JSInvokable]
    public static async Task FormulaHasChanged(string formula)
    {
        if (StaticSelfReference == null) return;
        await StaticSelfReference.Evaluate(formula);
    }

    [JSInvokable]
    public static async Task<string> GetCurrentContext()
    {
        await Task.Delay(0);
        if (StaticSelfReference == null) return string.Empty;
        return StaticSelfReference.contextObject;
    }

}

