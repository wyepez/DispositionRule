using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DispositionRule
{
    public class Article
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public int ArticleMaterialId { get; set; }
    }

    public class Stock
    {
        public int Id { get; set; }
        public int Weight { get; set; }
        public int Thickness { get; set; }
        public int Width { get; set; }
        public int Diameter { get; set; }
        public Article Article { get; set; }
    }

    public class GlobalParameters
    {
        public Stock StockBelow { get; set; }
        public Stock StockBelowSupport { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var stockBelow = new Stock
            {
                Id = 1,
                Weight = 100,
                Thickness = 2,
                Width = 500,
                Diameter = 1000,
                Article = new Article
                {
                    Id = 1,
                    GroupId = 1,
                    SubGroupId = 1,
                    ArticleMaterialId = 1,
                },
            };
            var stockBelowSupport = new Stock
            {
                Id = 2,
                Weight = 120,
                Thickness = 1,
                Width = 600,
                Diameter = 900,
                Article = new Article
                {
                    Id = 1,
                    GroupId = 1,
                    SubGroupId = 2,
                    ArticleMaterialId = 1,
                },
            };
            var globals = new GlobalParameters { StockBelow = stockBelow, StockBelowSupport = stockBelowSupport };

            var scoreRules = new[] {
                "if (StockBelow.Article.SubGroupId == 1 && StockBelowSupport.Article.SubGroupId == 1) return 5; else if (StockBelow.Article.SubGroupId == 2 || StockBelowSupport.Article.SubGroupId == 2) return 3; else return 0;"
            };

            var filterRules = new[] {
                "StockBelow is not null && StockBelowSupport is not null",
            };

            var compiledScoreRules = new List<(string script, ScriptRunner<double> execute)>();
            foreach (var rule in scoreRules)
            {
                try
                {
                    var script = CSharpScript.Create<double>(rule, globalsType: typeof(GlobalParameters));
                    var runner = script.CreateDelegate();
                    compiledScoreRules.Add((rule, runner));
                }
                catch (CompilationErrorException exception)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, exception.Diagnostics));
                }
            }

            var compiledFilterRules = new List<(string script, ScriptRunner<bool> execute)>();
            foreach (var rule in filterRules)
            {
                try
                {
                    var script = CSharpScript.Create<bool>(rule, globalsType: typeof(GlobalParameters));
                    var runner = script.CreateDelegate();
                    compiledFilterRules.Add((rule, runner));
                }
                catch (CompilationErrorException exception)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, exception.Diagnostics));
                }
            }

            // Evaluate each score rule
            foreach (var rule in compiledScoreRules)
            {
                Console.WriteLine(String.Format("Rule '{0}' was evaluated as {1}", rule.script, await rule.execute(globals)));
            }

            // Evaluate each filter rule
            foreach (var rule in compiledFilterRules)
            {
                Console.WriteLine(String.Format("Rule '{0}' was evaluated as {1}", rule.script, await rule.execute(globals)));
            }
        }
    }
}
