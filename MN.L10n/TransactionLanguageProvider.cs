using System;
using System.Collections.Generic;


namespace MN.L10n
{
    internal class TransactionLanguageProvider : IL10nLanguageProvider, IDisposable
    {
        private readonly IL10nLanguageProvider _langProvider;
        private readonly Func<IDictionary<object, object>> _getScopeContainer;

        public TransactionLanguageProvider(IL10nLanguageProvider provider, Func<IDictionary<object, object>> getScopeContainer = null)
        {
            _langProvider = provider;
            _getScopeContainer = getScopeContainer;
        }

        private const string language_key = "___l10nlang";

        private Stack<string> GetLangStack()
        {
            if (_getScopeContainer is null) return null;

            var scopeContainer = _getScopeContainer();
            if (scopeContainer is null) return null;

            if (scopeContainer.TryGetValue(language_key, out var tmpLang))
            {
                return tmpLang as Stack<string>;
            }

            return null;
        }

        private Stack<string> GetOrCreateStack()
        {
            if (_getScopeContainer is null) return null;

            var existing = GetLangStack();

            if (existing is object) return existing;

            var scopeContainer = _getScopeContainer();
            if (scopeContainer == null) return null;

            var stack = new Stack<string>();
            scopeContainer[language_key] = stack;
            return stack;
        }

        public string GetLanguage()
        {
            var stack = GetLangStack();
            if (stack is object && stack.Count > 0)
            {
                return stack.Peek();
            }
            return _langProvider.GetLanguage();
        }

        public IDisposable LocalLanguageContext(string language)
        {
            var stack = GetOrCreateStack();
            if (stack == null) throw new Exception("Unable to create language scope, unable to resolve scope container");

            stack.Push(language);
            return this;
        }

        public void Dispose()
        {
            var stack = GetLangStack();
            if (stack is object && stack.Count > 0)
                stack.Pop();
        }
    }
}
