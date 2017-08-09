namespace MN.L10n
{
    public interface IL10nDataProvider
    {
		L10n LoadL10n();
		bool SaveL10n(L10n l10n);
    }
}
