namespace Shared.Pages.TagBox;

public partial class TagBoxPage
{
    private IEnumerable<string> _tags1 = [];
    private IEnumerable<string> _tags2 = [];
    private IEnumerable<string> _tags3 = [];
    private IEnumerable<string> _tags4 = [];
    private IEnumerable<string> _tags5 = [];
    private IEnumerable<string> _tags6 = ["SystemDefault"];
    private IEnumerable<string> _tags7 = ["Database"];
    private readonly IEnumerable<string> _availableTags = ["SystemDefault", "Database", "MQTT", "Cluster", "Datatransfer"];
}
