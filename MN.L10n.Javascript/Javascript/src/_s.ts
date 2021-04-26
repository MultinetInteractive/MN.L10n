import { getPhrase } from "./internal/GetPhrase";
import { replaceKeywords } from "./internal/ReplaceKeywords";
import { ExtractL10nParameter } from "./publicTypes";

export function _s<T extends string>(
  l10nString: T,
  formatParameters?: {
    [key in ExtractL10nParameter<T>]: string | number;
  }
): string {
  return replaceKeywords(
    getPhrase(l10nString, formatParameters),
    formatParameters
  );
}
