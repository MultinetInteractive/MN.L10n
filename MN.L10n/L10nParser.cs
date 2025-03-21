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
            public int StartChar { get; set; }
            public int EndChar { get; set; }
            public bool IsEscaped { get; set; }
            public char StringContainer { get; set; }
        }

        public IEnumerable<PhraseInvocation> Parse(string source, bool allowEscapedStrings = false)
        {
            bool inToken = false;
            bool isVerbatim = false;
            bool isEscaped = false;
            StringBuilder _tokenContent = new StringBuilder();
            char _stringContainer = '"';
            int row = 1;
            int startChar = 1;

            for (int _pos = 0; _pos < source.Length; _pos++)
            {
#pragma warning disable S1854 // Unused assignments should be removed
                char peek = source[_pos];
#pragma warning restore S1854 // Unused assignments should be removed

                bool TryPeek(int forward)
                {
                    if (source.Length <= _pos + forward)
                    {
                        return false;
                    }

                    peek = source[_pos + forward];
                    return true;
                }

                switch (source[_pos])
                {
                    case '_': // Possible _s/_m, peek to see
                        if (!inToken)
                        {
                            _tokenContent.Clear();
                            if (!TryPeek(1))
                            {
                                break;
                            }

                            switch (peek)
                            {
                                case 's':
                                case 'm':
                                    // Even more likely to be _s/_m, proceed
                                    var modifier = 2;
                                    if (!TryPeek(modifier))
                                    {
                                        break;
                                    }

                                    // Special treatment for RawHtml-method
                                    if (peek == 'r' && !TryPeek(++modifier))
                                    {
                                        break;
                                    }

                                    if (peek == '(')
                                    {
                                        do
                                        {
                                            if (!TryPeek(++modifier))
                                            {
                                                break;
                                            }
                                        } while (char.IsWhiteSpace(peek));

                                        if (peek == '@')
                                        {
                                            if (!TryPeek(++modifier))
                                            {
                                                break;
                                            }
                                            isVerbatim = true;
                                        }
                                        else
                                        {
                                            isVerbatim = false;
                                        }

                                        bool peekedEscapeChar = false;
                                        if (allowEscapedStrings && peek == '\\')
                                        {
                                            TryPeek(++modifier);
                                            peekedEscapeChar = true;
                                        }

                                        if (peek == '"' || peek == '\'' || peek == '`')
                                        {
                                            _stringContainer = peek;
                                            inToken = true;
                                            _pos += modifier + 1;
                                            startChar = _pos + 1; // Behöver vara +1 för att _pos är 0-indexerad
                                            isEscaped = peekedEscapeChar;
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
                        {
                            row++;
                        }

                        if (inToken)
                        {
                            if (!isVerbatim)
                            {
                                var tail = source[_pos - 1];
                                var validTail = tail != '(' && isEscaped == (tail == '\\');
                                if (source[_pos] == _stringContainer && validTail)
                                {
                                    var phrase = _tokenContent.ToString();

                                    // Hoppa över sista \ om den är escape:ad
                                    if (isEscaped)
                                    {
                                        phrase = phrase.Substring(0, phrase.Length - 1);
                                    }

                                    yield return new PhraseInvocation
                                    {
                                        Phrase = phrase,
                                        Row = row,
                                        StartChar = startChar,
                                        EndChar = _pos,
                                        IsEscaped = isEscaped,
                                        StringContainer = _stringContainer
                                    };
                                    inToken = false;
                                }
                            }
                            else
                            {
                                var _tail = source[_pos - 1];
                                var _peek = source[_pos + 1];
                                if (source[_pos] == _stringContainer && _peek != _stringContainer && _tail != _stringContainer)
                                {
                                    var phrase = _tokenContent.ToString().Replace("\n", "\\n").Replace("\r", "").Replace("\"\"", "\\\"");

                                    // Hoppa över sista \ om den är escape:ad
                                    if (isEscaped)
                                    {
                                        phrase = phrase.Substring(0, phrase.Length - 1);
                                    }

                                    yield return new PhraseInvocation
                                    {
                                        Phrase = phrase,
                                        Row = row,
                                        StartChar = startChar,
                                        EndChar = _pos,
                                        StringContainer = _stringContainer
                                    };
                                    inToken = false;
                                }
                            }

                            if (inToken && !isVerbatim && source[_pos] == '\\' && TryPeek(1) && (source[_pos + 1] == 'n' || source[_pos + 1] == _stringContainer))
                            {
                                if (source[_pos + 1] == 'n')
                                {
                                    _tokenContent.Append('\n');
                                }
                                else if (source[_pos + 1] == _stringContainer)
                                {
                                    _tokenContent.Append(_stringContainer);
                                }
                                _pos++;
                            }
                            else
                            {
                                _tokenContent.Append(source[_pos]);
                            }
                        }
                        break;
                }
            }
        }
    }
}
