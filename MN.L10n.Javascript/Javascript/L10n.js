(function() {
  // src/internal/GetGlobal.ts
  function getGlobal() {
    var target = null;
    if (typeof window !== "undefined") {
      target = window;
    } else if (typeof global !== "undefined") {
      target = global;
    }
    return target;
  }

  // src/internal/GetL10n.ts
  function getL10n() {
    var global2 = getGlobal();
    if (global2 == null) {
      return null;
    }
    var l10n = global2.l10n;
    if (typeof l10n === "undefined") {
      return null;
    }
    return l10n;
  }

  // src/internal/GetPhrase.ts
  function getPhrase(phrase, args) {
    if (typeof args === "undefined")
      args = {};
    var l10n = getL10n();
    if (l10n == null) {
      return phrase;
    }
    var _p = l10n.Phrases[phrase];
    var _ri;
    if (typeof args.__count !== "undefined") {
      _ri = l10n.ruleEvaluator(args.__count).toString();
    }
    if (typeof _p !== "undefined") {
      if (typeof _ri !== "undefined" && typeof _p.r[_ri] !== "undefined") {
        phrase = _p.r[_ri];
      } else {
        phrase = _p.r["0"];
      }
    }
    return phrase;
  }

  // src/internal/ReplaceKeywords.ts
  function replaceKeywords(phrase, args) {
    var _a, _b;
    if (typeof args === "undefined") {
      return phrase;
    }
    for (var p in args) {
      if (args.hasOwnProperty(p)) {
        phrase = phrase.split("$" + p + "$").join((_b = (_a = args[p]) == null ? void 0 : _a.toString()) != null ? _b : "");
      }
    }
    return phrase;
  }

  // src/_s.ts
  function _s(l10nString, formatParameters) {
    return replaceKeywords(getPhrase(l10nString, formatParameters), formatParameters);
  }

  // src/L10n.ts
  if (typeof window !== "undefined") {
    window._s = _s;
  }
  if (typeof global !== "undefined") {
    global._s = _s;
  }
})();
//# sourceMappingURL=L10n.js.map
