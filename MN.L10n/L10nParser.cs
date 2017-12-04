using System.Collections.Generic;
using System.Text;

namespace MN.L10n
{
	public class L10nParser
	{
		public struct PhraseInvocation
		{
			public string Phrase { get; set; }
			public int Row { get; set; }
		}

		public List<PhraseInvocation> Parse(string source)
		{
			List<PhraseInvocation> Invocations = new List<PhraseInvocation>();
			bool inToken = false;
			StringBuilder _tokenContent = new StringBuilder();
			char _stringContainer = '"';
			int row = 1;

			for (int _pos = 0; _pos < source.Length; _pos++)
			{
				switch (source[_pos])
				{
					case '_': // Possible _s/_m, peek to see
						if (!inToken)
						{
							_tokenContent.Clear();

							var peek = source[_pos + 1];
							switch (peek)
							{
								case 's':
								case 'm':
									// Even more likely to be _s/_m, proceed
									peek = source[_pos + 2];
									if (peek == '(')
									{
										peek = source[_pos + 3];
										if (peek == '"' || peek == '\'')
										{
											_stringContainer = peek;
											inToken = true;
											_pos += 4;
										}
									}
									break;
								default:
									continue;
							}
						}
						if (inToken)
						{
							_tokenContent.Append(source[_pos]);
						}

						break;
					default:
						if (source[_pos] == '\n')
							row++;
						if (inToken)
						{
							var tail = source[_pos - 1];
							if (source[_pos] == _stringContainer && tail != '\\' && tail != '(')
							{
								Invocations.Add(new PhraseInvocation { Phrase = _tokenContent.ToString(), Row = row });
								inToken = false;
							}
							_tokenContent.Append(source[_pos]);
						}
						break;
				}
			}

			return Invocations;
		}
	}
}
