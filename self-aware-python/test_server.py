#!/usr/bin/env python3
"""
Test script for Self-Aware MCP Server
"""

import sys
import os
import json

# Add src to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'src'))

# Override location for testing BEFORE importing
import self_aware.server as server_module
server_module.LOCATION_ARG = "Test City, Test Country"

from self_aware.server import (
    get_current_location,
    get_current_date_time,
    get_current_system,
    calculate
)

def test_location():
    print("=" * 60)
    print("Testing get_current_location()")
    print("=" * 60)
    result = get_current_location()
    print(f"Location: {result}")
    assert result == "Test City, Test Country", f"Expected 'Test City, Test Country', got {result}"
    print("✅ PASSED\n")

def test_datetime():
    print("=" * 60)
    print("Testing get_current_date_time()")
    print("=" * 60)
    result = get_current_date_time()
    print(f"DateTime: {result}")
    assert result, "DateTime should not be empty"
    print("✅ PASSED\n")

def test_system():
    print("=" * 60)
    print("Testing get_current_system()")
    print("=" * 60)
    result = get_current_system()
    print(f"System: {result}")
    data = json.loads(result)
    assert "os" in data, "Should have 'os' key"
    assert "name" in data["os"], "Should have 'os.name' key"
    assert "displayName" in data["os"], "Should have 'os.displayName' key"
    print("✅ PASSED\n")

def test_calculator():
    print("=" * 60)
    print("Testing calculate()")
    print("=" * 60)
    
    tests = [
        ("2 + 2 * 3", 8),
        ("sin(pi/2)", 1),
        ("sqrt(16)", 4),
        ("factorial(5)", 120),
        ("mean([1, 2, 3, 4, 5])", 3),
        ("log(e)", 1),
        ("2**10", 1024),
        ("sum([1, 2, 3, 4, 5])", 15),
        ("abs(-5)", 5),
    ]
    
    passed = 0
    failed = 0
    
    for expr, expected in tests:
        result = calculate(expr)
        # Handle both int and float results
        try:
            if isinstance(result, str):
                # Try to parse as number
                if "Error" in result:
                    print(f"❌ {expr} returned error: {result}")
                    failed += 1
                    continue
                result = float(result) if "." in result else int(result)
            
            if isinstance(result, (int, float)):
                if abs(result - expected) < 0.0001:
                    print(f"✅ {expr} = {result}")
                    passed += 1
                else:
                    print(f"❌ {expr} = {result} (expected: {expected})")
                    failed += 1
            else:
                print(f"❌ {expr} returned: {result}")
                failed += 1
        except Exception as e:
            print(f"❌ {expr} error: {e}")
            failed += 1
    
    print(f"\nCalculator: {passed} passed, {failed} failed")

def test_security():
    print("\n" + "=" * 60)
    print("Testing calculate() security")
    print("=" * 60)
    
    dangerous = [
        "import os",
        "exec('print(1)')",
        "open('/etc/passwd')",
        "__import__('os')",
        "globals()",
    ]
    
    for expr in dangerous:
        result = calculate(expr)
        if "Error" in str(result) or "Forbidden" in str(result):
            print(f"🔒 Blocked: {expr[:30]}...")
        else:
            print(f"⚠️  NOT BLOCKED: {expr} -> {result}")

if __name__ == "__main__":
    test_location()
    test_datetime()
    test_system()
    test_calculator()
    test_security()
    
    print("\n" + "=" * 60)
    print("All tests completed!")
    print("=" * 60)
