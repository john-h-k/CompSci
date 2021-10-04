from test import *
from sys import argv
import re

@testcases([
    [[""], ""],
    [[" "], "_"],
    [["_"], " "],
    [[" _"], "_ "],
    [["_ "], " _"],

    [["hello_bye"], "hello bye"],
    [["hello bye"], "hello_bye"],
    [["__hello_ _goodbye _ yes  "], "  hello _ goodbye_ _yes__"]
])
def _23(string):
    # or lambda x: dict(zip('_ ',' _'))[x.group(0)]
    return re.sub(r"[_ ]", lambda c: "_" if c.group(0) == " " else " ", string)

@testcases([
    [[""], []],
    [["hello"], []],
    [["hello allo ello"], ["allo", "ello"]],
    [["ello hello ello"], ["ello", "ello"]],
    [["a e"], ["a", "e"]]
])
def _28(string):
    return re.findall(r"\b[ae].*?\b", string)

@testcases([
    [[""], ""],
    [[" "], ":"],
    [[","], ":"],
    [["."], ":"],
    [[" .,"], ":::"],
    [["hello goodbye.yes,no"], "hello:goodbye:yes:no"],
    [["  hello:goodbye..yes:no  "], "::hello:goodbye::yes:no::"],
])
def _31(string):
    return re.sub(r"[ ,.]", ":", string)

@testcases([
    [[""], []],
    [["a"], []],
    [["aa"], []],
    [["aaa"], ["aaa"]],
    [["aaaa"], ["aaaa"]],
    [["aaaaa"], ["aaaaa"]],
    [["aaaaaa"], []],

    [["a bb ccc dddd eeeee fffff gggg hhh ii j"], ["ccc", "dddd", "eeeee", "fffff", "gggg", "hhh"]]
])
def _34(string):
    return re.findall(r"\b[a-zA-Z]{3,5}\b", string)
 
@testcases([
    [[""], []],
    [["hello"], []],
    [["'mismatched quotes\""], []],
    [["\"mismatched quotes'"], []],
    [["'hello'"], ["hello"]],
    [["'hello'"], ["hello"]],
    [["'he\"llo' oh\""], ["he\"llo"]],
    [["hello my name is 'tom' and I am sitting down. It 'is' very cold and am feeling quite 'stupid'"], ["tom", "is", "stupid"]]
])
def _38(string):
    return [g[1] for g in re.findall(r"([\"'])(.+?)\1", string)]

@testcases([
    [[""], ""],
    [[" "], " "],
    [["  "], " "],
    [["             "], " "],
    [["hello goodbye  hello   goodbye    hello\tgoodbye"], "hello goodbye hello goodbye hello\tgoodbye"],
    [[" hello "], " hello "],
    [[" hel  lo "], " hel lo "],
    [["  hel lo  "], " hel lo "],
    [["  hel  lo  "], " hel lo "],
])
def _39(string):
    return re.sub(r" +", " ", string)

@testcases([
    [[""], ""],
    [["abcABC012"], "abcABC012"],
    [[".abcABC012."], "abcABC012"],
    [[".a#b-c^A%B@C*0#1~2."], "abcABC012"],
    [["hello goodbye\tbye"], "hellogoodbyebye"],
    [[" "], ""],
    [["  "], ""],
    [[",./;'#[]!\"£$%^&*(){}'\\|¬<>~@:-=+"], ""],
])
def _41(string):
    return re.sub(r"\W", "", string)

