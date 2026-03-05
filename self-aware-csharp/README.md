# Self-Aware MCP Server (C#)

MCP (Model Context Protocol) сервер на C#, предоставляющий инструменты самосознания для LLM.

**Author:** Alexander Kazantsev with z.ai  
**Email:** akazant@gmail.com

## Требования

- .NET 8.0 SDK или выше

## Установка

```bash
cd self-aware-csharp
dotnet build
```

## Использование с Claude Desktop

Добавьте в конфигурацию Claude Desktop (`claude_desktop_config.json`):

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "dotnet",
      "args": [
        "C:/путь/к/self-aware-csharp/bin/Release/net8.0/self-aware-mcp.dll",
        "Moscow, Russia"
      ]
    }
  }
}
```

Или используйте опубликованную версию:

```bash
dotnet publish -c Release -o ./publish
```

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "dotnet",
      "args": [
        "C:/путь/к/self-aware-csharp/publish/self-aware-mcp.dll",
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

### 2. `get_current_date_time`

Возвращает текущую дату и время в формате: `weekday, year-month-day hours:minutes:seconds`

### 3. `get_current_system`

Возвращает основные параметры операционной системы в формате JSON:

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

### 4. `calculate`

Безопасный калькулятор математических выражений.

**Supported Mathematical Functions:**

| Категория | Функции |
|-----------|---------|
| Trigonometric (radians) | `sin`, `cos`, `tan`, `asin`, `acos`, `atan` |
| Hyperbolic | `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh` |
| Logarithmic | `log`, `log10`, `log2`, `exp`, `sqrt` |
| Other | `ceil`, `floor`, `abs`, `fabs`, `factorial`, `gamma`, `lgamma` |
| Constants | `pi`, `e`, `tau`, `inf`, `nan` |

**Supported Statistical Functions:**

| Функция | Описание |
|---------|----------|
| `mean(data)` | среднее арифметическое |
| `median(data)` | медиана |
| `mode(data)` | мода |
| `stdev(data)` | стандартное отклонение |
| `variance(data)` | дисперсия |
| `harmonic_mean(data)` | гармоническое среднее |
| `geometric_mean(data)` | геометрическое среднее |
| `sum(data)` | сумма элементов |
| `len(data)` | длина последовательности |

**Примеры:**
```
calculate("2 + 2 * 3")                      → 8
calculate("sin(pi/2)")                      → 1
calculate("sqrt(16)")                       → 4
calculate("factorial(5)")                   → 120
calculate("mean([1, 2, 3, 4, 5])")          → 3
calculate("stdev([1, 2, 3, 4, 5])")         → 1.58...
calculate("harmonic_mean([1, 2, 3, 4, 5])") → 2.19...
calculate("2**10")                          → 1024
calculate("sum([1, 2, 3, 4, 5])")           → 15
```

## Разработка

```bash
# Сборка
dotnet build

# Сборка для релиза
dotnet build -c Release

# Публикация как standalone приложение
dotnet publish -c Release -r win-x64 --self-contained

# Тестовый запуск
dotnet run -- "Test Location"
```

## Тестирование

Для тестирования сервера можно использовать MCP Inspector:

```bash
npx @modelcontextprotocol/inspector dotnet bin/Release/net8.0/self-aware-mcp.dll "Test Location"
```

## Структура проекта

```
self-aware-csharp/
├── self-aware-mcp.csproj
├── Program.cs
└── README.md
```

## License

MIT License

## Author

**Alexander Kazantsev with z.ai**  
Email: akazant@gmail.com
