import { getGlobal } from "./GetGlobal";

type GlobalL10n = {
  Phrases: {
    [phrase: string]: {
      r: {
        "0": string;
        [ruleIndex: string]: string | undefined;
      };
    };
  };
  ruleEvaluator: (count: number) => number;
};

export function getL10n(): GlobalL10n | null {
  var global = getGlobal();
  if (global == null) {
    return null;
  }

  var l10n = global.l10n;
  if ("undefined" === typeof l10n) {
    return null;
  }

  return l10n as GlobalL10n;
}