def url(s):
    # defined in RFC
    #   - delimiters not used by the actual URL format
    sub_delims = r"!$&'()*+,;="
    #   - unreserved letters
    unreserved = r"A-Za-z0-9\@[_`{|}~\-\^\]"
    #   - perecentage encoded token (%HEXHEX e.g %2E)
    pct = r"(?:%[a-fA-F0-9]{2})"

    # all valid characters in 'userinfo'
    uchar = f"(?:[{unreserved}{sub_delims}:]|(?:{pct}))"

    # all valid characters in 'path' (same as uchar but with '@')
    pchar = f"{unreserved}{sub_delims}:@"

    # all valid characters in 'query' (same as uchar but with '/')
    qchar = f"(?:[{unreserved}{sub_delims}:@/]|(?:{pct}))"

    # number from 0->255
    ipv4 = f"(?:(?:25[0-5])|(?:2[0-4]\d)|(?:[0-1]\d\d)|(?:\d\d)|(?:\d))"
    # domain label, alphanumeric and hyphens but not starting or ending in hyphen
    dlabel = f"(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]*[a-zA-Z0-9])?)"

    return re.finditer(fr"""
        # check URL is either preceded by a character that can't start a URL
        (?<![{unreserved}{sub_delims}:@/])
        # check it is not empty
        (?!\s|\Z)
        (?P<url>
            (?:
                # parse the schema according to RFC (starts with letter, then alphanumeric + '-')
                # and lookahead for the suffix of ://
                (?P<scheme>[a-zA-Z][a-zA-Z0-9+.\-]*(?=://))?
                :?
                (?P<authority>
                    # either allow none or 2 slashes for prefix. One would cause ambiguity with path
                    (?P<prefix>//)?
                    # accepts all not special URL chars, normally seperated with colon, followed by @
                    #  - e.g john:password@foobar.com
                    #  - but also :@foo.com, john@foo.com, john:@foo.com, john:@foo.com, :::@foo.com, a:b:c:d@foo.com
                    (?P<userinfo>{uchar}*@)?
                    # allow but don't mandate @ even if no userinfo provided e.g http://@google.com
                    (?:(?<!@)@?)
                    (?P<host>
                        # any ipv4 address
                        (?P<ipv4>{ipv4}\.{ipv4}\.{ipv4}\.{ipv4})
                        |
                        # any valid reg name
                        # a set of 1 or more domain labels seperated by '.', optionally ending in '.'
                        # the final label must not be all digits
                        (?P<domain>(?:{dlabel}\.)+((?=[a-zA-Z0-9]*[^\d.][a-zA-Z0-9]*(?:[:\/?#\s]|\Z)){dlabel}\.?))
                        |
                        # always accept 'localhost', else require schema
                        #   - e.g
                        #   - 'localhost:4444?schema=public' and 'postgresql://localhost:4444?schema=public' both succeed
                        #   - 'db:4444?schema=public' fails, 'postgresql://db:4444?schema=public' succeeds
                        (?P<localdomain>(?(scheme)(?:[a-zA-Z0-9]+)|localhost))
                    )
                    # set of sequential digits
                    (?P<port>:\d*)?
                    # ensure authority ends with either / (path), ? (query), # (fragment), or end of URL (whitespace/end of string)
                    (?=[/?#\s]|\Z)
                )?
            )
            (?:
                # if no authority provided, must start with a '/', e.g /path/to/file
                (?(authority)|(?=/))
                # if schema is provided, enforce that authority is also provided
                # to prevent ambiguities between 'http://myfile.0' (is it a schema + path of '//myfile.0' or a domain of 'myfile.0'?)
                (?P<path>(?(scheme)(?(authority)(?=.?)|($.^))|(?=.?))(?:/(?:[{pchar}]|{pct})*)*)
                # match for any valid query string (empty allowed)
                \??
                (?P<query>(?<=\?){qchar}*)?
                # match for any valid fragment string (empty allowed)
                \#?
                (?P<fragment>(?<=\#){qchar}*)?
            )?
        )
        # ensure either path or authority was found
        (?:(?(path)|(?!.?))|(?(authority)|(?!.?)))
        # check URL ends in either end of file or a non-URL valid character
        (?=(?:[^{unreserved}{sub_delims}:@/])|(?<={pct})...|\Z)
    """, s, re.VERBOSE)

@testcases([
    [[""], []],
    [[" foo "], []],
    [["localhost"], ["localhost"]],
    [["localhost:1234"], ["localhost:1234"]],
    [["db"], []],
    [["db:1234"], []],
    [["/path/to/file"], ["/path/to/file"]],
    [["path/to/file"], []],
    [["http://localhost"], ["http://localhost"]],
    [["http://localhost:1234"], ["http://localhost:1234"]],
    [["http://db"], ["http://db"]],
    [["http://db:1234"], ["http://db:1234"]],
    [["https://google.com"], ["https://google.com"]],
    [["https://go1ogle.com"], ["https://go1ogle.com"]],
    [["%3https://google.com"], []],
    [["%3Fhttps://google.com"], []],
    [["£https://google.com"], ["https://google.com"]],
    [[" https://google.com"], ["https://google.com"]],
    [["#https://google.com"], ["https://google.com"]],
    [["?https://google.com"], ["https://google.com"]],
    [["https://111.go1ogle.com"], ["https://111.go1ogle.com"]],
    [["https://111.go1ogle.c0m"], ["https://111.go1ogle.c0m"]],
    [["https://111.go1ogle.000"], []],
    [["db:4444?schema=public"], []],
    [["postgresql://db:4444?schema=public"], ["postgresql://db:4444?schema=public"]],
    [["https://google.com https://google.com"], ["https://google.com", "https://google.com"]],
    [["https://google.com/"], ["https://google.com/"]],
    [["https://google.com?"], ["https://google.com?"]],
    [["https://google.com#"], ["https://google.com#"]],
    [["https://google.com/?"], ["https://google.com/?"]],
    [["https://google.com?#"], ["https://google.com?#"]],
    [["https://google.com/?#"], ["https://google.com/?#"]],
    [["https://google.com/foo?foo=1&bar=2#banter"], ["https://google.com/foo?foo=1&bar=2#banter"]],
    [["https://google.com/?foo=1&bar=2#banter"], ["https://google.com/?foo=1&bar=2#banter"]],
    [["https://google.com?foo=1&bar=2#banter"], ["https://google.com?foo=1&bar=2#banter"]],
    [["https://google.com#banter"], ["https://google.com#banter"]],
    [["https://google.com/#banter"], ["https://google.com/#banter"]],
    [["https://google.com?#banter"], ["https://google.com?#banter"]],
    [["https://google.com/?#banter"], ["https://google.com/?#banter"]],
    [["ftp://bar.google.com/quiz tcp://localhost:4444/quiz"], ["ftp://bar.google.com/quiz", "tcp://localhost:4444/quiz"]],
    [["ftp://bar.google.com/quiz?foo=1&bar=2 tcp://localhost:4444/quiz?foo=1&bar=2#cat"], ["ftp://bar.google.com/quiz?foo=1&bar=2", "tcp://localhost:4444/quiz?foo=1&bar=2#cat"]],
])
def _42(string):
    return [m.group(0) for m in url(string)]

def _43(string):
    return re.findall(r"(?:^[^A-Z]*)|[A-Z][^A-Z]*", string)


args = [globals().get(v, globals().get(f"_{v}", None)) for v in argv[1:]]
test(*args)