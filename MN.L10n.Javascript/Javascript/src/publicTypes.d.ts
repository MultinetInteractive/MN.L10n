export type ExtractL10nParameter<T> =
  T extends `${infer prefix}$${infer U}$${infer suffix}`
    ? ExtractL10nParameter<prefix> | U | ExtractL10nParameter<suffix>
    : never;

type StringWithoutFormatArgs<T> = [ExtractL10nParameter<T>] extends [never]
  ? T
  : never;

export type TranslatedString = string & {
  /**
   * This is a fake property to enforce nominal type matching.
   * A translated string is in fact just a normal string during runtime
   */
  readonly __l10nuid__: never;
};

declare global {
  function _s<T extends string>(
    l10nString: string extends T
      ? never
      : "" extends T
      ? never
      : TranslatedString extends T
      ? never
      : StringWithoutFormatArgs<T>
  ): TranslatedString;
  function _s<T extends string>(
    l10nString: string extends T
      ? never
      : T extends StringWithoutFormatArgs<T>
      ? never
      : T,
    formatParameters: {
      [key in ExtractL10nParameter<T>]: string | number;
    }
  ): TranslatedString;
}

interface Window {
  _s: typeof _s;
}
