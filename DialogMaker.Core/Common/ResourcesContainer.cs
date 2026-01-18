using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public abstract class ResourcesContainer : Disposable
    {
        public abstract DialogResources Resources { get; }

        #region Управление

        public DialogResourceVariable GetVariable(string id) => Resources.Variables[id];
        public void SetVariable(string id, OperandValue value) => GetVariable(id).Value = value;
        public Dictionary<ResourcePath, OperandValue> SaveVariables()
        {
            Dictionary<ResourcePath, OperandValue> result = [];

            SaveVariables(result, Resources);

            foreach (var childResources in GetChildResources())
            {
                SaveVariables(result, childResources);
            }

            return result;
        }
        public void RestoreVariables(Dictionary<ResourcePath, OperandValue> values)
        {
            var resourcesOwner = GetResourcesOwner();

            if (resourcesOwner == null)
            {
                return;
            }

            foreach (var info in values)
            {
                var item = info.Key.Find(resourcesOwner);

                if (item is IVariable variable)
                {
                    variable.Value = info.Value;
                }
            }
        }

        public abstract bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result);

        protected virtual IEnumerable<DialogResources> GetChildResources()
        {
            yield break;
        }
        protected virtual IResourcesOwner? GetResourcesOwner()
        {
            return this as IResourcesOwner;
        }

        #endregion

        #region Статика

        private static void SaveVariables(Dictionary<ResourcePath, OperandValue> container, DialogResources resources)
        {
            foreach (var variable in resources.Variables.Values)
            {
                if (!variable.IsSeparated)
                {
                    container.Add(variable.GetPath(), variable.Value);
                }
            }
        }

        #endregion
    }
}
