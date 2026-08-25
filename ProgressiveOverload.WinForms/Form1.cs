using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Persistence;
using ProgressiveOverload.Core.Services;

namespace ProgressiveOverload.WinForms;

public partial class Form1 : Form
{
    private readonly IProgressiveOverloadRepository _repository;
    private readonly IProfileService _profileService;
    private readonly IWorkoutService _workoutService;
    private readonly IDefinitionService _definitionService;
    private readonly IHistorySummaryService _summaryService;
    private readonly IExerciseSelectionService _selectionService;

    private readonly Label _statusLabel = new() { AutoSize = true, Text = "Ready" };

    private readonly TextBox _profileNameText = new();
    private readonly DataGridView _baselineGrid = new();

    private readonly DateTimePicker _workoutDatePicker = new();
    private readonly DataGridView _workoutExerciseGrid = new();
    private readonly DataGridView _workoutSetGrid = new();
    private readonly TextBox _workoutNotesText = new();

    private readonly TextBox _definitionCategoryText = new();
    private readonly TextBox _definitionExerciseText = new();
    private readonly TextBox _definitionNotesText = new();
    private readonly DataGridView _variationGrid = new();
    private readonly ListBox _catalogList = new();

    private readonly TextBox _summaryText = new();

    private readonly TextBox _recommendCategoryText = new();
    private readonly TextBox _recommendText = new();

    public Form1()
    {
        InitializeComponent();

        _repository = new JsonFileProgressiveOverloadRepository();
        _profileService = new ProfileService(_repository);
        _workoutService = new WorkoutService(_repository);
        _definitionService = new DefinitionService(_repository);
        _summaryService = new HistorySummaryService(_repository);
        _selectionService = new ExerciseSelectionService(_repository);

        Text = "Progressive Overload";
        MinimumSize = new Size(1000, 700);

        BuildLayout();
        _ = LoadStartupDataAsync();
    }

    private async Task LoadStartupDataAsync()
    {
        await LoadProfileAsync();
        await RefreshCatalogAsync();
        await RefreshSummaryAsync();
        SetStatus($"Data file: {_repository.DataFilePath}");
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildProfileTab());
        tabs.TabPages.Add(BuildWorkoutTab());
        tabs.TabPages.Add(BuildDefinitionTab());
        tabs.TabPages.Add(BuildSummaryTab());
        tabs.TabPages.Add(BuildRecommendationTab());

