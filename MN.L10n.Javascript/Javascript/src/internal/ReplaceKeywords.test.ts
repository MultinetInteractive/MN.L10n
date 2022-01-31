import { replaceKeywords } from "./ReplaceKeywords";

describe("replaceKeywords", () => {
  it("handles undefined args", () => {
    expect(replaceKeywords("phrase")).toBe("phrase");
  });
  it("handles undefined args 2", () => {
    expect(replaceKeywords("phrase $param$ test")).toBe("phrase $param$ test");
  });

  it("replaces keyword", () => {
    expect(
      replaceKeywords("Hello $name$!", {
        name: "world",
      })
    ).toBe("Hello world!");
  });

  it("replaces keyword 2", () => {
    expect(
      replaceKeywords("Hello $name$! $name$", {
        name: "world",
      })
    ).toBe("Hello world! world");
  });

  it("replaces keyword 3", () => {
    expect(
      replaceKeywords("Hello $tester$ $name$", {
        name: "world",
      })
    ).toBe("Hello $tester$ world");
  });

  it("replaces keyword 4", () => {
    expect(
      replaceKeywords("Hello $tester$ $name$", {
        name: "world",
      })
    ).toBe("Hello $tester$ world");
  });

  it("replaces keyword 5", () => {
    expect(
      replaceKeywords("Hello $tester$ $name$", {
        name: undefined,
      })
    ).toBe("Hello $tester$ ");
  });

  //unclear if this behavior is expected, but it is how it works currently.
  //should not matter during normal usage
  it("does not use prototype args property", () => {
    function args() {}
    args.prototype = {
      name: "world",
    };
    const formatArgument = new args();
    expect(formatArgument.name).toBe("world");
    expect(replaceKeywords("Hello $tester$ $name$", formatArgument)).toBe(
      "Hello $tester$ $name$"
    );
  });
});
