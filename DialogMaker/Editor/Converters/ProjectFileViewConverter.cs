using DialogMaker.Lib.Elements;
using System.Collections;

namespace DialogMaker.Editor
{
    public class ProjectFileViewConverter : ICollectionValueConverter<ProjectFile, FileView>
    {

        public FileView Convert(ProjectFile Value, int Index, IList FirstCollection, IList SecondCollection)
        {
            if (Index < SecondCollection.Count &&
                SecondCollection[Index] is FileView view)
            {
                return view;
            }

            return Value.View;
        }
        public ProjectFile ConvertBack(FileView Value, int Index, IList FirstCollection, IList SecondCollection)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return Value.File;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }
    }
}
