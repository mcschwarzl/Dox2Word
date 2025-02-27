﻿using System.Collections.Generic;

namespace Dox2Word.Parser
{
    public static class DocEmptyParser
    {
        // Taken from https://github.com/doxygen/doxygen/blob/98c67549bc3cd855873e0ef5eeab7c6410699d78/src/htmlentity.cpp#L44
        private static readonly Dictionary<string, string> lookup = new()
        {
            { "nonbreakablespace", "\xA0" },
            { "iexcl", "\xA1" },
            { "cent", "\xA2" },
            { "pound", "\xA3" },
            { "curren", "\xA4" },
            { "yen", "\xA5" },
            { "brvbar", "\xA6" },
            { "umlaut", "\xA8" },
            { "copy", "\xA9" },
            { "ordf", "\xAA" },
            { "laquo", "\xAB" },
            { "not", "\xAC" },
            { "shy", "\xAD" },
            { "registered", "\xAE" },
            { "macr", "\xAF" },
            { "deg", "\xB0" },
            { "plusmn", "\xB1" },
            { "sup2", "\xB2" },
            { "sup3", "\xB3" },
            { "acute", "\xB4" },
            { "micro", "\xB5" },
            { "para", "\xB6" },
            { "middot", "\xB7" },
            { "cedil", "\xB8" },
            { "sup1", "\xB9" },
            { "ordm", "\xBA" },
            { "raquo", "\xBB" },
            { "frac14", "\xBC" },
            { "frac12", "\xBD" },
            { "frac34", "\xBE" },
            { "iquest", "\xBF" },
            { "Agrave", "\xC0" },
            { "Aacute", "\xC1" },
            { "Acirc", "\xC2" },
            { "Atilde", "\xC3" },
            { "Aumlaut", "\xC4" },
            { "Aring", "\xC5" },
            { "AElig", "\xC6" },
            { "Ccedil", "\xC7" },
            { "Egrave", "\xC8" },
            { "Eacute", "\xC9" },
            { "Ecirc", "\xCA" },
            { "Eumlaut", "\xCB" },
            { "Igrave", "\xCC" },
            { "Iacute", "\xCD" },
            { "Icirc", "\xCE" },
            { "Iumlaut", "\xCF" },
            { "ETH", "\xD0" },
            { "Ntilde", "\xD1" },
            { "Ograve", "\xD2" },
            { "Oacute", "\xD3" },
            { "Ocirc", "\xD4" },
            { "Otilde", "\xD5" },
            { "Oumlaut", "\xD6" },
            { "times", "\xD7" },
            { "Oslash", "\xD8" },
            { "Ugrave", "\xD9" },
            { "Uacute", "\xDA" },
            { "Ucirc", "\xDB" },
            { "Uumlaut", "\xDC" },
            { "Yacute", "\xDD" },
            { "THORN", "\xDE" },
            { "szlig", "\xDF" },
            { "agrave", "\xE0" },
            { "aacute", "\xE1" },
            { "acirc", "\xE2" },
            { "atilde", "\xE3" },
            { "aumlaut", "\xE4" },
            { "aring", "\xE5" },
            { "aelig", "\xE6" },
            { "ccedil", "\xE7" },
            { "egrave", "\xE8" },
            { "eacute", "\xE9" },
            { "ecirc", "\xEA" },
            { "eumlaut", "\xEB" },
            { "igrave", "\xEC" },
            { "iacute", "\xED" },
            { "icirc", "\xEE" },
            { "iumlaut", "\xEF" },
            { "eth", "\xF0" },
            { "ntilde", "\xF1" },
            { "ograve", "\xF2" },
            { "oacute", "\xF3" },
            { "ocirc", "\xF4" },
            { "otilde", "\xF5" },
            { "oumlaut", "\xF6" },
            { "divide", "\xF7" },
            { "oslash", "\xF8" },
            { "ugrave", "\xF9" },
            { "uacute", "\xFA" },
            { "ucirc", "\xFB" },
            { "uumlaut", "\xFC" },
            { "yacute", "\xFD" },
            { "thorn", "\xFE" },
            { "yumlaut", "\xFF" },
            { "fnof", "\x192" },
            { "Alpha", "\x391" },
            { "Beta", "\x392" },
            { "Gamma", "\x393" },
            { "Delta", "\x394" },
            { "Epsilon", "\x395" },
            { "Zeta", "\x396" },
            { "Eta", "\x397" },
            { "Theta", "\x398" },
            { "Iota", "\x399" },
            { "Kappa", "\x39A" },
            { "Lambda", "\x39B" },
            { "Mu", "\x39C" },
            { "Nu", "\x39D" },
            { "Xi", "\x39E" },
            { "Omicron", "\x39F" },
            { "Pi", "\x3A0" },
            { "Rho", "\x3A1" },
            { "Sigma", "\x3A3" },
            { "Tau", "\x3A4" },
            { "Upsilon", "\x3A5" },
            { "Phi", "\x3A6" },
            { "Chi", "\x3A7" },
            { "Psi", "\x3A8" },
            { "Omega", "\x3A9" },
            { "alpha", "\x3B1" },
            { "beta", "\x3B2" },
            { "gamma", "\x3B3" },
            { "delta", "\x3B4" },
            { "epsilon", "\x3B5" },
            { "zeta", "\x3B6" },
            { "eta", "\x3B7" },
            { "theta", "\x3B8" },
            { "iota", "\x3B9" },
            { "kappa", "\x3BA" },
            { "lambda", "\x3BB" },
            { "mu", "\x3BC" },
            { "nu", "\x3BD" },
            { "xi", "\x3BE" },
            { "omicron", "\x3BF" },
            { "pi", "\x3C0" },
            { "rho", "\x3C1" },
            { "sigmaf", "\x3C2" },
            { "sigma", "\x3C3" },
            { "tau", "\x3C4" },
            { "upsilon", "\x3C5" },
            { "phi", "\x3C6" },
            { "chi", "\x3C7" },
            { "psi", "\x3C8" },
            { "omega", "\x3C9" },
            { "thetasym", "\x3D1" },
            { "upsih", "\x3D2" },
            { "piv", "\x3D6" },
            { "bull", "\x2022" },
            { "hellip", "\x2026" },
            { "prime", "\x2032" },
            { "Prime", "\x2033" },
            { "oline", "\x203E" },
            { "frasl", "\x2044" },
            { "weierp", "\x2118" },
            { "imaginary", "\x2111" },
            { "real", "\x211C" },
            { "trademark", "\x2122" },
            { "alefsym", "\x2135" },
            { "larr", "\x2190" },
            { "uarr", "\x2191" },
            { "rarr", "\x2192" },
            { "darr", "\x2193" },
            { "harr", "\x2194" },
            { "crarr", "\x21B5" },
            { "lArr", "\x21D0" },
            { "uArr", "\x21D1" },
            { "rArr", "\x21D2" },
            { "dArr", "\x21D3" },
            { "hArr", "\x21D4" },
            { "forall", "\x2200" },
            { "part", "\x2202" },
            { "exist", "\x2203" },
            { "empty", "\x2205" },
            { "nabla", "\x2207" },
            { "isin", "\x2208" },
            { "notin", "\x2209" },
            { "ni", "\x220B" },
            { "prod", "\x220F" },
            { "sum", "\x2211" },
            { "minus", "\x2212" },
            { "lowast", "\x2217" },
            { "radic", "\x221A" },
            { "prop", "\x221D" },
            { "infin", "\x221E" },
            { "ang", "\x2220" },
            { "and", "\x2227" },
            { "or", "\x2228" },
            { "cap", "\x2229" },
            { "cup", "\x222A" },
            { "int", "\x222B" },
            { "there4", "\x2234" },
            { "sim", "\x223C" },
            { "cong", "\x2245" },
            { "asymp", "\x2248" },
            { "ne", "\x2260" },
            { "equiv", "\x2261" },
            { "le", "\x2264" },
            { "ge", "\x2265" },
            { "sub", "\x2282" },
            { "sup", "\x2283" },
            { "nsub", "\x2284" },
            { "sube", "\x2286" },
            { "supe", "\x2287" },
            { "oplus", "\x2295" },
            { "otimes", "\x2297" },
            { "perp", "\x22A5" },
            { "sdot", "\x22C5" },
            { "lceil", "\x2308" },
            { "rceil", "\x2309" },
            { "lfloor", "\x230A" },
            { "rfloor", "\x230B" },
            { "lang", "\x2329" },
            { "rang", "\x232A" },
            { "loz", "\x25CA" },
            { "spades", "\x2660" },
            { "clubs", "\x2663" },
            { "hearts", "\x2665" },
            { "diams", "\x2666" },
            { "OElig", "\x152" },
            { "oelig", "\x153" },
            { "Scaron", "\x160" },
            { "scaron", "\x161" },
            { "Yumlaut", "\x178" },
            { "circ", "\x2C6" },
            { "tilde", "\x2DC" },
            { "ensp", "\x2002" },
            { "emsp", "\x2003" },
            { "thinsp", "\x2009" },
            { "zwnj", "\x200C" },
            { "zwj", "\x200D" },
            { "lrm", "\x200E" },
            { "rlm", "\x200F" },
            { "ndash", "\x2013" },
            { "mdash", "\x2014" },
            { "lsquo", "\x2018" },
            { "rsquo", "\x2019" },
            { "sbquo", "\x201A" },
            { "ldquo", "\x201C" },
            { "rdquo", "\x201D" },
            { "bdquo", "\x201E" },
            { "dagger", "\x2020" },
            { "Dagger", "\x2021" },
            { "permil", "\x2030" },
            { "lsaquo", "\x2039" },
            { "rsaquo", "\x203A" },
            { "euro", "\x20AC" },
            { "tm", "\x2122" },
        };

        public static bool TryLookup(string name, out string? result)
        {
            return lookup.TryGetValue(name, out result);
        }
    }
}
