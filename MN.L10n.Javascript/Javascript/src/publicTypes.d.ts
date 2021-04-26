export type ExtractL10nParameter<
  T
> = T extends `${infer prefix}$${infer U}$${infer suffix}`
  ? ExtractL10nParameter<prefix> | U | ExtractL10nParameter<suffix>
  : never;

type StringWithoutFormatArgs<T> = [ExtractL10nParameter<T>] extends [never]
  ? T
  : never;

declare global {
  function _s<T extends string>(l10nString: StringWithoutFormatArgs<T>): string;

  function _s<T extends string>(
    l10nString: T extends StringWithoutFormatArgs<T> ? never : T,
    formatParameters: {
      [key in ExtractL10nParameter<T>]: string | number;
    }
  ): string;
}

interface Window {
  _s: typeof _s;
}
