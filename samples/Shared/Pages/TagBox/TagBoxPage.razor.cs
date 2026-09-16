namespace Shared.Pages.TagBox;

public partial class TagBoxPage
{
    private static readonly IEnumerable<string> s_availableTags = ["SystemDefault", "Database", "MQTT", "Cluster", "Datatransfer"];
    private static readonly IEnumerable<string> s_availableTagsWithWiderItem =
    [
        .. s_availableTags,
        "internal ClusterEditor MQTT-DataPort connection"
    ];
    private static readonly IEnumerable<string> s_manyAvailableTags = [.. Enumerable.Range(1, 30).Select(i => $"Tag {i}")];

    private IEnumerable<string> _tags1 = [];
    private IEnumerable<string> _tags2 = [];
    private IEnumerable<string> _tags3 = [];
    private IEnumerable<string> _tags4 = [];
    private IEnumerable<string> _tags5 = [];
    private IEnumerable<string> _tags6 = ["SystemDefault"];
    private IEnumerable<string> _tags7 = ["Database"];
    private IEnumerable<string> _tags8 = [];
    private IEnumerable<string> _tags9 = [];
}