        var statusPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 6) };
        statusPanel.Controls.Add(_statusLabel);

        root.Controls.Add(tabs, 0, 0);
        root.Controls.Add(statusPanel, 0, 1);

        Controls.Add(root);
    }

    private TabPage BuildProfileTab()
    {
        var tab = new TabPage("Profile");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

        var namePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        namePanel.Controls.Add(new Label { AutoSize = true, Text = "Name", Margin = new Padding(0, 8, 8, 0) });
        _profileNameText.Width = 320;
        namePanel.Controls.Add(_profileNameText);

        panel.Controls.Add(namePanel, 0, 0);
        panel.Controls.Add(new Label { Text = "Baselines: exercise | weight | reps | notes", AutoSize = true }, 0, 1);

        ConfigureBaselineGrid();
        panel.Controls.Add(_baselineGrid, 0, 2);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var saveButton = new Button { Text = "Save Profile", AutoSize = true };
        saveButton.Click += async (_, _) => await SaveProfileAsync();
        var reloadButton = new Button { Text = "Reload Profile", AutoSize = true };
        reloadButton.Click += async (_, _) => await LoadProfileAsync();
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(reloadButton);
        panel.Controls.Add(buttonPanel, 0, 3);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage BuildWorkoutTab()
    {
        var tab = new TabPage("Log Workout");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

        var datePanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        datePanel.Controls.Add(new Label { AutoSize = true, Text = "Session Date", Margin = new Padding(0, 8, 8, 0) });
        _workoutDatePicker.Format = DateTimePickerFormat.Short;
        _workoutDatePicker.Value = DateTime.Today;
        datePanel.Controls.Add(_workoutDatePicker);
        panel.Controls.Add(datePanel, 0, 0);

        panel.Controls.Add(new Label { Text = "Exercises (one row per exercise)", AutoSize = true }, 0, 1);

        ConfigureWorkoutExerciseGrid();
        panel.Controls.Add(_workoutExerciseGrid, 0, 2);

        panel.Controls.Add(new Label { Text = "Sets (Exercise # maps to exercise row number starting at 1)", AutoSize = true }, 0, 3);

        ConfigureWorkoutSetGrid();
        panel.Controls.Add(_workoutSetGrid, 0, 4);

        var notesPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        notesPanel.Controls.Add(new Label { AutoSize = true, Text = "Session Notes", Margin = new Padding(0, 8, 8, 0) });
        _workoutNotesText.Width = 650;
        notesPanel.Controls.Add(_workoutNotesText);
        panel.Controls.Add(notesPanel, 0, 5);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var saveButton = new Button { Text = "Save Workout", AutoSize = true };
        saveButton.Click += async (_, _) => await SaveWorkoutAsync();
        buttonPanel.Controls.Add(saveButton);
        panel.Controls.Add(buttonPanel, 0, 6);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage BuildDefinitionTab()
    {
        var tab = new TabPage("Definitions");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));

        panel.Controls.Add(BuildLabeledTextRow("Category", _definitionCategoryText), 0, 0);
        panel.Controls.Add(BuildLabeledTextRow("Exercise", _definitionExerciseText), 0, 1);
        panel.Controls.Add(BuildLabeledTextRow("Exercise Notes", _definitionNotesText), 0, 2);

        panel.Controls.Add(new Label { Text = "Variations (1-2 rows): name | volume multiplier | notes", AutoSize = true }, 0, 3);

        ConfigureVariationGrid();
        panel.Controls.Add(_variationGrid, 0, 4);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var saveButton = new Button { Text = "Save Definition", AutoSize = true };
        saveButton.Click += async (_, _) => await SaveDefinitionAsync();
        var refreshButton = new Button { Text = "Refresh Catalog", AutoSize = true };
        refreshButton.Click += async (_, _) => await RefreshCatalogAsync();
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(refreshButton);
        panel.Controls.Add(buttonPanel, 0, 5);

        panel.Controls.Add(new Label { Text = "Catalog", AutoSize = true }, 0, 6);
        _catalogList.Dock = DockStyle.Fill;
        panel.Controls.Add(_catalogList, 0, 7);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage BuildSummaryTab()
    {
        var tab = new TabPage("Summary");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var refreshButton = new Button { Text = "Refresh Summary", AutoSize = true };
        refreshButton.Click += async (_, _) => await RefreshSummaryAsync();
        panel.Controls.Add(refreshButton, 0, 0);

        _summaryText.Dock = DockStyle.Fill;
        _summaryText.Multiline = true;
        _summaryText.ScrollBars = ScrollBars.Vertical;
        _summaryText.ReadOnly = true;
        panel.Controls.Add(_summaryText, 0, 1);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage BuildRecommendationTab()
    {
        var tab = new TabPage("Recommendations");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        topPanel.Controls.Add(new Label { AutoSize = true, Text = "Category", Margin = new Padding(0, 8, 8, 0) });
        _recommendCategoryText.Width = 260;
        topPanel.Controls.Add(_recommendCategoryText);

        var recommendButton = new Button { Text = "Recommend", AutoSize = true };
        recommendButton.Click += async (_, _) => await RecommendAsync();
        topPanel.Controls.Add(recommendButton);

        panel.Controls.Add(topPanel, 0, 0);

        _recommendText.Dock = DockStyle.Fill;
        _recommendText.Multiline = true;
        _recommendText.ScrollBars = ScrollBars.Vertical;
        _recommendText.ReadOnly = true;
        panel.Controls.Add(_recommendText, 0, 1);

        tab.Controls.Add(panel);
        return tab;
    }

    private static Control BuildLabeledTextRow(string label, TextBox targetTextBox)
    {
        targetTextBox.Width = 600;
        var row = new FlowLayoutPanel { Dock = DockStyle.Fill };
        row.Controls.Add(new Label { AutoSize = true, Text = label, Margin = new Padding(0, 8, 8, 0), Width = 120 });
        row.Controls.Add(targetTextBox);
        return row;
    }

    private void ConfigureBaselineGrid()
    {
        _baselineGrid.Dock = DockStyle.Fill;
        _baselineGrid.AutoGenerateColumns = false;
        _baselineGrid.AllowUserToAddRows = true;
        _baselineGrid.AllowUserToDeleteRows = true;
        _baselineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Exercise", Name = "Exercise", Width = 220 });
        _baselineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Weight", Name = "Weight", Width = 90 });
        _baselineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Reps", Name = "Reps", Width = 80 });
        _baselineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes", Name = "Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private void ConfigureWorkoutExerciseGrid()
    {
        _workoutExerciseGrid.Dock = DockStyle.Fill;
        _workoutExerciseGrid.AutoGenerateColumns = false;
        _workoutExerciseGrid.AllowUserToAddRows = true;
        _workoutExerciseGrid.AllowUserToDeleteRows = true;
        _workoutExerciseGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Exercise", Name = "Exercise", Width = 220 });
        _workoutExerciseGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Variation", Name = "Variation", Width = 150 });
        _workoutExerciseGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes", Name = "Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private void ConfigureWorkoutSetGrid()
    {
        _workoutSetGrid.Dock = DockStyle.Fill;
        _workoutSetGrid.AutoGenerateColumns = false;
        _workoutSetGrid.AllowUserToAddRows = true;
        _workoutSetGrid.AllowUserToDeleteRows = true;
        _workoutSetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Exercise #", Name = "ExerciseIndex", Width = 90 });
        _workoutSetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Reps", Name = "Reps", Width = 80 });
        _workoutSetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Weight", Name = "Weight", Width = 100 });
    }

    private void ConfigureVariationGrid()
    {
        _variationGrid.Dock = DockStyle.Fill;
        _variationGrid.AutoGenerateColumns = false;
        _variationGrid.AllowUserToAddRows = true;
        _variationGrid.AllowUserToDeleteRows = true;
        _variationGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Variation", Name = "Variation", Width = 180 });
        _variationGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Volume Multiplier", Name = "Multiplier", Width = 140 });
        _variationGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes", Name = "Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private async Task SaveProfileAsync()
    {
        try
        {
            var baselines = new List<ExerciseBaselineInput>();
            foreach (DataGridViewRow row in _baselineGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var exercise = GetCellText(row, "Exercise");
                if (string.IsNullOrWhiteSpace(exercise))
                {
                    continue;
                }

                if (!TryReadDecimal(row, "Weight", out var weight) || !TryReadInt(row, "Reps", out var reps))
                {
                    SetStatus("Baseline rows require valid numeric Weight and Reps values.");
                    return;
                }

                baselines.Add(new ExerciseBaselineInput
                {
                    ExerciseName = exercise,
                    BaselineWeight = weight,
                    BaselineReps = reps,
                    Notes = GetCellText(row, "Notes")
                });
            }

            var result = await _profileService.UpsertProfileAsync(new ProfileUpsertRequest
            {
                Name = _profileNameText.Text,
                ExerciseBaselines = baselines
            });

            SetStatus(result.IsSuccess ? "Profile saved." : $"Profile error: {result.ErrorMessage}");
            await RefreshSummaryAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Unexpected profile error: {ex.Message}");
        }
    }

    private async Task LoadProfileAsync()
    {
        var data = await _repository.LoadAsync();
        var profile = data.UserProfile;

        _profileNameText.Text = profile?.Name ?? string.Empty;
        _baselineGrid.Rows.Clear();

        if (profile is not null)
        {
            foreach (var baseline in profile.ExerciseBaselines)
            {
                _baselineGrid.Rows.Add(baseline.ExerciseName, baseline.BaselineWeight, baseline.BaselineReps, baseline.Notes ?? string.Empty);
            }
        }
    }

    private async Task SaveWorkoutAsync()
    {
        try
        {
            var exerciseRows = new List<(string Exercise, string? Variation, string? Notes)>();
            foreach (DataGridViewRow row in _workoutExerciseGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var exerciseName = GetCellText(row, "Exercise");
                if (string.IsNullOrWhiteSpace(exerciseName))
                {
                    continue;
                }

                exerciseRows.Add((exerciseName, EmptyToNull(GetCellText(row, "Variation")), EmptyToNull(GetCellText(row, "Notes"))));
            }

            if (exerciseRows.Count == 0)
            {
                SetStatus("Add at least one exercise row before saving a workout.");
                return;
            }

            var setsByExercise = new Dictionary<int, List<SetDraft>>();
            foreach (DataGridViewRow row in _workoutSetGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                if (!TryReadInt(row, "ExerciseIndex", out var exerciseIndex) || exerciseIndex <= 0 || exerciseIndex > exerciseRows.Count)
                {
                    SetStatus($"Set rows require a valid Exercise # between 1 and {exerciseRows.Count}.");
                    return;
                }

                if (!TryReadInt(row, "Reps", out var reps) || !TryReadDecimal(row, "Weight", out var weight))
                {
                    SetStatus("Set rows require valid numeric Reps and Weight values.");
                    return;
                }

                if (!setsByExercise.TryGetValue(exerciseIndex, out var sets))
                {
                    sets = [];
                    setsByExercise[exerciseIndex] = sets;
                }

                sets.Add(new SetDraft { Reps = reps, Weight = weight });
            }

            var exerciseDrafts = new List<ExerciseDraft>();
            for (var i = 0; i < exerciseRows.Count; i++)
            {
                var exerciseNumber = i + 1;
                if (!setsByExercise.TryGetValue(exerciseNumber, out var sets) || sets.Count == 0)
                {
                    SetStatus($"Exercise #{exerciseNumber} needs at least one set row.");
                    return;
                }

                var row = exerciseRows[i];
                exerciseDrafts.Add(new ExerciseDraft
                {
                    ExerciseName = row.Exercise,
                    VariationName = row.Variation,
                    Notes = row.Notes,
                    Sets = sets
                });
            }

            var request = new WorkoutSessionDraft
            {
                SessionDate = DateOnly.FromDateTime(_workoutDatePicker.Value.Date),
                Notes = EmptyToNull(_workoutNotesText.Text),
                Exercises = exerciseDrafts
            };

            var result = await _workoutService.LogWorkoutAsync(request);
            SetStatus(result.IsSuccess ? "Workout saved." : $"Workout error: {result.ErrorMessage}");
            await RefreshSummaryAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Unexpected workout error: {ex.Message}");
        }
    }

    private async Task SaveDefinitionAsync()
    {
        try
        {
            var variations = new List<ExerciseVariationInput>();
            foreach (DataGridViewRow row in _variationGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var variationName = GetCellText(row, "Variation");
                if (string.IsNullOrWhiteSpace(variationName))
                {
                    continue;
                }

                if (!TryReadDecimal(row, "Multiplier", out var multiplier))
                {
                    SetStatus("Variation rows require a valid numeric multiplier.");
                    return;
                }

                variations.Add(new ExerciseVariationInput
                {
                    Name = variationName,
                    VolumeMultiplier = multiplier,
                    Notes = EmptyToNull(GetCellText(row, "Notes"))
                });
            }

            var result = await _definitionService.UpsertExerciseDefinitionAsync(new UpsertExerciseDefinitionRequest
            {
                CategoryName = _definitionCategoryText.Text,
                ExerciseName = _definitionExerciseText.Text,
                Notes = EmptyToNull(_definitionNotesText.Text),
                Variations = variations
            });

            SetStatus(result.IsSuccess ? "Definition saved." : $"Definition error: {result.ErrorMessage}");
            await RefreshCatalogAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Unexpected definition error: {ex.Message}");
        }
    }

    private async Task RefreshCatalogAsync()
    {
        var catalog = await _definitionService.GetCatalogAsync();
        _catalogList.Items.Clear();

        foreach (var exercise in catalog.Exercises.OrderBy(e => e.CategoryName).ThenBy(e => e.Name))
        {
            _catalogList.Items.Add($"[{exercise.CategoryName}] {exercise.Name}");
            foreach (var variation in exercise.Variations)
            {
                _catalogList.Items.Add($"    - {variation.Name} (multiplier {variation.VolumeMultiplier})");
            }
        }
    }

    private async Task RefreshSummaryAsync()
    {
        var data = await _repository.LoadAsync();
        var summary = await _summaryService.GetSummaryAsync();

        var lines = new List<string>();

        lines.Add("Baselines");
        if (data.UserProfile?.ExerciseBaselines.Count > 0)
        {
            foreach (var baseline in data.UserProfile.ExerciseBaselines)
            {
                lines.Add($"- {baseline.ExerciseName}: {baseline.BaselineWeight} x {baseline.BaselineReps}");
            }
        }
        else
        {
            lines.Add("- none");
        }

        lines.Add(string.Empty);
        lines.Add("Workout Summary");
        if (summary.Exercises.Count > 0)
        {
            foreach (var item in summary.Exercises)
            {
                lines.Add($"- {item.ExerciseName}: sets={item.TotalSets}, reps={item.TotalReps}, volume={item.TotalVolume}, last={item.LastPerformedDate:yyyy-MM-dd}");
            }
        }
        else
        {
            lines.Add("- none");
        }

        _summaryText.Text = string.Join(Environment.NewLine, lines);
    }

    private async Task RecommendAsync()
    {
        var category = _recommendCategoryText.Text;
        var recommendation = await _selectionService.RecommendNextVariationAsync(category);
        if (recommendation is null)
        {
            _recommendText.Text = "No recommendation available for that category.";
            return;
        }

        var lastPerformedText = recommendation.LastPerformedDate?.ToString("yyyy-MM-dd") ?? "never";
        _recommendText.Text =
            $"Category: {recommendation.CategoryName}{Environment.NewLine}" +
            $"Exercise: {recommendation.ExerciseName}{Environment.NewLine}" +
            $"Variation: {recommendation.VariationName}{Environment.NewLine}" +
            $"Weighted Volume: {recommendation.WeightedVolume}{Environment.NewLine}" +
            $"Last Performed: {lastPerformedText}{Environment.NewLine}" +
            $"Days Since: {recommendation.DaysSinceLastPerformed}{Environment.NewLine}" +
            $"Score: {recommendation.Score}{Environment.NewLine}" +
            $"Reason: {recommendation.Reason}";
    }

    private static string GetCellText(DataGridViewRow row, string columnName)
    {
        return Convert.ToString(row.Cells[columnName].Value)?.Trim() ?? string.Empty;
    }

    private static bool TryReadInt(DataGridViewRow row, string columnName, out int value)
    {
        return int.TryParse(GetCellText(row, columnName), out value);
    }

    private static bool TryReadDecimal(DataGridViewRow row, string columnName, out decimal value)
    {
        return decimal.TryParse(GetCellText(row, columnName), out value);
    }

    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }
}
