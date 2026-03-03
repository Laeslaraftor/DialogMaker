using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Nodes.Structs
{
    public struct TriggerNodeCompileTimeInfo(string id)
    {
        public string Id { get; } = id;
        public Dictionary<string, DialogExecutionParameter> Inputs { get; } = [];
        public Dictionary<string, DialogExecutionParameter> Outputs { get; } = [];
        
    }
}
