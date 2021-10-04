from sys import stdout, stderr
from io import StringIO
from time import sleep
from dataclasses import dataclass
import inspect

class ColorCode:
    def __init__(self, code0, code1):
        self.code0 = code0
        self.code1 = code1

class ColorCode:
    Black = ColorCode(0, 30)
    DarkGray = ColorCode(1, 30)

    Red = ColorCode(0, 31)
    LightRed = ColorCode(1, 31)

    Green = ColorCode(0, 32)
    LightGreen = ColorCode(1, 32)

    BrownOrange = ColorCode(0, 33)
    Yellow = ColorCode(1, 33)

    Blue = ColorCode(0, 34)
    LightBlue = ColorCode(1, 34)

    Purple = ColorCode(0, 35)
    LightPurple = ColorCode(1, 35)

    Cyan = ColorCode(0, 36)
    LightCyan = ColorCode(1, 36)

    LightGray = ColorCode(0, 37)
    White = ColorCode(1, 37)


class ColorBlock:
    def __init__(self, color, stream=stdout):
        self.color = color
        self.stream = stream

    def __enter__(self):
        print(f"\033[{self.color.code0};{self.color.code1}m", end="", file=self.stream)

    def __exit__(self, *args):
        print(f"\033[0m", end="", file=self.stream)

def color(color, text):
    s = StringIO()
    with ColorBlock(color, s):
        print(text, file=s, end="")
    return s.getvalue()

def unittest(func, data_or_data_source):
    return func

@dataclass
class _TestCase:
    args: list
    name: str
    expected: object
    throws: Exception

def _testcase(func, args, *, name=None, expected=None, throws=None):
    module = inspect.getmodule(func)
    if not hasattr(module, "testables"):
        module.testables = []

    if func not in module.testables:
        module.testables.append(func)

    if not hasattr(func, "test_cases"):
        func.test_cases = []

    func.test_cases.append(_TestCase(args, name, expected, throws))

def testcase(args, *, name=None, expected=None, throws=None):
    def _wrapper(func):
        _testcase(func, args, name=name, expected=expected, throws=throws)
        return func
    
    return _wrapper

def testcases(cases):
    def _wrapper(func):
        for case in cases:
            args, expected = case if len(case) == 2 else [case, None]
            _testcase(func, args, expected=expected)

        return func
    
    return _wrapper

def _barrier():
    print("--------------------")

def _test(func):
    name = func.__name__
    if not hasattr(func, "test_cases"):
        raise ValueError(f"Function '{name}' has no attached test cases!")
    cases = func.test_cases

    _barrier()
    print(f"Running '{name}'...")
    
    passes = []
    failures = []

    count = len(cases)
    print(f"0/{count} cases completed...", end="")
    for i, case in enumerate(cases):
        print("\r", end="")

        try:
            result = func(*case.args)
        except Exception as e:
            if not case.throws or type(e) != case.throws:
                raise

        (passes if result == case.expected else failures).append([i, result])
        sleep(1/count)
        print(f"{i + 1}/{count} cases completed", end="")
    print("\n")
    
    if len(failures) == 0:
        print(f"{color(ColorCode.Green, 'PASSED:')} {name} ({count} test cases ran)")
    else:
        print(f"{color(ColorCode.Red, 'FAILURES:')} {name} - {len(failures)} test cases failed ({count} test cases ran)")

        with ColorBlock(ColorCode.Red):
            _barrier()
            print("FAILURES")
            print("\n")
            for i, result in failures:
                failure = cases[i]
                name = f"'{failure.name}'" if failure.name else f"{i}"
                print(f"Case {name} (args: {[str(a) for a in failure.args]}) failed!")
                print(f"    Expected '{failure.expected}', got '{result}'")
                print("\n")
        
    _barrier()
    print("\n")

def test(*funcs):
    if len(funcs) == 0:
        module = inspect.getmodule(inspect.stack()[1][0])
        funcs = module.testables

    for func in funcs:
        _test(func)
