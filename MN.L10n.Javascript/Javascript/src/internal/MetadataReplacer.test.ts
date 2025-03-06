import { replaceMetadata } from "./MetadataReplacer";
describe("it replaces metadata correctly", () => {
	const testData: [string, string][] = [
		["Deltagare", "Deltagare"],
		["Deltagare !ctx=1", "Deltagare"],
		["Deltagare !Ctx=1", "Deltagare"],
		["Deltagare!ctx=1", "Deltagare!ctx=1"],
		[
			"Det finns en massa deltagare !ctx=1 all this text is meta",
			"Det finns en massa deltagare"
		]
	];

	for (const [input, expected] of testData) {
		it(`replaces metadata in "${input}" correctly`, () => {
			const result = replaceMetadata(input);
			expect(result).toBe(expected);
		});
	}
});
