# Self-Aware MCP Server (Python)

MCP сервер, предоставляющий инструменты получения базовой информации (самосознания) для LLM - текущее время, место, ОС, математические вычисления.

## Требования

- Python 3.10 или выше
- pip или uv

## Установка

### Вариант 1: Из исходников

```bash
cd self-aware-python
pip install -e .
```

### Вариант 2: Через uv (рекомендуется)

```bash
cd self-aware-python
uv pip install -e .
```

### Вариант 3: Без установки (прямой запуск)

```bash
cd self-aware-python
pip install mcp
python src/self_aware/server.py "Moscow, Russia"
```

## Использование с Claude Desktop

Добавьте в конфигурацию Claude Desktop (`claude_desktop_config.json`):

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

### Вариант 1: Через установленную команду

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "self-aware-mcp",
      "args": ["Moscow, Russia"]
    }
  }
}
```

### Вариант 2: Прямой запуск через python

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "python",
      "args": [
        "C:/путь/к/self-aware-python/src/self_aware/server.py",
        "Moscow, Russia"
      ]
    }
  }
}
```

### Вариант 3: Через uv

```json
{
  "mcpServers": {
    "self-aware": {
      "command": "uv",
      "args": [
        "--directory",
        "C:/путь/к/self-aware-python",
        "run",
        "self-aware-mcp",
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
    "platform": "windows",
    "cpu": "amd64"
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
| Logarithmic | `log`, `log10`, `log2`, `exp`, `expm1`, `log1p` |
| Other | `sqrt`, `ceil`, `floor`, `fabs`, `factorial`, `gamma`, `lgamma`, `modf`, `fsum`, `abs` |
| Constants | `pi`, `e`, `tau`, `inf`, `nan` |

**Supported Statistical Functions:**

| Функция | Описание |
|---------|----------|
| `mean([data])` | среднее арифметическое |
| `median([data])` | медиана |
| `mode([data])` | мода |
| `stdev([data])` | стандартное отклонение |
| `variance([data])` | дисперсия |
| `harmonic_mean([data])` | гармоническое среднее |
| `geometric_mean([data])` | геометрическое среднее |
| `sum([data])` | сумма элементов |
| `len([data])` | длина последовательности |

**Примеры:**
```
calculate("2 + 2 * 3")           → 8
calculate("sin(pi/2)")           → 1
calculate("sqrt(16)")            → 4
calculate("factorial(5)")        → 120
calculate("mean([1, 2, 3, 4, 5])") → 3
calculate("stdev([1, 2, 3, 4, 5])") → 1.58...
calculate("2**10")               → 1024
calculate("sum([1, 2, 3, 4, 5])") → 15
```

## Безопасность

Функция `calculate` использует безопасное вычисление выражений:
- Запрещены операторы `import`, `exec`, `eval`
- Запрещен доступ к файловой системе (`open`, `file`)
- Запрещен доступ к `__dunder__` методам
- Поддерживаются только функции из разрешенного списка
- Результаты автоматически нормализуются для JSON-сериализации

## Разработка

```bash
# Установка зависимостей для разработки
pip install -e ".[dev]"

# Тестовый запуск
python src/self_aware/server.py "Test Location"

# Запуск через установленную команду
self-aware-mcp "Test Location"
```

## Тестирование

Для тестирования сервера можно использовать MCP Inspector:

```bash
npx @modelcontextprotocol/inspector python src/self_aware/server.py "Test Location"
```

Или через uv:

```bash
npx @modelcontextprotocol/inspector uv run self-aware-mcp "Test Location"
```

## Структура проекта

```
self-aware-python/
├── pyproject.toml
├── src/
│   └── self_aware/
│       ├── __init__.py
│       └── server.py
└── README.md
```

## License

MIT License

## Author

**Alexander Kazantsev with z.ai**  
Email: akazant@gmail.com
