// Thực nghiệm Tuần 1 (issue #4): hỏi trực tiếp AI cho từng cặp bài nộp, qua API chuẩn OpenAI Chat Completions (mặc định OpenRouter).
// Chưa dùng AI Harness, Tool Calling hay Self-check Loop. Chạy từ thư mục gốc repository.
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultEndpoint = "https://openrouter.ai/api/v1/chat/completions";

static string? Env(string name) => Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : null;

var options = Options.Parse(args);

foreach (string dir in options.SubmissionsDirs)
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Không tìm thấy thư mục bài nộp: {Path.GetFullPath(dir)}");
        Console.Error.WriteLine("Hãy chạy từ thư mục gốc repository hoặc dùng --submissions <thư mục> trỏ đúng nơi chứa các file .py.");
        return 1;
    }
}
if (!File.Exists(options.PromptPath))
{
    Console.Error.WriteLine($"Không tìm thấy prompt: {Path.GetFullPath(options.PromptPath)}. Hãy chạy từ thư mục gốc repository.");
    return 1;
}

string template = PromptTemplate.Load(options.PromptPath);
List<string> files = options.SubmissionsDirs
    .SelectMany(d => Directory.EnumerateFiles(d, "*.py", SearchOption.AllDirectories))
    .OrderBy(f => f, StringComparer.Ordinal)
    .ToList();

// Tên file là khóa của file phản hồi; trùng tên giữa các thư mục sẽ ghi đè nhau.
string[] duplicates = files.GroupBy(f => Path.GetFileNameWithoutExtension(f)).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
if (duplicates.Length > 0)
{
    Console.Error.WriteLine($"Trùng tên bài nộp giữa các thư mục: {string.Join(", ", duplicates)}");
    return 1;
}

var pairs = new List<(string Id, string A, string B)>();
for (int i = 0; i < files.Count; i++)
{
    for (int j = i + 1; j < files.Count; j++)
    {
        pairs.Add(($"P{pairs.Count + 1:D2}", files[i], files[j]));
    }
}

// Nhãn chuẩn chỉ dùng để đối chiếu kết quả, không bao giờ đưa vào prompt.
Dictionary<string, string>? truth = File.Exists(options.GroundTruthPath) ? GroundTruth.Load(options.GroundTruthPath) : null;
var labels = new Dictionary<string, (string Truth, string Level)>();
foreach (var (id, a, b) in pairs)
{
    string key = GroundTruth.Key(Path.GetFullPath(a), Path.GetFullPath(b));
    labels[id] = truth is null ? ("", "") : truth.TryGetValue(key, out string? level) ? ("copy", level) : ("not_copy", "");
}

Console.WriteLine($"Thư mục bài nộp: {string.Join(", ", options.SubmissionsDirs)}");
Console.WriteLine($"Số bài: {files.Count} | Số cặp: {pairs.Count} (kỳ vọng 10 bài / 45 cặp)");
foreach (string f in files)
{
    Console.WriteLine($"  - {Path.GetFileNameWithoutExtension(f)}  ({f})");
}
if (truth is null)
{
    Console.WriteLine($"Không có ground truth tại {options.GroundTruthPath}: để trống ground_truth, level, outcome.");
}
else
{
    int copyPairs = labels.Values.Count(l => l.Truth == "copy");
    Console.WriteLine($"Ground truth: {copyPairs} cặp sao chép / {pairs.Count - copyPairs} cặp không (khai báo {truth.Count} cặp sao chép).");
    if (copyPairs != truth.Count)
    {
        Console.Error.WriteLine("Cảnh báo: có cặp trong ground-truth.csv không khớp bài nào trong thư mục bài nộp.");
    }
}

if (!options.Run)
{
    Console.WriteLine("Chế độ xem trước: chưa gọi API. Thêm --run để chạy thật.");
    return 0;
}

DotEnv.Load();
// Mọi dịch vụ theo chuẩn OpenAI Chat Completions đều dùng được (OpenRouter, DeepSeek...). Tên OPENROUTER_* cũ vẫn được đọc.
string endpoint = Env("AI_ENDPOINT") ?? DefaultEndpoint;
string? apiKey = Env("AI_API_KEY") ?? Env("OPENROUTER_API_KEY");
string? model = Env("AI_MODEL") ?? Env("OPENROUTER_MODEL");
if (apiKey is null || model is null)
{
    Console.Error.WriteLine("Thiếu AI_API_KEY hoặc AI_MODEL. Điền vào .env.local (xem .env.example) hoặc đặt biến môi trường.");
    return 1;
}
Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine($"Model: {model}");
if (endpoint.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase) && !model.EndsWith(":free", StringComparison.Ordinal))
{
    Console.Error.WriteLine("Cảnh báo: model OpenRouter không có hậu tố :free, có thể tính phí.");
}

