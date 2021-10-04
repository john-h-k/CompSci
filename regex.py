import re

one = "^[^\W_]+$"
two = "a"
three = "ab+"
four = "ab?([^b]|$)"
five = "ab{3}([^b]|$)"
six = "ab{2,3}([^b]|$)"
seven = "\\b[a-zA-Z]+(\\b|_[a-zA-Z])"
eight = "[A-Z][a-z]+"
nine = "^a.+b$"
ten = "^\w+\\b"
eleven = "\\b\w+\.?$"
twelve = "\\b\w*z\w*\\b"
thirteen = "\\b\w+z\w+\\b"
fourteen = "^\w*$"

class ColorBlock:
    def __init__(self, code=91):
        self.code = code

    def __enter__(self):
        print(f"\033[{self.code}m", end="")

    def __exit__(sel, *args):
        print(f"\033[0m", end="")

def search(pattern, string, expected=True):
    matches = bool(re.search(pattern, string))
    with ColorBlock(0 if matches == expected else 91):
        print(f"'{string}' matches '{pattern}': {matches} (expected: {expected})")

def test(name,pattern, cases):
    print("------------------------------------")
    print(f"TESTING '{name}'")
    print("RUNNING TEST CASES...")
    print("\n\n")
    for string, expected in cases.items():
        search(pattern, string, expected)

    print("------------------------------------\n\n")

test("one", one, {
    "abcdefghijkjlmAZCJEHFFSEF0987654321asdasdjASJFASF": True,
    "abcdefghijkjlmAZCJEHFFSEF0987_654321asdasdjASJFASF": False,
})

test("two", two, {
    # simple expected true
    **dict.fromkeys(["a", "ab", "abb"], True),
    **dict.fromkeys(["pa", "pab", "pabb"], True),

    # expected false
    **dict.fromkeys(["b", "bb", "bbb"], False),
    **dict.fromkeys(["pb", "pbb", "pbbb"], False)
})

test("three", three, {
    # simple expected true
    **dict.fromkeys(["ab", "abb"], True),
    **dict.fromkeys(["pab", "pabb"], True),

    # expected false
    **dict.fromkeys(["a", "ap", "ba"], False),
    **dict.fromkeys(["b", "bb", "bbb"], False),
    **dict.fromkeys(["pb", "pbb", "pbbb"], False)
})

test("four", four, {
    # simple expected true
    **dict.fromkeys(["a", "ab"], True),
    **dict.fromkeys(["pa", "pab", "pab"], True),

    # expected false
    **dict.fromkeys(["b", "bb", "bbb"], False),
    **dict.fromkeys(["pb", "pbb", "pbbb"], False),
    **dict.fromkeys(["abb", "pabb", "pabbb"], False)
})

test("five", five, {
    # simple expected true
    **dict.fromkeys(["abbb"], True),
    **dict.fromkeys(["pabbb", "asioasdabbb", "_____#abbb"], True),

    # expected false
    **dict.fromkeys(["b", "bb", "bbb"], False),
    **dict.fromkeys(["pb", "pbb", "pbbb"], False),
    **dict.fromkeys(["ab", "abb", "abbbb"], False),
    **dict.fromkeys(["pab", "pabb", "pabbbb"], False)
})

test("six", six, {
    # simple expected true
    **dict.fromkeys(["abb", "abbb"], True),
    **dict.fromkeys(["pabbb", "asioasdabbb", "_____#abbb"], True),
    **dict.fromkeys(["pabb", "asioasdabb", "_____#abb"], True),

    # expected false
    **dict.fromkeys(["b", "bb", "bbb"], False),
    **dict.fromkeys(["pb", "pbb", "pbbb"], False),
    **dict.fromkeys(["ab", "abbbb"], False),
    **dict.fromkeys(["pab", "pabbbb"], False)
})

test("seven", seven, {
    "a": True,
    "_": False,
    "a_": False,
    "_a": False,
    **dict.fromkeys(["abcd", "HELLO", "GOODBYE"], True),
    **dict.fromkeys(["hi_BYE_goodbye", "beePP_bop_BADOP", "a_b_c_D_E_FGHIKLMNOPQRSTUV"], True),
    **dict.fromkeys(["a_b", "ab_ba", "ab_bc_cd_de"], True),

    **dict.fromkeys(["a_b_", "ab_ba_", "ab_bc_cd_de_"], False),
    **dict.fromkeys(["_a_b", "_ab_ba", "_ab_bc_cd_de"], False),
    **dict.fromkeys(["_a_b_", "_ab_ba_", "_ab_bc_cd_de_"], False),
})
