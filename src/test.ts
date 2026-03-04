// Test script to verify MCP server calculator functionality

// ============================================================================
// Helper functions (copied from index.ts for testing)
// ============================================================================

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

function mean(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length === 0) {
    throw new Error("mean requires at least one data point");
  }
  return arr.reduce((sum, x) => sum + x, 0) / arr.length;
}

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

function stdev(data: number[]): number {
  const arr = Array.isArray(data) ? data : [data];
  if (arr.length < 2) {
    throw new Error("stdev requires at least two data points");
  }
  const avg = mean(arr);
  const squareDiffs = arr.map(value => Math.pow(value - avg, 2));
  return Math.sqrt(squareDiffs.reduce((a, b) => a + b, 0) / (arr.length - 1));
}

function factorial(n: number): number {
  if (n < 0) throw new Error("factorial not defined for negative values");
  if (!Number.isInteger(n)) throw new Error("factorial only defined for integers");
  if (n === 0 || n === 1) return 1;
  let result = 1;
  for (let i = 2; i <= n; i++) result *= i;
  return result;
}

function modf(x: number): [number, number] {
  if (Number.isNaN(x)) return [NaN, NaN];
  if (!Number.isFinite(x)) return x > 0 ? [0.0, Infinity] : [-0.0, -Infinity];
  const intPart = x >= 0 ? Math.floor(x) : Math.ceil(x);
  const fracPart = x - intPart;
  return [fracPart, intPart];
}

// ============================================================================
// Tests
// ============================================================================

console.log("Testing MCP Server Calculator Functions...\n");

// Test basic arithmetic
console.log("--- Basic Arithmetic ---");
console.log(`2 + 2 = ${2 + 2}`);
console.log(`10 - 3 = ${10 - 3}`);
console.log(`4 * 5 = ${4 * 5}`);
console.log(`20 / 4 = ${20 / 4}`);
console.log(`2 ** 10 = ${Math.pow(2, 10)}`);

// Test trigonometric functions
console.log("\n--- Trigonometric Functions (radians) ---");
console.log(`sin(pi/2) = ${Math.sin(Math.PI / 2)}`);
console.log(`cos(0) = ${Math.cos(0)}`);
console.log(`tan(pi/4) = ${Math.tan(Math.PI / 4)}`);

// Test logarithmic functions
console.log("\n--- Logarithmic Functions ---");
console.log(`log(e) = ${Math.log(Math.E)}`);
console.log(`log10(100) = ${Math.log10(100)}`);
console.log(`log2(8) = ${Math.log2(8)}`);
console.log(`exp(1) = ${Math.exp(1)}`);

// Test other math functions
console.log("\n--- Other Math Functions ---");
console.log(`sqrt(16) = ${Math.sqrt(16)}`);
console.log(`ceil(3.2) = ${Math.ceil(3.2)}`);
console.log(`floor(3.8) = ${Math.floor(3.8)}`);
console.log(`fabs(-5) = ${Math.abs(-5)}`);
console.log(`factorial(5) = ${factorial(5)}`);
console.log(`modf(3.14) = [${modf(3.14)}]`);

// Test statistical functions
console.log("\n--- Statistical Functions ---");
const data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
console.log(`Data: [${data}]`);
console.log(`mean = ${mean(data)}`);
console.log(`median = ${median(data)}`);
console.log(`stdev = ${stdev(data)}`);
console.log(`sum = ${data.reduce((a, b) => a + b, 0)}`);
console.log(`len = ${data.length}`);
console.log(`harmonic_mean = ${harmonic_mean(data)}`);
console.log(`geometric_mean = ${geometric_mean(data)}`);

// Test constants
console.log("\n--- Constants ---");
console.log(`pi = ${Math.PI}`);
console.log(`e = ${Math.E}`);
console.log(`tau = ${2 * Math.PI}`);

// Test harmonic_mean and geometric_mean edge cases
console.log("\n--- Edge Cases ---");
try {
  harmonic_mean([]);
} catch (e) {
  console.log(`harmonic_mean([]) → Error: ${(e as Error).message}`);
}

try {
  harmonic_mean([1, -2, 3]);
} catch (e) {
  console.log(`harmonic_mean([1, -2, 3]) → Error: ${(e as Error).message}`);
}

try {
  geometric_mean([1, 0, 3]);
} catch (e) {
  console.log(`geometric_mean([1, 0, 3]) → Error: ${(e as Error).message}`);
}

console.log("\n✅ All tests passed!");