string responsesDir = Path.Combine(options.OutDir, "responses");
Directory.CreateDirectory(responsesDir);
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
var records = new List<PairRecord>();
bool rateLimited = false;

foreach (var (id, fileA, fileB) in pairs)
{
    string nameA = Path.GetFileNameWithoutExtension(fileA);
    string nameB = Path.GetFileNameWithoutExtension(fileB);
    string responsePath = Path.Combine(responsesDir, $"{nameA}__{nameB}.md");

    // Bỏ qua cặp đã có phản hồi để chạy lại không gọi API hai lần.
    if (File.Exists(responsePath))
    {
        // Không trộn kết quả của hai model trong cùng một thư mục kết quả.
        string previous = Regex.Match(File.ReadAllText(responsePath), @"^requested_model: (.*)$", RegexOptions.Multiline).Groups[1].Value.Trim();
        if (previous != model)
        {
            Console.Error.WriteLine($"{responsePath} được tạo bằng model {previous}, khác model hiện tại {model}.");
            Console.Error.WriteLine("Dùng --out <thư mục khác> cho model mới, hoặc đặt lại AI_MODEL như lần chạy trước.");
            return 1;
        }
        records.Add(PairRecord.FromFile(id, nameA, nameB, responsePath));
        Console.WriteLine($"{id} {nameA} vs {nameB}: đã có, bỏ qua");
        continue;
    }

    string prompt = PromptTemplate.Fill(template, File.ReadAllText(fileA), File.ReadAllText(fileB));
    var sw = Stopwatch.StartNew();
    try
    {
        ChatResult result = await ChatApi.Send(http, endpoint, model, prompt);
        sw.Stop();
        PairRecord.WriteResponseFile(responsePath, id, model, sw.Elapsed.TotalSeconds, result);
        var record = PairRecord.FromFile(id, nameA, nameB, responsePath);
        records.Add(record);
        Console.WriteLine($"{id} {nameA} vs {nameB}: {record.Verdict} {record.Similarity} ({record.LatencySeconds:F1}s)");
    }
    catch (RateLimitException ex)
    {
        // Dừng ngay, không đổi model. Chạy lại cùng lệnh sẽ tiếp tục từ cặp này.
        sw.Stop();
        rateLimited = true;
        Console.Error.WriteLine($"{id} {nameA} vs {nameB}: HTTP 429 - bị giới hạn tốc độ ({ex.Message}).");
        Console.Error.WriteLine($"Đã dừng ở {id}. Đợi một lúc rồi chạy lại đúng lệnh này với cùng model {model}; các cặp đã có phản hồi sẽ được bỏ qua.");
        break;
    }
    catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
    {
        // Không ghi file phản hồi để lần chạy sau thử lại đúng cặp này.
        sw.Stop();
        string reason = ex is TaskCanceledException ? "het_thoi_gian_cho" : ex.Message;
        records.Add(PairRecord.Failed(id, nameA, nameB, $"loi_api: {reason}"));
        Console.Error.WriteLine($"{id} {nameA} vs {nameB}: lỗi API - {reason}");
    }
}

PairRecord.WriteCsv(Path.Combine(options.OutDir, "results.csv"), records, options.OutDir, labels);
int parsed = records.Count(r => r.Verdict.Length > 0);
Console.WriteLine($"Đã ghi {records.Count}/{pairs.Count} dòng vào results.csv; {parsed}/{pairs.Count} cặp có kết luận hợp lệ.");
if (rateLimited) return 2;
if (parsed < pairs.Count)
{
    // Không báo thành công khi còn cặp lỗi API hoặc phản hồi không đọc được.
    Console.Error.WriteLine("Chưa đủ kết luận cho mọi cặp: chạy lại để thử các cặp lỗi API; xem cột notes với cặp khong_parse_duoc.");
    return 3;
}
return 0;

