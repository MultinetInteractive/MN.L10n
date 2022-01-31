import { MockedFunction } from "ts-jest/dist/utils/testing";
import { getPhrase } from "./GetPhrase";
import * as getL10nModule from "./GetL10n";

jest.mock("./GetL10n");
const getL10nMock = (getL10nModule.getL10n as any) as MockedFunction<
  typeof getL10nModule.getL10n
>;

beforeEach(() => getL10nMock.mockReset());

describe("GetPhrase", () => {
  it("returns the phrase if l10n is unavailable", () => {
    getL10nMock.mockImplementation(() => null);
    const phrase = getPhrase("Testar $__count$", {
      __count: 1,
    });
    expect(phrase).toBe("Testar $__count$");
  });
  it("returns the phrase if it is unavailable", () => {
    getL10nMock.mockImplementation(() => ({
      Phrases: {},
      ruleEvaluator: (c) => c,
    }));
    const phrase = getPhrase("Testar $__count$", {
      __count: 1,
    });
    expect(phrase).toBe("Testar $__count$");
  });
  it("uses the correct phrase if available", () => {
    getL10nMock.mockImplementation(() => ({
      ruleEvaluator: (c) => c,
      Phrases: {
        ["test"]: {
          r: {
            "0": "test2",
          },
        },
        ["test2"]: {
          r: {
            "0": "test3",
          },
        },
      },
    }));

    const phrase = getPhrase("test");
    expect(phrase).toBe("test2");
  });
  it("uses the 0 phrase if no other is available", () => {
    getL10nMock.mockImplementation(() => ({
      ruleEvaluator: (c) => c,
      Phrases: {
        ["test"]: {
          r: {
            "0": "test2",
          },
        },
        ["test2"]: {
          r: {
            "0": "test3",
          },
        },
      },
    }));

    const phrase = getPhrase("test", {
      __count: 3,
    });
    expect(phrase).toBe("test2");
  });

  it("uses the correct counted phrase if available", () => {
    getL10nMock.mockImplementation(() => ({
      ruleEvaluator: (c) => c,
      Phrases: {
        ["test"]: {
          r: {
            "0": "test2",
            "1": "test4",
          },
        },
        ["test2"]: {
          r: {
            "0": "test3",
          },
        },
      },
    }));

    const phrase = getPhrase("test", {
      __count: 1,
    });
    expect(phrase).toBe("test4");
  });

  it("uses the correct counted phrase if available 2", () => {
    getL10nMock.mockImplementation(() => ({
      ruleEvaluator: (c) => (c === 1 ? 6 : c),
      Phrases: {
        ["test"]: {
          r: {
            "0": "test2",
            "6": "test4",
          },
        },
        ["test2"]: {
          r: {
            "0": "test3",
          },
        },
      },
    }));

    const phrase = getPhrase("test", {
      __count: 1,
    });
    expect(phrase).toBe("test4");
  });
});
