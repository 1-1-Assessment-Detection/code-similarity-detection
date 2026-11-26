using System.Globalization;
using Microsoft.VisualBasic.FileIO;

const double threshold = 0.80;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: TextSimilarityExperiment <dataset-directory> <results.csv>");
    return 1;
}

try
{
    var dataset = Path.GetFullPath(args[0]);
    var output = Path.GetFullPath(args[1]);
    var truthPath = Path.Combine(dataset, "ground-truth.csv");
    var submissions = new[] { "original", "copied" }
        .SelectMany(folder => Directory.EnumerateFiles(Path.Combine(dataset, folder), "*.py"))
        .Select(path => new Submission(
            Path.GetRelativePath(dataset, path).Replace('\\', '/'),
            File.ReadAllText(path).ReplaceLineEndings("\n")))
        .OrderBy(item => item.Path, StringComparer.Ordinal)
        .ToArray();

    if (submissions.Length != 10)
        throw new InvalidDataException($"Expected 10 submissions, found {submissions.Length}.");

    var paths = submissions.Select(item => item.Path).ToHashSet(StringComparer.Ordinal);
    var truth = ReadGroundTruth(truthPath, paths);
    var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    if (string.Equals(output, truthPath, comparison) ||
        submissions.Any(item => string.Equals(output, Path.GetFullPath(item.Path, dataset), comparison)))
        throw new InvalidDataException("The output cannot overwrite a dataset input.");

    var rows = new List<string>
    {
        "file_a,file_b,expected,level,equals,equals_outcome,a_contains_b,b_contains_a," +
        "contains,contains_outcome,levenshtein_distance,levenshtein_score,levenshtein_match,levenshtein_outcome"
    };

    for (var i = 0; i < submissions.Length; i++)
    {
        for (var j = i + 1; j < submissions.Length; j++)
        {
            var left = submissions[i];
            var right = submissions[j];
            var equals = string.Equals(left.Content, right.Content, StringComparison.Ordinal);
            var leftContainsRight = left.Content.Contains(right.Content, StringComparison.Ordinal);
            var rightContainsLeft = right.Content.Contains(left.Content, StringComparison.Ordinal);
            var contains = leftContainsRight || rightContainsLeft;

            var distance = LevenshteinDistance(left.Content, right.Content);
            var maxLength = Math.Max(left.Content.Length, right.Content.Length);
            var score = maxLength == 0 ? 1.0 : (maxLength - distance) / (double)maxLength;
            var match = score >= threshold;
            var expected = truth.TryGetValue((left.Path, right.Path), out var level);

            rows.Add(string.Join(",", QuoteCsv(left.Path), QuoteCsv(right.Path), expected,
                expected ? level.ToString(CultureInfo.InvariantCulture) : "",
                equals, Outcome(expected, equals),
                leftContainsRight, rightContainsLeft, contains, Outcome(expected, contains),
                distance.ToString(CultureInfo.InvariantCulture), score.ToString("F6", CultureInfo.InvariantCulture),
                match, Outcome(expected, match)));
        }
    }

    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    File.WriteAllText(output, string.Join("\n", rows) + "\n");
    Console.WriteLine($"Wrote {rows.Count - 1} pairs to {output}.");
    return 0;
}
catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException
    or MalformedLineException or ArgumentException or NotSupportedException)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

static Dictionary<(string, string), int> ReadGroundTruth(string path, HashSet<string> knownPaths)
{
    using var parser = new TextFieldParser(path);
    parser.SetDelimiters(",");
    var header = parser.ReadFields() ?? throw new InvalidDataException("Missing Ground Truth header.");
    var copyColumn = Array.IndexOf(header, "copy_path");
    var originalColumn = Array.IndexOf(header, "original_path");
    var levelColumn = Array.IndexOf(header, "level");
    if (copyColumn < 0 || originalColumn < 0 || levelColumn < 0)
        throw new InvalidDataException("Ground Truth requires copy_path, original_path and level.");

    var truth = new Dictionary<(string, string), int>();
    while (!parser.EndOfData)
    {
        var row = parser.ReadFields();
        if (row is null || row.Length != header.Length)
            throw new InvalidDataException("A Ground Truth row has the wrong number of fields.");

        var copy = row[copyColumn];
        var original = row[originalColumn];
        if (!knownPaths.Contains(copy) || !knownPaths.Contains(original) || copy == original ||
            !int.TryParse(row[levelColumn], NumberStyles.None, CultureInfo.InvariantCulture, out var level) ||
            level is < 1 or > 4)
            throw new InvalidDataException($"Invalid Ground Truth entry for {copy} and {original}.");

        var key = string.CompareOrdinal(copy, original) < 0 ? (copy, original) : (original, copy);
        if (!truth.TryAdd(key, level))
            throw new InvalidDataException($"Duplicate Ground Truth pair: {copy} and {original}.");
    }

    if (truth.Count != 5)
        throw new InvalidDataException($"Expected 5 positive pairs, found {truth.Count}.");

    return truth;
}

static int LevenshteinDistance(string left, string right)
{
    var previous = Enumerable.Range(0, right.Length + 1).ToArray();
    var current = new int[right.Length + 1];

    for (var i = 1; i <= left.Length; i++)
    {
        current[0] = i;
        for (var j = 1; j <= right.Length; j++)
        {
            var deletion = previous[j] + 1;
            var insertion = current[j - 1] + 1;
            var substitution = previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
            current[j] = Math.Min(substitution, Math.Min(deletion, insertion));
        }

        (previous, current) = (current, previous);
    }

    return previous[right.Length];
}

static string Outcome(bool expected, bool predicted) => (expected, predicted) switch
{
    (true, true) => "TP",
    (false, true) => "FP",
    (true, false) => "FN",
    (false, false) => "TN"
};

static string QuoteCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

internal sealed record Submission(string Path, string Content);