sealed record Options(List<string> SubmissionsDirs, string GroundTruthPath, string PromptPath, string OutDir, bool Run)
{
    public static Options Parse(string[] args)
    {
        // Mặc định chỉ lấy original/ và copied/, bỏ qua verify/ (không phải bài nộp).
        var submissions = new List<string>
        {
            Path.Combine("datasets", "week-01", "original"),
            Path.Combine("datasets", "week-01", "copied"),
        };
        string groundTruth = Path.Combine("datasets", "week-01", "ground-truth.csv");
        string prompt = Path.Combine("experiments", "week-01", "direct-ai", "prompt.md");
        string outDir = Path.Combine("experiments", "week-01", "direct-ai");
        bool run = false, submissionsGiven = false;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--submissions":
                    if (!submissionsGiven) { submissions.Clear(); submissionsGiven = true; }
                    submissions.Add(args[++i]);
                    break;
                case "--ground-truth": groundTruth = args[++i]; break;
                case "--prompt": prompt = args[++i]; break;
                case "--out": outDir = args[++i]; break;
                case "--run": run = true; break;
                default: throw new ArgumentException($"Tham số không hợp lệ: {args[i]}");
            }
        }
        return new Options(submissions, groundTruth, prompt, outDir, run);
    }
}

static class GroundTruth
{
    // Trả về các cặp sao chép trong ground-truth.csv: khóa là cặp đường dẫn đầy đủ (không phân biệt thứ tự), giá trị là level.
    public static Dictionary<string, string> Load(string csvPath)
    {
        string baseDir = Path.GetDirectoryName(Path.GetFullPath(csvPath))!;
        string[] lines = File.ReadAllLines(csvPath);
        string[] header = SplitLine(lines[0].TrimStart('﻿'));
        int copyIdx = Array.IndexOf(header, "copy_path");
        int originalIdx = Array.IndexOf(header, "original_path");
        int levelIdx = Array.IndexOf(header, "level");
        if (copyIdx < 0 || originalIdx < 0 || levelIdx < 0)
        {
            throw new InvalidOperationException("ground-truth.csv thiếu cột copy_path, original_path hoặc level.");
        }

        var map = new Dictionary<string, string>();
        foreach (string line in lines.Skip(1).Where(l => l.Trim().Length > 0))
        {
            string[] cells = SplitLine(line);
            string copy = Path.GetFullPath(Path.Combine(baseDir, cells[copyIdx]));
            string original = Path.GetFullPath(Path.Combine(baseDir, cells[originalIdx]));
            map[Key(copy, original)] = cells[levelIdx];
        }
        return map;
    }

    public static string Key(string a, string b) =>
        string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";

    static string[] SplitLine(string line)
    {
        var cells = new List<string>();
        var current = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quoted && c == '"' && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
            else if (c == '"') quoted = !quoted;
            else if (c == ',' && !quoted) { cells.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        cells.Add(current.ToString());
        return cells.ToArray();
    }
}

static class DotEnv
{
    // Đọc .env rồi .env.local ở thư mục hiện tại (thư mục gốc repo). Biến môi trường
    // đã đặt sẵn trong terminal luôn được ưu tiên; .env.local ghi đè .env.
    public static void Load()
    {
        var values = new Dictionary<string, string>();
        foreach (string file in new[] { ".env", ".env.local" })
        {
            if (!File.Exists(file)) continue;
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim();
                int eq = line.IndexOf('=');
                if (line.Length == 0 || line.StartsWith('#') || eq <= 0) continue;
                string value = line[(eq + 1)..].Trim().Trim('"');
                if (value.Length > 0) values[line[..eq].Trim()] = value;
            }
        }
        foreach (var (key, value) in values)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

static class PromptTemplate
{
    // Lấy khối ```text đầu tiên trong prompt.md làm mẫu prompt.
    public static string Load(string path)
    {
        Match m = Regex.Match(File.ReadAllText(path), "```text\\r?\\n(.*?)\\r?\\n```", RegexOptions.Singleline);
        if (!m.Success)
        {
            throw new InvalidOperationException($"Không tìm thấy khối ```text trong {path}");
        }
        return m.Groups[1].Value;
    }

