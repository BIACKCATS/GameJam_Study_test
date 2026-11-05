using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JSH.Data;
using UnityEditor;
using UnityEngine;

namespace JSH.Editor
{
    /// <summary>
    /// Converts a dialogue CSV (UTF-8) into one DialogueData asset per CharacterID:
    /// Assets/Prefabs/JSH/Data/Dialogue/Dialogue_&lt;CharacterID&gt;.asset (existing assets are overwritten in place,
    /// so Talkable references survive a re-import).
    /// Columns (header names, any order, case-insensitive): CharacterID, Path, Text, ChoiceA, ChoiceB, Outcome.
    ///  - No speaker column in the team format: the dialogue window shows the Talkable's display name.
    ///    A legacy optional "Speaker" column is still read (non-empty cell overrides the name for that line).
    ///  - Path: "S" = start, then one letter per choice taken ("SA", "SAB", ...). Several rows with the same Path are
    ///    consecutive lines; only the last one may have choices / Outcome.
    ///  - Outcome (final lines only): Embrace/포용 or Defeat/퇴치. Empty = dialogue just ends.
    ///  - Rows whose first cell starts with '#' and empty rows are ignored.
    /// Menu: JSH/Dialogue/Import CSV..., or right-click a .csv in the Project window: JSH/Import Dialogue CSV.
    /// </summary>
    public static class DialogueCsvImporter
    {
        private const string OutputFolder = "Assets/Prefabs/JSH/Data/Dialogue";
        private const string AssetPrefix = "Dialogue_";
        private const string LogTag = "[DialogueCsv]";

        private sealed class Row
        {
            public int Line;
            public string Path;
            public string Speaker;
            public string Text;
            public string ChoiceA;
            public string ChoiceB;
            public DialogueOutcome Outcome;
            public bool HasOutcomeText;
        }

        [MenuItem("JSH/Dialogue/Import CSV...")]
        private static void ImportFromFilePanel()
        {
            string path = EditorUtility.OpenFilePanel("Import dialogue CSV", Application.dataPath, "csv");
            if (!string.IsNullOrEmpty(path)) Import(path);
        }

