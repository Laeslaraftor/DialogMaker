using DialogMaker.Core.Editor;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogPackage : Disposable, IDialogResourcesContainer
    {
        public DialogPackage(DialogProject project)
        {
            var languages = project.Languages.Select(l =>
            {
                DialogLanguage lang = new(this, l);

                if (l.Id == project.DefaultLanguage?.Id)
                {
                    CurrentLanguage = lang;
                }

                return lang;
            }).ToDictionary(v => v.Id);

            Languages = new(languages);
        }

        public string Id { get; }
        public ReadOnlyDictionary<string, DialogLanguage> Languages { get; }
        public DialogLanguage? CurrentLanguage
        {
            get => field;
            set
            {
                if (field != value)
                {
                    if (value != null && value.Package != this)
                    {
                        throw new ArgumentException("Невозможно задать язык из другого пакета диалогов!", nameof(value));
                    }

                    InvokePropertyChanging(nameof(CurrentLanguage));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentLanguage));
                }
            }
        }
        public IDialogResourcesContainer? Parent => null;
        public DialogResources Resources => throw new NotImplementedException();

        DialogPackage IDialogResourcesContainer.Package => this;
        IResourcesOwner IResourcesOwner.Root => this;
        IResourcesOwner? IResourcesOwner.Parent => null;
        IResourcesContainer IResourcesOwner.Resources => Resources;


        #region Константы

        public const string CurrentLanguageProperty = nameof(CurrentLanguage);

        public bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
