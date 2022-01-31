import { getGlobal } from "./GetGlobal";

describe("getGlobal", () => {
  it("returns window if defined", () => {
    const fakeWindow = {};
    (global as any).window = fakeWindow;
    expect(getGlobal()).toBe(fakeWindow);
    delete global.window;
  });
  it("returns global if defined and window not defined", () => {
    expect(getGlobal()).toBe(global);
  });
});