        [MenuItem("Assets/JSH/Import Dialogue CSV")]
        private static void ImportSelected()
        {
            Import(Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject)));
        }

        [MenuItem("Assets/JSH/Import Dialogue CSV", true)]
        private static bool ImportSelectedValidate()
        {
            string path = Selection.activeObject != null ? AssetDatabase.GetAssetPath(Selection.activeObject) : null;
            return !string.IsNullOrEmpty(path) && path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

        public static void Import(string filePath)
        {
            string text;
            try
            {
                text = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LogTag} Can't read {filePath}: {e.Message}");
                return;
            }

            List<string[]> cells = ParseCsv(text, out List<int> lines);
            var errors = new List<string>();
            var warnings = new List<string>();
            Dictionary<string, List<Row>> byCharacter = ReadRows(cells, lines, errors, out List<string> order);
            if (byCharacter == null)
            {
                Report(Path.GetFileName(filePath), 0, errors, warnings);
                return;
            }

            EnsureFolder(OutputFolder);
            int written = 0;
            foreach (string id in order)
            {
                int errorCount = errors.Count;
                DialogueNode[] nodes = BuildNodes(id, byCharacter[id], errors, warnings);
                if (nodes == null || errors.Count > errorCount)
                {
                    errors.Add($"{id}: skipped (asset not changed).");
                    continue;
                }
                WriteAsset(id, nodes);
                written++;
            }
            AssetDatabase.SaveAssets();
            Report(Path.GetFileName(filePath), written, errors, warnings);
        }

        // ---------- CSV parsing ----------

        /// <summary>RFC 4180: quoted fields may contain commas, quotes ("") and line breaks. Returns rows + start line numbers.</summary>
        private static List<string[]> ParseCsv(string text, out List<int> startLines)
        {
            var rows = new List<string[]>();
            startLines = new List<int>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            int line = 1;
            int rowStart = 1;

            int i = text.Length > 0 && text[0] == '﻿' ? 1 : 0;
            for (; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n') line++;
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else if (c != '\r') field.Append(c);
                }
                else if (c == '"') inQuotes = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n')
                {
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row.ToArray());
                    startLines.Add(rowStart);
                    row.Clear();
                    rowStart = line;
                }
                else if (c != '\r') field.Append(c);
            }
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row.ToArray());
                startLines.Add(rowStart);
            }
            return rows;
        }

        private static bool IsSkippable(string[] r)
        {
            if (r.Length > 0 && r[0].TrimStart().StartsWith("#", StringComparison.Ordinal)) return true;
            for (int i = 0; i < r.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(r[i])) return false;
            }
            return true;
        }

        private static Dictionary<string, List<Row>> ReadRows(List<string[]> cells, List<int> lines, List<string> errors, out List<string> order)
        {
            order = new List<string>();
            int headerIndex = -1;
            for (int i = 0; i < cells.Count; i++)
            {
                if (!IsSkippable(cells[i])) { headerIndex = i; break; }
            }
            if (headerIndex < 0)
            {
                errors.Add("File is empty.");
                return null;
            }

            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string[] header = cells[headerIndex];
            for (int c = 0; c < header.Length; c++)
            {
                string name = header[c].Trim();
                if (name.Length > 0 && !columns.ContainsKey(name)) columns.Add(name, c);
            }
            int colId = Column(columns, "CharacterID"), colPath = Column(columns, "Path"), colText = Column(columns, "Text");
            if (colId < 0 || colPath < 0 || colText < 0)
            {
                errors.Add("Header must contain CharacterID, Path and Text (optional: ChoiceA, ChoiceB, Outcome).");
                return null;
            }
            int colSpeaker = Column(columns, "Speaker"), colA = Column(columns, "ChoiceA"), colB = Column(columns, "ChoiceB"), colOutcome = Column(columns, "Outcome");

            var result = new Dictionary<string, List<Row>>();
            for (int i = headerIndex + 1; i < cells.Count; i++)
            {
                string[] r = cells[i];
                if (IsSkippable(r)) continue;
                int line = lines[i];

                string id = Cell(r, colId);
                if (id.Length == 0) { errors.Add($"Line {line}: CharacterID is empty."); continue; }
                if (id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { errors.Add($"Line {line}: CharacterID '{id}' has characters not allowed in file names."); continue; }

                string outcomeText = Cell(r, colOutcome);
                if (!TryParseOutcome(outcomeText, out DialogueOutcome outcome))
                {
                    errors.Add($"{id} line {line}: unknown Outcome '{outcomeText}' (use Embrace/포용 or Defeat/퇴치).");
                    continue;
                }

                var row = new Row
                {
                    Line = line,
                    Path = Cell(r, colPath).ToUpperInvariant(),
                    Speaker = Cell(r, colSpeaker),
                    Text = Cell(r, colText), // inner line breaks are kept
                    ChoiceA = Cell(r, colA),
                    ChoiceB = Cell(r, colB),
                    Outcome = outcome,
                    HasOutcomeText = outcomeText.Length > 0,
                };
                if (!IsValidPath(row.Path)) { errors.Add($"{id} line {line}: Path '{row.Path}' must be S followed by A/B letters (e.g. S, SA, SAB)."); continue; }

                if (!result.TryGetValue(id, out List<Row> list))
                {
                    list = new List<Row>();
                    result.Add(id, list);
                    order.Add(id);
                }
                list.Add(row);
            }
            if (order.Count == 0) errors.Add("No dialogue rows found.");
            return result;
        }

        private static int Column(Dictionary<string, int> columns, string name) => columns.TryGetValue(name, out int c) ? c : -1;

        private static string Cell(string[] r, int c) => c >= 0 && c < r.Length ? r[c].Trim() : string.Empty;

        private static bool IsValidPath(string path)
        {
            if (path.Length == 0 || path[0] != 'S') return false;
            for (int i = 1; i < path.Length; i++)
            {
                if (path[i] != 'A' && path[i] != 'B') return false;
            }
            return true;
        }

        private static bool TryParseOutcome(string s, out DialogueOutcome outcome)
        {
            outcome = DialogueOutcome.None;
            if (s.Length == 0 || s.Equals("None", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Equals("Embrace", StringComparison.OrdinalIgnoreCase) || s == "포용" || s == "설득"
                || s.Equals("Persuade", StringComparison.OrdinalIgnoreCase))
            {
                outcome = DialogueOutcome.Embrace;
                return true;
            }
            if (s.Equals("Defeat", StringComparison.OrdinalIgnoreCase) || s == "퇴치"
                || s.Equals("Accuse", StringComparison.OrdinalIgnoreCase))
            {
                outcome = DialogueOutcome.Defeat;
                return true;
            }
            return false;
        }

        // ---------- Tree building ----------

        private static DialogueNode[] BuildNodes(string id, List<Row> rows, List<string> errors, List<string> warnings)
        {
            // Group rows by path (file order inside a path = line order).
            var byPath = new Dictionary<string, List<Row>>();
            var paths = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                Row r = rows[i];
                if (!byPath.TryGetValue(r.Path, out List<Row> list))
                {
                    list = new List<Row>();
                    byPath.Add(r.Path, list);
                    paths.Add(r.Path);
                }
                list.Add(r);
            }
            if (!byPath.ContainsKey("S"))
            {
                errors.Add($"{id}: no start row (Path = S).");
                return null;
            }

            // Breadth-first order: S, SA, SB, SAA, ... -> node 0 is the start line.
            paths.Sort((a, b) => a.Length != b.Length ? a.Length.CompareTo(b.Length) : string.CompareOrdinal(a, b));
            var firstIndex = new Dictionary<string, int>();
            int count = 0;
            foreach (string p in paths)
            {
                firstIndex.Add(p, count);
                count += byPath[p].Count;
            }

            var nodes = new DialogueNode[count];
            foreach (string p in paths)
            {
                List<Row> lines = byPath[p];
                int start = firstIndex[p];
                for (int i = 0; i < lines.Count; i++)
                {
                    Row r = lines[i];
                    bool isLast = i == lines.Count - 1;
                    bool hasA = r.ChoiceA.Length > 0, hasB = r.ChoiceB.Length > 0;
                    if (r.Text.Length == 0) warnings.Add($"{id} line {r.Line} ({p}): Text is empty.");

                    if (!isLast)
                    {
                        if (hasA || hasB || r.HasOutcomeText)
                            warnings.Add($"{id} line {r.Line} ({p}): choices/Outcome ignored - only the last line of a path may have them.");
                        nodes[start + i] = new DialogueNode(r.Speaker, r.Text, null, start + i + 1, DialogueOutcome.None);
                        continue;
                    }

                    if (hasA || hasB)
                    {
                        if (r.HasOutcomeText) warnings.Add($"{id} line {r.Line} ({p}): Outcome ignored - a line with choices is not the end.");
                        if (hasA != hasB) warnings.Add($"{id} line {r.Line} ({p}): only one choice filled (design expects A and B).");
                        var choices = new List<DialogueChoice>(2);
                        if (hasA) choices.Add(new DialogueChoice(r.ChoiceA, ChildIndex(id, p + "A", r, firstIndex, warnings)));
                        if (hasB) choices.Add(new DialogueChoice(r.ChoiceB, ChildIndex(id, p + "B", r, firstIndex, warnings)));
                        nodes[start + i] = new DialogueNode(r.Speaker, r.Text, choices.ToArray(), -1, DialogueOutcome.None);
                    }
                    else
                    {
                        if (r.Outcome == DialogueOutcome.None) warnings.Add($"{id} line {r.Line} ({p}): final line without Outcome - dialogue just ends.");
                        nodes[start + i] = new DialogueNode(r.Speaker, r.Text, null, -1, r.Outcome);
                    }
                }
            }

            // Unreachable branches (parent missing or parent has no choice for that letter).
            foreach (string p in paths)
            {
                if (p.Length == 1) continue;
                string parent = p.Substring(0, p.Length - 1);
                bool reachable = false;
                if (byPath.TryGetValue(parent, out List<Row> parentLines))
                {
                    Row last = parentLines[parentLines.Count - 1];
                    reachable = p[p.Length - 1] == 'A' ? last.ChoiceA.Length > 0 : last.ChoiceB.Length > 0;
                }
                if (!reachable) warnings.Add($"{id} line {byPath[p][0].Line} ({p}): unreachable - '{parent}' has no matching choice.");
            }
            return nodes;
        }

        private static int ChildIndex(string id, string childPath, Row r, Dictionary<string, int> firstIndex, List<string> warnings)
        {
            if (firstIndex.TryGetValue(childPath, out int index)) return index;
            warnings.Add($"{id} line {r.Line}: choice leads to '{childPath}' which has no rows - picking it ends the dialogue.");
            return -1;
        }

        // ---------- Assets ----------

        private static void WriteAsset(string id, DialogueNode[] nodes)
        {
            string assetPath = $"{OutputFolder}/{AssetPrefix}{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
            bool isNew = data == null;
            if (isNew) data = ScriptableObject.CreateInstance<DialogueData>();

            Undo.RecordObject(data, "Import Dialogue CSV");
            data.EditorSetNodes(nodes);
            if (isNew) AssetDatabase.CreateAsset(data, assetPath);
            else EditorUtility.SetDirty(data);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static void Report(string fileName, int written, List<string> errors, List<string> warnings)
        {
            for (int i = 0; i < errors.Count; i++) Debug.LogError($"{LogTag} {errors[i]}");
            for (int i = 0; i < warnings.Count; i++) Debug.LogWarning($"{LogTag} {warnings[i]}");
            string summary = $"{fileName}: {written} dialogue asset(s) written to {OutputFolder}\nErrors: {errors.Count}, Warnings: {warnings.Count} (details in Console)";
            Debug.Log($"{LogTag} {summary}");
            EditorUtility.DisplayDialog("Dialogue CSV Import", summary, "OK");
        }
    }
}
