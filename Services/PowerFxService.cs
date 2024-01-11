namespace PowerFxBlazorDemo.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using PowerFxBlazorDemo.Commons;
using PowerFxBlazorDemo.Factories;
using PowerFxBlazorDemo.Helpers;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;

public class PowerFxService
{

    private readonly ILogger<PowerFxService> _logger;
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);
    private readonly JsonSerializerOptions _jsonSerializerOptions;    

    public PowerFxService(ILogger<PowerFxService> logger)
    {
        
        _logger = logger;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new NodeConverter<TexlNode>(),
                new NodeConverter<VariadicOpNode>(),
                new NodeConverter<ListNode>(),
                new NodeConverter<CallNode>(),
                new NodeConverter<Identifier>(),
                new NodeConverter<DName>()

            }
        };

    }

    public async Task<EvaluationResult> Evaluate(string context, string expression)
    {

        IReadOnlyList<Token> tokens = null;
        CheckResult check = null;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(_timeout);
        try
        {
            var engine = new PowerFxScopeFactory().GetEngine();

            var parameters = (RecordValue)FormulaValueJSON.FromJson(context);

            if (parameters == null)
            {
                parameters = RecordValue.Empty();
            }

            tokens = engine.Tokenize(expression);
            check = engine.Check(expression, parameters.Type, options: null);
            check.ThrowOnErrors();
            var eval = check.GetEvaluator();                
            var result = await eval.EvalAsync(cts.Token, parameters);
            var resultString = PowerFxHelper.TestToString(result);

            return new EvaluationResult
            {
                Result = resultString,
                Tokens = tokens,
                Parse = JsonSerializer.Serialize(check.Parse.Root, _jsonSerializerOptions)
            };

        }
        catch (Exception ex)
        {

            return new EvaluationResult
            {
                Error = ex.Message,
                Result = string.Empty,
                Tokens = tokens,
                Parse = check != null ? JsonSerializer.Serialize(check.Parse.Root, _jsonSerializerOptions) : null
            };

        }
        finally
        {
            cts.Dispose();
        }

    }

    public class EvaluationResult
    {

        public string Result { get; set; } = string.Empty;
        public IReadOnlyList<Token>? Tokens { get; set; }
        public string? Parse { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

    }

}