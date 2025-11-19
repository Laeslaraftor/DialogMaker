using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core
{
    public class DialogProjectResources
    {
        public List<DialogProjectReplica> Replicas { get; } = new();
        public List<DialogProjectResourceItem> Items { get; } = new();
    }
}
