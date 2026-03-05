"""
Self-Aware MCP Server
MCP (Model Context Protocol) сервер на Python, предоставляющий инструменты самосознания для LLM.

@author Alexander Kazantsev with z.ai
@email akazant@gmail.com
@license MIT
"""

__version__ = "1.0.0"
__author__ = "Alexander Kazantsev with z.ai"

from .server import (
    main,
    get_current_location,
    get_current_date_time,
    get_current_system,
    calculate,
    LOCATION_ARG
)

__all__ = [
    "main",
    "get_current_location",
    "get_current_date_time",
    "get_current_system",
    "calculate",
    "LOCATION_ARG"
]
