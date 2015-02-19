using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wiki_down.core
{
    public interface IExistsInArticleTree
    {
        string GlobalId { get; }

        string ParentArticlePath { get; }

        string Path { get; }

    }
}
