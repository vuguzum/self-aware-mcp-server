# Self-Aware MCP Server

MCP (Model Context Protocol) сервер, предоставляющий инструменты получения базовой информации (самосознания) для LLM - **текущее время, место, ОС, математические вычисления**.
Версия для Node.js - рабочая: установлена и протестирована. Версии на Python и C# сделаны для сравнения - проверены на ошибки компиляции и протестированы агентом GLM-5.


## Доступные версии

| Язык | Папка | Статус |
|------|-------|--------|
| **TypeScript/Node.js** | `self-aware/` | ✅ проверено установкой |
| **Python** | `self-aware-python/` | ✅ протестировано агентом |
| **C#** | `self-aware-csharp/` | ✅ скомпилировано |

---

## Доступные инструменты

### 1. `get_current_location`
Возвращает местоположение, переданное как аргумент командной строки.

### 2. `get_current_date_time`
Возвращает текущую дату и время: `weekday, year-month-day hours:minutes:seconds`

### 3. `get_current_system`
Возвращает информацию об ОС:
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

### 4. `calculate`
Безопасный калькулятор математических выражений с поддержкой:
- Тригонометрических функций (радианы)
- Гиперболических функций
- Логарифмических функций
- Статистических функций (mean, median, stdev, variance, etc.)
- Математических констант (pi, e, tau, inf, nan)

---

## Использование с Claude Desktop

Добавьте в `claude_desktop_config.json`:

### TypeScript/Node.js версия
```json
{
  "mcpServers": {
    "self-aware": {
      "command": "node",
      "args": [
        "/path/to/self-aware/dist/index.js",
        "Moscow, Russia"
      ]
    }
  }
}
```

### Python версия
```json
{
  "mcpServers": {
    "self-aware": {
      "command": "python",
      "args": [
        "/path/to/self-aware-python/src/self_aware/server.py",
        "Moscow, Russia"
      ]
    }
  }
}
```

### C# версия
```json
{
  "mcpServers": {
    "self-aware": {
      "command": "dotnet",
      "args": [
        "/path/to/self-aware-csharp/bin/Release/net8.0/self-aware-mcp.dll",
        "Moscow, Russia"
      ]
    }
  }
}
```

---

## Быстрый старт

### TypeScript
```bash
cd self-aware
bun install
bun run build
node dist/index.js "Moscow, Russia"
```

### Python
```bash
cd self-aware-python
pip install mcp
python src/self_aware/server.py "Moscow, Russia"
```

### C#
```bash
cd self-aware-csharp
dotnet build -c Release
dotnet bin/Release/net8.0/self-aware-mcp.dll "Moscow, Russia"
```

### Структура проекта
```
mcp-servers/
├── README.md                    # Общий README
│
├── self-aware/                  # TypeScript/Node.js версия
│   ├── package.json
│   ├── tsconfig.json
│   ├── src/
│   │   └── index.ts
│   ├── dist/
│   │   └── index.js
│   └── README.md
│
├── self-aware-python/           # Python версия
│   ├── pyproject.toml
│   ├── requirements.txt
│   ├── src/
│   │   └── self_aware/
│   │       ├── __init__.py
│   │       └── server.py
│   ├── test_server.py
│   └── README.md
│
└── self-aware-csharp/           # C# версия
    ├── self-aware-mcp.csproj
    ├── Program.cs
    └── README.md
```

## License

MIT License

---

## Author

**Alexander Kazantsev** with z.ai  

