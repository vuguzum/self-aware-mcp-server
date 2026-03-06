# Self-Aware MCP Server (Node.js)

MCP сервер, предоставляющий инструменты получения базовой информации (самосознания) для LLM - текущее время, место, ОС, математические вычисления.

## Установка

```bash
cd mcp-servers/self-aware
bun install
bun run build
```

## Использование с Claude Desktop

Добавьте в конфигурацию Claude Desktop (`claude_desktop_config.json`):

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "node",
      "args": [
        "/absolute/path/to/mcp-servers/self-aware/dist/index.js",
        "Moscow, Russia"
      ]
    }
  }
}
```

> **Важно**: Второй аргумент (`Moscow, Russia`) — это местоположение, которое будет возвращать `get_current_location()`.

## Доступные инструменты

### 1. `get_current_location`

Возвращает местоположение, переданное как аргумент командной строки при запуске сервера.

**Пример использования:**
```
User: Where am I?
AI: [calls get_current_location]
You are currently in: Moscow, Russia
```

### 2. `get_current_date_time`

Возвращает текущую дату и время в формате: `weekday, year-month-day hours:minutes:seconds`

**Пример использования:**
```
User: What time is it?
AI: [calls get_current_date_time]
Current date and time: Wednesday, 2026-03-04 21:56:01
```

### 3. `get_current_system`

Возвращает основные параметры операционной системы в формате JSON:

```json
{
  "os": {
    "name": "Linux",
    "version": "6.1.0-generic",
    "displayName": "Ubuntu 22.04.3 LTS",
    "platform": "linux",
    "cpu": "x64"
  }
}
```

**Windows пример:**
```json
{
  "os": {
    "name": "Windows_NT",
    "version": "10.0.22631",
    "displayName": "Windows 11",
    "platform": "win32",
    "cpu": "x64"
  }
}
```

**macOS пример:**
```json
{
  "os": {
    "name": "Darwin",
    "version": "23.2.0",
    "displayName": "macOS Sonoma",
    "platform": "darwin",
    "cpu": "arm64"
  }
}
```

**Возможные значения `cpu`:**
- `"x64"` — 64-битный Intel/AMD
- `"arm64"` — 64-битный ARM (Apple Silicon)
- `"ia32"` — 32-битный Intel/AMD
- `"arm"` — 32-битный ARM

**Поддерживаемые displayName:**
- **Windows**: "Windows 11", "Windows 10", "Windows 8.1", "Windows 8", "Windows 7"
- **macOS**: "macOS Sequoia", "macOS Sonoma", "macOS Ventura", "macOS Monterey", "macOS Big Sur", "macOS Catalina", "macOS Mojave" и др.
- **Linux**: определяется из `/etc/os-release` (например, "Ubuntu 22.04.3 LTS")

**Пример использования:**
```
User: What system am I running on?
AI: [calls get_current_system]
System info: Linux (Ubuntu 22.04.3 LTS) on x64 architecture
```

### 4. `calculate`

Эта функция позволяет вычислять математические выражения, переданные в виде строки, с защитой от выполнения небезопасного кода. Поддерживает широкий набор математических и статистических функций, безопасных для выполнения в изолированной среде.

**Тригонометрические функции принимают аргументы в радианах.**

**Parameters:**
- `expression` : str — Строка с математическим выражением для вычисления. Может содержать:
  - Арифметические операции (`+`, `-`, `*`, `/`, `**`)
  - Математические функции (`sin`, `cos`, `log`, `sqrt` и др.)
  - Статистические функции (`mean`, `median`, `stdev` и др.)
  - Математические константы (`pi`, `e`, `inf`, `nan`)

**Returns:**
- `float` или `int` — при успешном вычислении числового результата
- `str` — сообщение об ошибке при возникновении исключения

**Supported Mathematical Functions:**

| Категория | Функции |
|-----------|---------|
| Trigonometric (radians) | `sin`, `cos`, `tan`, `asin`, `acos`, `atan` |
| Hyperbolic | `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh` |
| Logarithmic | `log`, `log10`, `log2`, `exp`, `expm1`, `log1p` |
| Other | `sqrt`, `ceil`, `floor`, `fabs`, `factorial`, `gamma`, `lgamma`, `modf`, `fsum`, `abs` |
| Constants | `pi`, `e`, `tau`, `inf`, `nan` |

**Supported Statistical Functions:**

| Функция | Описание |
|---------|----------|
| `mean(data)` | среднее арифметическое |
| `median(data)` | медиана |
| `mode(data)` | мода (наиболее частое значение) |
| `stdev(data)` | стандартное отклонение |
| `variance(data)` | дисперсия |
| `harmonic_mean(data)` | гармоническое среднее |
| `geometric_mean(data)` | геометрическое среднее |
| `sum(iterable)` | сумма элементов |
| `len(iterable)` | длина последовательности |

**Примеры:**
```
calculate("2 + 2 * 3")                      → 8
calculate("sin(pi/2)")                      → 1
calculate("sqrt(16)")                       → 4
calculate("factorial(5)")                   → 120
calculate("mean([1, 2, 3, 4, 5])")          → 3
calculate("stdev([1, 2, 3, 4, 5])")         → 1.58...
calculate("harmonic_mean([1, 2, 3, 4, 5])") → 2.19...
calculate("geometric_mean([1, 2, 3, 4, 5])")→ 2.61...
calculate("log(e)")                         → 1
calculate("2**10")                          → 1024
calculate("sum([1, 2, 3, 4, 5])")           → 15
calculate("len([1, 2, 3, 4, 5])")           → 5
calculate("modf(3.14)")                     → [0.14, 3]
```

## Безопасность

Функция `calculate` использует безопасное вычисление выражений:
- Запрещены операторы `import`, `require`, `eval`
- Запрещен доступ к системным объектам (`process`, `global`, `window`)
- Запрещен доступ к прототипам и конструкторам
- Поддерживаются только функции из разрешенного списка
- Результаты автоматически нормализуются для JSON-сериализации

## Разработка

```bash
# Запуск в режиме разработки с горячей перезагрузкой
bun run dev

# Сборка для продакшена
bun run build

# Запуск собранного сервера
bun run start

# Запуск тестов
bun run src/test.ts
```

## Тестирование

Для тестирования сервера можно использовать MCP Inspector:

```bash
npx @modelcontextprotocol/inspector node /путь/к/dist/index.js "Test Location"
```

## Структура проекта

```
self-aware/
├── package.json     # Зависимости проекта
├── tsconfig.json    # Конфигурация TypeScript
├── src/
│   ├── index.ts     # Основной код MCP сервера
│   └── test.ts      # Тесты для калькулятора
├── dist/            # Скомпилированный JavaScript
└── README.md        # Документация
```

## License

MIT License

## Author

**Alexander Kazantsev with z.ai**  
Email: akazant@gmail.com
