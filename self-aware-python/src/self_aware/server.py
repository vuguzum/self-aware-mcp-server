"""
@author Alexander Kazantsev with z.ai
@email akazant@gmail.com
@license MIT
"""

import asyncio
import json
import math
import os
import platform
import statistics
import subprocess
import sys
from datetime import datetime
from typing import Union

from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent

# Get location from command line argument
LOCATION_ARG = sys.argv[1] if len(sys.argv) > 1 else "Unknown"

# Create MCP server
app = Server("self-aware")


# ============================================================================
# Tool handlers
# ============================================================================

@app.list_tools()
async def list_tools() -> list[Tool]:
    """Return list of available tools"""
    return [
        Tool(
            name="get_current_location",
            description="Returns the current location passed as a command line argument when starting the MCP server.",
            inputSchema={"type": "object", "properties": {}}
        ),
        Tool(
            name="get_current_date_time",
            description="Get current date and time as weekday, year-month-day hours:minutes:seconds",
            inputSchema={"type": "object", "properties": {}}
        ),
        Tool(
            name="get_current_system",
            description="Returns the main operating system parameters including name, version, displayName, platform, and architecture.",
            inputSchema={"type": "object", "properties": {}}
        ),
        Tool(
            name="calculate",
            description="""Эта функция позволяет вычислять математические выражения, переданные в виде строки,
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
- Для статистических функций требуется передавать массив (например, [1, 2, 3, 4, 5])""",
            inputSchema={
                "type": "object",
                "properties": {
                    "expression": {
                        "type": "string",
                        "description": "Строка с математическим выражением для вычисления"
                    }
                },
                "required": ["expression"]
            }
        )
    ]


@app.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    """Handle tool calls"""
    if name == "get_current_location":
        return [TextContent(type="text", text=get_current_location())]
    elif name == "get_current_date_time":
        return [TextContent(type="text", text=get_current_date_time())]
    elif name == "get_current_system":
        return [TextContent(type="text", text=get_current_system())]
    elif name == "calculate":
        expression = arguments.get("expression", "")
        result = calculate(expression)
        return [TextContent(type="text", text=str(result))]
    else:
        return [TextContent(type="text", text=f"Unknown tool: {name}")]


# ============================================================================
# Tool implementations
# ============================================================================

def get_current_location() -> str:
    """Get current location from command line argument"""
    return LOCATION_ARG


def get_current_date_time() -> str:
    """Get current date and time"""
    now = datetime.now()
    weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    weekday = weekdays[now.weekday()]
    return f"{weekday}, {now.year}-{now.month:02d}-{now.day:02d} {now.hour:02d}:{now.minute:02d}:{now.second:02d}"


def get_current_system() -> str:
    """Get current system information"""
    system_info = {
        "os": {
            "name": _get_os_name(),
            "version": platform.version(),
            "displayName": _get_os_display_name(),
            "platform": platform.system().lower(),
            "cpu": platform.machine().lower()
        }
    }
    return json.dumps(system_info, indent=2)


def _get_os_name() -> str:
    """Get OS name"""
    system = platform.system()
    if system == "Windows":
        return "Windows_NT"
    elif system == "Linux":
        return "Linux"
    elif system == "Darwin":
        return "Darwin"
    return "Unknown"


def _get_os_display_name() -> str:
    """Get human-readable OS display name"""
    system = platform.system()
    
    if system == "Windows":
        version = platform.version()
        try:
            build = int(version.split('.')[-1])
            if build >= 22000:
                return "Windows 11"
            elif build >= 10240:
                return "Windows 10"
            elif build >= 9600:
                return "Windows 8.1"
            elif build >= 9200:
                return "Windows 8"
            elif build >= 7600:
                return "Windows 7"
        except:
            pass
        return "Windows"
    
    elif system == "Darwin":
        try:
            result = subprocess.run(["sw_vers", "-productVersion"], capture_output=True, text=True)
            version = result.stdout.strip()
            major = int(version.split('.')[0])
            
            macos_names = {
                15: "macOS Sequoia",
                14: "macOS Sonoma",
                13: "macOS Ventura",
                12: "macOS Monterey",
                11: "macOS Big Sur"
            }
            return macos_names.get(major, f"macOS {version}")
        except:
            return "macOS"
    
    elif system == "Linux":
        try:
            with open("/etc/os-release", "r") as f:
                lines = f.readlines()
                for line in lines:
                    if line.startswith("PRETTY_NAME="):
                        return line.split("=", 1)[1].strip().strip('"')
                    if line.startswith("NAME="):
                        return line.split("=", 1)[1].strip().strip('"')
        except:
            pass
        return "Linux"
    
    return "Unknown OS"


