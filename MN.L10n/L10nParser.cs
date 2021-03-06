﻿using System.Collections.Generic;
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
            bool isVerbatim = false;
            StringBuilder _tokenContent = new StringBuilder();
            char _stringContainer = '"';
            int row = 1;

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
                                return Invocations;
                            }

                            switch (peek)
                            {
                                case 's':
                                case 'm':
                                    // Even more likely to be _s/_m, proceed
                                    var modifier = 2;
                                    if (!TryPeek(modifier))
                                    {
                                        return Invocations;
                                    }

                                    // Special treatment for RawHtml-method
                                    if (peek == 'r' && !TryPeek(++modifier))
                                    {
                                        return Invocations;
                                    }

                                    if (peek == '(')
                                    {
                                        do
                                        {
                                            if (!TryPeek(++modifier))
                                            {
                                                return Invocations;
                                            }
                                        } while (char.IsWhiteSpace(peek));

                                        if (peek == '@')
                                        {
                                            if (!TryPeek(++modifier))
                                            {
                                                return Invocations;
                                            }
                                            isVerbatim = true;
                                        }
                                        else
                                        {
                                            isVerbatim = false;
                                        }

                                        if (peek == '"' || peek == '\'' || peek == '`')
                                        {
                                            _stringContainer = peek;
                                            inToken = true;
                                            _pos += modifier + 1;
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
                            if (!isVerbatim)
                            {
                                var tail = source[_pos - 1];
                                if (source[_pos] == _stringContainer && tail != '\\' && tail != '(')
                                {
                                    Invocations.Add(new PhraseInvocation { Phrase = _tokenContent.ToString(), Row = row });
                                    inToken = false;
                                }
                            }
                            else
                            {
                                var _tail = source[_pos - 1];
                                var _peek = source[_pos + 1];
                                if (source[_pos] == _stringContainer && _peek != _stringContainer && _tail != _stringContainer)
                                {
                                    Invocations.Add(new PhraseInvocation
                                    {
                                        Phrase = _tokenContent.ToString().Replace("\n", "\\n").Replace("\r", "").Replace("\"\"", "\\\""),
                                        Row = row
                                    });
                                    inToken = false;
                                }
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
