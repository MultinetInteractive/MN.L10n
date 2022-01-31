import { MockedFunction } from "ts-jest/dist/utils/testing";
import * as getGlobal from "./GetGlobal";
import { getL10n } from "./GetL10n";

jest.mock("./GetGlobal");
const getGlobalMock = (getGlobal.getGlobal as any) as MockedFunction<
  typeof getGlobal.getGlobal
>;

beforeEach(() => getGlobalMock.mockReset());

describe("GetL10n", () => {
  it("returns null if global is null", () => {
    getGlobalMock.mockImplementation(() => null);
    const l10n = getL10n();
    expect(l10n).toBeNull();
  });

  it("returns null if global is undefined", () => {
    getGlobalMock.mockImplementation(() => null);
    const l10n = getL10n();
    expect(l10n).toBeNull();
  });
  it("returns null if l10n is null", () => {
    getGlobalMock.mockImplementation(() => ({
      l10n: null,
    }));
    const l10n = getL10n();
    expect(l10n).toBeNull();
  });

  it("returns null if l10n is undefined", () => {
    getGlobalMock.mockImplementation(() => ({
      l10n: undefined,
    }));
    const l10n = getL10n();
    expect(l10n).toBeNull();
  });

  it("returns l10nObject if l10nObject is defined", () => {
    const expectedL10n = Object.freeze({});
    getGlobalMock.mockImplementation(() => ({
      l10n: expectedL10n,
    }));
    const l10n = getL10n();
    expect(l10n).toBe(expectedL10n);
  });
});
