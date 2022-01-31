import { getPhrase } from "./internal/GetPhrase";
import { replaceKeywords } from "./internal/ReplaceKeywords";
import { ExtractL10nParameter, TranslatedString } from "./publicTypes";

export function _s<T extends string>(
  l10nString: T,
  formatParameters?: {
    [key in ExtractL10nParameter<T>]: string | number;
  }
): TranslatedString {
  return replaceKeywords(
    getPhrase(l10nString, formatParameters),
    formatParameters
  ) as unknown as TranslatedString;
}
