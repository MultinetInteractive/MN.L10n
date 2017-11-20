using System.Collections.Generic;
using System.Text;

namespace MN.L10n
{
	public class L10nParser
	{
		public List<string> Parse(string source)
		{
			List<string> Invocations = new List<string>();
			bool inToken = false;
			StringBuilder _tokenContent = new StringBuilder();
			char _stringContainer = '"';

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
						if (inToken)
						{
							var tail = source[_pos - 1];
							if (source[_pos] == _stringContainer && tail != '\\' && tail != '(')
							{
								Invocations.Add(_tokenContent.ToString());
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
