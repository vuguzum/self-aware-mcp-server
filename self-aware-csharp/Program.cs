#!/usr/bin/env dotnet

/**
 * Self-Aware MCP Server
 * MCP (Model Context Protocol) сервер на C#, предоставляющий инструменты самосознания для LLM.
 * 
 * @author Alexander Kazantsev with z.ai
 * @email akazant@gmail.com
 * @license MIT
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SelfAwareMcp;

/// <summary>
/// MCP Server implementation using stdio transport
/// </summary>
class Program
{
    // Location from command line argument
    private static readonly string LocationArg;

    static Program()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        LocationArg = args.Length > 0 ? args[0] ?? "Unknown" : "Unknown";
    }

    static async Task Main(string[] args)
    {
        // Log to stderr (stdout is used for MCP protocol)
        await Console.Error.WriteLineAsync("Self-aware MCP server running on stdio");
        await Console.Error.WriteLineAsync($"Location: {LocationArg}");

        // Set up JSON serialization options
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Process incoming JSON-RPC messages
        using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        using var writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, jsonOptions);
                if (request == null) continue;

                var response = await HandleRequestAsync(request, jsonOptions);
                var responseJson = JsonSerializer.Serialize(response, jsonOptions);
                await writer.WriteLineAsync(responseJson);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error processing request: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handle incoming JSON-RPC request
    /// </summary>
    static async Task<JsonRpcResponse> HandleRequestAsync(JsonRpcRequest request, JsonSerializerOptions jsonOptions)
    {
        var response = new JsonRpcResponse { Id = request.Id };

        try
        {
            switch (request.Method)
            {
                case "initialize":
                    response.Result = GetInitializeResult();
                    break;

                case "tools/list":
                    response.Result = GetToolsListResult();
                    break;

                case "tools/call":
                    var toolCallParams = ((JsonElement)request.Params!).Deserialize<ToolCallParams>(jsonOptions);
                    response.Result = await CallToolAsync(toolCallParams!, jsonOptions);
                    break;

                default:
                    response.Error = new JsonRpcError { Code = -32601, Message = $"Method not found: {request.Method}" };
                    break;
            }
        }
        catch (Exception ex)
        {
            response.Error = new JsonRpcError { Code = -32603, Message = ex.Message };
        }

        return response;
    }

    /// <summary>
    /// Get server capabilities
    /// </summary>
    static object GetInitializeResult()
    {
        return new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "self-aware",
                version = "1.0.0"
            }
        };
    }

    /// <summary>
    /// Get list of available tools
    /// </summary>
    static object GetToolsListResult()
    {
        return new
        {
            tools = new object[]
            {
                new
                {
                    name = "get_current_location",
                    description = "Returns the current location passed as a command line argument when starting the MCP server.",
                    inputSchema = new { type = "object", properties = new { } }
                },
                new
                {
                    name = "get_current_date_time",
                    description = "Get current date and time as weekday, year-month-day hours:minutes:seconds",
                    inputSchema = new { type = "object", properties = new { } }
                },
                new
                {
                    name = "get_current_system",
                    description = "Returns the main operating system parameters including name, version, displayName, platform, and architecture.",
                    inputSchema = new { type = "object", properties = new { } }
                },
                new
                {
                    name = "calculate",
                    description = @"Эта функция позволяет вычислять математические выражения, переданные в виде строки,
с защитой от выполнения небезопасного кода. Поддерживает широкий набор математических
и статистических функций, безопасных для выполнения в изолированной среде.
Тригонометрические функции принимают аргументы в радианах.

Parameters:
expression : str
    Строка с математическим выражением для вычисления. Может содержать:
    - Арифметические операции (+, -, *, /, **)
    - Математические функции (sin, cos, log, sqrt и др.)
    - Статистические функции (mean, median, stdev и др.)
    - Математические константы (pi, e, inf, nan)

Returns:
Union[float, int, str]
    Результат вычисления:
    - float или int: при успешном вычислении числового результата
    - str: сообщение об ошибке при возникновении исключения

Supported Mathematical Functions:
Trigonometric with radians as input: sin, cos, tan, asin, acos, atan
Hyperbolic: sinh, cosh, tanh, asinh, acosh, atanh
Logarithmic: log, log10, log2, exp, expm1, log1p
Other: sqrt, ceil, floor, fabs, factorial, gamma, lgamma, modf, fsum, abs
Constants: pi, e, tau, inf, nan

Supported Statistical Functions:
- mean(data): среднее арифметическое
- median(data): медиана
- mode(data): мода (наиболее частое значение)
- stdev(data): стандартное отклонение
- variance(data): дисперсия
- harmonic_mean(data): гармоническое среднее
- geometric_mean(data): геометрическое среднее
- sum(iterable): сумма элементов
- len(iterable): длина последовательности

Notes:
- Функция использует безопасное вычисление выражений
- Оператор import не поддерживается
- Поддерживаются только функции из разрешенного списка
- Результаты автоматически нормализуются для JSON-сериализации
- В случае ошибки возвращается строка с описанием проблемы
- Для статистических функций требуется передавать массив (например, [1, 2, 3, 4, 5])",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            expression = new
                            {
                                type = "string",
                                description = "Строка с математическим выражением для вычисления"
                            }
                        },
                        required = new[] { "expression" }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Call a tool by name
    /// </summary>
    static Task<object> CallToolAsync(ToolCallParams toolCall, JsonSerializerOptions jsonOptions)
    {
        var result = toolCall.Name switch
        {
            "get_current_location" => GetCurrentLocation(),
            "get_current_date_time" => GetCurrentDateTime(),
            "get_current_system" => GetCurrentSystem(),
            "calculate" => Calculate(toolCall.Arguments?.GetProperty("expression").GetString() ?? ""),
            _ => $"Unknown tool: {toolCall.Name}"
        };

        return Task.FromResult((object)new
        {
            content = new[]
            {
                new { type = "text", text = result }
            }
        });
    }

    // ========================================================================
    // Tool implementations
    // ========================================================================

    /// <summary>
    /// Get current location from command line argument
    /// </summary>
    static string GetCurrentLocation()
    {
        return LocationArg;
    }

    /// <summary>
    /// Get current date and time
    /// </summary>
    static string GetCurrentDateTime()
    {
        var now = DateTime.Now;
        var weekdays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        var weekday = weekdays[(int)now.DayOfWeek];
        return $"{weekday}, {now.Year}-{now.Month:D2}-{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}";
    }

    /// <summary>
    /// Get current system information
    /// </summary>
    static string GetCurrentSystem()
    {
        var systemInfo = new
        {
            os = new
            {
                name = GetOsName(),
                version = Environment.OSVersion.Version.ToString(),
                displayName = GetOsDisplayName(),
                platform = GetPlatformName(),
                cpu = RuntimeInformation.ProcessArchitecture.ToString().ToLower()
            }
        };
        return JsonSerializer.Serialize(systemInfo, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Get OS name
    /// </summary>
    static string GetOsName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "Windows_NT";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "Darwin";
        return "Unknown";
    }

    /// <summary>
    /// Get platform name
    /// </summary>
    static string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win32";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "darwin";
        return "unknown";
    }

    /// <summary>
    /// Get human-readable OS display name
    /// </summary>
    static string GetOsDisplayName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var version = Environment.OSVersion.Version;
            var build = version.Build;
            
            // Windows 11: build >= 22000
            if (version.Major == 10 && build >= 22000) return "Windows 11";
            // Windows 10: build < 22000
            if (version.Major == 10 && build < 22000) return "Windows 10";
            // Windows 8.1
            if (version.Major == 6 && version.Minor == 3) return "Windows 8.1";
            // Windows 8
            if (version.Major == 6 && version.Minor == 2) return "Windows 8";
            // Windows 7
            if (version.Major == 6 && version.Minor == 1) return "Windows 7";
            return "Windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sw_vers",
                    Arguments = "-productVersion",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var process = Process.Start(psi);
                var version = process?.StandardOutput.ReadToEnd().Trim() ?? "";
                var majorVersion = int.Parse(version.Split('.')[0]);
                
                var macosNames = new Dictionary<int, string>
                {
                    { 15, "macOS Sequoia" },
                    { 14, "macOS Sonoma" },
                    { 13, "macOS Ventura" },
                    { 12, "macOS Monterey" },
                    { 11, "macOS Big Sur" }
                };
                
                return macosNames.GetValueOrDefault(majorVersion, $"macOS {version}");
            }
            catch
            {
                return "macOS";
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                if (File.Exists("/etc/os-release"))
                {
                    var lines = File.ReadAllLines("/etc/os-release");
                    var prettyName = lines.FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));
                    if (prettyName != null)
                    {
                        return prettyName.Substring(12).Trim('"');
                    }
                    var name = lines.FirstOrDefault(l => l.StartsWith("NAME="));
                    if (name != null)
                    {
                        return name.Substring(5).Trim('"');
                    }
                }
            }
            catch { }
            return "Linux";
        }

        return "Unknown OS";
    }

    // ========================================================================
    // Calculator implementation
    // ========================================================================

    /// <summary>
    /// Safe mathematical expression evaluator
    /// </summary>
    static string Calculate(string expression)
    {
        try
        {
            // Basic validation - check for forbidden patterns
            var forbiddenPatterns = new[]
            {
                "import", "require", "eval", "Function", "constructor",
                "prototype", "__proto__", "process.", "global.", "window.", "document."
            };
            
            foreach (var pattern in forbiddenPatterns)
            {
                if (expression.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return "Error: Forbidden pattern detected in expression";
                }
            }

            // Create evaluation context
            var context = new EvaluationContext();
            var result = context.Evaluate(expression);
            
            return FormatResult(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Format result for output
    /// </summary>
    static string FormatResult(object? result)
    {
        if (result == null) return "null";
        
        if (result is double d)
        {
            if (double.IsNaN(d)) return "NaN";
            if (double.IsInfinity(d)) return d > 0 ? "Infinity" : "-Infinity";
            if (d == Math.Truncate(d)) return ((long)d).ToString();
            return d.ToString();
        }
        
        if (result is float f)
        {
            if (float.IsNaN(f)) return "NaN";
            if (float.IsInfinity(f)) return f > 0 ? "Infinity" : "-Infinity";
            if (f == Math.Truncate(f)) return ((long)f).ToString();
            return f.ToString();
        }
        
        if (result is double[] arr)
        {
            return JsonSerializer.Serialize(arr);
        }
        
        return result.ToString() ?? "";
    }
}

// ========================================================================
// JSON-RPC types
// ========================================================================

class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("method")]
    public string Method { get; set; } = "";
    
    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }
}

class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

class ToolCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("arguments")]
    public JsonElement? Arguments { get; set; }
}

// ========================================================================
// Calculator evaluation context
// ========================================================================

class EvaluationContext
{
    private readonly Dictionary<string, object> _constants;
    private readonly Dictionary<string, Func<double, double>> _functions1;
    private readonly Dictionary<string, Func<double[], double>> _functionsN;

    public EvaluationContext()
    {
        // Constants
        _constants = new Dictionary<string, object>
        {
            ["pi"] = Math.PI,
            ["e"] = Math.E,
            ["tau"] = 2 * Math.PI,
            ["inf"] = double.PositiveInfinity,
            ["nan"] = double.NaN
        };

        // Single-argument functions
        _functions1 = new Dictionary<string, Func<double, double>>
        {
            ["sin"] = Math.Sin,
            ["cos"] = Math.Cos,
            ["tan"] = Math.Tan,
            ["asin"] = Math.Asin,
            ["acos"] = Math.Acos,
            ["atan"] = Math.Atan,
            ["sinh"] = Math.Sinh,
            ["cosh"] = Math.Cosh,
            ["tanh"] = Math.Tanh,
            ["asinh"] = Math.Asinh,
            ["acosh"] = Math.Acosh,
            ["atanh"] = Math.Atanh,
            ["log"] = Math.Log,
            ["log10"] = Math.Log10,
            ["log2"] = Math.Log2,
            ["exp"] = Math.Exp,
            ["sqrt"] = Math.Sqrt,
            ["ceil"] = Math.Ceiling,
            ["floor"] = Math.Floor,
            ["abs"] = Math.Abs,
            ["fabs"] = Math.Abs,
            ["factorial"] = Factorial,
            ["gamma"] = Gamma,
            ["lgamma"] = x => Math.Log(Gamma(x))
        };

        // N-argument functions (arrays)
        _functionsN = new Dictionary<string, Func<double[], double>>
        {
            ["mean"] = Mean,
            ["median"] = Median,
            ["mode"] = Mode,
            ["stdev"] = Stdev,
            ["variance"] = Variance,
            ["harmonic_mean"] = HarmonicMean,
            ["geometric_mean"] = GeometricMean,
            ["sum"] = arr => arr.Sum(),
            ["len"] = arr => arr.Length,
            ["fsum"] = arr => arr.Sum()
        };
    }

    /// <summary>
    /// Evaluate mathematical expression
    /// </summary>
    public object? Evaluate(string expression)
    {
        // Tokenize and parse expression
        var tokens = Tokenize(expression);
        var result = ParseExpression(tokens, 0);
        return result.Value;
    }

    // Tokenization
    private enum TokenType { Number, Operator, Function, Constant, Paren, Comma, Bracket, End }

    private record Token(TokenType Type, string Value);

    private List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var i = 0;
        
        while (i < expression.Length)
        {
            var c = expression[i];
            
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }
            
            if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
            {
                var start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    i++;
                tokens.Add(new Token(TokenType.Number, expression.Substring(start, i - start)));
                continue;
            }
            
            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                    i++;
                var name = expression.Substring(start, i - start);
                
                if (_constants.ContainsKey(name))
                    tokens.Add(new Token(TokenType.Constant, name));
                else if (_functions1.ContainsKey(name) || _functionsN.ContainsKey(name))
                    tokens.Add(new Token(TokenType.Function, name));
                else
                    throw new Exception($"Unknown identifier: {name}");
                continue;
            }
            
            if (c == '(' || c == ')')
            {
                tokens.Add(new Token(TokenType.Paren, c.ToString()));
                i++;
                continue;
            }
            
            if (c == '[' || c == ']')
            {
                tokens.Add(new Token(TokenType.Bracket, c.ToString()));
                i++;
                continue;
            }
            
            if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ","));
                i++;
                continue;
            }
            
            // Operators: + - * / ^ **
            if ("+-*/^".Contains(c))
            {
                if (c == '*' && i + 1 < expression.Length && expression[i + 1] == '*')
                {
                    tokens.Add(new Token(TokenType.Operator, "**"));
                    i += 2;
                }
                else
                {
                    tokens.Add(new Token(TokenType.Operator, c.ToString()));
                    i++;
                }
                continue;
            }
            
            throw new Exception($"Unexpected character: {c}");
        }
        
        tokens.Add(new Token(TokenType.End, ""));
        return tokens;
    }

    // Parsing
    private record ParseResult(object? Value, int Position);

    private ParseResult ParseExpression(List<Token> tokens, int pos)
    {
        return ParseAddSub(tokens, pos);
    }

    private ParseResult ParseAddSub(List<Token> tokens, int pos)
    {
        var left = ParseMulDiv(tokens, pos);
        
        while (tokens[left.Position].Type == TokenType.Operator && 
               (tokens[left.Position].Value == "+" || tokens[left.Position].Value == "-"))
        {
            var op = tokens[left.Position].Value;
            var right = ParseMulDiv(tokens, left.Position + 1);
            left = new ParseResult(
                op == "+" ? ToDouble(left.Value) + ToDouble(right.Value) : ToDouble(left.Value) - ToDouble(right.Value),
                right.Position
            );
        }
        
        return left;
    }

    private ParseResult ParseMulDiv(List<Token> tokens, int pos)
    {
        var left = ParsePower(tokens, pos);
        
        while (tokens[left.Position].Type == TokenType.Operator && 
               (tokens[left.Position].Value == "*" || tokens[left.Position].Value == "/"))
        {
            var op = tokens[left.Position].Value;
            var right = ParsePower(tokens, left.Position + 1);
            left = new ParseResult(
                op == "*" ? ToDouble(left.Value) * ToDouble(right.Value) : ToDouble(left.Value) / ToDouble(right.Value),
                right.Position
            );
        }
        
        return left;
    }

    private ParseResult ParsePower(List<Token> tokens, int pos)
    {
        var left = ParseUnary(tokens, pos);
        
        while (tokens[left.Position].Type == TokenType.Operator && 
               (tokens[left.Position].Value == "^" || tokens[left.Position].Value == "**"))
        {
            var right = ParseUnary(tokens, left.Position + 1);
            left = new ParseResult(
                Math.Pow(ToDouble(left.Value), ToDouble(right.Value)),
                right.Position
            );
        }
        
        return left;
    }

    private ParseResult ParseUnary(List<Token> tokens, int pos)
    {
        if (tokens[pos].Type == TokenType.Operator && tokens[pos].Value == "-")
        {
            var inner = ParsePrimary(tokens, pos + 1);
            return new ParseResult(-ToDouble(inner.Value), inner.Position);
        }
        
        if (tokens[pos].Type == TokenType.Operator && tokens[pos].Value == "+")
        {
            return ParsePrimary(tokens, pos + 1);
        }
        
        return ParsePrimary(tokens, pos);
    }

    private ParseResult ParsePrimary(List<Token> tokens, int pos)
    {
        var token = tokens[pos];
        
        // Number
        if (token.Type == TokenType.Number)
        {
            return new ParseResult(double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture), pos + 1);
        }
        
        // Constant
        if (token.Type == TokenType.Constant)
        {
            return new ParseResult(_constants[token.Value], pos + 1);
        }
        
        // Function call
        if (token.Type == TokenType.Function)
        {
            return ParseFunctionCall(tokens, pos);
        }
        
        // Parenthesized expression
        if (token.Type == TokenType.Paren && token.Value == "(")
        {
            var inner = ParseExpression(tokens, pos + 1);
            if (tokens[inner.Position].Type != TokenType.Paren || tokens[inner.Position].Value != ")")
                throw new Exception("Expected closing parenthesis");
            return new ParseResult(inner.Value, inner.Position + 1);
        }
        
        // Array literal [1, 2, 3]
        if (token.Type == TokenType.Bracket && token.Value == "[")
        {
            var elements = new List<double>();
            var p = pos + 1;
            
            while (tokens[p].Type != TokenType.Bracket || tokens[p].Value != "]")
            {
                var elem = ParseExpression(tokens, p);
                elements.Add(ToDouble(elem.Value));
                p = elem.Position;
                
                if (tokens[p].Type == TokenType.Comma)
                    p++;
            }
            
            return new ParseResult(elements.ToArray(), p + 1);
        }
        
        throw new Exception($"Unexpected token: {token.Value}");
    }

    private ParseResult ParseFunctionCall(List<Token> tokens, int pos)
    {
        var funcName = tokens[pos].Value;
        pos++;
        
        // Expect opening paren
        if (tokens[pos].Type != TokenType.Paren || tokens[pos].Value != "(")
            throw new Exception($"Expected ( after function name {funcName}");
        pos++;
        
        // Parse arguments
        var args = new List<object?>();
        
        while (tokens[pos].Type != TokenType.Paren || tokens[pos].Value != ")")
        {
            var arg = ParseExpression(tokens, pos);
            args.Add(arg.Value);
            pos = arg.Position;
            
            if (tokens[pos].Type == TokenType.Comma)
                pos++;
        }
        
        pos++; // Skip closing paren

        // Call function
        if (_functions1.ContainsKey(funcName) && args.Count == 1)
        {
            return new ParseResult(_functions1[funcName](ToDouble(args[0])), pos);
        }

        if (_functionsN.ContainsKey(funcName))
        {
            // If single argument is already an array, use it directly
            if (args.Count == 1 && args[0] is double[] directArr)
            {
                return new ParseResult(_functionsN[funcName](directArr), pos);
            }
            // Otherwise convert each argument to double
            var arr = args.Select(ToDouble).ToArray();
            return new ParseResult(_functionsN[funcName](arr), pos);
        }

        throw new Exception($"Unknown function: {funcName}");
    }

    private double ToDouble(object? value)
    {
        if (value is double d) return d;
        if (value is int i) return i;
        if (value is long l) return l;
        if (value is float f) return f;
        if (value is double[] arr) return arr[0];
        if (value is JsonElement elem)
        {
            if (elem.ValueKind == JsonValueKind.Number) return elem.GetDouble();
            if (elem.ValueKind == JsonValueKind.Array)
                return elem.EnumerateArray().Select(e => e.GetDouble()).ToArray()[0];
        }
        throw new Exception($"Cannot convert {value?.GetType().Name} to double");
    }

    // Mathematical functions
    private static double Factorial(double n)
    {
        if (n < 0) throw new Exception("factorial not defined for negative values");
        if (!IsInteger(n)) throw new Exception("factorial only defined for integers");
        var result = 1L;
        for (var i = 2; i <= (long)n; i++) result *= i;
        return result;
    }

    private static bool IsInteger(double n) => Math.Abs(n - Math.Truncate(n)) < 1e-10;

    // Gamma function using Lanczos approximation
    private static double Gamma(double z)
    {
        var g = 7;
        var c = new[]
        {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7
        };

        double GammaFunc(double x)
        {
            if (x < 0.5)
                return Math.PI / (Math.Sin(Math.PI * x) * GammaFunc(1 - x));
            
            x -= 1;
            var t = x + g + 0.5;
            var sum = c[0];
            for (var i = 1; i < g + 2; i++)
                sum += c[i] / (x + i);
            
            return Math.Sqrt(2 * Math.PI) * Math.Pow(t, x + 0.5) * Math.Exp(-t) * sum;
        }

        return GammaFunc(z);
    }

    // Statistical functions
    private static double Mean(double[] data)
    {
        if (data.Length == 0) throw new Exception("mean requires at least one data point");
        return data.Average();
    }

    private static double Median(double[] data)
    {
        if (data.Length == 0) throw new Exception("median requires at least one data point");
        var sorted = data.OrderBy(x => x).ToArray();
        var mid = sorted.Length / 2;
        return sorted.Length % 2 != 0 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
    }

    private static double Mode(double[] data)
    {
        if (data.Length == 0) throw new Exception("mode requires at least one data point");
        var counts = data.GroupBy(x => x).OrderByDescending(g => g.Count());
        return counts.First().Key;
    }

    private static double Stdev(double[] data)
    {
        if (data.Length < 2) throw new Exception("stdev requires at least two data points");
        var avg = data.Average();
        var sumSq = data.Sum(x => (x - avg) * (x - avg));
        return Math.Sqrt(sumSq / (data.Length - 1));
    }

    private static double Variance(double[] data)
    {
        if (data.Length < 2) throw new Exception("variance requires at least two data points");
        var avg = data.Average();
        var sumSq = data.Sum(x => (x - avg) * (x - avg));
        return sumSq / (data.Length - 1);
    }

    private static double HarmonicMean(double[] data)
    {
        if (data.Length == 0) throw new Exception("harmonic_mean requires at least one data point");
        if (data.Any(x => x <= 0)) throw new Exception("harmonic_mean only defined for positive real numbers");
        return data.Length / data.Sum(x => 1 / x);
    }

    private static double GeometricMean(double[] data)
    {
        if (data.Length == 0) throw new Exception("geometric_mean requires at least one data point");
        if (data.Any(x => x <= 0)) throw new Exception("geometric_mean only defined for positive real numbers");
        var product = data.Aggregate(1.0, (p, x) => p * x);
        return Math.Pow(product, 1.0 / data.Length);
    }
}
