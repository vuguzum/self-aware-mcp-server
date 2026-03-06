#!/usr/bin/env node

/**
 * Self-Aware MCP Server
 * MCP (Model Context Protocol) сервер на Node.js, предоставляющий инструменты самосознания для LLM.
 * 
 * @author Alexander Kazantsev with z.ai
 * @email akazant@gmail.com
 * @license MIT
 */

import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import * as os from "os";

// Get location from command line argument
const LOCATION_ARG = process.argv[2] || "Unknown";

// Create MCP server
const server = new McpServer({
  name: "self-aware",
  version: "1.0.0",
  description: "MCP server providing self-awareness tools: location, datetime, and calculator"
});

// Tool 1: get_current_location
server.tool(
  "get_current_location",
  "Returns the current location passed as a command line argument when starting the MCP server.",
  {},
  async () => {
    return {
      content: [
        {
          type: "text" as const,
          text: LOCATION_ARG
        }
      ]
    };
  }
);

// Tool 2: get_current_date_time
server.tool(
  "get_current_date_time",
  "Get current date and time as weekday, year-month-day hours:minutes:seconds",
  {},
  async () => {
    const now = new Date();
    
    // Format options for weekday
    const weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    const weekday = weekdays[now.getDay()];
    
    // Format date and time
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    const hours = String(now.getHours()).padStart(2, "0");
    const minutes = String(now.getMinutes()).padStart(2, "0");
    const seconds = String(now.getSeconds()).padStart(2, "0");
    
    const formattedDateTime = `${weekday}, ${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    
    return {
      content: [
        {
          type: "text" as const,
          text: formattedDateTime
        }
      ]
    };
  }
);

// Tool 3: get_current_system
server.tool(
  "get_current_system",
  "Returns the main operating system parameters including name, version, displayName, platform, and architecture.",
  {},
  async () => {
    const systemInfo = {
      os: {
        name: os.type(),
        version: os.release(),
        displayName: getDisplayName(),
        platform: os.platform(),
        cpu: os.arch()
      }
    };
    
    return {
      content: [
        {
          type: "text" as const,
          text: JSON.stringify(systemInfo, null, 2)
        }
      ]
    };
  }
);

// Helper function to get display name for the OS
function getDisplayName(): string {
  const platform = os.platform();
  
  if (platform === "linux") {
    try {
      const fs = require("fs");
      // Try to read /etc/os-release
      if (fs.existsSync("/etc/os-release")) {
        const content = fs.readFileSync("/etc/os-release", "utf-8");
        const prettyNameMatch = content.match(/^PRETTY_NAME="(.+)"/m);
        if (prettyNameMatch) {
          return prettyNameMatch[1];
        }
        const nameMatch = content.match(/^NAME="(.+)"/m);
        if (nameMatch) {
          return nameMatch[1];
        }
      }
    } catch {
      // Fallback to generic Linux
    }
    return "Linux";
  }
  
  if (platform === "darwin") {
    // Try to get macOS version name
    try {
      const execSync = require("child_process").execSync;
      const version = execSync("sw_vers -productVersion", { encoding: "utf-8" }).trim();
      const majorVersion = parseInt(version.split(".")[0]);
      
      const macosNames: Record<number, string> = {
        15: "macOS Sequoia",
        14: "macOS Sonoma",
        13: "macOS Ventura",
        12: "macOS Monterey",
        11: "macOS Big Sur",
        10: "macOS " + getMacOSName(version)
      };
      
      return macosNames[majorVersion] || `macOS ${version}`;
    } catch {
      return "macOS";
    }
  }
  
  if (platform === "win32") {
    // Determine Windows version by build number
    const release = os.release();
    const buildMatch = release.match(/(\d+)\.(\d+)\.(\d+)/);
    
    if (buildMatch) {
      const major = parseInt(buildMatch[1]);
      const minor = parseInt(buildMatch[2]);
      const build = parseInt(buildMatch[3]);
      
      // Windows 11: build >= 22000
      if (major === 10 && build >= 22000) {
        return "Windows 11";
      }
      // Windows 10: build < 22000
      if (major === 10 && build < 22000) {
        return "Windows 10";
      }
      // Windows 8.1
      if (major === 6 && minor === 3) {
        return "Windows 8.1";
      }
      // Windows 8
      if (major === 6 && minor === 2) {
        return "Windows 8";
      }
      // Windows 7
      if (major === 6 && minor === 1) {
        return "Windows 7";
      }
    }
    return "Windows";
  }
  
  return os.type();
}

// Helper for older macOS versions
function getMacOSName(version: string): string {
  const minorVersion = parseInt(version.split(".")[1]);
  const names: Record<number, string> = {
    15: "Sequoia",
    14: "Sonoma", 
    13: "Ventura",
    12: "Monterey",
    11: "Big Sur",
    10: "Catalina",
    9: "Mojave",
    8: "High Sierra",
    7: "Sierra",
    6: "El Capitan",
    5: "Yosemite"
  };
  return names[minorVersion] || version;
}

// ============================================================================
// Tool 4: calculate - Safe mathematical expression evaluator
// ============================================================================

/**
 * Helper: Parse array from expression
 * Supports both [1, 2, 3] and (1, 2, 3) formats
 */
function parseArrayArg(arg: unknown): number[] {
  if (Array.isArray(arg)) {
    return arg as number[];
  }
  // If it's a single number, wrap it
  if (typeof arg === "number") {
    return [arg];
  }
  return [];
}

/**
 * Harmonic mean - Python statistics.harmonic_mean implementation
 */
function harmonic_mean(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("harmonic_mean requires at least one data point");
  }
  if (arr.some(x => typeof x !== "number" || x <= 0)) {
    throw new Error("harmonic_mean only defined for positive real numbers");
  }
  return arr.length / arr.reduce((sum, x) => sum + 1 / x, 0);
}

/**
 * Geometric mean - Python statistics.geometric_mean implementation
 */
function geometric_mean(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("geometric_mean requires at least one data point");
  }
  if (arr.some(x => typeof x !== "number" || x <= 0)) {
    throw new Error("geometric_mean only defined for positive real numbers");
  }
  const product = arr.reduce((prod, x) => prod * x, 1);
  return Math.pow(product, 1 / arr.length);
}

/**
 * Mean - Python statistics.mean implementation
 */
function mean(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("mean requires at least one data point");
  }
  return arr.reduce((sum, x) => sum + x, 0) / arr.length;
}

/**
 * Median - Python statistics.median implementation
 */
function median(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("median requires at least one data point");
  }
  const sorted = [...arr].sort((a, b) => a - b);
  const mid = Math.floor(sorted.length / 2);
  return sorted.length % 2 !== 0
    ? sorted[mid]
    : (sorted[mid - 1] + sorted[mid]) / 2;
}

/**
 * Mode - Python statistics.mode implementation
 */
function mode(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("mode requires at least one data point");
  }
  const counts = new Map<number, number>();
  arr.forEach(n => counts.set(n, (counts.get(n) || 0) + 1));
  let maxCount = 0;
  let result = arr[0];
  counts.forEach((count, value) => {
    if (count > maxCount) {
      maxCount = count;
      result = value;
    }
  });
  return result;
}

/**
 * Standard deviation (population) - Python statistics.stdev implementation
 */
function stdev(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length < 2) {
    throw new Error("stdev requires at least two data points");
  }
  const avg = mean(arr);
  const squareDiffs = arr.map(value => Math.pow(value - avg, 2));
  return Math.sqrt(squareDiffs.reduce((a, b) => a + b, 0) / (arr.length - 1));
}

/**
 * Variance - Python statistics.variance implementation
 */
function variance(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length < 2) {
    throw new Error("variance requires at least two data points");
  }
  const avg = mean(arr);
  const squareDiffs = arr.map(value => Math.pow(value - avg, 2));
  return squareDiffs.reduce((a, b) => a + b, 0) / (arr.length - 1);
}

/**
 * Sum - Python sum implementation
 */
function sum(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  return arr.reduce((s, x) => s + x, 0);
}

/**
 * Length - Python len implementation
 */
function len(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  return arr.length;
}

/**
 * Factorial - Python math.factorial implementation
 */
function factorial(n: number): number {
  if (n < 0) {
    throw new Error("factorial not defined for negative values");
  }
  if (!Number.isInteger(n)) {
    throw new Error("factorial only defined for integers");
  }
  if (n === 0 || n === 1) return 1;
  let result = 1;
  for (let i = 2; i <= n; i++) {
    result *= i;
  }
  return result;
}

/**
 * Gamma function - Python math.gamma implementation using Lanczos approximation
 */
function gamma(n: number): number {
  const g = 7;
  const c = [
    0.99999999999980993,
    676.5203681218851,
    -1259.1392167224028,
    771.32342877765313,
    -176.61502916214059,
    12.507343278686905,
    -0.13857109526572012,
    9.9843695780195716e-6,
    1.5056327351493116e-7
  ];
  
  const gammaFunc = (z: number): number => {
    if (z < 0.5) {
      return Math.PI / (Math.sin(Math.PI * z) * gammaFunc(1 - z));
    }
    z -= 1;
    let x = c[0];
    for (let i = 1; i < g + 2; i++) {
      x += c[i] / (z + i);
    }
    const t = z + g + 0.5;
    return Math.sqrt(2 * Math.PI) * Math.pow(t, z + 0.5) * Math.exp(-t) * x;
  };
  
  return gammaFunc(n);
}

/**
 * Log gamma - Python math.lgamma implementation
 */
function lgamma(n: number): number {
  return Math.log(gamma(n));
}

/**
 * Modf - Python math.modf implementation
 * Returns [fractional, integer] parts
 */
function modf(x: number): [number, number] {
  if (Number.isNaN(x)) return [NaN, NaN];
  if (!Number.isFinite(x)) {
    return x > 0 ? [0.0, Infinity] : [-0.0, -Infinity];
  }
  const intPart = x >= 0 ? Math.floor(x) : Math.ceil(x);
  const fracPart = x - intPart;
  return [fracPart, intPart];
}

/**
 * Fsum - Python math.fsum implementation (accurate floating point sum)
 */
function fsum(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  // Simple implementation - for full accuracy would need Kahan summation
  return arr.reduce((s, x) => s + x, 0);
}

/**
 * Fabs - Python math.fabs implementation (absolute value for float)
 */
function fabs(x: number): number {
  return Math.abs(x);
}

/**
 * Safe expression evaluator using AST-like parsing
 */
function safeEvaluate(expression: string): number | string {
  try {
    // Basic validation - check for forbidden patterns
    const forbiddenPatterns = [
      /import\s+/i,
      /require\s*\(/i,
      /eval\s*\(/i,
      /Function\s*\(/i,
      /constructor/i,
      /prototype/i,
      /__proto__/i,
      /process\s*\./i,
      /global\s*\./i,
      /window\s*\./i,
      /document\s*\./i,
      /\.\s*\./,
    ];
    
    for (const pattern of forbiddenPatterns) {
      if (pattern.test(expression)) {
        return `Error: Forbidden pattern detected in expression`;
      }
    }
    
    // Create safe scope with allowed functions and constants
    const scope: Record<string, unknown> = {
      // Math object for internal use (pow, etc.)
      Math: { pow: Math.pow },

      // Math constants
      pi: Math.PI,
      e: Math.E,
      tau: 2 * Math.PI,
      inf: Infinity,
      nan: NaN,
      
      // Trigonometric functions (radians)
      sin: Math.sin,
      cos: Math.cos,
      tan: Math.tan,
      asin: Math.asin,
      acos: Math.acos,
      atan: Math.atan,
      
      // Hyperbolic functions
      sinh: Math.sinh,
      cosh: Math.cosh,
      tanh: Math.tanh,
      asinh: Math.asinh,
      acosh: Math.acosh,
      atanh: Math.atanh,
      
      // Logarithmic functions
      log: Math.log,
      log10: Math.log10,
      log2: Math.log2,
      exp: Math.exp,
      expm1: Math.expm1,
      log1p: Math.log1p,
      
      // Other math functions
      sqrt: Math.sqrt,
      ceil: Math.ceil,
      floor: Math.floor,
      fabs: fabs,
      factorial: factorial,
      gamma: gamma,
      lgamma: lgamma,
      modf: modf,
      fsum: fsum,
      abs: Math.abs,
      
      // Statistical functions
      mean: mean,
      median: median,
      mode: mode,
      stdev: stdev,
      variance: variance,
      harmonic_mean: harmonic_mean,
      geometric_mean: geometric_mean,
      sum: sum,
      len: len,
    };
    
    // Preprocess expression: convert ** to Math.pow() for proper exponentiation
    // Handle arrays first
    let processedExpr = expression
      .replace(/\[(\s*[\d.]+\s*(,\s*[\d.]+\s*)*)\]/g, "[$1]");

    // Convert a ** b to Math.pow(a, b) using a more robust approach
    // This handles nested powers correctly: a ** b ** c = a ** (b ** c)
    processedExpr = processedExpr.replace(/([^*\s]|^)\*\*([^*]|$)/g, "$1__POW__$2");
    while (processedExpr.includes("__POW__")) {
      processedExpr = processedExpr.replace(/(\S+?)__POW__(\S+)/g, "Math.pow($1, $2)");
    }

    // Use Function constructor with restricted scope (safer than eval)
    const funcBody = `
      "use strict";
      const {${Object.keys(scope).join(", ")}} = this;
      return (${processedExpr});
    `;

    const func = new Function(funcBody);
    const result = func.call(scope);
    
    // Normalize result for JSON serialization
    if (typeof result === "number") {
      if (Number.isNaN(result)) {
        return "NaN";
      }
      if (!Number.isFinite(result)) {
        return result > 0 ? "Infinity" : "-Infinity";
      }
      if (Number.isInteger(result)) {
        return result;
      }
      return result;
    }
    
    if (typeof result === "boolean") {
      return result ? 1 : 0;
    }
    
    // Handle array results (e.g., from modf)
    if (Array.isArray(result)) {
      return JSON.stringify(result);
    }
    
    return String(result);
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    return `Error: ${errorMessage}`;
  }
}

server.tool(
  "calculate",
  `Эта функция позволяет вычислять математические выражения, переданные в виде строки,
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
- Для статистических функций требуется передавать массив (например, [1, 2, 3, 4, 5])`,
  {
    expression: z.string().describe("Строка с математическим выражением для вычисления")
  },
  async ({ expression }) => {
    const result = safeEvaluate(expression);
    
    let resultText: string;
    if (typeof result === "number") {
      resultText = String(result);
    } else {
      resultText = result;
    }
    
    return {
      content: [
        {
          type: "text" as const,
          text: resultText
        }
      ]
    };
  }
);

// Start the server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("Self-aware MCP server running on stdio");
  console.error(`Location: ${LOCATION_ARG}`);
}

main().catch((error) => {
  console.error("Fatal error:", error);
  process.exit(1);
});
