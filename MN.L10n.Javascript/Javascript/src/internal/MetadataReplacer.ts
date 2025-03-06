const metaDataStartToken = " !ctx=";

export function replaceMetadata(phrase: string | null) {
	if (phrase == null || typeof phrase != "string") {
		return phrase;
	}

	const ioMetaStart = phrase.toLowerCase().indexOf(metaDataStartToken);
	if (ioMetaStart < 0) {
		return phrase;
	}

	return phrase.substring(0, ioMetaStart);
}
