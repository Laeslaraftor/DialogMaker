using DialogMaker.Lib.Elements;
using System.Collections;

namespace DialogMaker.Editor
{
    public class ProjectFileViewConverter : ICollectionValueConverter<ProjectFile, FileView>
    {

        public FileView Convert(ProjectFile value, int Index, IList firstCollection, IList secondCollection)
        {
            if (Index < secondCollection.Count &&
                secondCollection[Index] is FileView view)
            {
                return view;
            }

            return value.View;
        }
        public ProjectFile ConvertBack(FileView value, int index, IList firstCollection, IList secondCollection)
        {
            return value.File!;
        }
    }
}
