import { getL10n } from "./GetL10n";
import { replaceMetadata } from "./MetadataReplacer";

/**
 * Fetches the phrase from the internal list of phrases, and evaluates potential count-rules
 */
export function getPhrase(
	phrase: string,
	args?: {
		__count?: number;
	}
) {
	if ("undefined" === typeof args) args = {};
	var l10n = getL10n();
	if (l10n == null) {
		return phrase;
	}

	var _p = l10n.Phrases[phrase];
	var _ri: string | undefined;
	if ("undefined" !== typeof args.__count) {
		_ri = l10n.ruleEvaluator(args.__count).toString();
	}
	if ("undefined" !== typeof _p) {
		if ("undefined" !== typeof _ri && "undefined" !== typeof _p.r[_ri]) {
			phrase = _p.r[_ri]!;
		} else {
			phrase = _p.r["0"];
		}
	}

	return replaceMetadata(phrase);
}
