﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using TestHelper;
using Xunit;

namespace MN.L10n.Analyzer.Test
{
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [Fact]
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

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0003",
                    Message = "The string cannot start or end with whitespaces",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 14, 6)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0003",
                    Message = "The string cannot start or end with whitespaces",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [Fact]
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

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0003",
                    Message = "The string cannot start or end with whitespaces",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 14, 6)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0003",
                    Message = "The string cannot start or end with whitespaces",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        //No diagnostics expected to show up
        [Fact]
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

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0007",
                    Message = "L10n requires a class or anonymous type (or explicitly null) for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [Fact]
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

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0007",
                    Message = "L10n requires a class or anonymous type (or explicitly null) for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [Fact]
        public void Test_MN0007_AllowNullAsKeywordArgument()
        {
            var test = @"
		  namespace ConsoleApplication1
		  {
			  class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
			  class TypeName
			  {
				public void Main() {
					_s(""Testing testing"", null);
				}
			  }
		  }";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Test_MN0007_AllowNullAsKeywordArgument_WithClassName()
        {
            var test = @"
		  namespace ConsoleApplication1
		  {
			  class TestClass { public string someParameter { get; set; } = ""I am the testiest of strings""; }
			  class TypeName
			  {
				public void Main() {
					L10n._s(""Testing testing"", null);
				}
			  }
		  }";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Test_MN0008_DoNotAllowMissingKeywordArgument()
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
					_s(""Testing $someParameter$"");
			  }
		  }";

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0008",
                    Message = "L10n requires a class or anonymous type (or explicitly null) for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [Fact]
        public void Test_MN0008_DoNotAllowMissingKeywordArgument_WithClassName()
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
					L10n._s(""Testing $someParameter$"");
				}
			  }
		  }";

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0008",
                    Message = "L10n requires a class or anonymous type (or explicitly null) for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 15, 6)
                }
                }
            };

            VerifyCSharpDiagnostic(test, expectations.ToArray());
        }

        [Fact]
        public void Test_MN0008_NoMissingKeywordArgument()
        {
            var test = @"
		  namespace ConsoleApplication1
		  {
			  class TypeName
			  {
				public void Main() {
					_s(""Testing $someParameter$"", new { someParameter = ""I am Cornholio"" });
				}
			  }
		  }";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Test_MN0008_NoMissingKeywordArgument_WithClassName()
        {
            var test = @"
		  namespace ConsoleApplication1
		  {
			  class TypeName
			  {
				public void Main() {
					L10n._s(""Testing $someParameter$"", new { someParameter = ""I am Cornholio"" });
				}
			  }
		  }";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Test_MN0009_MissingKeywordsInObjectArgument()
        {
            var test = @"
		  namespace ConsoleApplication1
		  {
			  class TypeName
			  {
				public void Main() {
					_s(""Testing $someParameter$"", new { wrongParameter = ""I am Cornholio"" });
					L10n._s(""Testing $someParameter$"", new { wrongParameter = ""I am Cornholio"" });
					_s(""Testing $someParameter$"", new { someParameter = ""I am Cornholio"" });
                    _s(@""Din adress, $email$, har registrerats som prenumerant hos $company$.
Vänligen bekräfta ditt intresse genom att klicka på länken nedan.

$verificationlink$

Observera! Har du inte verifierat adressen inom en månad kommer den att plockas bort och du måste registrera dig igen.
----------------------------------------------------------------------
Denna registrering inkom från ip:
$ipaddress$

Om du inte själv har gjort denna registrering, kan du anmäla detta till $abusemail$"", new

		{
				company,
			    abusemail = ""$abusemail$"",

		});
		}
				}
			  }
		  }";

            var expectations = new List<DiagnosticResult>
            {
                new DiagnosticResult
                {
                    Id = "MN0009",
                    Message = "L10n is missing '$someParameter$' in the object for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 7, 6)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0009",
                    Message = "L10n is missing '$someParameter$' in the object for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 8, 6)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0009",
                    Message = "L10n is missing '$email$' in the object for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 10, 21)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0009",
                    Message = "L10n is missing '$verificationlink$' in the object for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 10, 21)
                }
                },
                new DiagnosticResult
                {
                    Id = "MN0009",
                    Message = "L10n is missing '$ipaddress$' in the object for keywords",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 10, 21)
                }
                }
            };

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
