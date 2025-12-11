using DialogMaker.Lib.Elements;

namespace DialogMaker.Editor.Nodes
{
    public class ProjectNodesFabric : IDisposable
    {
        ~ProjectNodesFabric()
        {
            Dispose();
        }

        private readonly ElementsPool<DiagramNode> _nodes = new();
        private readonly ElementsPool<DiagramNodePort> _ports = new();

        #region Управление

        public DiagramNode GetNode()
        {
            return _nodes.GetElement();
        }
        public DiagramNodePort GetPort()
        {
            return _ports.GetElement();
        }
        public void Free(DiagramNode node)
        {
            _nodes.Free(node);
            node.Node = null;
        }
        public void Free(DiagramNodePort port)
        {
            _ports.Free(port);
            port.DataContext = null;
        }

        public void Dispose()
        {
            _nodes.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