    // Thay trong một lượt để nội dung bài A không bị thay nhầm {source_b}.
    public static string Fill(string template, string sourceA, string sourceB) =>
        Regex.Replace(template, @"\{source_(a|b)\}", m => m.Groups[1].Value == "a" ? sourceA : sourceB);
}

sealed record ChatResult(string Text, string? ResponseModel, string? FinishReason, long? InputTokens, long? OutputTokens);

sealed class RateLimitException(string message) : Exception(message);

sealed class ApiException(string message) : Exception(message);

static class ChatApi
{
    // Mỗi lần gọi là một request độc lập, chỉ gồm một tin nhắn user (không giữ ngữ cảnh).
    public static async Task<ChatResult> Send(HttpClient http, string endpoint, string model, string prompt)
    {
        string body = JsonSerializer.Serialize(new { model, messages = new[] { new { role = "user", content = prompt } } });
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        using HttpResponseMessage response = await http.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            string retry = response.Headers.TryGetValues("Retry-After", out var v) ? $"Retry-After: {v.First()}s" : "không có Retry-After";
            throw new RateLimitException($"{retry}; {ErrorMessage(json)}");
        }

        JsonElement root;
        try
        {
            root = JsonDocument.Parse(json).RootElement;
        }
        catch (JsonException)
        {
            throw new ApiException($"HTTP {(int)response.StatusCode}, phản hồi không phải JSON");
        }

        // OpenRouter đôi khi trả HTTP 200 kèm trường "error" khi nhà cung cấp phía sau lỗi.
        if (root.TryGetProperty("error", out JsonElement error))
        {
            string message = error.TryGetProperty("message", out var m) ? m.ToString() : "không rõ";
            int code = error.TryGetProperty("code", out var c) && c.TryGetInt32(out int n) ? n : (int)response.StatusCode;
            if (code == 429) throw new RateLimitException(Truncate(message));
            throw new ApiException($"HTTP {code}: {Truncate(message)}");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException($"HTTP {(int)response.StatusCode}");
        }

        // Model miễn phí đôi khi trả JSON thiếu choices/message; coi là lỗi API để lần chạy sau thử lại.
        if (!root.TryGetProperty("choices", out JsonElement choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new ApiException($"HTTP {(int)response.StatusCode}, phản hồi không có choices");
        }
        JsonElement choice = choices[0];
        string? finishReason = choice.TryGetProperty("finish_reason", out var fr) && fr.ValueKind == JsonValueKind.String ? fr.GetString() : null;
        string text = choice.TryGetProperty("message", out JsonElement reply)
            && reply.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String
            ? content.GetString()! : "";
        // Phản hồi rỗng là lỗi tạm thời của nhà cung cấp, không phải kết luận của AI: không ghi file để chạy lại hỏi lại cặp này.
        if (text.Trim().Length == 0)
        {
            throw new ApiException($"phan_hoi_rong (finish_reason={finishReason ?? "không rõ"})");
        }
        long? inTok = null, outTok = null;
        if (root.TryGetProperty("usage", out JsonElement usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var p) && p.TryGetInt64(out long pv)) inTok = pv;
            if (usage.TryGetProperty("completion_tokens", out var q) && q.TryGetInt64(out long qv)) outTok = qv;
        }
        return new ChatResult(
            text,
            root.TryGetProperty("model", out var rm) ? rm.GetString() : null,
            finishReason,
            inTok, outTok);
    }

    // Lấy nội dung lỗi OpenRouter trả kèm (nếu có) để biết nguyên nhân của 429.
    static string ErrorMessage(string json)
    {
        try
        {
            JsonElement root = JsonDocument.Parse(json).RootElement;
            if (root.TryGetProperty("error", out JsonElement e) && e.TryGetProperty("message", out JsonElement m))
            {
                return Truncate(m.ToString());
            }
        }
        catch (JsonException)
        {
        }
        return "không có nội dung lỗi";
    }