# ============================================================================
# Calculator implementation
# ============================================================================

def _harmonic_mean(data: list) -> float:
    """Python statistics.harmonic_mean implementation"""
    if len(data) == 0:
        raise ValueError("harmonic_mean requires at least one data point")
    if any(not isinstance(x, (int, float)) or x <= 0 for x in data):
        raise ValueError("harmonic_mean only defined for positive real numbers")
    return len(data) / sum(1/x for x in data)


def _geometric_mean(data: list) -> float:
    """Python statistics.geometric_mean implementation"""
    if len(data) == 0:
        raise ValueError("geometric_mean requires at least one data point")
    if any(not isinstance(x, (int, float)) or x <= 0 for x in data):
        raise ValueError("geometric_mean only defined for positive real numbers")
    product = 1.0
    for x in data:
        product *= x
    return product ** (1 / len(data))


# Safe evaluation scope
SAFE_SCOPE = {
    # Math constants
    "pi": math.pi,
    "e": math.e,
    "tau": math.tau,
    "inf": math.inf,
    "nan": math.nan,
    
    # Trigonometric functions (radians)
    "sin": math.sin,
    "cos": math.cos,
    "tan": math.tan,
    "asin": math.asin,
    "acos": math.acos,
    "atan": math.atan,
    
    # Hyperbolic functions
    "sinh": math.sinh,
    "cosh": math.cosh,
    "tanh": math.tanh,
    "asinh": math.asinh,
    "acosh": math.acosh,
    "atanh": math.atanh,
    
    # Logarithmic functions
    "log": math.log,
    "log10": math.log10,
    "log2": math.log2,
    "exp": math.exp,
    "expm1": math.expm1,
    "log1p": math.log1p,
    
    # Other math functions
    "sqrt": math.sqrt,
    "ceil": math.ceil,
    "floor": math.floor,
    "fabs": math.fabs,
    "factorial": math.factorial,
    "gamma": math.gamma,
    "lgamma": math.lgamma,
    "modf": math.modf,
    "fsum": math.fsum,
    "abs": abs,
    
    # Statistical functions
    "mean": statistics.mean,
    "median": statistics.median,
    "mode": statistics.mode,
    "stdev": statistics.stdev,
    "variance": statistics.variance,
    "harmonic_mean": _harmonic_mean,
    "geometric_mean": _geometric_mean,
    "sum": sum,
    "len": len,
}


def calculate(expression: str) -> Union[float, int, str]:
    """
    Safe mathematical expression evaluator.
    
    Uses AST-like validation and restricted scope for safe evaluation.
    """
    try:
        # Basic validation - check for forbidden patterns
        forbidden = [
            "import", "exec", "eval", "compile", "open", "file",
            "__", "getattr", "setattr", "delattr", "globals", "locals",
            "vars", "dir", "type", "class", "def", "lambda",
        ]
        
        for word in forbidden:
            if word in expression.lower():
                return "Error: Forbidden pattern detected in expression"
        
        # Evaluate with restricted scope
        result = eval(expression, {"__builtins__": {}}, SAFE_SCOPE)
        
        # Normalize result
        if isinstance(result, float):
            if math.isnan(result):
                return "NaN"
            if math.isinf(result):
                return "Infinity" if result > 0 else "-Infinity"
            # Return as int if it's a whole number
            if result == int(result):
                return int(result)
            return result
        
        if isinstance(result, bool):
            return 1 if result else 0
        
        if isinstance(result, (list, tuple)):
            return json.dumps(list(result))
        
        return str(result)
        
    except Exception as e:
        return f"Error: {e}"


# ============================================================================
# Main entry point
# ============================================================================

def main():
    """Run the MCP server"""
    print(f"Self-aware MCP server running on stdio", file=sys.stderr)
    print(f"Location: {LOCATION_ARG}", file=sys.stderr)
    
    asyncio.run(run_server())


async def run_server():
    """Async server runner"""
    async with stdio_server() as (read_stream, write_stream):
        await app.run(read_stream, write_stream, app.create_initialization_options())


if __name__ == "__main__":
    main()
