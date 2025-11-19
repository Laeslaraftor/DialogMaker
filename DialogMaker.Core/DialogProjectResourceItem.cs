using System;

namespace DialogMaker.Core
{
    public class DialogProjectResourceItem
    {
        public Guid Id { get; }
        public string FilePath { get; }
        public DialogResourceType Type { get; }
        public string Name { get; set; }
    }
}
