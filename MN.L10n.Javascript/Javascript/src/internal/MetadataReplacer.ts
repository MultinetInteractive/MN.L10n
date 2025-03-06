var metaDataStartToken = " !ctx=";

export function replaceMetadata(phrase: string | null): string {
	if (phrase == null) {
		return "";
	}

	if (typeof phrase != "string") {
		return phrase;
	}

	var ioMetaStart = phrase.toLowerCase().indexOf(metaDataStartToken);
	if (ioMetaStart < 0) {
		return phrase;
	}

	return phrase.substring(0, ioMetaStart);
}
