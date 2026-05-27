using DialogMaker.Core.Scripting.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Tests
{
    internal class ScriptParserTests
    {
        [Test]
        public static void ParseAttribute()
        {
            string[] values = ["[SomeAttribute]", "[Space Attribute]", "Old_Attribute]", "MegaAttribute_"];
            var parser = DialogScriptAttributeTokenParser.Instance;

            foreach (var value in values)
            {
                Console.WriteLine();
                Console.WriteLine(value + ":");
                bool canParse = parser.CanParse(value);
                Console.WriteLine($"Can parse: {canParse}");

                if (!canParse)
                {
                    continue;
                }

                var token = parser.Parse(new(value));
                Console.WriteLine($"ParsedToken: {token}");

                if (token is DialogScriptAttributeToken attributeToken)
                {
                    Console.WriteLine($"Name: {attributeToken.Name}");
                }
            }
        } 
    }
}