    static string Truncate(string s) => s.Length <= 300 ? s : s[..300] + "...";
}

sealed record PairRecord(
    string PairId, string A, string B, string Verdict, string Similarity,
    string ResponseFile, double LatencySeconds, long? InputTokens, long? OutputTokens, string Notes)
{
    public static void WriteResponseFile(string path, string pairId, string requestedModel, double latency, ChatResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"pair_id: {pairId}");
        sb.AppendLine($"requested_model: {requestedModel}");
        sb.AppendLine($"model: {r.ResponseModel}");
        sb.AppendLine($"latency_seconds: {latency.ToString("F2", CultureInfo.InvariantCulture)}");
        if (r.InputTokens is { } i) sb.AppendLine($"input_tokens: {i}");
        if (r.OutputTokens is { } o) sb.AppendLine($"output_tokens: {o}");
        sb.AppendLine($"finish_reason: {r.FinishReason}");
        sb.AppendLine("---");
        sb.Append(r.Text);
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    public static PairRecord FromFile(string id, string a, string b, string path)
    {
        string content = File.ReadAllText(path);
        Match head = Regex.Match(content, @"\A---\r?\n(.*?)\r?\n---\r?\n", RegexOptions.Singleline);
        string body = head.Success ? content[head.Length..] : content;
        string Meta(string key) => Regex.Match(head.Value, $@"^{key}: (.*)$", RegexOptions.Multiline).Groups[1].Value.Trim();

        // Chấp nhận nhãn có dấu (KẾT_LUẬN, ĐIỂM_GIỐNG) và định dạng Markdown/backtick quanh nhãn hoặc giá trị,
        // ví dụ "**KET_LUAN:** copy" hay "DIEM_GIONG: **85**". Phản hồi gốc vẫn được lưu nguyên trong file.
        const string Sep = @"[\s*_`:]*";
        Match verdict = Regex.Match(body, $@"K[EẾ]T[_ ]LU[AẬ]N{Sep}(not_copy|copy)\b", RegexOptions.IgnoreCase);
        Match score = Regex.Match(body, $@"[ĐD]I[ỂE]M[_ ]GI[ỐO]NG{Sep}(\d{{1,3}})\b", RegexOptions.IgnoreCase);
        string finish = Meta("finish_reason");
        string requested = Meta("requested_model");
        string actual = Meta("model");

        var notes = new List<string>();
        if (body.Trim().Length == 0) notes.Add("phan_hoi_rong");
        else if (!verdict.Success || !score.Success) notes.Add("khong_parse_duoc");
        if (finish.Length > 0 && !finish.Equals("stop", StringComparison.OrdinalIgnoreCase)) notes.Add($"finish_reason={finish}");
        if (requested.Length > 0 && actual.Length > 0 && !requested.Equals(actual, StringComparison.Ordinal)) notes.Add($"model_tra_ve={actual}");

        return new PairRecord(id, a, b,
            verdict.Success ? verdict.Groups[1].Value.ToLowerInvariant() : "",
            score.Success ? score.Groups[1].Value : "",
            path,
            double.TryParse(Meta("latency_seconds"), NumberStyles.Float, CultureInfo.InvariantCulture, out double l) ? l : 0,
            long.TryParse(Meta("input_tokens"), out long i) ? i : null,
            long.TryParse(Meta("output_tokens"), out long o) ? o : null,
            string.Join("; ", notes));
    }

    public static PairRecord Failed(string id, string a, string b, string note) =>
        new(id, a, b, "", "", "", 0, null, null, note);

    // correct / false_positive / missed; để trống nếu thiếu nhãn chuẩn hoặc AI chưa có kết luận hợp lệ.
    static string Outcome(string verdict, string truth) =>
        verdict.Length == 0 || truth.Length == 0 ? ""
        : verdict == truth ? "correct"
        : verdict == "copy" ? "false_positive" : "missed";

    public static void WriteCsv(string path, List<PairRecord> records, string outDir, Dictionary<string, (string Truth, string Level)> labels)
    {
        var sb = new StringBuilder();
        sb.AppendLine("pair_id,submission_a,submission_b,ai_verdict,ai_similarity,response_file,ground_truth,level,outcome,latency_seconds,input_tokens,output_tokens,notes");
        foreach (PairRecord r in records)
        {
            string relative = r.ResponseFile.Length == 0 ? "" : Path.GetRelativePath(outDir, r.ResponseFile).Replace('\\', '/');
            (string truth, string level) = labels.TryGetValue(r.PairId, out var l) ? l : ("", "");
            sb.AppendLine(string.Join(",", new[]
            {
                r.PairId, r.A, r.B, r.Verdict, r.Similarity, relative, truth, level, Outcome(r.Verdict, truth),
                r.LatencySeconds.ToString("F2", CultureInfo.InvariantCulture),
                r.InputTokens?.ToString(CultureInfo.InvariantCulture) ?? "",
                r.OutputTokens?.ToString(CultureInfo.InvariantCulture) ?? "",
                r.Notes,
            }.Select(Csv)));
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    static string Csv(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
