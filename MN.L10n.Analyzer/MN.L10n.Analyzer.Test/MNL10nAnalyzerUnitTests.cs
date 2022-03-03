using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TestHelper;

namespace MN.L10n.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void Test_MN0003_DoNotAllowWhiteSpaceAtStartOrEndOfString()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TypeName
              {
                public void Main() {
                    _s(""Testing "");
                    _s("" Testing"");
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            expectations.Add(new DiagnosticResult
            {
                Id = "MN0003",
                Message = "The string cannot start or end with whitespaces.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 14, 21)
                }
            });

            expectations.Add(new DiagnosticResult
            {
                Id = "MN0003",
                Message = "The string cannot start or end with whitespaces.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 21)
                }
            });

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        public void Test_MN0003_DoNotAllowWhiteSpaceAtStartOrEndOfString_WithClassName()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TypeName
              {
                public void Main() {
                    L10n._s(""Testing "");
                    L10n._s("" Testing"");
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            expectations.Add(new DiagnosticResult
            {
                Id = "MN0003",
                Message = "The string cannot start or end with whitespaces.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 14, 21)
                }
            });

            expectations.Add(new DiagnosticResult
            {
                Id = "MN0003",
                Message = "The string cannot start or end with whitespaces.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 21)
                }
            });

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void Test_MN0007_DoNotAllowNonClassAsKeywordArgument()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
              class TypeName
              {
                public void Main() {
                    _s(""Testing $someParameter$"", ""I am a test string"");
                    _s(""Testing $someParameter$"", new { someParameter = ""I am a test string"" });
                    _s(""Testing $someParameter$"", new TestClass());
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            expectations.Add(new DiagnosticResult
            {
                Id = "MN0007",
                Message = "L10n requires a class or anonymous type (or explicitly null) for keywords.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 21)
                }
            });

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [TestMethod]
        public void Test_MN0007_DoNotAllowNonClassAsKeywordArgument_WithClassName()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
              class TypeName
              {
                public void Main() {
                    L10n._s(""Testing $someParameter$"", ""I am a test string"");
                    L10n._s(""Testing $someParameter$"", new { someParameter = ""I am a test string"" });
                    L10n._s(""Testing $someParameter$"", new TestClass());
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            expectations.Add(new DiagnosticResult
            {
                Id = "MN0007",
                Message = "L10n requires a class or anonymous type (or explicitly null) for keywords.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 21)
                }
            });

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [TestMethod]
        public void Test_MN0007_AllowNullAsKeywordArgument()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
              class TypeName
              {
                public void Main() {
                    _s(""Testing $someParameter$"", null);
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [TestMethod]
        public void Test_MN0007_AllowNullAsKeywordArgument_WithClassName()
        {
            var test = @"
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Text;
          using System.Threading.Tasks;
          using System.Diagnostics;

          namespace ConsoleApplication1
          {
              class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
              class TypeName
              {
                public void Main() {
                    L10n._s(""Testing $someParameter$"", null);
                }
              }
          }";

            var expectations = new List<DiagnosticResult>();
            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MNL10nCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MNL10nAnalyzer();
        }
    }
}
